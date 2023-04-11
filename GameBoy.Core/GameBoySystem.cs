using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions;
using GameBoy.Core.Instructions.OpCodes;
using GameBoy.Core.Instructions.Operands;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GameBoy.Core
{
    public class GameBoySystem
    {
        const int ClockSpeed = 4_194_304;
        const int MaxCyclesPerFrame = ClockSpeed / 60;

        private Action CpuCycleCompleted { get; }
        public Action ExceptionEvent { get; }
        private bool SkipBoot { get; }

        public Cpu Cpu { get; private set; }
        public Mmu Mmu { get; private set; }
        public Gpu Gpu { get; private set; }
        public Hardware.Timer Timer { get; private set; }
        public RegisterOperandManager RegisterOperandManager { get; private set; }
        public MemoryOperandManager MemoryOperandManager { get; private set; }
        public OpCodeManager OpCodeManager { get; private set; }
        public Joypad Joypad { get; private set; }
        public Cartridge Cartridge { get; private set; }

        private bool DelayedEnableInterruptRequested = false;

        public GameBoySystem(string gameBoyRomFile, bool skipBoot, JoypadState joypadState, Func<byte[], Task> frameRenderedCallback, Action cpuCycleCompleted, Action exceptionEvent)
        {
            //var bytes = File.ReadAllBytes(gameBoyRomFile);
            Cartridge = new Cartridge(gameBoyRomFile);

            SkipBoot = skipBoot;

            CpuCycleCompleted = cpuCycleCompleted;
            ExceptionEvent = exceptionEvent;

            Cpu = new Cpu();
            Mmu = new Mmu(Cartridge);
            Gpu = new Gpu(Mmu, frameRenderedCallback);
            Timer = new Hardware.Timer(Mmu);

            RegisterOperandManager = new RegisterOperandManager(Cpu);
            MemoryOperandManager = new MemoryOperandManager(Cpu, Mmu);
            OpCodeManager = new OpCodeManager(Cpu, RegisterOperandManager, MemoryOperandManager);
            Joypad = new Joypad(Mmu, joypadState);
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            OpCodeResult defaultOpCodeResult = new OpCodeResult()
            {
                MoveProgramCounter = false,
                StopRequested = false
            };

            const float frameTime = 1000 / (float)60;
            var frameTimeDelay = TimeSpan.FromMilliseconds(frameTime);
            var stopWatch = new Stopwatch();

            if (SkipBoot)
            {
                Mmu.LeaveBootRom();
                Cpu.ProgramCounter = 100;
            }

            try
            {
                bool executingInstructions = true;

                while (!cancellationToken.IsCancellationRequested)
                {
                    stopWatch.Restart();

                    // Conduct operations
                    int operationsPerFrame = 0;
                    int operationsPerStep = 0;

                    while (executingInstructions && operationsPerFrame < MaxCyclesPerFrame)
                    {
                        try
                        {
                            operationsPerStep = 0;

                            var interruptResult = ProccessInterrupts();

                            if (interruptResult)
                            {
                                operationsPerStep += 20; // Takes 5 cpu cycles to handle an interrrupt
                            }

                            if (DelayedEnableInterruptRequested)
                            {
                                Mmu.WriteByte(Mmu.InterruptFlags.Start, 0);
                                Cpu.InterruptsEnabled = true;
                                DelayedEnableInterruptRequested = false;
                            }

                            OpCodeResult result = defaultOpCodeResult;

                            if (!Cpu.IsHalted)
                            {
                                var opCode = OpCodeManager.GetOpCode(Cpu, Mmu);

                                if (opCode is EnableInterrupt)
                                {
                                    DelayedEnableInterruptRequested = true;
                                }

                                if (opCode.Id == 0x40)
                                {
                                    // Homebrew debug indicator
                                    //throw new InvalidOperationException("LD B, B.");
                                }

                                result = opCode.Execute(Cpu, Mmu);

                                operationsPerStep += result.Cycles;
                            }
                            else
                            {
                                operationsPerStep += 4; // Faking background operations
                            }

                            // Update the GPU
                            Gpu.Step(operationsPerStep);

                            // Update the timer
                            Timer.Update(operationsPerStep);

                            if (result.MoveProgramCounter)
                            {
                                Cpu.ProgramCounter += result.Length;

                                if (Mmu.InBootRom && Cpu.ProgramCounter >= 0x0100)
                                {
                                    Mmu.LeaveBootRom();
                                }
                            }

                            // STOP processor & screen until button press
                            if (result.StopRequested)
                            {
                                Timer.ResetDiv();

                                Gpu.ForceRender();
                                Debug.WriteLine("STOP");
                            }

                            operationsPerFrame += operationsPerStep;

                            CpuCycleCompleted?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            executingInstructions = false;

                            Gpu.ForceRender();
                            CpuCycleCompleted?.Invoke();

                            Debug.WriteLine("Exception", ex.ToString());

                            ExceptionEvent.Invoke();
                        }
                    }

                    stopWatch.Stop();

                    var waitTime = frameTimeDelay.Subtract(stopWatch.Elapsed);

                    // Wait if we have time to spare in this frame
                    if (waitTime.TotalMilliseconds > 0)
                    {
                        Thread.Sleep(waitTime);
                    }
                }
            }
            catch (Exception ex)
            { 
                Debugger.Break();
            }
        }

        private bool ProccessInterrupts()
        {
            if (!Cpu.InterruptsEnabled && !Cpu.IsHalted)
            {
                return false;
            }

            var interruptEnabledFlags = Mmu.ReadByte(0xFFFF);
            var interruptRequiredFlags = Mmu.ReadByte(Mmu.InterruptFlags.Start);

            var flags = interruptEnabledFlags & interruptRequiredFlags;

            // If no interrupts requested or enabled
            if (flags == 0)
            {
                return false;
            }
            else if (!Cpu.InterruptsEnabled)
            {
                Cpu.IsHalted = false;
                return false;
            }

            if ((flags & 0x01) != 0)
            {
                // VBlank interrupt
                RaiseInterrupt(0x0040);
                Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(interruptRequiredFlags & 0xFE));

                // Debug.WriteLine("VBlank interrupt triggered");
                return true;
            }
            if ((flags & 0x02) != 0)
            {
                // LCD STAT interrupt
                RaiseInterrupt(0x0048);
                Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(interruptRequiredFlags & 0xFD));

                // Debug.WriteLine("LCD STAT interrupt triggered");
                return true;
            }
            else if ((flags & 0x04) != 0)
            {
                // Timer interrupt
                RaiseInterrupt(0x0050);
                Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(interruptRequiredFlags & 0xFB));

                // Debug.WriteLine("Timer interrupt triggered");
                return true;
            }
            else if ((flags & 0x08) != 0)
            {
                // Serial interrupt
                RaiseInterrupt(0x0058);
                Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(interruptRequiredFlags & 0xF7));

                Debug.WriteLine("Serial interrupt triggered");
                return true;
            }
            else if ((flags & 0x10) != 0)
            {
                // Joypad interrupt
                RaiseInterrupt(0x0060);
                Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(interruptRequiredFlags & 0xEF));

                Debug.WriteLine("Joypad interrupt triggered");
                return true;
            }

            return false;
        }

        private void RaiseInterrupt(ushort interruptAddress)
        {
            Cpu.InterruptsEnabled = false;
            Cpu.IsHalted = false;

            OpCodeManager.PushPcToStackOpCode.Execute(Cpu, Mmu);

            Cpu.ProgramCounter = interruptAddress;
        }
    }
}
