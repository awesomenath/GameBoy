using GameBoy.Core.Hardware;
using System;

namespace GameBoy.Core.Instructions.Operands
{
    public class MemoryOperandManager
    {
        public Operand<byte> ImmediateByte { get; private set; }
        public Operand<sbyte> ImmediateSignedByte { get; private set; }
        public Operand<ushort> ImmediateWord { get; private set; }
        public Operand<ushort> Address { get; private set; }
        public Operand<ushort> LoadWordFromImmediateAddress { get; private set; }
        public Operand<byte> LoadByteFromImmediateAddress { get; private set; }
        public Operand<byte> LoadByteFromImmediateByteAddress { get; private set; }
        public Operand<byte> LoadByteFromAddressC { get; private set; }
        public Operand<byte> LoadByteFromAddressBC { get; private set; }
        public Operand<byte> LoadByteFromAddressDE { get; private set; }
        public Operand<byte> LoadByteFromAddressHL { get; private set; }
        public Operand<byte> LoadByteFromAddressHLIncrement { get; private set; }
        public Operand<byte> LoadByteFromAddressHLDecrement { get; private set; }
        public Operand<ushort> LoadWordFromStackPointer { get; private set; }
        public Operand<ushort> LoadWordFromStackPointerImediateByte { get; private set; }

        public MemoryOperandManager(Cpu cpu, Mmu mmu)
        {
            InitialiseOperands(cpu, mmu);
        }

        private void InitialiseOperands(Cpu cpu, Mmu mmu)
        {
            ImmediateByte = new Operand<byte>("Load Immediate Byte", 
                () =>
                {
                    var address = (ushort)(cpu.ProgramCounter + 1);

                    var retVal = mmu.ReadByte(address);

                    return retVal;
                }, 
                val => { throw new NotImplementedException(); });

            ImmediateSignedByte = new Operand<sbyte>("Load Immediate Signed Byte", 
                () =>
                {
                    var address = (ushort)(cpu.ProgramCounter + 1);

                    var retVal = (sbyte)mmu.ReadByte(address);

                    return retVal;
                }, 
                val => { throw new NotImplementedException(); });

            ImmediateWord = new Operand<ushort>("Load Immediate Word", 
                () =>
                {
                    var address = (ushort)(cpu.ProgramCounter + 1);

                    var retVal = mmu.ReadWord(address);
                    return retVal;
                }, 
                val => { throw new NotImplementedException(); });

            Address = new Operand<ushort>("Load 16-bit Address", 
                () =>
                {
                    var address = (ushort)(cpu.ProgramCounter + 1);

                    var retVal = mmu.ReadWord(address);

                    return retVal;
                },
                value =>
                {
                    var addressToGet = (ushort)(cpu.ProgramCounter + 1);

                    var address = mmu.ReadWord(addressToGet);

                    mmu.WriteWord(address, value);
                });

            LoadWordFromImmediateAddress = new Operand<ushort>("Load word from the immediate address", 
                () =>
                {
                    var immediateAddress = (ushort)(cpu.ProgramCounter + 1);
                    var wordAtImmediateAddress = mmu.ReadWord(immediateAddress);

                    var foundByte = mmu.ReadWord(wordAtImmediateAddress);

                    return foundByte;
                },
                value =>
                {
                    var immediateAddress = (ushort)(cpu.ProgramCounter + 1);
                    var wordAtImmediateAddress = mmu.ReadWord(immediateAddress);

                    mmu.WriteWord(wordAtImmediateAddress, value);
                });

            LoadByteFromImmediateAddress = new Operand<byte>("Load byte from the immediate address", 
                () =>
                {
                    var immediateAddress = (ushort)(cpu.ProgramCounter + 1);
                    var wordAtImmediateAddress = mmu.ReadWord(immediateAddress);

                    var foundByte = mmu.ReadByte(wordAtImmediateAddress);

                    return foundByte;
                },
                value =>
                {
                    var immediateAddress = (ushort)(cpu.ProgramCounter + 1);
                    var wordAtImmediateAddress = mmu.ReadWord(immediateAddress);

                    mmu.WriteByte(wordAtImmediateAddress, value);
                });

            LoadByteFromImmediateByteAddress = new Operand<byte>("Load byte from address defined by 0xFF00 + immediate byte", 
                () =>
                {
                    var immediateByteAddress = (ushort)(cpu.ProgramCounter + 1);
                    var byteAtImmediateByteAddress = mmu.ReadByte(immediateByteAddress);

                    var targetAddress = (ushort)(0xFF00 + byteAtImmediateByteAddress);
                    var foundByte = mmu.ReadByte(targetAddress);

                    return foundByte;
                },
                value =>
                {
                    var immediateByteAddress = (ushort)(cpu.ProgramCounter + 1);
                    var byteAtImmediateByteAddress = mmu.ReadByte(immediateByteAddress);

                    var targetAddress = (ushort)(0xFF00 + byteAtImmediateByteAddress);

                    mmu.WriteByte(targetAddress, value);
                });

            LoadByteFromAddressC = new Operand<byte>("Load byte from address in registers C (0xFF00 + C)", 
                () =>
                {
                    var address = (ushort)(0xFF00 + cpu.C);

                    var retVal = mmu.ReadByte(address);
                    return retVal;
                },
                value =>
                {
                    var address = (ushort)(0xFF00 + cpu.C);

                    mmu.WriteByte(address, value);
                });

            LoadByteFromAddressBC = new Operand<byte>("Load byte from address in registers BC", 
                () =>
                {
                    var address = cpu.BC;

                    var retVal = mmu.ReadByte(address);
                    return retVal;
                },
                value =>
                {
                    var address = cpu.BC;

                    mmu.WriteByte(address, value);
                });

            LoadByteFromAddressDE = new Operand<byte>("Load byte from address in registers DE", 
                () =>
                {
                    var address = cpu.DE;

                    var retVal = mmu.ReadByte(address);
                    return retVal;
                },
                value =>
                {
                    var address = cpu.DE;

                    mmu.WriteByte(address, value);
                });

            LoadByteFromAddressHL = new Operand<byte>("Load byte from address in registers HL", 
                () =>
                {
                    var address = cpu.HL;

                    var retVal = mmu.ReadByte(address);
                    return retVal;
                },
                value =>
                {
                    var address = cpu.HL;

                    mmu.WriteByte(address, value);
                });

            LoadByteFromAddressHLIncrement = new Operand<byte>("Load byte from address in registers HL, then increment HL", 
                () =>
                {
                    var address = cpu.HL;
                    var retVal = mmu.ReadByte(address);

                    cpu.HL = (ushort)(cpu.HL + 1);

                    return retVal;
                },
                value =>
                {
                    var address = cpu.HL;

                    mmu.WriteByte(address, value);

                    cpu.HL = (ushort)(cpu.HL + 1);
                });

            LoadByteFromAddressHLDecrement = new Operand<byte>("Load byte from address in registers HL, then decrement HL", 
                () =>
                {
                    var address = cpu.HL;
                    var retVal = mmu.ReadByte(address);

                    cpu.HL = (ushort)(cpu.HL - 1);
                
                    return retVal;
                },
                value =>
                {
                    var address = cpu.HL;

                    mmu.WriteByte(address, value);

                    cpu.HL = (ushort)(cpu.HL - 1);
                });

            LoadWordFromStackPointer = new Operand<ushort>("Load word from stack pointer", 
                () =>
                {
                    var address = cpu.StackPointer;

                    var retVal = mmu.ReadWord(address);

                    return retVal;
                },
                value =>
                {
                    var address = cpu.StackPointer;

                    mmu.WriteWord(address, value);
                });

            LoadWordFromStackPointerImediateByte = new Operand<ushort>("Load word from stack pointer, add immediate byte",
                () =>
                {
                    sbyte value = ImmediateSignedByte.Get();

                    var stackPointerValue = cpu.StackPointer;
                    var addResult = stackPointerValue + value;
                    var retVal = (ushort)addResult;

                    cpu.FlagZ = false;
                    cpu.FlagN = false;

                    // https://stackoverflow.com/a/7261149
                    if (value > 0)
                    {
                        cpu.FlagC = ((stackPointerValue & 0xFF) + value) > 0xFF;
                        cpu.FlagH = ((stackPointerValue & 0x0F) + (value & 0x0F)) > 0x0F;
                    }
                    else
                    {
                        cpu.FlagC = (retVal & 0xFF) <= (stackPointerValue & 0xFF);
                        cpu.FlagH = (retVal & 0x0F) <= (stackPointerValue & 0x0F);
                    }

                    return retVal;
                },
                value =>
                {
                    var address = cpu.StackPointer;

                    mmu.WriteWord(address, value);
                });
        }
    }
}
