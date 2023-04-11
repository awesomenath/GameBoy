using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class JumpByte : OpCode<sbyte>
    {
        public JumpByte(Operand<sbyte> operand) 
            : base(0x18, 2, 12, OpCodeType.JumpCalls)
        {
            LeftOperand = operand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var result = LeftOperand.Get();
            //var result = mmu.ReadByte((ushort)(cpu.ProgramCounter + 1));

            //cpu.ProgramCounter += (ushort)result;
            cpu.ProgramCounter = (ushort)(cpu.ProgramCounter + result);

            return new OpCodeResult(Length, Cycles);
        }
    }
}
