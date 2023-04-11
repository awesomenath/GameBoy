using GameBoy.Core.Hardware;
using GameBoy.Core.Instructions.OpCodes;
using GameBoy.Core.Instructions.Operands;
using System;

namespace GameBoy.Core.Instructions
{
    public class OpCodeManager
    {
        private readonly IOpCode[] OpCodes;
        private readonly IOpCode[] PrefixedOpCodes;

        public Span<IOpCode> OpCodeList => OpCodes.AsSpan();

        public IOpCode PushPcToStackOpCode { get; private set; }

        public OpCodeManager(Cpu cpu, RegisterOperandManager registerOperandManager, MemoryOperandManager memoryOperandManager)
        {
            OpCodes = new IOpCode[byte.MaxValue + 1];
            PrefixedOpCodes = new IOpCode[byte.MaxValue + 1];

            InitialiseOpCodes(cpu, registerOperandManager, memoryOperandManager);

            VerifyOpCodeValues(OpCodes);
            VerifyOpCodeValues(PrefixedOpCodes);
        }

        private void VerifyOpCodeValues(IOpCode[] opCodes)
        {
            for (var i = 0; i < opCodes.Length; i++)
            {
                if (opCodes[i] == null)
                {
                    //throw new Exception($"Missing OpCode at index {i}!");
                    continue;
                }

                // If not invalid op code and ID is mismatched
                if (opCodes[i] is not Invalid && opCodes[i].Id != i)
                {
                    throw new Exception($"OpCode ID {OpCodes[i].Id:X} does not match index {i:X} in array!");
                }
            }
        }

        private void InitialiseOpCodes(Cpu cpu, RegisterOperandManager registerOperandManager, MemoryOperandManager memoryOperandManager)
        {
            PushPcToStackOpCode = new Push(0, registerOperandManager.ProgramCounter);

            // 0x00 - 0x0F
            OpCodes[0x00] = new NoOp();
            OpCodes[0x01] = new LoadWord(0x01, 3, 12, OpCodeType.WordLoad, registerOperandManager.RegisterBC, memoryOperandManager.ImmediateWord);
            OpCodes[0x02] = new LoadByte(0x02, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressBC, registerOperandManager.RegisterA);
            OpCodes[0x03] = new IncrementWord(0x03, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterBC);
            OpCodes[0x04] = new IncrementByte(0x04, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterB);
            OpCodes[0x05] = new DecrementByte(0x05, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterB);
            OpCodes[0x06] = new LoadByte(0x06, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterB, memoryOperandManager.ImmediateByte);
            OpCodes[0x07] = new RotateLeftCarryA();
            OpCodes[0x08] = new LoadWord(0x08, 3, 20, OpCodeType.WordLoad, memoryOperandManager.Address, registerOperandManager.StackPointer); // We are writing the stack pointer value into the specified address
            OpCodes[0x09] = new AddWord(0x09, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterHL, registerOperandManager.RegisterBC);
            OpCodes[0x0A] = new LoadByte(0x0A, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressBC);
            OpCodes[0x0B] = new DecrementWord(0x0B, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterBC);
            OpCodes[0x0C] = new IncrementByte(0x0C, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterC);
            OpCodes[0x0D] = new DecrementByte(0x0D, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterC);
            OpCodes[0x0E] = new LoadByte(0x0E, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterC, memoryOperandManager.ImmediateByte);
            OpCodes[0x0F] = new RotateRightCarryA();

            // 0x10 - 0x1F
            OpCodes[0x10] = new Stop();
            OpCodes[0x11] = new LoadWord(0x11, 3, 12, OpCodeType.WordLoad, registerOperandManager.RegisterDE, memoryOperandManager.ImmediateWord);
            OpCodes[0x12] = new LoadByte(0x12, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressDE, registerOperandManager.RegisterA);
            OpCodes[0x13] = new IncrementWord(0x13, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterDE);
            OpCodes[0x14] = new IncrementByte(0x14, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterD);
            OpCodes[0x15] = new DecrementByte(0x15, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterD);
            OpCodes[0x16] = new LoadByte(0x16, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterD, memoryOperandManager.ImmediateByte);
            OpCodes[0x17] = new RotateLeftA();
            OpCodes[0x18] = new JumpByte(memoryOperandManager.ImmediateSignedByte);
            OpCodes[0x19] = new AddWord(0x19, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterHL, registerOperandManager.RegisterDE);
            OpCodes[0x1A] = new LoadByte(0x1A, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressDE);
            OpCodes[0x1B] = new DecrementWord(0x1B, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterDE);
            OpCodes[0x1C] = new IncrementByte(0x1C, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterE);
            OpCodes[0x1D] = new DecrementByte(0x1D, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterE);
            OpCodes[0x1E] = new LoadByte(0x1E, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterE, memoryOperandManager.ImmediateByte);
            OpCodes[0x1F] = new RotateRightA();

            // 0x20 - 0x2F
            OpCodes[0x20] = new JumpByteConditional(0x20, registerOperandManager.FlagZNot, memoryOperandManager.ImmediateSignedByte);
            OpCodes[0x21] = new LoadWord(0x21, 3, 12, OpCodeType.WordLoad, registerOperandManager.RegisterHL, memoryOperandManager.ImmediateWord);
            OpCodes[0x22] = new LoadByte(0x22, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHLIncrement, registerOperandManager.RegisterA);
            OpCodes[0x23] = new IncrementWord(0x23, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterHL);
            OpCodes[0x24] = new IncrementByte(0x24, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterH);
            OpCodes[0x25] = new DecrementByte(0x25, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterH);
            OpCodes[0x26] = new LoadByte(0x26, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterH, memoryOperandManager.ImmediateByte);
            OpCodes[0x27] = new DecimalAdjust();
            OpCodes[0x28] = new JumpByteConditional(0x28, registerOperandManager.FlagZ, memoryOperandManager.ImmediateSignedByte);
            OpCodes[0x29] = new AddWord(0x29, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterHL, registerOperandManager.RegisterHL);
            OpCodes[0x2A] = new LoadByte(0x2A, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHLIncrement);
            OpCodes[0x2B] = new DecrementWord(0x2B, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterHL);
            OpCodes[0x2C] = new IncrementByte(0x2C, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterL);
            OpCodes[0x2D] = new DecrementByte(0x2D, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterL);
            OpCodes[0x2E] = new LoadByte(0x2E, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterL, memoryOperandManager.ImmediateByte);
            OpCodes[0x2F] = new Complement();

            // 0x30 - 0x3F
            OpCodes[0x30] = new JumpByteConditional(0x30, registerOperandManager.FlagCNot, memoryOperandManager.ImmediateSignedByte);
            OpCodes[0x31] = new LoadWord(0x31, 3, 12, OpCodeType.WordLoad, registerOperandManager.StackPointer, memoryOperandManager.ImmediateWord);
            OpCodes[0x32] = new LoadByte(0x32, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHLDecrement, registerOperandManager.RegisterA);
            OpCodes[0x33] = new IncrementWord(0x33, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.StackPointer);
            OpCodes[0x34] = new IncrementByte(0x34, 1, 12, OpCodeType.ByteMathLogic, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x35] = new DecrementByte(0x35, 1, 12, OpCodeType.ByteMathLogic, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x36] = new LoadByte(0x36, 2, 12, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, memoryOperandManager.ImmediateByte);
            OpCodes[0x37] = new SetCarryFlag();
            OpCodes[0x38] = new JumpByteConditional(0x38, registerOperandManager.FlagC, memoryOperandManager.ImmediateSignedByte);
            OpCodes[0x39] = new AddWord(0x39, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.RegisterHL, registerOperandManager.StackPointer);
            OpCodes[0x3A] = new LoadByte(0x3A, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHLDecrement);
            OpCodes[0x3B] = new DecrementWord(0x3B, 1, 8, OpCodeType.WordMathLogic, registerOperandManager.StackPointer);
            OpCodes[0x3C] = new IncrementByte(0x3C, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA);
            OpCodes[0x3D] = new DecrementByte(0x3D, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA);
            OpCodes[0x3E] = new LoadByte(0x3E, 2, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.ImmediateByte);
            OpCodes[0x3F] = new ComplementCarryFlag();

            // 0x40 - 0x4F
            OpCodes[0x40] = new LoadByte(0x40, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterB);
            OpCodes[0x41] = new LoadByte(0x41, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterC);
            OpCodes[0x42] = new LoadByte(0x42, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterD);
            OpCodes[0x43] = new LoadByte(0x43, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterE);
            OpCodes[0x44] = new LoadByte(0x44, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterH);
            OpCodes[0x45] = new LoadByte(0x45, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterL);
            OpCodes[0x46] = new LoadByte(0x46, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterB, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x47] = new LoadByte(0x47, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterB, registerOperandManager.RegisterA);
            OpCodes[0x48] = new LoadByte(0x48, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterB);
            OpCodes[0x49] = new LoadByte(0x49, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterC);
            OpCodes[0x4A] = new LoadByte(0x4A, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterD);
            OpCodes[0x4B] = new LoadByte(0x4B, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterE);
            OpCodes[0x4C] = new LoadByte(0x4C, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterH);
            OpCodes[0x4D] = new LoadByte(0x4D, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterL);
            OpCodes[0x4E] = new LoadByte(0x4E, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterC, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x4F] = new LoadByte(0x4F, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterC, registerOperandManager.RegisterA);

            // 0x50 - 0x5F
            OpCodes[0x50] = new LoadByte(0x50, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterB);
            OpCodes[0x51] = new LoadByte(0x51, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterC);
            OpCodes[0x52] = new LoadByte(0x52, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterD);
            OpCodes[0x53] = new LoadByte(0x53, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterE);
            OpCodes[0x54] = new LoadByte(0x54, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterH);
            OpCodes[0x55] = new LoadByte(0x55, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterL);
            OpCodes[0x56] = new LoadByte(0x56, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterD, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x57] = new LoadByte(0x57, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterD, registerOperandManager.RegisterA);
            OpCodes[0x58] = new LoadByte(0x58, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterB);
            OpCodes[0x59] = new LoadByte(0x59, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterC);
            OpCodes[0x5A] = new LoadByte(0x5A, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterD);
            OpCodes[0x5B] = new LoadByte(0x5B, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterE);
            OpCodes[0x5C] = new LoadByte(0x5C, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterH);
            OpCodes[0x5D] = new LoadByte(0x5D, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterL);
            OpCodes[0x5E] = new LoadByte(0x5E, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterE, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x5F] = new LoadByte(0x5F, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterE, registerOperandManager.RegisterA);

            // 0x60 - 0x6F
            OpCodes[0x60] = new LoadByte(0x60, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterB);
            OpCodes[0x61] = new LoadByte(0x61, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterC);
            OpCodes[0x62] = new LoadByte(0x62, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterD);
            OpCodes[0x63] = new LoadByte(0x63, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterE);
            OpCodes[0x64] = new LoadByte(0x64, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterH);
            OpCodes[0x65] = new LoadByte(0x65, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterL);
            OpCodes[0x66] = new LoadByte(0x66, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterH, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x67] = new LoadByte(0x67, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterH, registerOperandManager.RegisterA);
            OpCodes[0x68] = new LoadByte(0x68, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterB);
            OpCodes[0x69] = new LoadByte(0x69, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterC);
            OpCodes[0x6A] = new LoadByte(0x6A, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterD);
            OpCodes[0x6B] = new LoadByte(0x6B, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterE);
            OpCodes[0x6C] = new LoadByte(0x6C, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterH);
            OpCodes[0x6D] = new LoadByte(0x6D, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterL);
            OpCodes[0x6E] = new LoadByte(0x6E, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterL, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x6F] = new LoadByte(0x6F, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterL, registerOperandManager.RegisterA);

            // 0x70 - 0x7F
            OpCodes[0x70] = new LoadByte(0x70, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterB);
            OpCodes[0x71] = new LoadByte(0x71, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterC);
            OpCodes[0x72] = new LoadByte(0x72, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterD);
            OpCodes[0x73] = new LoadByte(0x73, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterE);
            OpCodes[0x74] = new LoadByte(0x74, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterH);
            OpCodes[0x75] = new LoadByte(0x75, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterL);
            OpCodes[0x76] = new Halt();
            OpCodes[0x77] = new LoadByte(0x77, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressHL, registerOperandManager.RegisterA);
            OpCodes[0x78] = new LoadByte(0x78, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterB);
            OpCodes[0x79] = new LoadByte(0x79, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterC);
            OpCodes[0x7A] = new LoadByte(0x7A, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterD);
            OpCodes[0x7B] = new LoadByte(0x7B, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterE);
            OpCodes[0x7C] = new LoadByte(0x7C, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterH);
            OpCodes[0x7D] = new LoadByte(0x7D, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterL);
            OpCodes[0x7E] = new LoadByte(0x7E, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x7F] = new LoadByte(0x7F, 1, 4, OpCodeType.ByteLoad, registerOperandManager.RegisterA, registerOperandManager.RegisterA);

            // 0x80 - 0x8F
            OpCodes[0x80] = new AddByte(0x80, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterB);
            OpCodes[0x81] = new AddByte(0x81, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterC);
            OpCodes[0x82] = new AddByte(0x82, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterD);
            OpCodes[0x83] = new AddByte(0x83, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterE);
            OpCodes[0x84] = new AddByte(0x84, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterH);
            OpCodes[0x85] = new AddByte(0x85, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterL);
            OpCodes[0x86] = new AddByte(0x86, 1, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x87] = new AddByte(0x87, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterA);
            OpCodes[0x88] = new AddCarryByte(0x88, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterB);
            OpCodes[0x89] = new AddCarryByte(0x89, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterC);
            OpCodes[0x8A] = new AddCarryByte(0x8A, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterD);
            OpCodes[0x8B] = new AddCarryByte(0x8B, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterE);
            OpCodes[0x8C] = new AddCarryByte(0x8C, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterH);
            OpCodes[0x8D] = new AddCarryByte(0x8D, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterL);
            OpCodes[0x8E] = new AddCarryByte(0x8E, 1, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x8F] = new AddCarryByte(0x8F, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterA);

            // 0x90 - 0x9F
            OpCodes[0x90] = new SubtractByte(0x90, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterB);
            OpCodes[0x91] = new SubtractByte(0x91, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterC);
            OpCodes[0x92] = new SubtractByte(0x92, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterD);
            OpCodes[0x93] = new SubtractByte(0x93, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterE);
            OpCodes[0x94] = new SubtractByte(0x94, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterH);
            OpCodes[0x95] = new SubtractByte(0x95, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterL);
            OpCodes[0x96] = new SubtractByte(0x96, 1, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x97] = new SubtractByte(0x97, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterA);
            OpCodes[0x98] = new SubtractCarryByte(0x98, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterB);
            OpCodes[0x99] = new SubtractCarryByte(0x99, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterC);
            OpCodes[0x9A] = new SubtractCarryByte(0x9A, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterD);
            OpCodes[0x9B] = new SubtractCarryByte(0x9B, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterE);
            OpCodes[0x9C] = new SubtractCarryByte(0x9C, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterH);
            OpCodes[0x9D] = new SubtractCarryByte(0x9D, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterL);
            OpCodes[0x9E] = new SubtractCarryByte(0x9E, 1, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0x9F] = new SubtractCarryByte(0x9F, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, registerOperandManager.RegisterA);

            // 0xA0 - 0xAF
            OpCodes[0xA0] = new And(0xA0, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterB);
            OpCodes[0xA1] = new And(0xA1, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterC);
            OpCodes[0xA2] = new And(0xA2, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterD);
            OpCodes[0xA3] = new And(0xA3, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterE);
            OpCodes[0xA4] = new And(0xA4, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterH);
            OpCodes[0xA5] = new And(0xA5, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterL);
            OpCodes[0xA6] = new And(0xA6, 1, 8, OpCodeType.ByteMathLogic, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0xA7] = new And(0xA7, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA);
            OpCodes[0xA8] = new Xor(0xA8, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterB);
            OpCodes[0xA9] = new Xor(0xA9, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterC);
            OpCodes[0xAA] = new Xor(0xAA, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterD);
            OpCodes[0xAB] = new Xor(0xAB, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterE);
            OpCodes[0xAC] = new Xor(0xAC, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterH);
            OpCodes[0xAD] = new Xor(0xAD, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterL);
            OpCodes[0xAE] = new Xor(0xAE, 1, 8, OpCodeType.ByteMathLogic, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0xAF] = new Xor(0xAF, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA);

            // 0xB0 - 0xBF
            OpCodes[0xB0] = new Or(0xB0, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterB);
            OpCodes[0xB1] = new Or(0xB1, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterC);
            OpCodes[0xB2] = new Or(0xB2, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterD);
            OpCodes[0xB3] = new Or(0xB3, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterE);
            OpCodes[0xB4] = new Or(0xB4, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterH);
            OpCodes[0xB5] = new Or(0xB5, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterL);
            OpCodes[0xB6] = new Or(0xB6, 1, 8, OpCodeType.ByteMathLogic, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0xB7] = new Or(0xB7, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA);
            OpCodes[0xB8] = new CompareByte(0xB8, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterB);
            OpCodes[0xB9] = new CompareByte(0xB9, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterC);
            OpCodes[0xBA] = new CompareByte(0xBA, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterD);
            OpCodes[0xBB] = new CompareByte(0xBB, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterE);
            OpCodes[0xBC] = new CompareByte(0xBC, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterH);
            OpCodes[0xBD] = new CompareByte(0xBD, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterL);
            OpCodes[0xBE] = new CompareByte(0xBE, 1, 8, OpCodeType.ByteMathLogic, memoryOperandManager.LoadByteFromAddressHL);
            OpCodes[0xBF] = new CompareByte(0xBF, 1, 4, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA);

            // 0xC0 - 0xCF
            OpCodes[0xC0] = new ReturnConditional(0xC0, registerOperandManager.FlagZNot);
            OpCodes[0xC1] = new Pop(0xC1, registerOperandManager.RegisterBC);
            OpCodes[0xC2] = new JumpWordConditional(0xC2, registerOperandManager.FlagZNot, memoryOperandManager.Address);
            OpCodes[0xC3] = new JumpWord(memoryOperandManager.Address);
            OpCodes[0xC4] = new CallConditional(0xC4, registerOperandManager.FlagZNot, memoryOperandManager.Address);
            OpCodes[0xC5] = new Push(0xC5, registerOperandManager.RegisterBC);
            OpCodes[0xC6] = new AddByte(0xC6, 2, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.ImmediateByte);
            OpCodes[0xC7] = new Restart(0xC7, 0x00);
            OpCodes[0xC8] = new ReturnConditional(0xC8, registerOperandManager.FlagZ);
            OpCodes[0xC9] = new Return();
            OpCodes[0xCA] = new JumpWordConditional(0xCA, registerOperandManager.FlagZ, memoryOperandManager.Address);
            // 0xCB is prefix identifier
            OpCodes[0xCC] = new CallConditional(0xCC, registerOperandManager.FlagZ, memoryOperandManager.Address);
            OpCodes[0xCD] = new Call(memoryOperandManager.Address);
            OpCodes[0xCE] = new AddCarryByte(0xCE, 2, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.ImmediateByte);
            OpCodes[0xCF] = new Restart(0xCF, 0x08);

            // 0xD0 - 0xDF
            OpCodes[0xD0] = new ReturnConditional(0xD0, registerOperandManager.FlagCNot);
            OpCodes[0xD1] = new Pop(0xD1, registerOperandManager.RegisterDE);
            OpCodes[0xD2] = new JumpWordConditional(0xD2, registerOperandManager.FlagCNot, memoryOperandManager.Address);
            OpCodes[0xD3] = new Invalid(0xD3);
            OpCodes[0xD4] = new CallConditional(0xD4, registerOperandManager.FlagCNot, memoryOperandManager.Address);
            OpCodes[0xD5] = new Push(0xD5, registerOperandManager.RegisterDE);
            OpCodes[0xD6] = new SubtractByte(0xD6, 2, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.ImmediateByte);
            OpCodes[0xD7] = new Restart(0xD7, 0x10);
            OpCodes[0xD8] = new ReturnConditional(0xD8, registerOperandManager.FlagC);
            OpCodes[0xD9] = new ReturnInterrupt();
            OpCodes[0xDA] = new JumpWordConditional(0xDA, registerOperandManager.FlagC, memoryOperandManager.Address);
            OpCodes[0xDB] = new Invalid(0xDB);
            OpCodes[0xDC] = new CallConditional(0xDC, registerOperandManager.FlagC, memoryOperandManager.Address);
            OpCodes[0xDD] = new Invalid(0xDD);
            OpCodes[0xDE] = new SubtractCarryByte(0xDE, 2, 8, OpCodeType.ByteMathLogic, registerOperandManager.RegisterA, memoryOperandManager.ImmediateByte);
            OpCodes[0xDF] = new Restart(0xDF, 0x18);

            // 0xE0 - 0xEF
            OpCodes[0xE0] = new LoadByte(0xE0, 2, 12, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromImmediateByteAddress, registerOperandManager.RegisterA);
            OpCodes[0xE1] = new Pop(0xE1, registerOperandManager.RegisterHL);
            OpCodes[0xE2] = new LoadByte(0xE2, 1, 8, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromAddressC, registerOperandManager.RegisterA);
            OpCodes[0xE3] = new Invalid(0xE3);
            OpCodes[0xE4] = new Invalid(0xE4);
            OpCodes[0xE5] = new Push(0xE5, registerOperandManager.RegisterHL);
            OpCodes[0xE6] = new And(0xE6, 2, 8, OpCodeType.ByteMathLogic, memoryOperandManager.ImmediateByte);
            OpCodes[0xE7] = new Restart(0xE7, 0x20);
            OpCodes[0xE8] = new AddWord(0xE8, 2, 16, OpCodeType.WordMathLogic, registerOperandManager.StackPointer, memoryOperandManager.ImmediateSignedByte, true);
            OpCodes[0xE9] = new JumpWord(0xE9, 1, 4, registerOperandManager.RegisterHL); // JP HL
            OpCodes[0xEA] = new LoadByte(0xEA, 3, 16, OpCodeType.ByteLoad, memoryOperandManager.LoadByteFromImmediateAddress, registerOperandManager.RegisterA);
            OpCodes[0xEB] = new Invalid(0xEB);
            OpCodes[0xEC] = new Invalid(0xEC);
            OpCodes[0xED] = new Invalid(0xED);
            OpCodes[0xEE] = new Xor(0xEE, 2, 8, OpCodeType.ByteMathLogic, memoryOperandManager.ImmediateByte);
            OpCodes[0xEF] = new Restart(0xEF, 0x28);

            // 0xF0 - 0xFF
            OpCodes[0xF0] = new LoadByte(0xF0, 2, 12, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromImmediateByteAddress);
            OpCodes[0xF1] = new Pop(0xF1, registerOperandManager.RegisterAF);
            OpCodes[0xF2] = new LoadByte(0xF2, 1, 8, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromAddressC);
            OpCodes[0xF3] = new DisableInterrupt();
            OpCodes[0xF4] = new Invalid(0xF4);
            OpCodes[0xF5] = new Push(0xF5, registerOperandManager.RegisterAF);
            OpCodes[0xF6] = new Or(0xF6, 2, 8, OpCodeType.ByteMathLogic, memoryOperandManager.ImmediateByte);
            OpCodes[0xF7] = new Restart(0xF7, 0x20);
            OpCodes[0xF8] = new LoadWord(0xF8, 2, 16, OpCodeType.WordLoad, registerOperandManager.RegisterHL, memoryOperandManager.LoadWordFromStackPointerImediateByte);
            OpCodes[0xF9] = new LoadWord(0xF9, 1, 8, OpCodeType.WordLoad, registerOperandManager.StackPointer, registerOperandManager.RegisterHL);
            OpCodes[0xFA] = new LoadByte(0xFA, 3, 16, OpCodeType.ByteLoad, registerOperandManager.RegisterA, memoryOperandManager.LoadByteFromImmediateAddress);
            OpCodes[0xFB] = new EnableInterrupt();
            OpCodes[0xFC] = new Invalid(0xFC);
            OpCodes[0xFD] = new Invalid(0xFD);
            OpCodes[0xFE] = new CompareByte(0xFE, 2, 8, OpCodeType.ByteMathLogic, memoryOperandManager.ImmediateByte);
            OpCodes[0xFF] = new Restart(0xFF, 0x38);


            // Prefixes

            // 0x00 - 0x0F
            PrefixedOpCodes[0x00] = new RotateLeftCarry(0x00, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x01] = new RotateLeftCarry(0x01, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x02] = new RotateLeftCarry(0x02, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x03] = new RotateLeftCarry(0x03, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x04] = new RotateLeftCarry(0x04, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x05] = new RotateLeftCarry(0x05, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x06] = new RotateLeftCarry(0x06, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x07] = new RotateLeftCarry(0x07, 2, 8, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x08] = new RotateRightCarry(0x08, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x09] = new RotateRightCarry(0x09, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x0A] = new RotateRightCarry(0x0A, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x0B] = new RotateRightCarry(0x0B, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x0C] = new RotateRightCarry(0x0C, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x0D] = new RotateRightCarry(0x0D, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x0E] = new RotateRightCarry(0x0E, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x0F] = new RotateRightCarry(0x0F, 2, 8, registerOperandManager.RegisterA);

            // 0x10 - 0x1F
            PrefixedOpCodes[0x10] = new RotateLeft(0x10, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x11] = new RotateLeft(0x11, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x12] = new RotateLeft(0x12, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x13] = new RotateLeft(0x13, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x14] = new RotateLeft(0x14, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x15] = new RotateLeft(0x15, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x16] = new RotateLeft(0x16, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x17] = new RotateLeft(0x17, 2, 8, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x18] = new RotateRight(0x18, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x19] = new RotateRight(0x19, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x1A] = new RotateRight(0x1A, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x1B] = new RotateRight(0x1B, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x1C] = new RotateRight(0x1C, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x1D] = new RotateRight(0x1D, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x1E] = new RotateRight(0x1E, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x1F] = new RotateRight(0x1F, 2, 8, registerOperandManager.RegisterA);

            // 0x20 - 0x2F
            PrefixedOpCodes[0x20] = new ShiftLeft(0x20, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x21] = new ShiftLeft(0x21, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x22] = new ShiftLeft(0x22, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x23] = new ShiftLeft(0x23, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x24] = new ShiftLeft(0x24, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x25] = new ShiftLeft(0x25, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x26] = new ShiftLeft(0x26, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x27] = new ShiftLeft(0x27, 2, 8, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x28] = new ShiftRightKeep(0x28, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x29] = new ShiftRightKeep(0x29, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x2A] = new ShiftRightKeep(0x2A, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x2B] = new ShiftRightKeep(0x2B, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x2C] = new ShiftRightKeep(0x2C, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x2D] = new ShiftRightKeep(0x2D, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x2E] = new ShiftRightKeep(0x2E, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x2F] = new ShiftRightKeep(0x2F, 2, 8, registerOperandManager.RegisterA);

            // 0x30 - 0x3F
            PrefixedOpCodes[0x30] = new Swap(0x30, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x31] = new Swap(0x31, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x32] = new Swap(0x32, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x33] = new Swap(0x33, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x34] = new Swap(0x34, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x35] = new Swap(0x35, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x36] = new Swap(0x36, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x37] = new Swap(0x37, 2, 8, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x38] = new ShiftRight(0x38, 2, 8, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x39] = new ShiftRight(0x39, 2, 8, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x3A] = new ShiftRight(0x3A, 2, 8, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x3B] = new ShiftRight(0x3B, 2, 8, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x3C] = new ShiftRight(0x3C, 2, 8, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x3D] = new ShiftRight(0x3D, 2, 8, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x3E] = new ShiftRight(0x3E, 2, 16, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x3F] = new ShiftRight(0x3F, 2, 8, registerOperandManager.RegisterA);

            // 0x40 - 0x4F
            PrefixedOpCodes[0x40] = new Bit(0x40, 2, 8, 0x01, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x41] = new Bit(0x41, 2, 8, 0x01, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x42] = new Bit(0x42, 2, 8, 0x01, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x43] = new Bit(0x43, 2, 8, 0x01, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x44] = new Bit(0x44, 2, 8, 0x01, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x45] = new Bit(0x45, 2, 8, 0x01, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x46] = new Bit(0x46, 2, 12, 0x01, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x47] = new Bit(0x47, 2, 8, 0x01, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x48] = new Bit(0x48, 2, 8, 0x02, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x49] = new Bit(0x49, 2, 8, 0x02, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x4A] = new Bit(0x4A, 2, 8, 0x02, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x4B] = new Bit(0x4B, 2, 8, 0x02, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x4C] = new Bit(0x4C, 2, 8, 0x02, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x4D] = new Bit(0x4D, 2, 8, 0x02, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x4E] = new Bit(0x4E, 2, 12, 0x02, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x4F] = new Bit(0x4F, 2, 8, 0x02, registerOperandManager.RegisterA);

            // 0x50 - 0x5F
            PrefixedOpCodes[0x50] = new Bit(0x50, 2, 8, 0x04, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x51] = new Bit(0x51, 2, 8, 0x04, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x52] = new Bit(0x52, 2, 8, 0x04, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x53] = new Bit(0x53, 2, 8, 0x04, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x54] = new Bit(0x54, 2, 8, 0x04, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x55] = new Bit(0x55, 2, 8, 0x04, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x56] = new Bit(0x56, 2, 12, 0x04, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x57] = new Bit(0x57, 2, 8, 0x04, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x58] = new Bit(0x58, 2, 8, 0x08, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x59] = new Bit(0x59, 2, 8, 0x08, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x5A] = new Bit(0x5A, 2, 8, 0x08, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x5B] = new Bit(0x5B, 2, 8, 0x08, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x5C] = new Bit(0x5C, 2, 8, 0x08, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x5D] = new Bit(0x5D, 2, 8, 0x08, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x5E] = new Bit(0x5E, 2, 12, 0x08, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x5F] = new Bit(0x5F, 2, 8, 0x08, registerOperandManager.RegisterA);

            // 0x60 - 0x6F
            PrefixedOpCodes[0x60] = new Bit(0x60, 2, 8, 0x10, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x61] = new Bit(0x61, 2, 8, 0x10, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x62] = new Bit(0x62, 2, 8, 0x10, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x63] = new Bit(0x63, 2, 8, 0x10, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x64] = new Bit(0x64, 2, 8, 0x10, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x65] = new Bit(0x65, 2, 8, 0x10, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x66] = new Bit(0x66, 2, 12, 0x10, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x67] = new Bit(0x67, 2, 8, 0x10, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x68] = new Bit(0x68, 2, 8, 0x20, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x69] = new Bit(0x69, 2, 8, 0x20, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x6A] = new Bit(0x6A, 2, 8, 0x20, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x6B] = new Bit(0x6B, 2, 8, 0x20, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x6C] = new Bit(0x6C, 2, 8, 0x20, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x6D] = new Bit(0x6D, 2, 8, 0x20, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x6E] = new Bit(0x6E, 2, 12, 0x20, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x6F] = new Bit(0x6F, 2, 8, 0x20, registerOperandManager.RegisterA);

            // 0x70 - 0x7F
            PrefixedOpCodes[0x70] = new Bit(0x70, 2, 8, 0x40, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x71] = new Bit(0x71, 2, 8, 0x40, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x72] = new Bit(0x72, 2, 8, 0x40, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x73] = new Bit(0x73, 2, 8, 0x40, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x74] = new Bit(0x74, 2, 8, 0x40, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x75] = new Bit(0x75, 2, 8, 0x40, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x76] = new Bit(0x76, 2, 12, 0x40, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x77] = new Bit(0x77, 2, 8, 0x40, registerOperandManager.RegisterA);
            PrefixedOpCodes[0x78] = new Bit(0x78, 2, 8, 0x80, registerOperandManager.RegisterB);
            PrefixedOpCodes[0x79] = new Bit(0x79, 2, 8, 0x80, registerOperandManager.RegisterC);
            PrefixedOpCodes[0x7A] = new Bit(0x7A, 2, 8, 0x80, registerOperandManager.RegisterD);
            PrefixedOpCodes[0x7B] = new Bit(0x7B, 2, 8, 0x80, registerOperandManager.RegisterE);
            PrefixedOpCodes[0x7C] = new Bit(0x7C, 2, 8, 0x80, registerOperandManager.RegisterH);
            PrefixedOpCodes[0x7D] = new Bit(0x7D, 2, 8, 0x80, registerOperandManager.RegisterL);
            PrefixedOpCodes[0x7E] = new Bit(0x7E, 2, 12, 0x80, memoryOperandManager.LoadByteFromAddressHL);
            PrefixedOpCodes[0x7F] = new Bit(0x7F, 2, 8, 0x80, registerOperandManager.RegisterA);

            // 0x80 - 0x8F
            PrefixedOpCodes[0x80] = new ResetBit(0x80, 2, 8, registerOperandManager.RegisterB, 0);
            PrefixedOpCodes[0x81] = new ResetBit(0x81, 2, 8, registerOperandManager.RegisterC, 0);
            PrefixedOpCodes[0x82] = new ResetBit(0x82, 2, 8, registerOperandManager.RegisterD, 0);
            PrefixedOpCodes[0x83] = new ResetBit(0x83, 2, 8, registerOperandManager.RegisterE, 0);
            PrefixedOpCodes[0x84] = new ResetBit(0x84, 2, 8, registerOperandManager.RegisterH, 0);
            PrefixedOpCodes[0x85] = new ResetBit(0x85, 2, 8, registerOperandManager.RegisterL, 0);
            PrefixedOpCodes[0x86] = new ResetBit(0x86, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 0);
            PrefixedOpCodes[0x87] = new ResetBit(0x87, 2, 8, registerOperandManager.RegisterA, 0);
            PrefixedOpCodes[0x88] = new ResetBit(0x88, 2, 8, registerOperandManager.RegisterB, 1);
            PrefixedOpCodes[0x89] = new ResetBit(0x89, 2, 8, registerOperandManager.RegisterC, 1);
            PrefixedOpCodes[0x8A] = new ResetBit(0x8A, 2, 8, registerOperandManager.RegisterD, 1);
            PrefixedOpCodes[0x8B] = new ResetBit(0x8B, 2, 8, registerOperandManager.RegisterE, 1);
            PrefixedOpCodes[0x8C] = new ResetBit(0x8C, 2, 8, registerOperandManager.RegisterH, 1);
            PrefixedOpCodes[0x8D] = new ResetBit(0x8D, 2, 8, registerOperandManager.RegisterL, 1);
            PrefixedOpCodes[0x8E] = new ResetBit(0x8E, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 1);
            PrefixedOpCodes[0x8F] = new ResetBit(0x8F, 2, 8, registerOperandManager.RegisterA, 1);

            // 0x90 - 0x9F
            PrefixedOpCodes[0x90] = new ResetBit(0x90, 2, 8, registerOperandManager.RegisterB, 2);
            PrefixedOpCodes[0x91] = new ResetBit(0x91, 2, 8, registerOperandManager.RegisterC, 2);
            PrefixedOpCodes[0x92] = new ResetBit(0x92, 2, 8, registerOperandManager.RegisterD, 2);
            PrefixedOpCodes[0x93] = new ResetBit(0x93, 2, 8, registerOperandManager.RegisterE, 2);
            PrefixedOpCodes[0x94] = new ResetBit(0x94, 2, 8, registerOperandManager.RegisterH, 2);
            PrefixedOpCodes[0x95] = new ResetBit(0x95, 2, 8, registerOperandManager.RegisterL, 2);
            PrefixedOpCodes[0x96] = new ResetBit(0x96, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 2);
            PrefixedOpCodes[0x97] = new ResetBit(0x97, 2, 8, registerOperandManager.RegisterA, 2);
            PrefixedOpCodes[0x98] = new ResetBit(0x98, 2, 8, registerOperandManager.RegisterB, 3);
            PrefixedOpCodes[0x99] = new ResetBit(0x99, 2, 8, registerOperandManager.RegisterC, 3);
            PrefixedOpCodes[0x9A] = new ResetBit(0x9A, 2, 8, registerOperandManager.RegisterD, 3);
            PrefixedOpCodes[0x9B] = new ResetBit(0x9B, 2, 8, registerOperandManager.RegisterE, 3);
            PrefixedOpCodes[0x9C] = new ResetBit(0x9C, 2, 8, registerOperandManager.RegisterH, 3);
            PrefixedOpCodes[0x9D] = new ResetBit(0x9D, 2, 8, registerOperandManager.RegisterL, 3);
            PrefixedOpCodes[0x9E] = new ResetBit(0x9E, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 3);
            PrefixedOpCodes[0x9F] = new ResetBit(0x9F, 2, 8, registerOperandManager.RegisterA, 3);

            // 0xA0 - 0xAF
            PrefixedOpCodes[0xA0] = new ResetBit(0xA0, 2, 8, registerOperandManager.RegisterB, 4);
            PrefixedOpCodes[0xA1] = new ResetBit(0xA1, 2, 8, registerOperandManager.RegisterC, 4);
            PrefixedOpCodes[0xA2] = new ResetBit(0xA2, 2, 8, registerOperandManager.RegisterD, 4);
            PrefixedOpCodes[0xA3] = new ResetBit(0xA3, 2, 8, registerOperandManager.RegisterE, 4);
            PrefixedOpCodes[0xA4] = new ResetBit(0xA4, 2, 8, registerOperandManager.RegisterH, 4);
            PrefixedOpCodes[0xA5] = new ResetBit(0xA5, 2, 8, registerOperandManager.RegisterL, 4);
            PrefixedOpCodes[0xA6] = new ResetBit(0xA6, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 4);
            PrefixedOpCodes[0xA7] = new ResetBit(0xA7, 2, 8, registerOperandManager.RegisterA, 4);
            PrefixedOpCodes[0xA8] = new ResetBit(0xA8, 2, 8, registerOperandManager.RegisterB, 5);
            PrefixedOpCodes[0xA9] = new ResetBit(0xA9, 2, 8, registerOperandManager.RegisterC, 5);
            PrefixedOpCodes[0xAA] = new ResetBit(0xAA, 2, 8, registerOperandManager.RegisterD, 5);
            PrefixedOpCodes[0xAB] = new ResetBit(0xAB, 2, 8, registerOperandManager.RegisterE, 5);
            PrefixedOpCodes[0xAC] = new ResetBit(0xAC, 2, 8, registerOperandManager.RegisterH, 5);
            PrefixedOpCodes[0xAD] = new ResetBit(0xAD, 2, 8, registerOperandManager.RegisterL, 5);
            PrefixedOpCodes[0xAE] = new ResetBit(0xAE, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 5);
            PrefixedOpCodes[0xAF] = new ResetBit(0xAF, 2, 8, registerOperandManager.RegisterA, 5);

            // 0xB0 - 0xBF
            PrefixedOpCodes[0xB0] = new ResetBit(0xB0, 2, 8, registerOperandManager.RegisterB, 6);
            PrefixedOpCodes[0xB1] = new ResetBit(0xB1, 2, 8, registerOperandManager.RegisterC, 6);
            PrefixedOpCodes[0xB2] = new ResetBit(0xB2, 2, 8, registerOperandManager.RegisterD, 6);
            PrefixedOpCodes[0xB3] = new ResetBit(0xB3, 2, 8, registerOperandManager.RegisterE, 6);
            PrefixedOpCodes[0xB4] = new ResetBit(0xB4, 2, 8, registerOperandManager.RegisterH, 6);
            PrefixedOpCodes[0xB5] = new ResetBit(0xB5, 2, 8, registerOperandManager.RegisterL, 6);
            PrefixedOpCodes[0xB6] = new ResetBit(0xB6, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 6);
            PrefixedOpCodes[0xB7] = new ResetBit(0xB7, 2, 8, registerOperandManager.RegisterA, 6);
            PrefixedOpCodes[0xB8] = new ResetBit(0xB8, 2, 8, registerOperandManager.RegisterB, 7);
            PrefixedOpCodes[0xB9] = new ResetBit(0xB9, 2, 8, registerOperandManager.RegisterC, 7);
            PrefixedOpCodes[0xBA] = new ResetBit(0xBA, 2, 8, registerOperandManager.RegisterD, 7);
            PrefixedOpCodes[0xBB] = new ResetBit(0xBB, 2, 8, registerOperandManager.RegisterE, 7);
            PrefixedOpCodes[0xBC] = new ResetBit(0xBC, 2, 8, registerOperandManager.RegisterH, 7);
            PrefixedOpCodes[0xBD] = new ResetBit(0xBD, 2, 8, registerOperandManager.RegisterL, 7);
            PrefixedOpCodes[0xBE] = new ResetBit(0xBE, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 7);
            PrefixedOpCodes[0xBF] = new ResetBit(0xBF, 2, 8, registerOperandManager.RegisterA, 7);

            // 0xC0 - 0xCF
            PrefixedOpCodes[0xC0] = new SetBit(0xC0, 2, 8, registerOperandManager.RegisterB, 0);
            PrefixedOpCodes[0xC1] = new SetBit(0xC1, 2, 8, registerOperandManager.RegisterC, 0);
            PrefixedOpCodes[0xC2] = new SetBit(0xC2, 2, 8, registerOperandManager.RegisterD, 0);
            PrefixedOpCodes[0xC3] = new SetBit(0xC3, 2, 8, registerOperandManager.RegisterE, 0);
            PrefixedOpCodes[0xC4] = new SetBit(0xC4, 2, 8, registerOperandManager.RegisterH, 0);
            PrefixedOpCodes[0xC5] = new SetBit(0xC5, 2, 8, registerOperandManager.RegisterL, 0);
            PrefixedOpCodes[0xC6] = new SetBit(0xC6, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 0);
            PrefixedOpCodes[0xC7] = new SetBit(0xC7, 2, 8, registerOperandManager.RegisterA, 0);
            PrefixedOpCodes[0xC8] = new SetBit(0xC8, 2, 8, registerOperandManager.RegisterB, 1);
            PrefixedOpCodes[0xC9] = new SetBit(0xC9, 2, 8, registerOperandManager.RegisterC, 1);
            PrefixedOpCodes[0xCA] = new SetBit(0xCA, 2, 8, registerOperandManager.RegisterD, 1);
            PrefixedOpCodes[0xCB] = new SetBit(0xCB, 2, 8, registerOperandManager.RegisterE, 1);
            PrefixedOpCodes[0xCC] = new SetBit(0xCC, 2, 8, registerOperandManager.RegisterH, 1);
            PrefixedOpCodes[0xCD] = new SetBit(0xCD, 2, 8, registerOperandManager.RegisterL, 1);
            PrefixedOpCodes[0xCE] = new SetBit(0xCE, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 1);
            PrefixedOpCodes[0xCF] = new SetBit(0xCF, 2, 8, registerOperandManager.RegisterA, 1);

            // 0xD0 - 0xDF
            PrefixedOpCodes[0xD0] = new SetBit(0xD0, 2, 8, registerOperandManager.RegisterB, 2);
            PrefixedOpCodes[0xD1] = new SetBit(0xD1, 2, 8, registerOperandManager.RegisterC, 2);
            PrefixedOpCodes[0xD2] = new SetBit(0xD2, 2, 8, registerOperandManager.RegisterD, 2);
            PrefixedOpCodes[0xD3] = new SetBit(0xD3, 2, 8, registerOperandManager.RegisterE, 2);
            PrefixedOpCodes[0xD4] = new SetBit(0xD4, 2, 8, registerOperandManager.RegisterH, 2);
            PrefixedOpCodes[0xD5] = new SetBit(0xD5, 2, 8, registerOperandManager.RegisterL, 2);
            PrefixedOpCodes[0xD6] = new SetBit(0xD6, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 2);
            PrefixedOpCodes[0xD7] = new SetBit(0xD7, 2, 8, registerOperandManager.RegisterA, 2);
            PrefixedOpCodes[0xD8] = new SetBit(0xD8, 2, 8, registerOperandManager.RegisterB, 3);
            PrefixedOpCodes[0xD9] = new SetBit(0xD9, 2, 8, registerOperandManager.RegisterC, 3);
            PrefixedOpCodes[0xDA] = new SetBit(0xDA, 2, 8, registerOperandManager.RegisterD, 3);
            PrefixedOpCodes[0xDB] = new SetBit(0xDB, 2, 8, registerOperandManager.RegisterE, 3);
            PrefixedOpCodes[0xDC] = new SetBit(0xDC, 2, 8, registerOperandManager.RegisterH, 3);
            PrefixedOpCodes[0xDD] = new SetBit(0xDD, 2, 8, registerOperandManager.RegisterL, 3);
            PrefixedOpCodes[0xDE] = new SetBit(0xDE, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 3);
            PrefixedOpCodes[0xDF] = new SetBit(0xDF, 2, 8, registerOperandManager.RegisterA, 3);

            // 0xE0 - 0xEF
            PrefixedOpCodes[0xE0] = new SetBit(0xE0, 2, 8, registerOperandManager.RegisterB, 4);
            PrefixedOpCodes[0xE1] = new SetBit(0xE1, 2, 8, registerOperandManager.RegisterC, 4);
            PrefixedOpCodes[0xE2] = new SetBit(0xE2, 2, 8, registerOperandManager.RegisterD, 4);
            PrefixedOpCodes[0xE3] = new SetBit(0xE3, 2, 8, registerOperandManager.RegisterE, 4);
            PrefixedOpCodes[0xE4] = new SetBit(0xE4, 2, 8, registerOperandManager.RegisterH, 4);
            PrefixedOpCodes[0xE5] = new SetBit(0xE5, 2, 8, registerOperandManager.RegisterL, 4);
            PrefixedOpCodes[0xE6] = new SetBit(0xE6, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 4);
            PrefixedOpCodes[0xE7] = new SetBit(0xE7, 2, 8, registerOperandManager.RegisterA, 4);
            PrefixedOpCodes[0xE8] = new SetBit(0xE8, 2, 8, registerOperandManager.RegisterB, 5);
            PrefixedOpCodes[0xE9] = new SetBit(0xE9, 2, 8, registerOperandManager.RegisterC, 5);
            PrefixedOpCodes[0xEA] = new SetBit(0xEA, 2, 8, registerOperandManager.RegisterD, 5);
            PrefixedOpCodes[0xEB] = new SetBit(0xEB, 2, 8, registerOperandManager.RegisterE, 5);
            PrefixedOpCodes[0xEC] = new SetBit(0xEC, 2, 8, registerOperandManager.RegisterH, 5);
            PrefixedOpCodes[0xED] = new SetBit(0xED, 2, 8, registerOperandManager.RegisterL, 5);
            PrefixedOpCodes[0xEE] = new SetBit(0xEE, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 5);
            PrefixedOpCodes[0xEF] = new SetBit(0xEF, 2, 8, registerOperandManager.RegisterA, 5);

            // 0xF0 - 0xFF
            PrefixedOpCodes[0xF0] = new SetBit(0xF0, 2, 8, registerOperandManager.RegisterB, 6);
            PrefixedOpCodes[0xF1] = new SetBit(0xF1, 2, 8, registerOperandManager.RegisterC, 6);
            PrefixedOpCodes[0xF2] = new SetBit(0xF2, 2, 8, registerOperandManager.RegisterD, 6);
            PrefixedOpCodes[0xF3] = new SetBit(0xF3, 2, 8, registerOperandManager.RegisterE, 6);
            PrefixedOpCodes[0xF4] = new SetBit(0xF4, 2, 8, registerOperandManager.RegisterH, 6);
            PrefixedOpCodes[0xF5] = new SetBit(0xF5, 2, 8, registerOperandManager.RegisterL, 6);
            PrefixedOpCodes[0xF6] = new SetBit(0xF6, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 6);
            PrefixedOpCodes[0xF7] = new SetBit(0xF7, 2, 8, registerOperandManager.RegisterA, 6);
            PrefixedOpCodes[0xF8] = new SetBit(0xF8, 2, 8, registerOperandManager.RegisterB, 7);
            PrefixedOpCodes[0xF9] = new SetBit(0xF9, 2, 8, registerOperandManager.RegisterC, 7);
            PrefixedOpCodes[0xFA] = new SetBit(0xFA, 2, 8, registerOperandManager.RegisterD, 7);
            PrefixedOpCodes[0xFB] = new SetBit(0xFB, 2, 8, registerOperandManager.RegisterE, 7);
            PrefixedOpCodes[0xFC] = new SetBit(0xFC, 2, 8, registerOperandManager.RegisterH, 7);
            PrefixedOpCodes[0xFD] = new SetBit(0xFD, 2, 8, registerOperandManager.RegisterL, 7);
            PrefixedOpCodes[0xFE] = new SetBit(0xFE, 2, 16, memoryOperandManager.LoadByteFromAddressHL, 7);
            PrefixedOpCodes[0xFF] = new SetBit(0xFF, 2, 8, registerOperandManager.RegisterA, 7);
        }

        public IOpCode GetOpCode(Cpu cpu, Mmu mmu)
        {
            var opCodeIdentifier = mmu.ReadByte(cpu.ProgramCounter);

            if (opCodeIdentifier == 0xCB)
            {
                var prefixIdentifier = mmu.ReadByte((ushort)(cpu.ProgramCounter + 1));

                var prefixedRetVal = PrefixedOpCodes[prefixIdentifier];

                if (prefixedRetVal == null)
                {
                    throw new NullReferenceException($"Expected prefixed opcode 0x{prefixIdentifier:X}, but none was found.");
                }

                return prefixedRetVal;
            }

            var retVal = OpCodes[opCodeIdentifier];

            if (retVal == null)
            {
                throw new NullReferenceException($"Expected opcode 0x{opCodeIdentifier:X}, but none was found.");
            }

            return retVal;
        }
    }
}
