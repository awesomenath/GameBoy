using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class ComplementCarryFlag : OpCode<object>
    {
        public ComplementCarryFlag() 
            : base(0x3F, 1, 4, OpCodeType.ByteMathLogic)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Set the flags
            cpu.FlagN = false;
            cpu.FlagH = false;

            cpu.FlagC = !cpu.FlagC; // Invert / complement the Carry flag

            return base.Execute(cpu, mmu);
        }
    }
}
