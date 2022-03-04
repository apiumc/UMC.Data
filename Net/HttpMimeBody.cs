using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace UMC.Net
{
    public class HttpBadRequestException : Exception
    {
        public HttpBadRequestException(String header) : base(header)
        {

        }

    }
    public abstract class HttpMimeBody
    {
        protected static readonly byte[] HTTPHeader = System.Text.Encoding.ASCII.GetBytes("HTTP/");
        static readonly byte[] HeaderTransferEncodingChunked = System.Text.Encoding.ASCII.GetBytes("\r\nTransfer-Encoding: chunked");
        static readonly byte[] HeaderConnectionClose = System.Text.Encoding.ASCII.GetBytes("\r\nConnection: close");
        static readonly byte[] HeaderKeepAlive = System.Text.Encoding.ASCII.GetBytes("\r\nKeep-Alive:");
        static readonly byte[] HeaderContentLength = System.Text.Encoding.ASCII.GetBytes("\r\nContent-Length:");
        public static readonly byte[] HeaderEnd = new byte[] { 13, 10, 13, 10 };


        internal static readonly byte[][] HTTPMethods = new byte[10][];
        static HttpMimeBody()
        {
            HTTPMethods[0] = System.Text.Encoding.ASCII.GetBytes("GET");
            HTTPMethods[1] = System.Text.Encoding.ASCII.GetBytes("OPTIONS");
            HTTPMethods[2] = System.Text.Encoding.ASCII.GetBytes("HEAD");
            HTTPMethods[3] = System.Text.Encoding.ASCII.GetBytes("POST");
            HTTPMethods[4] = System.Text.Encoding.ASCII.GetBytes("PUT");
            HTTPMethods[5] = System.Text.Encoding.ASCII.GetBytes("DELETE");
            HTTPMethods[6] = System.Text.Encoding.ASCII.GetBytes("TRACE");
            HTTPMethods[7] = System.Text.Encoding.ASCII.GetBytes("CONNECT");
            HTTPMethods[8] = System.Text.Encoding.ASCII.GetBytes("HTTP/1.1");
            HTTPMethods[9] = System.Text.Encoding.ASCII.GetBytes("HTTP/1.0");

        }


        int _ChunkSize = -2;
        bool _isChunked = false;
        byte[] _buffers = new byte[0];
        bool _isClose = false;

        public bool IsClose => _isClose;

        public bool IsChunked => _isChunked;

        int _keepAlive = 60;
        public int KeepAlive => _keepAlive;

        protected abstract void Header(byte[] data, int offset, int size);
        protected virtual void MimeBody(byte[] data, int offset, int size)
        {

        }
        internal protected String Authority
        {
            get;
            set;
        }


        public virtual void Finish()
        {

        }
        public bool IsHttpFormatError
        {
            get;
            protected set;
        }
        public bool IsMimeFinish
        {
            get
            {
                if (_ChunkSize == -2)
                {
                    return false;
                }
                if (_ChunkSize > 0)
                {
                    return false;
                }
                else if (_isChunked)
                {

                    return isChunkedFinish;

                }
                else
                {
                    return true;

                }

            }

        }
        int _ContentLength = -1;
        public int ContentLength
        {
            get { return _ContentLength; }
            protected set { _ContentLength = value; }
        }


        internal void ReceiveException(Exception ex)
        {
            IsHttpFormatError = true;
            if (this.IsChunked && this._lastChunkSize > 0)
            {
                this.Body(this._lastChunk, 0, this._lastChunkSize);
                this._lastChunkSize = 0;
            }
            this.ReceiveError(ex);
        }
        protected virtual void ReceiveError(Exception ex)
        {

        }


        bool isChunkedFinish;// = false;

        byte[] _lastChunk = new byte[0x200];
        byte[] chunkNumBuffer = new byte[20];
        int chunkNumSize = 0;
        int _lastChunkSize = 0;
        int _lastChunkMaxSize = 0;
        protected virtual void Body(byte[] buffer, int offset, int size)
        {

        }
        void ChunkBody(byte[] buffer, int offset, int size)
        {

            var end = offset + size;

            var chunkOffset = offset;
            if (_lastChunkMaxSize > _lastChunkSize)
            {
                var len = _lastChunkMaxSize - _lastChunkSize;
                if (len > size)
                {
                    if (_lastChunkSize + size > _lastChunk.Length)
                    {
                        var b = new byte[_lastChunkSize + size + 0x200];
                        Array.Copy(_lastChunk, 0, b, 0, _lastChunkSize);
                        _lastChunk = b;
                    }
                    Array.Copy(buffer, offset, _lastChunk, _lastChunkSize, size);
                    _lastChunkSize += size;
                    return;
                }
                else
                {
                    if (_lastChunkSize + len > _lastChunk.Length)
                    {
                        var b = new byte[_lastChunkSize + len];
                        Array.Copy(_lastChunk, 0, b, 0, _lastChunkSize);
                        _lastChunk = b;
                    }

                    Array.Copy(buffer, offset, _lastChunk, _lastChunkSize, len);
                    _lastChunkSize += len;
                    this.Body(_lastChunk, 0, _lastChunkSize);

                    _lastChunkSize = 0;
                    _lastChunkMaxSize = 0;
                    chunkOffset = offset + len + 2;
                }
            }

            while (chunkOffset < end)
            {

                if (buffer[chunkOffset] == 10)
                {
                    if (chunkNumSize > 0)
                    {
                        if (chunkNumBuffer[chunkNumSize - 1] == 13 && chunkNumSize > 1)
                        {
                            chunkOffset++;
                            int chunkSize;
                            if (TryParse(chunkNumBuffer, 0, chunkNumSize - 1, 16, out chunkSize))
                            {
                                chunkNumSize = 0;
                                if (chunkSize == 0)
                                {
                                    this.isChunkedFinish = true;
                                }
                                else
                                {
                                    if (chunkSize + chunkOffset > end)
                                    {
                                        _lastChunkMaxSize = chunkSize;
                                        var wsize = end - chunkOffset;

                                        if (wsize > _lastChunk.Length)
                                        {
                                            this._lastChunk = new byte[wsize + 0x200];
                                        }
                                        Array.Copy(buffer, chunkOffset, _lastChunk, 0, wsize);
                                        _lastChunkSize = wsize;
                                        break;
                                    }
                                    else
                                    {
                                        _lastChunkSize = 0;
                                        _lastChunkMaxSize = 0;
                                        this.Body(buffer, chunkOffset, chunkSize);

                                        chunkOffset += chunkSize + 2;
                                    }
                                }
                            }
                            else
                            {
                                chunkNumSize = 0;
                                break;
                            }

                        }
                        else
                        {
                            chunkOffset++;
                            chunkNumSize = 0;
                            //break;
                        }
                    }
                    else
                    {
                        chunkOffset++;
                        chunkNumSize = 0;
                    }

                }
                else
                {
                    if (chunkNumSize > 19)
                    {
                        chunkNumSize = 0;
                    }
                    chunkNumBuffer[chunkNumSize] = buffer[chunkOffset];
                    chunkNumSize++;
                    chunkOffset++;
                }
            }


        }
        bool TryParse(byte[] data, int index, int count, int p, out int value)
        {
            value = 0;
            for (int i = 0; i < count; i++)
            {
                value = value * p;
                switch (data[i + index])
                {
                    case 0x30:
                        value += 0;
                        break;
                    case 0x31:
                        value += 1;
                        break;
                    case 0x32:
                        value += 2;
                        break;
                    case 0x33:
                        value += 3;
                        break;
                    case 0x34:
                        value += 4;
                        break;
                    case 0x35:
                        value += 5;
                        break;
                    case 0x36:
                        value += 6;
                        break;
                    case 0x37:
                        value += 7;
                        break;
                    case 0x38:
                        value += 8;
                        break;
                    case 0x39:
                        value += 9;
                        break;
                    case 0x61:
                    case 0x41:
                        value += 10;
                        break;
                    case 0x42:
                    case 0x62:
                        value += 11;
                        break;
                    case 0x43:
                    case 0x63:
                        value += 12;
                        break;
                    case 0x44:
                    case 0x64:
                        value += 13;
                        break;
                    case 0x45:
                    case 0x65:
                        value += 14;
                        break;
                    case 0x46:
                    case 0x66:
                        value += 15;
                        break;
                    default:
                        value = 0;
                        return false;


                }
            }
            return true;
        }
        bool _isCheckHeader = false;
        public void AppendData(byte[] buffer, int offset, int size)
        {
            if (_ChunkSize == -2)
            {
                byte[] data;
                var isBuffers = false;
                if (_buffers.Length == 0)
                {
                    data = buffer;
                }
                else
                {
                    isBuffers = true;
                    data = new byte[_buffers.Length + size];
                    Array.Copy(_buffers, 0, data, 0, _buffers.Length);
                    Array.Copy(buffer, offset, data, _buffers.Length, size);
                    size = data.Length;
                    offset = 0;
                }
                if (size > 7 && _isCheckHeader == false)
                {
                    if (data[offset] == 13 && data[offset + 1] == 10)
                    {
                        offset += 2;
                        size -= 2;
                    }
                    else if (data[offset] == 10)
                    {
                        offset++;
                        size--;
                    }
                    int index = UMC.Data.Utility.FindIndex(data, offset, offset + size, new byte[] { 32 });
                    if (index > 0)
                    {
                        var mcount = index - offset;
                        if (mcount < 10)
                        {
                            foreach (var method in HTTPMethods)
                            {
                                if (UMC.Data.Utility.FindIndex(data, offset, index, method) == offset)
                                {
                                    _isCheckHeader = true;
                                    break;
                                }
                            }
                            if (_isCheckHeader == false)
                            {
                                ReceiveException(new HttpBadRequestException(Encoding.ASCII.GetString(data, offset, size)));
                                return;

                            }
                        }
                        else
                        {
                            ReceiveException(new HttpBadRequestException(Encoding.ASCII.GetString(data, offset, size)));
                            return;
                        }
                    }
                    else
                    {
                        ReceiveException(new HttpBadRequestException(Encoding.ASCII.GetString(data, offset, size)));
                        return;

                    }
                }

                var end = UMC.Data.Utility.FindIndex(data, offset, offset + size, HeaderEnd);
                if (end > -1)
                {
                    int lIndex = UMC.Data.Utility.FindIndexIgnoreCase(data, offset, end, HeaderConnectionClose);

                    if (lIndex > -1)
                    {
                        this._isClose = true;
                        this._keepAlive = 0;
                    }
                    else
                    {
                        this._isClose = false;
                        lIndex = UMC.Data.Utility.FindIndexIgnoreCase(data, offset, end, HeaderKeepAlive);
                        if (lIndex > -1)
                        {
                            var startIndex = lIndex + HeaderKeepAlive.Length;
                            var endIndex = UMC.Data.Utility.FindIndex(data, startIndex, end + HeaderEnd.Length, new byte[] { 13, 10 });

                            var findex = UMC.Data.Utility.FindIndex(data, startIndex, endIndex, new byte[] { 61 });
                            if (findex > 0)
                            {
                                if (this.TryParse(data, findex + 1, endIndex - findex - 1, 10, out this._keepAlive) == false)
                                {
                                    this._keepAlive = 60;
                                }

                            }
                            else
                            {
                                if (this.TryParse(data, startIndex + 1, endIndex - startIndex - 1, 10, out this._keepAlive) == false)
                                {
                                    this._keepAlive = 60;
                                }

                            }
                        }
                        else
                        {
                            this._keepAlive = 60;
                        }
                    }


                    var headerSize = end - offset + 4;
                    var bodySize = size - headerSize;
                    lIndex = UMC.Data.Utility.FindIndexIgnoreCase(data, offset, end, HeaderContentLength);
                    if (lIndex == -1)
                    {
                        _ChunkSize = -1;
                        _isChunked = UMC.Data.Utility.FindIndexIgnoreCase(data, offset, end, HeaderTransferEncodingChunked) > -1;
                    }
                    else
                    {
                        int startIndex = lIndex + HeaderContentLength.Length;
                        int endIndex = UMC.Data.Utility.FindIndex(data, startIndex, end + HeaderEnd.Length, new byte[] { 13, 10 });

                        TryParse(data, startIndex + 1, endIndex - startIndex - 1, 10, out _ContentLength);
                        _ChunkSize = _ContentLength - bodySize;
                    }


                    this.Header(data, offset, headerSize);
                    if (bodySize > 0)
                    {
                        this.MimeBody(data, offset + headerSize, bodySize);
                        if (this._isChunked)
                        {
                            this.ChunkBody(data, offset + headerSize, bodySize);
                        }
                        else
                        {
                            this.Body(data, offset + headerSize, bodySize);
                        }
                    }
                }
                else
                {
                    if (isBuffers)
                    {
                        this._buffers = data;
                    }
                    else
                    {
                        var buffers2 = new byte[_buffers.Length + size];
                        Array.Copy(this._buffers, 0, buffers2, 0, _buffers.Length);
                        Array.Copy(data, offset, buffers2, _buffers.Length, size);
                        this._buffers = buffers2;
                    }
                    return;
                }
            }

            else if (_isChunked)
            {
                this.MimeBody(buffer, offset, size);
                this.ChunkBody(buffer, offset, size);
            }
            else if (_ChunkSize > 0)
            {
                this._ChunkSize -= size;

                this.MimeBody(buffer, offset, size);
                this.Body(buffer, offset, size);
            }


            if (this.IsMimeFinish)
            {
                this.Finish();
            }

        }
    }

}
