using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class IncrementByte : OpCode<byte>
    {
        public IncrementByte(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<byte> leftOperand)
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            var value = LeftOperand.Get();

            var newValue = value + 1;
            var newValueByte = (byte)newValue;

            cpu.FlagZ = newValueByte == 0;

            cpu.FlagN = false;

            // Set if carry from bit 3
            //if ((value & 0x08) == 0x08 && (newValueByte & 0x08) == 0 && (newValueByte & 0x10) == 0x10)
            if ((newValueByte & 0x0F) == 0)
            {
                cpu.FlagH = true;
            }
            else
            {
                cpu.FlagH = false;
            }

            // Set
            LeftOperand.Set(newValueByte);

            return base.Execute(cpu, mmu);
        }
    }
}
