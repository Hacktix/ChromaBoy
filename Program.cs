using System;
using System.IO;

namespace ChromaBoy
{
    class Program
    {
        static void Main(string[] args)
        {
            Emulator emu;
            if (args.Length > 0)
            {
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine("Error reading ROM: Could not find file specified.");
                    return;
                }
                byte[] rom = File.ReadAllBytes(args[0]);
                emu = new Emulator(rom);
            }
            else
                emu = new Emulator();
            emu.Run();
        }
    }
}
