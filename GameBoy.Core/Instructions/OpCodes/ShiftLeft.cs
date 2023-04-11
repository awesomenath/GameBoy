using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class ShiftLeft : OpCode<byte>
    {
        public ShiftLeft(byte id, byte length, byte cycles, Operand<byte> leftOperand)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Shift left into Carry. LSB set to 0
            byte operandValue = LeftOperand.Get();

            cpu.FlagC = (operandValue & 0x80) != 0;

            LeftOperand.Set((byte)(operandValue << 1));

            cpu.FlagN = false;
            cpu.FlagH = false;
            cpu.FlagZ = LeftOperand.Get() == 0;

            return base.Execute(cpu, mmu);
        }
    }
}
