using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class LoadWord : OpCode<ushort>
    {
        public LoadWord(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<ushort> leftOperand, Operand<ushort> rightOperand) 
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
