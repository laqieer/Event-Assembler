using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nintenlord.Event_Assembler.Core.IO.Input
{
    public sealed class InputByteStream : BinaryReader, IInputByteStream
    {
        public int Offset
        {
            get
            {
                return (int)this.BaseStream.Position;
            }
            set
            {
                this.BaseStream.Position = value;
            }
        }
        public int Length
        {
            get { return (int)this.BaseStream.Length; }
        }
        public int BytesLeft
        {
            get { return this.Length - this.Offset; }
        }
        
        public InputByteStream(Stream input)
            : base(input) { }

        public InputByteStream(Stream input, Encoding encoding)
            : base(input, encoding) { }
        
        public byte[] PeekBytes(int amount)
        {
            var offset = this.BaseStream.Position;
            var bytes = this.ReadBytes(amount);
            this.BaseStream.Position = offset;
            return bytes;
        }

        public short PeekInt16()
        {
            return BitConverter.ToInt16(this.PeekBytes(2), 0);
        }

        public ushort PeekUInt16()
        {
            return BitConverter.ToUInt16(this.PeekBytes(2), 0);
        }

        public int PeekInt32()
        {
            return BitConverter.ToInt32(this.PeekBytes(4), 0);
        }

        public uint PeekUInt32()
        {
            return BitConverter.ToUInt32(this.PeekBytes(4), 0);
        }
    }
}
