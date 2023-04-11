using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Pop : OpCode<ushort>
    {
        public Pop(byte id, Operand<ushort> leftOperand) 
            : base(id, 1, 12, OpCodeType.WordLoad)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var result = mmu.ReadWord(cpu.StackPointer);

            cpu.StackPointer += 2;

            LeftOperand.Set(result);

            return base.Execute(cpu, mmu);
        }
    }
}
