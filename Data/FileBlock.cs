using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using UMC.Net;

namespace UMC.Data
{
   
    public class FileBlock
    {

        class BlockStream : System.IO.Stream
        {
            FileBlock _fileStream;
            byte[] _buffers, _blong;
            public BlockStream(FileBlock file, long fileid, byte[] fileHbytes, bool isAppend)
            {
                _fileStream = file;
                _fileid = fileid;

                _blong = new byte[12];

                _startFileBlock = BitConverter.ToInt64(fileHbytes, 0);

                _currentBlock = _startFileBlock;
                _length = BitConverter.ToInt32(fileHbytes, 8);

                if (_length <= 0)
                {
                    _lastBlock = 0;
                    _currentBlock = 0;
                    _length = 0;
                }
                else if (isAppend)
                {
                    var stream = _fileStream.GetStream();
                    var l = _currentBlock;
                    while (l > 0)
                    {
                        _lastBlock = l;
                        stream.Seek(l, System.IO.SeekOrigin.Begin);

                        stream.Read(_blong, 0, 12);
                        l = BitConverter.ToInt64(_blong, 0);

                    }
                    _fileStream.Release(stream);

                }

            }


            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;
            //bool _isAppend;
            int _length, _startBlock, _bufferSize;
            //int? _state;
            long _currentBlock, _fileid, _lastBlock, _startFileBlock;
            public override long Length => _length;

            public override long Position
            {
                get => _fileid; set
                {
                    if (value == 0)
                    {
                        this.Flush();
                        var stream = _fileStream.GetStream();
                        try
                        {
                            stream.Seek(_fileid, System.IO.SeekOrigin.Begin);

                            stream.Read(_blong, 0, 12);

                            _startFileBlock = BitConverter.ToInt64(_blong, 0);

                            _currentBlock = _startFileBlock;
                            _startBlock = 0;

                        }
                        finally
                        {
                            _fileStream.Release(stream);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            public override void Flush()
            {
                if (_bufferSize > 0)
                {
                    Strategy(Array.Empty<byte>(), 0, 0);
                }
            }
            protected override void Dispose(bool disposing)
            {
                Flush();
                //System.Buffers.ArrayPool<byte>.Shared.Return(_blong);
                _buffers = null;
                base.Dispose(disposing);

            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                int _osize = 0;
                int endIndex = offset + count;

                if (_currentBlock > 0)
                {
                    var stream = _fileStream.GetStream();
                    try
                    {
                        stream.Seek(_currentBlock, System.IO.SeekOrigin.Begin);

                        stream.Read(_blong, 0, 12);

                        var size = BitConverter.ToInt32(_blong, 8) - _startBlock;
                        if (size > 0 && _startBlock > 0)
                        {
                            stream.Seek(_startBlock, SeekOrigin.Current);
                        }
                        while (offset < endIndex)
                        {
                            if (size == 0)
                            {
                                var next = BitConverter.ToInt64(_blong, 0);

                                if (next == 0)
                                {
                                    return _osize;
                                }
                                else
                                {
                                    stream.Seek(next, System.IO.SeekOrigin.Begin);
                                    stream.Read(_blong, 0, 12);
                                    _currentBlock = next;
                                    _startBlock = 0;
                                    size = BitConverter.ToInt32(_blong, 8);// - _startBlock;
                                }
                            }

                            if (offset + size < endIndex)
                            {
                                _osize += stream.Read(buffer, offset, size);
                                offset += size;
                                _startBlock += size;
                                size = 0;
                            }
                            else
                            {
                                int c = endIndex - offset;

                                _osize += stream.Read(buffer, offset, c);

                                offset += c;
                                _startBlock += c;

                            }

                        }
                    }
                    finally
                    {
                        _fileStream.Release(stream);
                    }
                }
                return _osize;
                //_fileStream.Append
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                if (offset == 0 && origin == SeekOrigin.Begin)
                {
                    this.Flush();
                    var stream = _fileStream.GetStream();
                    try
                    {
                        stream.Seek(_fileid, System.IO.SeekOrigin.Begin);

                        stream.Read(_blong, 0, 12);

                        _startFileBlock = BitConverter.ToInt64(_blong, 0);

                        _currentBlock = _startFileBlock;
                        _startBlock = 0;

                    }
                    finally
                    {
                        _fileStream.Release(stream);
                    }
                    return _fileid;
                }
                else
                {
                    throw new NotSupportedException();
                }

            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            void Strategy(byte[] buffer, int offset, int count)
            {

                int l = _bufferSize + count + 12;
                if (l > 12)
                {
                    lock (_fileStream.mappedFile)
                    {
                        var stream = _fileStream.mappedFile;//.GetStream();

                        long nLastBlock;
                        if (_lastBlock == 0)
                        {
                            nLastBlock = stream.Seek(l + 8, SeekOrigin.End) - l;
                            stream.Write(HttpMimeBody.HeaderEnd, 0, 4);
                            stream.Seek(nLastBlock - 8, SeekOrigin.Begin);
                            BitConverter.TryWriteBytes(_blong, _startFileBlock);
                            stream.Write(_blong, 0, 8);
                            _length = 0;

                        }
                        else
                        {
                            nLastBlock = stream.Seek(l, SeekOrigin.End) - l;
                            stream.Write(HttpMimeBody.HeaderEnd, 0, 4);
                            stream.Seek(nLastBlock, SeekOrigin.Begin);
                        }



                        Array.Clear(_blong, 0, 12);

                        BitConverter.TryWriteBytes(new Span<byte>(_blong, 8, 4), _bufferSize + count);

                        stream.Write(_blong, 0, 12);
                        stream.Write(_buffers, 0, _bufferSize);
                        stream.Write(buffer, offset, count);
                        _length += _bufferSize + count;
                        _bufferSize = 0;

                        if (_lastBlock == 0)
                        {
                            BitConverter.TryWriteBytes(_blong, nLastBlock);
                            stream.Seek(_fileid, System.IO.SeekOrigin.Begin);
                            stream.Write(_blong, 0, 12);
                        }
                        else
                        {
                            stream.Seek(_lastBlock, SeekOrigin.Begin);
                            BitConverter.TryWriteBytes(_blong, nLastBlock);
                            stream.Write(_blong, 0, 8);

                            BitConverter.TryWriteBytes(_blong, _length);
                            stream.Seek(_fileid + 8, System.IO.SeekOrigin.Begin);
                            stream.Write(_blong, 0, 4);
                        }
                        _lastBlock = nLastBlock;



                        stream.Flush();
                    }
                }
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_buffers == null)
                {
                    _buffers = new byte[4096];
                    _bufferSize = 0;
                }
                if (_bufferSize + count < _buffers.Length)
                {
                    Array.Copy(buffer, offset, _buffers, _bufferSize, count);
                    _bufferSize += count;
                }
                else
                {
                    Strategy(buffer, offset, count);
                }
            }
        }

        ConcurrentQueue<FileStream> fileStreams = new ConcurrentQueue<FileStream>();
        Semaphore sem = new Semaphore(5, 5);
        int _length = 0;
        const int IndexBlockLength = 0xffff;
        String _path;
        FileStream mappedFile;
        public FileBlock(string path)
        {
            this._path = path;
            mappedFile = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (mappedFile.Length == 0)
            {
                mappedFile.Seek(IndexBlockLength * 9, System.IO.SeekOrigin.End);
                mappedFile.Write(HttpMimeBody.HeaderEnd, 0, 4);
                mappedFile.Flush();

            }
            else
            {
                this.Size(mappedFile, 0);
            }
            //fileStreams.Enqueue(mappedFile);
        }
        int Delete(Stream stream, long indexP, int indexBlock, int blockIndex, Span<byte> value)
        {
            var row = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
            var blong = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            try
            {
                int count = 0;
                for (; indexBlock < IndexBlockLength; indexBlock++)
                {
                    stream.Seek(indexBlock * 9 + indexP, System.IO.SeekOrigin.Begin);
                    stream.Read(blong, 0, 9);
                    var p2 = BitConverter.ToInt64(blong, 0);
                    if (p2 > 0)
                    {
                        int b = blong[8] + 1;
                        while (blockIndex < b)
                        {
                            stream.Seek(blockIndex * 256 + p2, System.IO.SeekOrigin.Begin);
                            var lay = stream.Read(row, 0, 256);

                            int order = Compare(row, 13, row[12], value);
                            if (order >= 0)
                            {
                                return count;
                            }
                            else if (row[8] > 127)
                            {
                                row[8] += 128;
                                stream.Seek(-256, System.IO.SeekOrigin.Current);
                                stream.Write(row, 0, 256);
                                count++;

                            }
                            blockIndex++;
                        }
                        blockIndex = 0;

                    }
                    else
                    {
                        return count;
                    }
                }
                if (indexBlock == IndexBlockLength)
                {

                    stream.Seek(IndexBlockLength * 9 + indexP, System.IO.SeekOrigin.Begin);
                    stream.Read(blong, 0, 8);
                    var p2 = BitConverter.ToInt64(blong, 0);
                    if (p2 > 0)
                    {
                        count += Delete(stream, p2, 0, 0, value);
                    }

                }
                return count;
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(row);
                System.Buffers.ArrayPool<byte>.Shared.Return(blong);
            }
        }

        FileStream GetStream()
        {
            //return mappedFile;
            sem.WaitOne();
            FileStream fileStream;
            if (fileStreams.TryDequeue(out fileStream))
            {
                return fileStream;
            }
            else
            {
                var mapFile = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return mapFile;
            }
        }
        void Release(FileStream fileStream)
        {
            fileStreams.Enqueue(fileStream);
            sem.Release();
        }
        public int Delete(string key)
        {
            var values = System.Buffers.ArrayPool<byte>.Shared.Rent(key.Length * 2);
            try
            {
                int l = System.Text.Encoding.UTF8.GetBytes(key, values);
                if (l > 243)
                {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        Array.Copy(md5.ComputeHash(values, 0, l), 0, values, 227, 16);
                    }
                    l = 243;
                }

                var value = new Span<byte>(values, 0, l);
                return Delete(new Span<byte>(values, 0, l));
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(values);
            }
        }
        int Delete(Span<byte> value)
        {
            lock (mappedFile)
            {
                var stream = mappedFile;// GetStream();

                var row = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
                var values = System.Buffers.ArrayPool<byte>.Shared.Rent(value.Length + 1);
                var blong = System.Buffers.ArrayPool<byte>.Shared.Rent(9);

                try
                {
                    long pindex = 0;
                    int indexBlock = 0;
                    int blockIndex = 0;
                    long spindex = 0;
                    int sindexBlock = 0;
                    int sblockIndex = 0;

                    int lo = 0;
                    int hi = _length - 1;
                    while (lo <= hi)
                    {
                        int i = lo + ((hi - lo) >> 1);
                        pindex = 0;
                        long offset = GetOffset(stream, i, ref pindex, out indexBlock, out blockIndex);

                        stream.Seek(offset, System.IO.SeekOrigin.Begin);
                        stream.Read(row, 0, 256);

                        int order = Compare(row, 13, row[12], value);

                        if (order == 0)
                        {
                            if (value[value.Length - 1] == 47)
                            {
                                if (row[8] > 127)
                                {
                                    row[8] += 128;
                                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                                    stream.Write(row, 0, 256);
                                }
                                spindex = pindex;
                                sindexBlock = indexBlock;
                                sblockIndex = blockIndex;
                                lo = i;
                                break;

                            }
                            else
                            {
                                if (row[8] > 127)
                                {
                                    row[8] += 128;
                                    stream.Seek(offset, System.IO.SeekOrigin.Begin);
                                    stream.Write(row, 0, 256);
                                    return 1;
                                }
                                return 0;
                            }
                        }
                        if (order < 0)
                        {
                            spindex = pindex;
                            sindexBlock = indexBlock;
                            sblockIndex = blockIndex;
                            lo = i + 1;
                        }
                        else
                        {
                            hi = i - 1;
                        }
                    }
                    switch (value[value.Length - 1])
                    {
                        case 47:
                            value[value.Length - 1]++;
                            break;
                        default:
                            return 0;
                    }

                    return Delete(stream, spindex, sindexBlock, sblockIndex, value);
                }
                finally
                {
                    stream.Flush();
                    //Release(stream);

                    System.Buffers.ArrayPool<byte>.Shared.Return(row);
                    System.Buffers.ArrayPool<byte>.Shared.Return(values);
                    System.Buffers.ArrayPool<byte>.Shared.Return(blong);
                }
            }
        }
        public Stream Put(string key, bool isAppend)
        {
            var values = System.Buffers.ArrayPool<byte>.Shared.Rent(key.Length * 2);
            try
            {
                int l = System.Text.Encoding.UTF8.GetBytes(key, values);
                if (l > 243)
                {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        Array.Copy(md5.ComputeHash(values, 0, l), 0, values, 227, 16);
                    }
                    l = 243;
                }

                var value = new Span<byte>(values, 0, l);
                return Put(new Span<byte>(values, 0, l), isAppend);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(values);
            }
        }

        Stream Put(Span<byte> value, bool isAppend)
        {

            var row = System.Buffers.ArrayPool<byte>.Shared.Rent(256);

            long opindex = 0;
            int oindexBlock = 0;
            int oBlockIndex = 0;
            int lo = 0;
            int hi = _length - 1;

            long pindex = 0;
            int indexBlock = 0;
            int blockIndex = 0;
            lock (mappedFile)
            {
                var stream = mappedFile;// GetStream();
                try
                {

                    while (lo <= hi)
                    {
                        int i = lo + ((hi - lo) >> 1);
                        pindex = 0;
                        long offset = GetOffset(stream, i, ref pindex, out indexBlock, out blockIndex);

                        stream.Seek(offset, System.IO.SeekOrigin.Begin);
                        var lay = stream.Read(row, 0, 256);

                        int order = Compare(row, 13, row[12], value);

                        if (order == 0)
                        {
                            return new BlockStream(this, offset, row, isAppend);
                        }
                        if (order < 0)
                        {
                            opindex = pindex;
                            oindexBlock = indexBlock;
                            oBlockIndex = blockIndex;

                            lo = i + 1;
                        }
                        else
                        {
                            hi = i - 1;
                        }
                    }
                    Array.Clear(row, 0, row.Length);
                    var span = new Span<byte>(row, 13, 243);
                    span.Fill(0);
                    value.CopyTo(span);



                    row[12] = (byte)value.Length;

                    long fileid = Insert(stream, opindex, oindexBlock, oBlockIndex, new Span<byte>(row, 0, 256));
                    if (fileid == -1)
                    {
                        throw new NotSupportedException();
                    }
                    return new BlockStream(this, fileid, row, isAppend);


                }
                finally
                {
                    stream.Flush();

                    System.Buffers.ArrayPool<byte>.Shared.Return(row);


                }
            }
        }

        public bool TryGet(string key, out Stream stream)
        {
            var values = System.Buffers.ArrayPool<byte>.Shared.Rent(key.Length * 2);
            try
            {
                int l = System.Text.Encoding.UTF8.GetBytes(key, values);
                if (l > 243)
                {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        Array.Copy(md5.ComputeHash(values, 0, l), 0, values, 227, 16);
                    }
                    l = 243;
                }

                var value = new Span<byte>(values, 0, l);
                return TryGet(new Span<byte>(values, 0, l), out stream);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(values);
            }
        }

        bool TryGet(Span<byte> value, out Stream stream)
        {
            stream = null;
            var fileStream = this.GetStream();// ;//.CreateViewStream();
            var bs = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
            try
            {
                int lo = 0;
                int hi = 0 + _length - 1;

                long pindex = 0;

                while (lo <= hi)
                {
                    int i = lo + ((hi - lo) >> 1);
                    pindex = 0;
                    long offset = GetOffset(fileStream, i, ref pindex, out var _, out var _);
                    fileStream.Seek(offset, System.IO.SeekOrigin.Begin);
                    var lay = fileStream.Read(bs, 0, 256);

                    int order = Compare(bs, 13, bs[12], value);

                    if (order == 0)
                    {
                        stream = new BlockStream(this, offset, bs, false);

                        return true;
                    }
                    if (order < 0)
                    {
                        lo = i + 1;
                    }
                    else
                    {
                        hi = i - 1;
                    }
                }
                return false;

            }
            finally
            {
                fileStream.Flush();
                Release(fileStream);
                System.Buffers.ArrayPool<byte>.Shared.Return(bs);

            }
        }
        void Size(Stream stream, long p)
        {
            var blong = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            try
            {

                int i = 0;
                for (; i < IndexBlockLength; i++)
                {
                    stream.Seek(i * 9 + p, System.IO.SeekOrigin.Begin);

                    stream.Read(blong, 0, 9);
                    var p2 = BitConverter.ToInt64(blong, 0);

                    if (p2 > 0)
                    {
                        _length += blong[8] + 1;

                    }
                    else
                    {
                        return;
                    }

                }
                if (i == IndexBlockLength)
                {
                    stream.Seek(i * 9 + p, System.IO.SeekOrigin.Begin);
                    stream.Read(blong, 0, 8);
                    var p2 = BitConverter.ToInt64(blong, 0);
                    if (p2 > 0)
                    {
                        Size(stream, p2);
                    }

                }
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(blong);
            }
        }
        long GetOffset(Stream stream, int index, ref long p, out int indexBlock, out int blockIndex)
        {
            var blong = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            try
            {
                int len = 0;
                indexBlock = 0;

                for (; indexBlock < IndexBlockLength; indexBlock++)
                {
                    stream.Seek(indexBlock * 9 + p, System.IO.SeekOrigin.Begin);
                    stream.Read(blong, 0, 9);
                    var p2 = BitConverter.ToInt64(blong, 0);
                    if (p2 > 0)
                    {
                        int b = blong[8] + 1;

                        if (index < (len + b))
                        {
                            blockIndex = (index - len);
                            return p2 + 256 * blockIndex;
                        }
                        else
                        {
                            len += b;
                        }

                    }
                    else
                    {
                        blockIndex = 0;
                        return -1;
                    }
                }
                if (indexBlock == IndexBlockLength)
                {

                    stream.Seek(IndexBlockLength * 9 + p, System.IO.SeekOrigin.Begin);
                    stream.Read(blong, 0, 8);
                    var p2 = BitConverter.ToInt64(blong, 0);
                    if (p2 > 0)
                    {
                        p = p2;
                        return GetOffset(stream, index - len, ref p, out indexBlock, out blockIndex);
                    }

                }
                blockIndex = 0;
                return -1;
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(blong);
            }
        }
        public void Close()
        {
            //mappedFile.Close();
            FileStream mappedFile;//
            while (fileStreams.TryDequeue(out mappedFile))
            {
                mappedFile.Close();
            }
            sem.Dispose();
        }

        void PutIndex(Stream stream, int index, long p, byte[] indexs)
        {
            var blong = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            var blong2 = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            Array.Copy(indexs, blong2, 9);

            stream.Seek(index * 9 + p, System.IO.SeekOrigin.Begin);
            do
            {
                stream.Read(blong, 0, 9);
                stream.Seek(-9, System.IO.SeekOrigin.Current);
                stream.Write(blong2, 0, 9);
                var p2 = BitConverter.ToInt64(blong, 0);
                if (p2 > 0)
                {
                    blong2 = blong;
                }
                else
                {
                    return;
                }
                index++;
            }
            while (index < IndexBlockLength);


            if (index == IndexBlockLength)
            {
                stream.Seek(index * 9 + p, System.IO.SeekOrigin.Begin);
                stream.Read(blong, 0, 8);
                var p2 = BitConverter.ToInt64(blong, 0);
                if (p2 > 0)
                {
                    PutIndex(stream, 0, p2, blong2);
                }
                else
                {
                    long position = stream.Seek(IndexBlockLength * 9, System.IO.SeekOrigin.End) - (IndexBlockLength * 9);
                    stream.Write(HttpMimeBody.HeaderEnd, 0, 4);

                    var span = new Span<byte>(blong, 0, 9);
                    BitConverter.TryWriteBytes(span, position);
                    stream.Seek(index * 9 + p, System.IO.SeekOrigin.Begin);
                    stream.Write(blong, 0, 9);
                    stream.Seek(position, System.IO.SeekOrigin.Begin);
                    stream.Write(blong2, 0, 9);
                }

            }
        }

        long Insert(Stream stream, long indexP, int indexBlock, int blockIndex, Span<byte> value)
        {
            var blong = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            var insert = System.Buffers.ArrayPool<byte>.Shared.Rent(9);
            var row = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
            var row2 = System.Buffers.ArrayPool<byte>.Shared.Rent(256);
            try
            {

                stream.Seek(indexBlock * 9 + indexP, System.IO.SeekOrigin.Begin);
                stream.Read(blong, 0, 9);
                var p2 = BitConverter.ToInt64(blong, 0);


                if (p2 > 0)
                {
                    int b = blong[8] + 1;

                    if (b == 256)
                    {
                        long position = stream.Seek(IndexBlockLength, System.IO.SeekOrigin.End) - IndexBlockLength;


                        stream.Write(HttpMimeBody.HeaderEnd, 0, 4);

                        for (var c = 0; c < 128; c++)
                        {
                            stream.Seek(p2 + 256 * (c + 128), System.IO.SeekOrigin.Begin);
                            stream.Read(row, 0, 256);

                            stream.Seek(position + 256 * c, System.IO.SeekOrigin.Begin);
                            stream.Write(row, 0, 256);
                        }
                        BitConverter.TryWriteBytes(new Span<byte>(insert, 0, 9), position);

                        insert[8] = 127;
                        PutIndex(stream, indexBlock + 1, indexP, insert);

                        stream.Seek(indexBlock * 9 + indexP, System.IO.SeekOrigin.Begin);
                        blong[8] = 127;
                        stream.Write(blong, 0, 9);

                        b = 128;
                        if (blockIndex > 127)
                        {
                            p2 = position;
                            blockIndex -= 128;
                        }
                    }

                    var span = new Span<byte>(row2, 0, 256);
                    span.Fill(0);

                    value.CopyTo(span);
                    blockIndex++;
                    stream.Seek(p2 + blockIndex * 256, System.IO.SeekOrigin.Begin);
                    while (b > blockIndex)
                    {
                        stream.Read(row, 0, 256);

                        var cuur = stream.Seek(-256, System.IO.SeekOrigin.Current);

                        stream.Write(row2, 0, 256);
                        if (row[8] >= 128)
                        {
                            return cuur;// stream.Position - 256;
                        }
                        else
                        {
                            row2 = row;
                        }
                        blockIndex++;
                    }
                    var insertP = stream.Position;
                    stream.Write(row2, 0, 256);

                    stream.Seek(indexBlock * 9 + indexP + 8, System.IO.SeekOrigin.Begin);
                    blong[8]++;
                    _length++;
                    stream.Write(blong, 8, 1);
                    return insertP;

                }
                else if (indexP == 0 && indexBlock == 0)
                {

                    stream.Seek(IndexBlockLength * 9, System.IO.SeekOrigin.Begin);
                    stream.Write(HttpMimeBody.HeaderEnd, 0, 4);

                    long position = stream.Seek(IndexBlockLength, System.IO.SeekOrigin.End) - IndexBlockLength;

                    stream.Write(HttpMimeBody.HeaderEnd, 0, 4);


                    var span = new Span<byte>(blong, 0, 9);
                    BitConverter.TryWriteBytes(span, position);
                    blong[8] = 0;

                    stream.Seek(0, System.IO.SeekOrigin.Begin);
                    stream.Write(blong, 0, 9);

                    var span2 = new Span<byte>(row2, 0, 256);
                    span2.Fill(0);

                    value.CopyTo(span2);

                    stream.Seek(position, System.IO.SeekOrigin.Begin);
                    stream.Write(row2, 0, 256);
                    _length++;
                    return position;
                }
                return -1;
            }
            finally
            {

                System.Buffers.ArrayPool<byte>.Shared.Return(blong);
                System.Buffers.ArrayPool<byte>.Shared.Return(insert);
                System.Buffers.ArrayPool<byte>.Shared.Return(row);
                System.Buffers.ArrayPool<byte>.Shared.Return(row2);
            }
        }


        int Compare(byte[] xbufer, int xindex, int xsize, Span<byte> y)
        {
            if (xsize > y.Length)
            {

                for (var i = 0; i < y.Length; i++)
                {
                    var v = xbufer[xindex + i].CompareTo(y[i]);
                    if (v != 0)
                    {
                        return v;
                    }
                }
                return 1;
            }
            else
            {
                for (var i = 0; i < xsize; i++)
                {
                    var v = xbufer[xindex + i].CompareTo(y[i]);
                    if (v != 0)
                    {
                        return v;
                    }
                }
                if (xsize < y.Length)
                {
                    return -1;
                }
            }
            return 0;
        }

    }
}

