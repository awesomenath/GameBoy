using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class RotateRightCarry : OpCode<byte>
    {
        public RotateRightCarry(byte id, byte length, byte cycles, Operand<byte> operand)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            LeftOperand = operand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Rotate A right. Old bit 0 to Carry flag. Old bit 0 to bit 7.
            byte operandValue = LeftOperand.Get();

            if ((operandValue & 0x01) != 0)
            {
                cpu.FlagC = true;
                LeftOperand.Set((byte)((operandValue >> 1) | 0x80));
            }
            else
            {
                cpu.FlagC = false;
                LeftOperand.Set((byte)(operandValue >> 1));
            }

            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagZ = LeftOperand.Get() == 0;

            return base.Execute(cpu, mmu);
        }
    }
}
