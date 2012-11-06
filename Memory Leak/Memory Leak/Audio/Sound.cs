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
        public Sound(Stream r)
        {
            //fix this plz

            /*WaveChannel32 inputStream;
            if (fileName.EndsWith(".mp3"))
            {
                WaveStream mp3Reader = new FileReader(r);
                inputStream = new WaveChannel32(mp3Reader);
            }
            else
            {
                throw new InvalidOperationException("Unsupported extension");
            }
            volumeStream = inputStream;
            return volumeStream;*/
        }

        public static Sound FromStream(Stream r)
        {
            return new Sound(r);
        }
    }
}
