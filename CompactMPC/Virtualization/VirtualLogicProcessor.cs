using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace CompactMPC.Virtualization
{
    public abstract class VirtualLogicProcessor<T>
    {
        private int _nextGateId;
        private Task<T> _zero;
        private Task<T> _one;

        public VirtualLogicProcessor()
        {
            _nextGateId = 0;
            _zero = null;
            _one = null;
        }

        public Task<T> And(Task<T> leftInput, Task<T> rightInput)
        {
            if (leftInput == Zero || rightInput == Zero)
                return Zero;

            if (leftInput == One)
                return rightInput;

            if (rightInput == One)
                return leftInput;

            return And(leftInput, rightInput, RequestGateContext());
        }
        
        public Task<T> Xor(Task<T> leftInput, Task<T> rightInput)
        {
            if (leftInput == Zero)
                return rightInput;

            if (rightInput == Zero)
                return leftInput;

            if (leftInput == One)
                return Not(rightInput);

            if (rightInput == One)
                return Not(leftInput);

            return Xor(leftInput, rightInput, RequestGateContext());
        }
        
        public Task<T> Or(Task<T> leftInput, Task<T> rightInput)
        {
            return Not(And(Not(leftInput), Not(rightInput)));
        }

        public Task<T> Not(Task<T> input)
        {
            if (input == Zero)
                return One;

            if (input == One)
                return Zero;

            return Not(input, RequestGateContext());
        }

        public Task<T> Virtualize(Task<bool> value)
        {
            return Virtualize(value, RequestGateContext());
        }

        public Task<bool> Unvirtualize(Task<T> input)
        {
            return Unvirtualize(input, RequestGateContext());
        }

        private GateContext RequestGateContext()
        {
            return new GateContext(Interlocked.Increment(ref _nextGateId));
        }
        
        protected abstract Task<T> And(Task<T> leftInput, Task<T> rightInput, GateContext context);
        protected abstract Task<T> Xor(Task<T> leftInput, Task<T> rightInput, GateContext context);
        protected abstract Task<T> Not(Task<T> input, GateContext context);
        protected abstract Task<T> Virtualize(Task<bool> value, GateContext context);
        protected abstract Task<bool> Unvirtualize(Task<T> input, GateContext context);

        protected abstract T CreateZero();
        protected abstract T CreateOne();
        
        public Task<T> Zero
        {
            get
            {
                if (_zero == null)
                    _zero = Task.FromResult(CreateZero());

                return _zero;
            }
        }

        public Task<T> One
        {
            get
            {
                if (_one == null)
                    _one = Task.FromResult(CreateOne());

                return _one;
            }
        }
    }
}
