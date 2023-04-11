using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class ShiftRightKeep : OpCode<byte>
    {
        public ShiftRightKeep(byte id, byte length, byte cycles, Operand<byte> leftOperand)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Shift right into Carry. MSB doesn't change
            byte operandValue = LeftOperand.Get();

            cpu.FlagC = (operandValue & 0x01) != 0;

            var highValue = (operandValue & 0x80) != 0;
            operandValue = (byte)(operandValue >> 1);

            if (highValue)
            {
                operandValue |= 0x80;
            }

            LeftOperand.Set(operandValue);

            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagZ = LeftOperand.Get() == 0;

            return base.Execute(cpu, mmu);
        }
    }
}
