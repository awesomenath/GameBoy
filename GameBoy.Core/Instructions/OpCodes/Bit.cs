using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class Bit : OpCode<byte>
    {
        private byte BitFilter { get; set; }

        public Bit(byte id, byte length, byte cycles, byte bitFilter, Operand<byte> leftOperand)
            : base(id, length, cycles, OpCodeType.ByteModify)
        {
            BitFilter = bitFilter;
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Rotate left. Old bit 7 to Carry flag. Carry flag to bit 0.
            byte operandValue = LeftOperand.Get();

            var bitSet = (operandValue & BitFilter) > 0;

            cpu.FlagN = false;
            cpu.FlagH = true;
            cpu.FlagZ = !bitSet;

            return base.Execute(cpu, mmu);
        }
    }
}
