using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class AddCarryByte : OpCode<byte>
    {
        public AddCarryByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand, Operand<byte> rightOperand) 
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

            // Half Carry is set if adding the lower nibbles of the value and register A
            // together result in a value bigger than 0xF. If the result is larger than 0xF
            // than the addition caused a carry from the lower nibble to the upper nibble.
            //if ((leftVal & 0x0F) + (rightVal & 0x0F) > 0x0F)
            //if ((((leftVal & 0x0F) + (rightVal & 0x0F)) & 0x10) == 0x10)
            var halfCarryoccurred = (leftVal & 0x0F) + (rightVal & 0x0F) + (cpu.FlagC ? 1 : 0) > 0x0F;
            cpu.FlagH = halfCarryoccurred;

            // If carry flag, then add 1 to result
            if (cpu.FlagC)
            {
                addResult++;
            }

            cpu.FlagC = addResult > byte.MaxValue;

            var addResultByte = (byte)addResult;

            cpu.FlagZ = addResultByte == 0;
            cpu.FlagN = false; // No Subsctract

            LeftOperand.Set(addResultByte);

            return base.Execute(cpu, mmu);
        }
    }
}
