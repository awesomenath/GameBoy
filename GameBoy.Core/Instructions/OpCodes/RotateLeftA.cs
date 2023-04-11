using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class RotateLeftA : OpCode<byte>
    {
        public RotateLeftA()
            : base(0x17, 1, 4, OpCodeType.ByteModify)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Rotate A left. Old bit 7 to Carry flag. Carry flag to bit 0.
            if (cpu.FlagC)
            {
                cpu.FlagC = (cpu.A & 0x80) != 0;
                cpu.A = (byte)((cpu.A << 1) | 0x01);
            }
            else
            {
                cpu.FlagC = (cpu.A & 0x80) != 0;
                cpu.A <<= 1;
            }

            cpu.FlagN = false;
            cpu.FlagH = false;
            //cpu.FlagZ = cpu.A == 0;
            cpu.FlagZ = false;

            return base.Execute(cpu, mmu);
        }
    }
}
