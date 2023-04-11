using GameBoy.Core.Hardware.MemoryBankControllers;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GameBoy.Core.Hardware
{
    public class Cartridge
    {
        public string Title { get; private set; }
        public string ManufacturerCode { get; private set; }
        public byte ColourGameBoyFlag { get; private set; }
        public string NewLicenseeCode { get; private set; }
        public byte SgbFlag { get; private set; }

        public IMbc MemoryBankController { get; private set; }
        public int RomSize { get; private set; }
        public int RamSize { get; private set; }
        public bool OverseasOnly { get; private set; }
        public byte VersionNo { get; private set; }

        private byte[] CatridgeBuffer { get; set; }

        public Cartridge(string fileName)
        {
            CatridgeBuffer = File.ReadAllBytes(fileName);

            if (CatridgeBuffer.Length < 0x0200)
            {
                CatridgeBuffer = new byte[0x0200];
            }

            Title = Encoding.ASCII.GetString(CatridgeBuffer.AsSpan(0x0134, 15)).Trim();

            Debug.WriteLine($"Loaded {Title}.");

            RomSize = 32 * 1024 * (1 << CatridgeBuffer[0x0148]);

            OverseasOnly = CatridgeBuffer[0x014A] != 0;
            VersionNo = CatridgeBuffer[0x014C];

            MemoryBankController = SelectMbc(fileName);
        }

        private IMbc SelectMbc(string fileName)
        {
            var mbcVal = CatridgeBuffer[0x0147];

            IMbc loadedMbc = null;

            switch (mbcVal)
            {
                case 0:
                    loadedMbc = new NoMbc(CatridgeBuffer);
                    break;
                case 1:
                    //loadedMbc = new MBC1(CatridgeBuffer);
                    loadedMbc = new Mbc1(CatridgeBuffer, false, false);
                    break;
                case 2:
                    //loadedMbc = new MBC1+RAM(CatridgeBuffer);
                    loadedMbc = new Mbc1(CatridgeBuffer, true, false);
                    break;
                case 3:
                    //loadedMbc = new MBC1+RAM+BATTERY(CatridgeBuffer);
                    loadedMbc = new Mbc1(CatridgeBuffer, true, true, fileName);
                    break;
                case 5:
                    //loadedMbc = new MBC2(CatridgeBuffer);
                    break;
                case 6:
                    //loadedMbc = new MBC2+BATTERY(CatridgeBuffer);
                    break;
                case 8:
                    //loadedMbc = new ROM+RAM(CatridgeBuffer);
                    break;
                case 9:
                    //loadedMbc = new ROM+RAM+BATTERY(CatridgeBuffer);
                    break;
                case 0x0B:
                    //loadedMbc = new MMM01(CatridgeBuffer);
                    break;
                case 0x0C:
                    //loadedMbc = new MMM01+RAM(CatridgeBuffer);
                    break;
                case 0x0D:
                    //loadedMbc = new MMM01+RAM+BATTERY(CatridgeBuffer);
                    break;
                case 0x0F:
                    //loadedMbc = new MBC3+TIMER+BATTERY(CatridgeBuffer);
                    loadedMbc = new Mbc3(CatridgeBuffer, true, false, true);
                    break;
                case 0x10:
                    //loadedMbc = new MBC3+TIMER+RAM+BATTERY(CatridgeBuffer);
                    loadedMbc = new Mbc3(CatridgeBuffer, true, true, true);
                    break;
                case 0x11:
                    //loadedMbc = new MBC3(CatridgeBuffer);
                    loadedMbc = new Mbc3(CatridgeBuffer, false, false, false);
                    break;
                case 0x12:
                    //loadedMbc = new MBC3+RAM(CatridgeBuffer);
                    loadedMbc = new Mbc3(CatridgeBuffer, false, true, false);
                    break;
                case 0x13:
                    //loadedMbc = new MBC3+RAM+BATTERY(CatridgeBuffer);
                    loadedMbc = new Mbc3(CatridgeBuffer, false, true, true, fileName);
                    break;
                case 0x19:
                    //loadedMbc = new MBC5(CatridgeBuffer);
                    loadedMbc = new Mbc5(CatridgeBuffer, false, false, false, fileName);
                    break;
                case 0x1A:
                    //loadedMbc = new MBC5+RAM(CatridgeBuffer);
                    loadedMbc = new Mbc5(CatridgeBuffer, true, false, false, fileName);
                    break;
                case 0x1B:
                    //loadedMbc = new MBC5+RAM+BATTERY(CatridgeBuffer);
                    loadedMbc = new Mbc5(CatridgeBuffer, true, true, false, fileName);
                    break;
                case 0x1C:
                    //loadedMbc = new MBC5+RUMBLE(CatridgeBuffer);
                    loadedMbc = new Mbc5(CatridgeBuffer, false, false, true, fileName);
                    break;
                case 0x1D:
                    //loadedMbc = new MBC5+RUMBLE+RAM(CatridgeBuffer);
                    loadedMbc = new Mbc5(CatridgeBuffer, true, false, true, fileName);
                    break;
                case 0x1E:
                    //loadedMbc = new MBC5+RUMBLE+RAM+BATTERY(CatridgeBuffer);
                    loadedMbc = new Mbc5(CatridgeBuffer, true, true, true, fileName);
                    break;
                case 0x20:
                    //loadedMbc = new MBC6(CatridgeBuffer);
                    break;
                case 0x22:
                    //loadedMbc = new MBC7+SENSOR+RUMBLE+RAM+BATTERY(CatridgeBuffer);
                    break;
                case 0xFC:
                    //loadedMbc = new POCKET CAMERA(CatridgeBuffer);
                    break;
                case 0xFD:
                    //loadedMbc = new BANDAI TAMA5(CatridgeBuffer);
                    break;
                case 0xFE:
                    //loadedMbc = new HuC3(CatridgeBuffer);
                    break;
                case 0xFF:
                    //loadedMbc = new HuC1+RAM+BATTERY(CatridgeBuffer);
                    break;
                default:
                    throw new InvalidOperationException($"Unkown MBC type {mbcVal:x2}");
            }

            Debug.WriteLine($"Detected MBC 0x{mbcVal:x2} {loadedMbc.MbcType}.");

            return loadedMbc;
        }

    }
}
