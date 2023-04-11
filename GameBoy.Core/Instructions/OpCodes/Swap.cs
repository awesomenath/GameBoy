using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Swap : OpCode<byte>
    {
        public Swap(byte id, byte length, byte cycles, Operand<byte> leftOperand)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            byte operandValue = LeftOperand.Get();

            byte leftNibble = (byte)(operandValue & 0xF0);            
            leftNibble >>= 4; // Get a copy of the upper nibble shifted over

            operandValue <<= 4; // shift lower nibble over

            operandValue |= leftNibble;

            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagC = false;
            cpu.FlagZ = operandValue == 0;

            LeftOperand.Set(operandValue);

            return base.Execute(cpu, mmu);
        }
    }
}
