using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class DisableInterrupt : OpCode<object>
    {
        public DisableInterrupt() 
            : base(0xF3, 1, 4, OpCodeType.MiscControl)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Disable Interrupts
            cpu.InterruptsEnabled = false;

            return base.Execute(cpu, mmu);
        }
    }
}
