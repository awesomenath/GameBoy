using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class ResetBit : OpCode<byte>
    {
        byte BitIndex { get; set; }
        byte Filter { get; }

        public ResetBit(byte id, byte length, byte cycles, Operand<byte> leftOperand, byte bitIndex)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            LeftOperand = leftOperand;
            BitIndex = bitIndex;
            Filter = (byte) ~(0x01 << bitIndex);
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            byte operandValue = LeftOperand.Get();

            operandValue &= Filter;

            LeftOperand.Set(operandValue);

            return base.Execute(cpu, mmu);
        }
    }
}
