﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.ExpressionsNew
{
    public class InputBinding<T> : IInputBinding
    {
        private readonly Expression<T> _expression;
        private readonly T _value;

        public InputBinding(Expression<T> expression, T value)
        {
            if (expression.Wires.Any(wire => wire.IsConstant))
                throw new ArgumentException("Cannot create input binding for constant expression.", nameof(expression));
            
            _expression = expression;
            _value = value;
        }

        public IReadOnlyList<Wire> Wires
        {
            get
            {
                return _expression.Wires;
            }
        }
        
        public IReadOnlyList<Bit> Bits
        {
            get
            {
                return _expression.Converter.ToBits(_value, _expression.Wires.Count);
            }
        }
    }
}