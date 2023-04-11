using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class NoOp : OpCode<object>
    {
        public NoOp() 
            : base(0, 1, 4, OpCodeType.MiscControl)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // No op
            return base.Execute(cpu, mmu);
        }
    }
}
