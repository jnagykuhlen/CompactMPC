using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC.SampleCircuits
{
    public static class CounterHelper
    {
        public static int RequiredNumberOfBits(int maximumValue)
        {
            int numberOfBits = 1;
            while ((1 << numberOfBits) <= maximumValue)
                numberOfBits++;

            return numberOfBits;
        }
    }
}
