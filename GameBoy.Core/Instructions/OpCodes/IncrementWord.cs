using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class IncrementWord : OpCode<ushort>
    {
        public IncrementWord(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<ushort> leftOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var val = LeftOperand.Get();

            var newVal = (ushort)((val == ushort.MaxValue) ? 0 : (val + 1));

            LeftOperand.Set(newVal);

            return base.Execute(cpu, mmu);
        }
    }
}
