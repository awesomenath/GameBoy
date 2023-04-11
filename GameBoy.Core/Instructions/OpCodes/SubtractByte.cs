using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class SubtractByte : OpCode<byte>
    {
        public SubtractByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand, Operand<byte> rightOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Subtract
            var leftVal = LeftOperand.Get();
            var rightVal = RightOperand.Get();
            var result = leftVal - rightVal;
            var resultByte = (byte)result;

            cpu.FlagC = leftVal < rightVal; // Made Carry

            var halfCarryoccurred = (leftVal & 0x0F) < (rightVal & 0x0F);
            cpu.FlagH = halfCarryoccurred;

            cpu.FlagZ = resultByte == 0;
            cpu.FlagN = true; // Subtract

            LeftOperand.Set(resultByte);

            return base.Execute(cpu, mmu);
        }
    }
}
