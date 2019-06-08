using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC.Virtualization
{
    public class LocalLogicProcessor : VirtualLogicProcessor<bool>
    {
        protected override Task<bool> Unvirtualize(Task<bool> input, GateContext context)
        {
            return input;
        }

        protected override Task<bool> Virtualize(Task<bool> value, GateContext context)
        {
            return value;
        }

        protected override async Task<bool> And(Task<bool> leftInput, Task<bool> rightInput, GateContext context)
        {
            return (await leftInput) && (await rightInput);
        }
        
        protected override async Task<bool> Xor(Task<bool> leftInput, Task<bool> rightInput, GateContext context)
        {
            return (await leftInput) ^ (await rightInput);
        }

        protected override async Task<bool> Not(Task<bool> input, GateContext context)
        {
            return !(await input);
        }

        protected override bool CreateZero()
        {
            return false;
        }

        protected override bool CreateOne()
        {
            return true;
        }
    }
}
