using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class AddByte : OpCode<byte>
    {
        public AddByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand, Operand<byte> rightOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Add
            byte leftVal = LeftOperand.Get();
            byte rightVal = RightOperand.Get();

            var addResult = leftVal + rightVal;
            var addResultByte = (byte)addResult;

            // Half Carry is set if adding the lower nibbles of the value and register A
            // together result in a value bigger than 0x0F. If the result is larger than 0xF
            // than the addition caused a carry from the lower nibble to the upper nibble.
            if ((leftVal & 0x0F) + (rightVal & 0x0F) > 0x0F)
            {
                cpu.FlagH = true;
            }
            else
            {
                cpu.FlagH = false;
            }

            cpu.FlagC = addResult > byte.MaxValue;
            cpu.FlagZ = addResultByte == 0;
            cpu.FlagN = false; // No Subtract

            LeftOperand.Set(addResultByte);

            return base.Execute(cpu, mmu);
        }
    }
}
