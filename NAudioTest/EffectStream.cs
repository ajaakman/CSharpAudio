using System;
using NAudio.Wave;

namespace NAudioTest
{
    class EffectStream : WaveStream
    {        
        public WaveStream SourceStream { get; set; }

        public EffectStream(WaveStream stream)
        {
            this.SourceStream = stream;
        }
        public override long Length => SourceStream.Length;

        public override long Position { get => SourceStream.Position; set => SourceStream.Position = value; }

        public override WaveFormat WaveFormat => SourceStream.WaveFormat;

        public override int Read(byte[] buffer, int offset, int count)
        {
            Console.WriteLine("hello");
            return SourceStream.Read(buffer, offset, count);
        }
    }
}
