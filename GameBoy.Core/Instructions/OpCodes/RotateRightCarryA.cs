using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class RotateRightCarryA : OpCode<byte>
    {
        public RotateRightCarryA() 
            : base(0x0F, 1, 4, OpCodeType.ByteModify)
        {
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Rotate A right. Old bit 0 to Carry flag. Old bit 0 to bit 7.
            if ((cpu.A & 0x01) != 0)
            {
                cpu.FlagC = true;
                cpu.A = (byte)((cpu.A >> 1) | 0x80);
            }
            else
            {
                cpu.FlagC = false;
                cpu.A >>= 1;
            }

            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagZ = false;

            return base.Execute(cpu, mmu);
        }
    }
}
