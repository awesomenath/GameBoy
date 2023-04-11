# Introduction 
A personal project to learn basic emulation and lower level instruction handling. Uses .NET Core 6 and WPF Desktop tooling for rendering.
- Currently only supports the original GameBoy and some games.
- Game saves are interoperable with other emulators e.g. [BGB](http://bgb.bircd.org/)

# Getting Started
1. Fork / Download the code
2. Start debugging / Compile and run
3. Click on the main render section to then select the ROM to play

## Input Mapping
|Input|Key binding|
|---|---|
|Start|Enter|
|Select|Shift|
|A|A|
|B|B|
|Left|Arrow key Left|
|Right|Arrow key Right|
|Up|Arrow key Up|
|Down|Arrow key Down|

# Useful Links
A list of links that provided useful information, documentation for development or test ROMs

- [Pan Docs](https://gbdev.io/pandocs/)
- [Instruction Set](https://gbdev.io/gb-opcodes/optables/)
- [CPU Manual](http://marc.rawer.de/Gameboy/Docs/GBCPUman.pdf)
- [Awesome GB Dev](https://github.com/gbdev/awesome-gbdev)
- [dmg-acid2](https://github.com/mattcurrie/dmg-acid2)
- [Blargg Test Roms](https://github.com/retrio/gb-test-roms)
- [Mooneye Test Suite](https://github.com/Gekkio/mooneye-test-suite)

# Rendering Status
## DMG-ACID2
![DMG-ACID2](./Docs/Images/AcidTestEmulated.PNG?raw=true "DMG-ACID2")

While appears to be passing the ACID test, still getting rendering issues with some ROMs. e.g.
![Murder Mansion](./Docs/Images/MurderMansion.PNG?raw=true "Murder Mansion")


# Blargg Test CPU ROM Status
|ROM Name|Status|
|---|---|
|01-special|:heavy_check_mark:|
|02-interrupts|:heavy_check_mark:|
|03-op sp,hl|:heavy_check_mark:|
|04-op r,imm|:heavy_check_mark:|
|05-op rp|:heavy_check_mark:|
|06-ld r,r|:heavy_check_mark:|
|07-jr,jp,call,ret,rst|:heavy_check_mark:|
|08-misc instrs|:heavy_check_mark:|
|09-op r,r|:heavy_check_mark:|
|10-bit ops|:heavy_check_mark:|
|11-op a,(hl)|:heavy_check_mark:|

# MBC Support
|MBC|Status|
|---|---|
|No MBC|:heavy_check_mark:|
|MBC1|:heavy_check_mark:|
|MBC2|:x:|
|MMM01|:x:|
|MBC3|:heavy_check_mark: (No Timer Support)|
|MBC5|:heavy_check_mark:|
|MBC6|:x:|
|MBC7|:x:|
|POCKET CAMERA|:x:|
|BANDAI TAMA5|:x:|
|HuC3|:x:|
|HuC1|:x:|

# Game Images
![Pokemon Blue](./Docs/Images/Blue.PNG?raw=true "Pokemon blue")
![Dr Mario](./Docs/Images/DrMario.PNG?raw=true "Dr Mario")
![2048](./Docs/Images/2048.PNG?raw=true "2048")
