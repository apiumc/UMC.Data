using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace UMC.Data
{

    public class CacheSet<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : class
    {
        TKey[] _keys;
        TValue[] _values;
        object _lock = new object();
        public CacheSet()
        {
            _keys = new TKey[0];
            _values = new TValue[0];
        }
        public void Clear()
        {
            Array.Clear(this._keys, 0, this._size);
            Array.Clear(this._values, 0, this._size);
            this._size = 0;
            _count = 0;
        }


        public int Count => _count;
        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
            {
                var index = Array.BinarySearch(_keys, 0, _size, key);
                if (index > -1)
                {
                    value = _values[index];
                    return value != null;
                }
            }
            value = default(TValue);
            return false;
        }

        public void Search(TKey min, TKey max, Action<TValue> action)
        {

            var start = Array.BinarySearch(_keys, 0, _size, min);
            var end = Array.BinarySearch(_keys, 0, _size, max);
            var start1 = start;
            if (start < 0)
            {
                start1 = ~start;
            }
            var end1 = end;
            if (end1 < 0)
            {
                end1 = ~end;
            }
            else
            {
                end1++;
            }
            for (; start1 < end1; start1++)
            {
                var v = _values[start1];
                if (v != null)
                {
                    action(v);
                }
            }
        }
        public void Put(TKey key, TValue value)
        {
            lock (_lock)
            {
                var index = Array.BinarySearch(_keys, 0, _size, key);
                if (index > -1)
                {
                    if (this._values[index] == null)
                    {
                        _count++;
                    }
                    _values[index] = value;
                }
                else
                {

                    Insert(~index, key, value);
                }
            }
        }
        int _size = 0, _count = 0;
        private void Insert(int index, TKey key, TValue value)
        {
            if (index < this._keys.Length && this._values[index] == null)
            {
                this._keys[index] = key;
                this._values[index] = value;

                _count++;
            }
            else
            {
                if (this._size == this._keys.Length)
                {
                    TKey[] destinationArray = new TKey[this._size + 1];
                    TValue[] localArray2 = new TValue[this._size + 1];
                    if (this._size > 0)
                    {
                        Array.Copy(this._keys, 0, destinationArray, 0, this._size);
                        Array.Copy(this._values, 0, localArray2, 0, this._size);
                    }
                    this._keys = destinationArray;
                    this._values = localArray2;

                }

                if (index < this._size)
                {
                    Array.Copy(this._keys, index, this._keys, index + 1, this._size - index);
                    Array.Copy(this._values, index, this._values, index + 1, this._size - index);
                }
                this._keys[index] = key;
                this._values[index] = value;
                this._size++;
                _count++;

            }

        }


        public void Delete(TKey key)
        {
            lock (_lock)
            {
                var index = Array.BinarySearch(_keys, 0, _size, key);
                if (index > -1)
                {
                    this._values[index] = null;
                    _count--;

                }
            }
        }
        public void Values(Action<TValue> action)
        {
            for (var i = 0; i < this._size; i++)
            {
                var v = _values[i];
                if (v != null)
                {
                    action(v);
                }
            }
        }

    }



}
