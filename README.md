![ChromaBoy](https://i.imgur.com/FpgsCER.png)

![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/Hacktix/ChromaBoy)
![GitHub last commit](https://img.shields.io/github/last-commit/Hacktix/ChromaBoy)
![GitHub Release Date](https://img.shields.io/github/release-date/Hacktix/ChromaBoy?label=latest%20release)
![GitHub (Pre-)Release Date](https://img.shields.io/github/release-date-pre/Hacktix/ChromaBoy?label=latest%20pre-release)

# What's this, exactly?
ChromaBoy is an experimental emulator of the classic Nintendo GameBoy. *Eventually* it's planned to expand to GameBoy Color too, however, getting the classic up and running is the first priority. As I've never emulated the GameBoy before, this project may be prone to beginner's mistakes. If you do face any issues in the pre-release and release versions (once they're published), please report them through GitHub Issues.

# Technical Details
This project is the third Chroma-based emulator project, preceded by [Chroma Invaders](https://github.com/Hacktix/Chroma-Invaders) and [CHROMA-8](https://github.com/Hacktix/CHROMA-8). As to be expected by now, all video output is handled by built-in Chroma features. As audio is not the first priority at the moment, it's not sure which technology will be used, however, chances are I will use my own [ChromaSynth](https://github.com/Hacktix/ChromaSynth) library, which allows for more-or-less simple audio synthesis at runtime.

# ToDo-List
```
# Emulator Core
[✓] ROM Metadata decoding
[✓] CPU Emulation
[ ] GBC-Specific Features

# Memory Bank Controllers (MBCs)
[~] MBC1
[ ] MBC2
[ ] MBC3
[ ] MBC5
[ ] MBC6
[ ] MBC7
[ ] HuC1
[ ] MBC1M
[ ] MMM01

# Input / Output
[~] Video Output
[ ] Sound Controller
[ ] Joypad Controls
[ ] Serial Data Transfer

# Accessories
[ ] Game Boy Printer
[ ] Game Boy Camera
[ ] Gamegenie / Gameshark Codes
```