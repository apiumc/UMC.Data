using System;
using System.Text;

namespace UMC.Net
{

    public class TextWriter : System.IO.TextWriter
    {
        System.IO.Stream _hlr;
        char[] buffers = new char[0x200];
        int bufferSize = 0;
        Encoding _encoding;
        public TextWriter(System.IO.Stream hlr)
        {
            this._hlr = hlr;
            _encoding = System.Text.Encoding.UTF8;
        }
        public override Encoding Encoding
        {
            get
            {
                return _encoding;
            }
        }
        protected override void Dispose(bool disposing)
        {

            buffers = null;
            base.Dispose(disposing);
        }
        public override void Write(char value)
        {
            if (bufferSize == buffers.Length)
            {
                var b = _encoding.GetBytes(buffers, 0, bufferSize);
                _hlr.Write(b, 0, b.Length);
                bufferSize = 0;
            }

            buffers[bufferSize] = value;
            bufferSize++;

        }
        public override void Flush()
        {
            if (bufferSize > 0)
            {
                var b = _encoding.GetBytes(buffers, 0, bufferSize);
                _hlr.Write(b, 0, b.Length);
                bufferSize = 0;
            }
        }
    }
}
