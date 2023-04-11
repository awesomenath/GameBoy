using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Xor : OpCode<byte>
    {
        public Xor(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand) 
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var result = cpu.A ^ LeftOperand.Get();
            var byteResult = (byte)result;

            cpu.A = byteResult;

            cpu.FlagZ = byteResult == 0;
            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagC = false;

            return base.Execute(cpu, mmu);
        }
    }
}
