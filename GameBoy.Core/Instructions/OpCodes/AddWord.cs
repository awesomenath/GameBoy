using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.Operands;

namespace GameBoy.Core.Instructions.OpCodes
{
    public class AddWord : OpCode<ushort>
    {
        private bool wrappedSignedByte = false;
        private bool clearZFlag = false;

        public AddWord(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<ushort> leftOperand, Operand<ushort> rightOperand, bool clearZFlag = false)
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
            this.clearZFlag = clearZFlag;
        }

        public AddWord(byte id, byte length, byte cycles, OpCodeType opCodeType, Operand<ushort> leftOperand, Operand<sbyte> rightOperand, bool clearZFlag = false)
            : base(id, length, cycles, opCodeType)
        {
            LeftOperand = leftOperand;

            wrappedSignedByte = true;

            this.clearZFlag = clearZFlag;

            RightOperand = new Operand<ushort>("word wrapped byte", () => (ushort)rightOperand.Get(), val => rightOperand.Set((sbyte)val));
        }

        public override OpCodeResult Execute(Cpu cpu, Mmu mmu)
        {
            // Add
            ushort leftVal = LeftOperand.Get();
            ushort rightVal = RightOperand.Get();
            int addResult;
            ushort addResultShort;

            if (wrappedSignedByte)
            {
                var signedRightVal = (sbyte)rightVal;
                addResult = leftVal + signedRightVal;
                addResultShort = (ushort)addResult;

                cpu.FlagZ = false;
                cpu.FlagN = false;

                // https://stackoverflow.com/a/7261149
                if (signedRightVal > 0)
                {
                    cpu.FlagC = ((leftVal & 0xFF) + signedRightVal) > 0xFF;
                    cpu.FlagH = ((leftVal & 0x0F) + (signedRightVal & 0x0F)) > 0x0F;
                }
                else
                {
                    cpu.FlagC = (addResultShort & 0xFF) <= (leftVal & 0xFF);
                    cpu.FlagH = (addResultShort & 0x0F) <= (leftVal & 0x0F);
                }
            }
            else
            {
                addResult = leftVal + rightVal;

                addResultShort = (ushort)addResult;

                var carryOccurred = addResult > ushort.MaxValue;
                cpu.FlagC = carryOccurred;

                var halfCarryOccurred = (leftVal & 0xFFF) + (rightVal & 0xFFF) > 0xFFF;
                cpu.FlagH = halfCarryOccurred;
            }

            if (clearZFlag)
            {
                cpu.FlagZ = false;
            }
            cpu.FlagN = false; // No Substract

            LeftOperand.Set(addResultShort);

            return base.Execute(cpu, mmu);
        }
    }
}
