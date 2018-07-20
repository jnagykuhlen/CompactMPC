using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public static class BinaryFormatHelper
    {
        public static string ToBinaryString(this IEnumerable<bool> bits)
        {
            return new string(bits.Select(bit => bit ? '1' : '0').ToArray());
        }

        public static string ToBinaryString(this BitArray bits)
        {
            return ToBinaryString(bits.Cast<bool>());
        }
    }
}
