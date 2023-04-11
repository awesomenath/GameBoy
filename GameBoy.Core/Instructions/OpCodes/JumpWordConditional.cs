using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class JumpWordConditional : OpCode<bool>
    {
        private Operand<ushort> MemoryOperand { get; set; }

        public JumpWordConditional(byte id, Operand<bool> operand, Operand<ushort> memoryOperand)
            : base(id, 3, 12, OpCodeType.JumpCalls)
        {
            LeftOperand = operand;
            MemoryOperand = memoryOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            if (LeftOperand.Get())
            {
                var result = MemoryOperand.Get();
                //var result = mmu.ReadByte((ushort)(cpu.ProgramCounter + 1));

                cpu.ProgramCounter = result;

                return new OpCodeResult(3, 16, false);
            }

            return new OpCodeResult(3, 12);
        }
    }
}
