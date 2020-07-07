using ChromaSynth;
using System;

namespace ChromaBoy.Hardware
{
    public class APU
    {
        private Gameboy parent;

        private PulseWave ch1 = new PulseWave(Emulator.AudioManager, 0f, 0f);
        private PulseWave ch2 = new PulseWave(Emulator.AudioManager, 0f, 0f);

        private byte ch1Len = 0;
        private byte ch2Len = 0;

        private byte ch1Vol = 0;
        private byte ch2Vol = 0;

        private long apuCycles = 0;

        public APU(Gameboy parent)
        {
            this.parent = parent;
        }

        public void ProcessCycle()
        {
            // Update Channels
            if(parent.Memory.UpdateAudioChannel1) UpdateChannel1();
            TickChannel1();
            if(parent.Memory.UpdateAudioChannel2) UpdateChannel2();
            TickChannel2();

            apuCycles++;
        }

        private void TickChannel1()
        {
            // Volume Envelope
            byte nr12 = parent.Memory[0xFF12];
            if ((apuCycles % 65536) == 0 && (nr12 & 0b111) != 0)
            {
                parent.Memory.Set(0xFF12, (byte)(nr12-1));
                int newVol = ch1Vol + ((nr12 & 0b1000) != 0 ? 1 : -1);
                if (newVol > 15) ch1Vol = 15;
                else if (newVol <= 0) ch1Vol = 0;
                else ch1Vol = (byte)newVol;
            }

            // Update Sound Length
            if (parent.Memory.UpdateAudioLength1)
            {
                parent.Memory.UpdateAudioLength1 = false;
                ch1Len = (byte)(64 - (parent.Memory[0xFF11] & 0x3F));
            }
            if ((parent.Memory.Get(0xFF14) & 0b1000000) != 0)
            {
                if (apuCycles % 16384 == 0 && ch1Len > 0)
                {
                    ch1Len--;
                }
                if (ch1Vol != 0 && ch1Len == 0)
                {
                    ch1Vol = 0;
                }
            }

            // Set Sound Volume
            ch1.Volume = (float)(ch1Vol / 15.0);
        }

        private void UpdateChannel1()
        {
            // Reset update flag
            parent.Memory.UpdateAudioChannel1 = false;

            // Set Duty
            ch1.Duty = 0.125f;
            switch((parent.Memory[0xFF11] & 0b11000000) >> 6)
            {
                case 0b01: ch1.Duty = 0.25f; break;
                case 0b10: ch1.Duty = 0.5f; break;
                case 0b11: ch1.Duty = 0.75f; break;
            }

            // Set Frequency
            ushort nr13 = (ushort)(parent.Memory[0xFF13] | ((parent.Memory[0xFF14] & 0b111) << 8));
            ch1.Frequency = (float)(131072.0 / (2048 - nr13)) * 2.0f;

            // Handle Triggers
            if (parent.Memory.TriggerAudio1)
            {
                parent.Memory.TriggerAudio1 = false;
                if (ch1Len == 0) ch1Len = 64;
                ch1Vol = (byte)((parent.Memory[0xFF12] & 0xF0) >> 4);
            }
        }

        private void TickChannel2()
        {
            // Volume Envelope
            byte nr22 = parent.Memory[0xFF17];
            if ((apuCycles % 65536) == 0 && (nr22 & 0b111) != 0)
            {
                parent.Memory.Set(0xFF17, (byte)(nr22 - 1));
                int newVol = ch2Vol + ((nr22 & 0b1000) != 0 ? 1 : -1);
                if (newVol > 15) ch2Vol = 15;
                else if (newVol <= 0) ch2Vol = 0;
                else ch2Vol = (byte)newVol;
            }

            // Update Sound Length
            if (parent.Memory.UpdateAudioLength2)
            {
                parent.Memory.UpdateAudioLength2 = false;
                ch2Len = (byte)(64 - (parent.Memory[0xFF16] & 0x3F));
            }
            if ((parent.Memory.Get(0xFF19) & 0b1000000) != 0)
            {
                if (apuCycles % 16384 == 0 && ch2Len > 0)
                    ch2Len--;
                if (ch2Vol != 0 && ch2Len == 0)
                {
                    ch2Vol = 0;
                }
            }

            // Set Sound Volume
            ch2.Volume = (float)(ch2Vol / 15.0);
        }

        private void UpdateChannel2()
        {
            // Reset update flag
            parent.Memory.UpdateAudioChannel2 = false;

            // Set Duty
            ch2.Duty = 0.125f;
            switch ((parent.Memory[0xFF16] & 0b11000000) >> 6)
            {
                case 0b01: ch2.Duty = 0.25f; break;
                case 0b10: ch2.Duty = 0.5f; break;
                case 0b11: ch2.Duty = 0.75f; break;
            }

            // Set Frequency
            ushort nr23 = (ushort)(parent.Memory[0xFF18] | ((parent.Memory[0xFF19] & 0b111) << 8));
            ch2.Frequency = (float)(131072.0 / (2048 - nr23)) * 2.0f;

            // Handle Triggers
            if (parent.Memory.TriggerAudio2)
            {
                parent.Memory.TriggerAudio2 = false;
                if (ch2Len == 0) ch2Len = 64;
                ch2Vol = (byte)((parent.Memory[0xFF17] & 0xF0) >> 4);
            }
        }

        public void MixAudio(ref Span<float> chunk)
        {
            if(ch1.Volume > 0)
                ch1.GenerateChunk(ref chunk);
            if(ch2.Volume > 0)
                ch2.GenerateChunk(ref chunk);
        }
    }
}
