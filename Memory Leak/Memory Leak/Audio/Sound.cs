using System.IO;
using NAudio.Wave;

namespace MemoryLeak.Audio
{
    internal class Sound
    {
        private readonly WaveOutEvent _waveOut = new WaveOutEvent();
        private readonly LoopStream _loop;

        public bool IsLooped
        {
            set { _loop.EnableLooping = value; }
            get { return _loop.EnableLooping; }
        }

        private Sound(Stream stream)
        {
            var streamCheck = stream as FileStream;

	        WaveStream waveStream;

            if(streamCheck != null && streamCheck.Name.EndsWith(".mp3"))
	            waveStream = CreateInputStream(stream);
            else waveStream = new WaveFileReader(stream);

            _loop = new LoopStream(waveStream);
            _waveOut.Init(_loop);
            
            IsLooped = false;
        }

        private WaveStream CreateInputStream(Stream stream)
        {
	        WaveStream mp3Reader = new Mp3FileReader(stream);
			WaveChannel32 inputStream = new WaveChannel32(mp3Reader);
	        return inputStream;
        }

        public void Play()
        {
            /*_waveOutDevice.Init(_inputStream);
            _waveOutDevice.Play();*/

            _waveOut.Play();
        }

        public void Stop()
        {
            _waveOut.Stop();
        }

        public static Sound FromStream(Stream stream)
        {
            return new Sound(stream);
        }
    }

    /// <summary>
    /// Stream for looping playback
    /// </summary>
    public class LoopStream : WaveStream
    {
        private readonly WaveStream _sourceStream;

        /// <summary>
        /// Creates a new Loop stream
        /// </summary>
        /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
        /// or else we will not loop to the start again.</param>
        public LoopStream(WaveStream sourceStream)
        {
            _sourceStream = sourceStream;
            EnableLooping = true;
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
            get { return _sourceStream.WaveFormat; }
        }

        /// <summary>
        /// LoopStream simply returns
        /// </summary>
        public override long Length
        {
            get { return _sourceStream.Length; }
        }

        /// <summary>
        /// LoopStream simply passes on positioning to source stream
        /// </summary>
        public override long Position
        {
            get { return _sourceStream.Position; }
            set { _sourceStream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0 || !EnableLooping)
                    {
                        // something wrong with the source stream
                        break;
                    }
                    // loop
                    _sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }
}
