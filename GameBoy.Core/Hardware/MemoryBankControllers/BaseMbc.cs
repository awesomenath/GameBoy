using System;
using System.IO;

namespace GameBoy.Core.Hardware.MemoryBankControllers
{
    public abstract class BaseMbc : IMbc
    {
        private readonly string SaveFileName;
        private readonly System.Threading.Timer saveTimer;

        private readonly int RomSize;
        private readonly int RamSize;

        protected readonly object RamLock = new();

        protected ushort CurrentRomBankNumber = 1;
        protected int CurrentRomBankOffset = 16 * 1024; // 0x4000

        protected byte CurrentRamBankNumber = 0;
        protected int CurrentRamBankOffset = 0;

        protected bool RamDirty = false;

        public byte[] Rom { get; set; }
        public byte[] Ram { get; set; }
        public string MbcType { get; init; }
        public bool TimerPresent { get; init; }
        public bool RamPresent { get; init; }
        public bool BatteryPresent { get; init; }

        protected BaseMbc(int romSize, int ramSize, bool battery, string romName)
        {
            RomSize = romSize;
            RamSize = ramSize;
            BatteryPresent = battery;

            Ram = new byte[ramSize];

            if (battery)
            {
                if (string.IsNullOrWhiteSpace(romName))
                {
                    throw new ArgumentNullException(nameof(romName));
                }

                SaveFileName = $"{romName}.sav";
                LoadSavedRam(SaveFileName);
                saveTimer = new System.Threading.Timer(FlushRam, null, 0, 5 * 1000);
            }
        }

        protected void InitRom(int romSize, byte[] rom)
        {
            Rom = new byte[rom.Length];

            Array.Copy(rom, Rom, rom.Length);
        }

        public virtual byte ReadByte(ushort address)
        {
            return Rom[address];
        }

        public virtual ushort ReadWord(ushort address)
        {
            return (ushort)((ReadByte((ushort)(address + 1)) << 8) | ReadByte(address));
        }

        public virtual void WriteByte(ushort address, byte value)
        {
            throw new NotImplementedException();
        }

        private void LoadSavedRam(string fileName)
        {
            if (File.Exists(fileName))
            {
                var bytes = File.ReadAllBytes(fileName);

                LoadSavedRam(bytes);
            }
            else
            {
                // Write an empty file to disk if missing
                File.Create(fileName, RamSize);
            }
        }

        protected abstract void LoadSavedRam(byte[] savedRam);

        private void FlushRam(object state)
        {
            if (RamDirty)
            {
                lock (RamLock)
                {
                    File.WriteAllBytes(SaveFileName, Ram);
                    RamDirty = false;
                }
            }
        }
    }
}
