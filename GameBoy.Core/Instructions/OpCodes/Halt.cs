using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Halt : OpCode<object>
    {
        public Halt()
            : base(0x76, 1, 4, OpCodeType.MiscControl)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Halt
            cpu.IsHalted = true;

            return base.Execute(cpu, mmu);
        }
    }
}
