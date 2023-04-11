using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class ReturnInterrupt : OpCode<object>
    {
        public ReturnInterrupt()
            : base(0xD9, 1, 16, OpCodeType.JumpCalls)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var result = mmu.ReadWord(cpu.StackPointer);

            cpu.StackPointer += 2;

            cpu.ProgramCounter = result;

            cpu.InterruptsEnabled = true;

            return new OpCodeResult(Length, Cycles, false);
        }
    }
}
