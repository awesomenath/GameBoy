using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class LoadByte : OpCode<byte>
    {
        public LoadByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand, Operand<byte> rightOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            LeftOperand.Set(RightOperand.Get());

            return base.Execute(cpu, mmu);
        }
    }
}
