using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class RotateRight : OpCode<byte>
    {
        public RotateRight(byte id, byte length, byte cycles, Operand<byte> leftOperand)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Rotate right. Old bit 0 to Carry flag. Carry flag to bit 7.
            byte operandValue = LeftOperand.Get();

            if (cpu.FlagC)
            {
                cpu.FlagC = (operandValue & 0x01) != 0;
                LeftOperand.Set((byte)((operandValue >> 1) | 0x80));
            }
            else
            {
                cpu.FlagC = (operandValue & 0x01) != 0;
                LeftOperand.Set((byte)(operandValue >> 1));
            }

            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagZ = LeftOperand.Get() == 0;

            return base.Execute(cpu, mmu);
        }
    }
}
