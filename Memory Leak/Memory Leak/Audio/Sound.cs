using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace MemoryLeak.Audio
{
    class Sound
    {
        private WaveOutEvent _waveOutDevice = new WaveOutEvent();
        private WaveChannel32 _inputStream;

        private Sound(Stream stream) //Oh my god this is so clever, a private constructor means that nobody can go create sounds like a fucking bitch and use up precious resources, but this'll force you to use FromStream to create sounds so that's just fucking plain perfect!
        {
            WaveStream waveStream = new WaveFileReader(stream);
            _inputStream = new WaveChannel32(waveStream);
        }

        public void Play()
        {
            _waveOutDevice.Init(_inputStream);
            _waveOutDevice.Play();
        }

        public static Sound FromStream(Stream stream)
        {
            return new Sound(stream);
        }
    }
}
