using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class And : OpCode<byte>
    {
        public And(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var result = (byte)(cpu.A & LeftOperand.Get());

            cpu.A = result;

            cpu.FlagZ = result == 0;
            cpu.FlagN = false;
            cpu.FlagH = true;
            cpu.FlagC = false;

            return base.Execute(cpu, mmu);
        }
    }
}
