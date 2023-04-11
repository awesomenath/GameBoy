using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class RotateRightA : OpCode<byte>
    {
        public RotateRightA() 
            : base(0x1F, 1, 4, OpCodeType.ByteModify)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var newCFlagVal = (cpu.A & 0x01) != 0;
            cpu.A >>= 1;

            // Rotate A right. Old bit 0 to Carry flag. Carry flag to bit 7.
            if (cpu.FlagC)
            {
                cpu.A |= 0x80;
            }

            cpu.FlagC = newCFlagVal;
            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagZ = false;

            return base.Execute(cpu, mmu);
        }
    }
}
