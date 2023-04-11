using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class JumpWord : OpCode<ushort>
    {
        public JumpWord(Operand<ushort> leftOperand) 
            : base(0xC3, 3, 16, OpCodeType.JumpCalls)
        {
            LeftOperand = leftOperand;
        }

        public JumpWord(byte id, byte length, byte cycles, Operand<ushort> leftOperand)
            : base(id, length, cycles, OpCodeType.JumpCalls)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var value = LeftOperand.Get();
            //var result = mmu.ReadWord((ushort)(cpu.ProgramCounter + 1));

            cpu.ProgramCounter = value;

            return new OpCodeResult(Length, Cycles, false);
        }
    }
}
