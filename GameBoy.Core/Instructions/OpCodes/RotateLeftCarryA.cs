using GameBoy.Core.Hardware;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class RotateLeftCarryA : OpCode<byte>
    {
        public RotateLeftCarryA() 
            : base(0x07, 1, 4, OpCodeType.ByteModify)
        { }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Rotate A left. Old bit 7 to Carry flag.

            if ((cpu.A & 0x80) != 0)
            {
                cpu.FlagC = true;
                cpu.A = (byte)((cpu.A << 1) | 0x01);
            }
            else
            {
                cpu.FlagC = false;
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
