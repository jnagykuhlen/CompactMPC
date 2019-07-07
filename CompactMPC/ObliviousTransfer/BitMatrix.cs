using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC.ObliviousTransfer
{
    internal class BitMatrix
    {
        private BitArray _values;

        public uint Rows { get; private set; }
        public uint Cols { get; private set; }
        public int Length { get { return (int)(UnsingedLength); } }
        public uint UnsingedLength { get { return Rows * Cols; } }

        public BitMatrix(uint rows, uint cols)
        {
            Rows = rows;
            Cols = cols;
            _values = new BitArray(Length);
        }

        public BitMatrix(uint rows, uint cols, BitArray values) : this(rows, cols)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Length)
                throw new ArgumentException("Argument must be of the correct dimensions!", nameof(values));


            _values = values.Clone();
        }

        private int GetValuesIndex(uint row, uint col)
        {
            return (int)(row * Cols + col);
        }

        public Bit this[uint row, uint col]
        {
            get
            {
                if (row > Rows)
                    throw new ArgumentOutOfRangeException(nameof(row));
                if (col > Cols)
                    throw new ArgumentOutOfRangeException(nameof(col));
                return _values[GetValuesIndex(row, col)];
            }
            set
            {
                if (row > Rows)
                    throw new ArgumentOutOfRangeException(nameof(row));
                if (col > Cols)
                    throw new ArgumentOutOfRangeException(nameof(col));
                _values[GetValuesIndex(row, col)] = value;
            }
        }

        public BitArray GetRow(uint row)
        {
            if (row > Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            BitArray result = new BitArray((int)Cols);
            for (uint j = 0; j < Cols; ++j)
            {
                result[(int)j] = _values[GetValuesIndex(row, j)];
            }
            return result;
        }

        public void SetRow(uint row, BitArray values)
        {
            if (row > Rows)
                throw new ArgumentOutOfRangeException(nameof(row));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Cols)
                throw new ArgumentException("Provided argument must match the number of columns.", nameof(values));
            for (uint j = 0; j < Cols; ++j)
            {
                _values[GetValuesIndex(row, j)] = values[(int)j];
            }
        }

        public BitArray GetColumn(uint col)
        {
            if (col > Cols)
                throw new ArgumentOutOfRangeException(nameof(col));
            BitArray result = new BitArray((int)Rows);
            for (uint i = 0; i < Rows; ++i)
            {
                result[(int)i] = _values[GetValuesIndex(i, col)];
            }
            return result;
        }

        public void SetColumn(uint col, BitArray values)
        {
            if (col > Cols)
                throw new ArgumentOutOfRangeException(nameof(col));
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != Rows)
                throw new ArgumentException("Provided argument must match the number of columns.", nameof(values));
            for (uint i = 0; i < Rows; ++i)
            {
                _values[GetValuesIndex(i, col)] = values[(int)i];
            }
        }

        public BitMatrix Transposed
        {
            get
            {
                BitMatrix transposed = new BitMatrix(Cols, Rows);
                for (uint i = 0; i < Rows; ++i)
                {
                    transposed.SetColumn(i, this.GetRow(i));
                }
                return transposed;
            }
        }
    }
}
