using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class SubtractCarryByte : OpCode<byte>
    {
        public SubtractCarryByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand, Operand<byte> rightOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Subtract
            byte leftVal = LeftOperand.Get();
            byte rightVal = RightOperand.Get();
            var result = leftVal - rightVal;

            // If carry flag, then subtract 1 to result
            if (cpu.FlagC)
            {
                result--;
            }

            var halfCarryOccurred = (leftVal & 0x0F) < ((rightVal & 0x0F) + (cpu.FlagC ? 1 : 0));
            cpu.FlagH = halfCarryOccurred;

            var resultByte = (byte)result;

            cpu.FlagC = result < 0; // Made Carry
            cpu.FlagZ = resultByte == 0;
            cpu.FlagN = true; // Subtract

            LeftOperand.Set(resultByte);

            return base.Execute(cpu, mmu);
        }
    }
}
