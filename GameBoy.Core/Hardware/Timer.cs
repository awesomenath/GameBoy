namespace GameBoy.Core.Hardware
{
    public class Timer
    {
        private const ushort DividerRegisterAddress = 0xFF04;
        private const ushort TimerCountAddress = 0xFF05; // TIMA
        private const ushort TimerModuloAddress = 0xFF06; // TMA
        private const ushort TimerControlAddress = 0xFF07; // TAC

        public MemorySpace TimerMemorySpace = new ("Timer registers", DividerRegisterAddress, TimerControlAddress);

        public Mmu Mmu { get; }

        public bool Enabled { get; private set; }
        public TimerSpeed TimerFrequency { get; private set; }

        public int TimerCurrentCycleCount { get; private set; }
        public int DivideCurrentCycleCount { get; private set; }

        private byte CurrentDividerValue = 0;
        private byte CurrentTimerValue = 0;
        private byte TimerModuloValue = 0;
        private int TimerCyclesRequiredForSelectedFrequencey = 0;

        public enum TimerSpeed
        {
            /// <summary>
            ///  CPU Clock / 1024 (DMG, SGB2, CGB Single Speed Mode:   4096 Hz, SGB1:   ~4194 Hz, CGB Double Speed Mode:   8192 Hz)
            /// </summary>
            Hz4096 = 0,

            /// <summary>
            /// CPU Clock / 16   (DMG, SGB2, CGB Single Speed Mode: 262144 Hz, SGB1: ~268400 Hz, CGB Double Speed Mode: 524288 Hz)
            /// </summary>
            Hz262144 = 1,

            /// <summary>
            /// CPU Clock / 64   (DMG, SGB2, CGB Single Speed Mode:  65536 Hz, SGB1:  ~67110 Hz, CGB Double Speed Mode: 131072 Hz)
            /// </summary>
            Hz65536 = 2,

            /// <summary>
            /// CPU Clock / 256  (DMG, SGB2, CGB Single Speed Mode:  16384 Hz, SGB1:  ~16780 Hz, CGB Double Speed Mode:  32768 Hz)
            /// </summary>
            Hz16384 = 3
        }

        public Timer(Mmu mmu)
        {
            Mmu = mmu;

            mmu.SubscribeToMemorySpaceEvents(TimerMemorySpace, HandleMemoryEvent);
        }

        private void HandleMemoryEvent(ushort address)
        {
            if (address == DividerRegisterAddress)
            {
                ResetDiv();
            }

            if (address == TimerModuloAddress)
            {
                TimerModuloValue = Mmu.ReadByte(address);
            }

            if (address == TimerControlAddress)
            {
                var value = Mmu.ReadByte(address);

                Enabled = (value | 0x04) > 0;

                var frequencyFlags = value & 0x03;

                CurrentTimerValue = 0;
                SetTimerFrequency(frequencyFlags);

            }
        }

        private void SetTimerFrequency(int frequencyFlags)
        {
            TimerFrequency = (TimerSpeed)frequencyFlags;

            int cyclesRequired = 0;
            switch (TimerFrequency)
            {
                case TimerSpeed.Hz4096:
                    cyclesRequired = 1024;
                    break;
                case TimerSpeed.Hz262144:
                    cyclesRequired = 16;
                    break;
                case TimerSpeed.Hz65536:
                    cyclesRequired = 64;
                    break;
                case TimerSpeed.Hz16384:
                    cyclesRequired = 256;
                    break;
            }

            TimerCyclesRequiredForSelectedFrequencey = cyclesRequired;
        }

        public void ResetDiv()
        {
            CurrentDividerValue = 0;
            Mmu.WriteByte(DividerRegisterAddress, CurrentDividerValue, true);
        }

        public void Update(int cyclesElapsed)
        {
            // Handle Divider register
            DivideCurrentCycleCount += cyclesElapsed;

            if (DivideCurrentCycleCount >= 256)
            {
                CurrentDividerValue++;

                Mmu.WriteByte(DividerRegisterAddress, CurrentDividerValue, true);

                DivideCurrentCycleCount -= 256;
            }

            // Handle other timers
            if (Enabled)
            {
                TimerCurrentCycleCount += cyclesElapsed;

                while(TimerCurrentCycleCount >= TimerCyclesRequiredForSelectedFrequencey)
                {
                    IncrementTimer();
                    TimerCurrentCycleCount -= TimerCyclesRequiredForSelectedFrequencey;
                }
            }
        }

        private void IncrementTimer()
        {
            CurrentTimerValue++;

            Mmu.WriteByte(TimerCountAddress, (byte)CurrentTimerValue);

            // Timer hit
            if (CurrentTimerValue == byte.MinValue)
            {
                CurrentTimerValue = TimerModuloValue;
                Mmu.WriteByte(Mmu.InterruptFlags.Start, (byte)(Mmu.ReadByte(Mmu.InterruptFlags.Start) | 0x04));
            }

            //TimerCurrentCycleCount %= cyclesRequired;
        }
    }
}
