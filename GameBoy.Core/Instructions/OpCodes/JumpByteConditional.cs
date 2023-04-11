using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class JumpByteConditional : OpCode<bool>
    {
        private Operand<sbyte> MemoryOperand { get; set; }

        public JumpByteConditional(byte id, Operand<bool> operand, Operand<sbyte> memoryOperand) 
            : base(id, 2, 8, OpCodeType.JumpCalls)
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

                //cpu.ProgramCounter += (ushort)result;
                cpu.ProgramCounter = (ushort)(cpu.ProgramCounter + result);

                return new OpCodeResult(2, 12);
            }

            return new OpCodeResult(2, 8);            
        }
    }
}
