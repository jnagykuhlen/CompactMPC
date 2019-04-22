using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public static class BufferHelper
    {
        public static byte[] Combine(params byte[][] buffers)
        {
            byte[] result = new byte[buffers.Sum(buffer => buffer.Length)];

            int offset = 0;
            for (int i = 0; i < buffers.Length; ++i)
            {
                byte[] buffer = buffers[i];
                Buffer.BlockCopy(buffer, 0, result, offset, buffer.Length);
                offset += buffer.Length;
            }

            return result;
        }

        public static byte[] Combine(byte[] buffer, params int[] ids)
        {
            byte[] result = new byte[buffer.Length + 4 * ids.Length];
            Buffer.BlockCopy(buffer, 0, result, 0, buffer.Length);
            
            int offset = buffer.Length;
            for (int i = 0; i < ids.Length; ++i)
            {
                int id = ids[i];
                result[offset + 0] = (byte)id;
                result[offset + 1] = (byte)(id >> 8);
                result[offset + 2] = (byte)(id >> 16);
                result[offset + 3] = (byte)(id >> 24);
                offset += 4;
            }

            return result;
        }
    }
}
