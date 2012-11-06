using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace MemoryLeak.Audio
{
    internal class Sound
    {
        private WaveOutEvent waveOut = new WaveOutEvent();

        private Sound(Stream stream)
        {
            WaveStream waveStream = new WaveFileReader(stream);
            WaveChannel32 waveChannel = new WaveChannel32(waveStream);

            LoopStream loop = new LoopStream(waveStream);
            waveOut.Init(loop);
        }

        public void Play()
        {
            /*_waveOutDevice.Init(_inputStream);
            _waveOutDevice.Play();*/

            waveOut.Play();
        }

        public void Stop()
        {
            waveOut.Stop();
        }

        public static Sound FromStream(Stream stream)
        {
            return new Sound(stream);
        }
    }

    ////////////// Everything down here ripped from http://mark-dot-net.blogspot.nl/2009/10/looped-playback-in-net-with-naudio.html //////////////

    /// <summary>
    /// Stream for looping playback
    /// </summary>
    public class LoopStream : WaveStream
    {
        private WaveStream sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream)
        {
            this.sourceStream = sourceStream;
            this.EnableLooping = true;
        }

        /// <summary>
        /// Use this to turn looping on or off
        /// </summary>
        public bool EnableLooping { get; set; }

        /// <summary>
        /// Return source stream's wave format
        /// </summary>
        public override WaveFormat WaveFormat
        {
            get { return sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length
        {
            get { return sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get { return sourceStream.Position; }
            set { sourceStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
