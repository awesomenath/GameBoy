using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class SetCarryFlag : OpCode<object>
    {
        public SetCarryFlag() 
            : base(0x37, 1, 4, OpCodeType.ByteMathLogic)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Set the carry flag
            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagC = true;

            return base.Execute(cpu, mmu);
        }
    }
}
