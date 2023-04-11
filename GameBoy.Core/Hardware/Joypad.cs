namespace GameBoy.Core.Hardware
{
    public class Joypad
    {
        public Mmu Mmu { get; }
        public JoypadState JoypadState { get; }

        public Joypad(Mmu mmu, JoypadState joypadState)
        {
            Mmu = mmu;
            JoypadState = joypadState;

            mmu.SubscribeToMemorySpaceEvents(mmu.Io, IoMemoryWriteEventTrigger);
        }

        private void IoMemoryWriteEventTrigger(ushort address)
        {
            if (address == 0xFF00)
            {
                var writtenByte = Mmu.ReadByte(address);

                bool directionKeys = (writtenByte & 0x10) == 0;
                bool buttonKeys = (writtenByte & 0x20) == 0;

                byte newByte = (byte)(writtenByte | 0x0F);

                if (directionKeys)
                {
                    if (JoypadState.InputDownPressed)
                    {
                        newByte ^= 0x08;
                    }

                    if (JoypadState.InputUpPressed)
                    {
                        newByte ^= 0x04;
                    }

                    if (JoypadState.InputLeftPressed)
                    {
                        newByte ^= 0x02;
                    }

                    if (JoypadState.InputRightPressed)
                    {
                        newByte ^= 0x01;
                    }
                }
                else if (buttonKeys)
                {
                    if (JoypadState.StartPressed)
                    {
                        newByte ^= 0x08;
                    }

                    if (JoypadState.SelectPressed)
                    {
                        newByte ^= 0x04;
                    }

                    if (JoypadState.BPressed)
                    {
                        newByte ^= 0x02;
                    }

                    if (JoypadState.APressed)
                    {
                        newByte ^= 0x01;
                    }
                }

                Mmu.WriteByte(address, newByte, true);
            }
        }
    }
}
