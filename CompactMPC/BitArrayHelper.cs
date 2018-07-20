using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace CompactMPC
{
    public static class BitArrayHelper
    {
        public static BitArray FromBinaryString(string bits)
        {
            BitArray result = new BitArray(bits.Length);
            for (int i = 0; i < bits.Length; ++i)
            {
                if (bits[i] != '0' && bits[i] != '1')
                    throw new ArgumentException("Binary string is only allowed to contain characters 0 and 1.", nameof(bits));
                
                result[i] = bits[i] == '1';
            }

            return result;
        }

        public static byte[] ToBytes(BitArray bitArray)
        {
            byte[] result = new byte[RequiredBytes(bitArray.Length)];
            for(int bitIndex = 0; bitIndex < bitArray.Length; ++bitIndex)
            {
                if (bitArray[bitIndex])
                {
                    int byteIndex = bitIndex / 8;
                    result[byteIndex] |= (byte)(1 << (bitIndex % 8));
                }
            }
            
            return result;
        }

        public static BitArray FromBytes(byte[] data, int numberOfBits)
        {
            return new BitArray(data) { Length = numberOfBits };
        }

        public static void WriteToStream(BitArray bitArray, Stream stream)
        {
            byte[] data = ToBytes(bitArray);
            stream.Write(BitConverter.GetBytes(bitArray.Length), 0, sizeof(int));
            stream.Write(data, 0, data.Length);
        }

        public static BitArray ReadFromStream(Stream stream)
        {
            byte[] numberOfBitsBuffer = stream.Read(sizeof(int));
            int numberOfBits = BitConverter.ToInt32(numberOfBitsBuffer, 0);

            byte[] data = stream.Read(RequiredBytes(numberOfBits));
            return FromBytes(data, numberOfBits);

            /*
            using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                int numberOfBits = reader.ReadInt32();
                byte[] data = reader.ReadBytes((numberOfBits - 1) / 8 + 1);
                return FromBytes(data, numberOfBits);
            }
            */
        }

        public static int RequiredBytes(int numberOfBits)
        {
            return (numberOfBits - 1) / 8 + 1;
        }
    }
}
