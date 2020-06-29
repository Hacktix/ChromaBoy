using System;
using System.IO;
using System.Reflection;

namespace ChromaBoy.Hardware
{
    public abstract class MemoryBankController
    {
        public int RAMSize;
        public int ROMSize;
        public int MemoryBanks;

        public MemoryBankController(int RAMSize, int ROMSize, int MemoryBanks)
        {
            this.RAMSize = RAMSize;
            this.ROMSize = ROMSize;
            this.MemoryBanks = MemoryBanks;
        }

        public abstract bool IsAddressWritable(int address);
        public abstract bool IsAddressReadable(int address);
        public abstract bool AccessesROM(int address);
        public abstract bool HandleWrite(int address, byte value);
        public abstract bool HandleRead(int address);
        public virtual byte MBCRead(int address) {
            return 0xFF;
        }
        public abstract int TranslateAddress(int address);
        public virtual void SaveExternalRam() { }

        protected static FileStream CreateSave()
        {
            string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string savefolder = Path.Combine(appDirectory, "saves");
            string savepath = Path.Combine(savefolder, Path.GetFileName(Program.Filename) + ".sav");

            if (!Directory.Exists(savefolder)) Directory.CreateDirectory(savefolder);
            if (File.Exists(savepath)) File.Delete(savepath);
            return File.Create(savepath);
        }

        protected static FileStream OpenSave()
        {
            string appDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string savefolder = Path.Combine(appDirectory, "saves");
            string savepath = Path.Combine(savefolder, Path.GetFileName(Program.Filename) + ".sav");

            if (!Directory.Exists(savefolder) || !File.Exists(savepath)) return null;
            return File.OpenRead(savepath);
        }

        protected virtual void LoadSave() { }
    }
}
