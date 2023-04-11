using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class EnableInterrupt : OpCode<object>
    {
        public EnableInterrupt() 
            : base(0xFB, 1, 4, OpCodeType.MiscControl)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Enable Interrupts
            cpu.InterruptsEnabled = true;

            return base.Execute(cpu, mmu);
        }
    }
}
