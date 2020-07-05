using System;
using CompactMPC.Circuits;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew
{
    [TestClass]
    public class ExpressionTest
    {
        [TestMethod]
        public void TestEvaluateFromInputs()
        {
            IntegerExpression a = IntegerExpression.FromInput(120);
            IntegerExpression b = IntegerExpression.FromInput(140);
            BooleanExpression c = BooleanExpression.FromInput();
            IntegerExpression d = IntegerExpression.Sum(a, b, IntegerExpression.FromBoolean(c));
            BooleanExpression e = b > a;
            BooleanExpression f = c && e;

            a.Evaluate(a.Bind(113)).Should().Be(113);
            d.Evaluate(a.Bind(42), b.Bind(137), c.Bind(true)).Should().Be(180);
            e.Evaluate(a.Bind(42), b.Bind(137), c.Bind(true)).Should().BeTrue();
            f.Evaluate(a.Bind(42), b.Bind(137), c.Bind(false)).Should().BeFalse();
        }
        
        [TestMethod]
        public void TestEvaluateWithMissingInputValues()
        {
            IntegerExpression a = IntegerExpression.FromInput(120);
            IntegerExpression b = IntegerExpression.FromInput(140);
            IntegerExpression c = a + b;

            Action evaluate = () => c.Evaluate(a.Bind(113));
            
            evaluate.Should().Throw<CircuitEvaluationException>();
        }
        
        [TestMethod]
        public void TestEvaluateWithConstants()
        {
            IntegerExpression input = IntegerExpression.FromInput(4);
            IntegerExpression constant = IntegerExpression.FromConstant(42);
            IntegerExpression sum = input + constant;

            constant.Evaluate().Should().Be(42);
            sum.Evaluate(input.Bind(3)).Should().Be(45);
        }
        
        [TestMethod]
        public void TestConstantWithConstantBinding()
        {
            IntegerExpression constant = IntegerExpression.FromConstant(42);
            
            Action evaluate = () => constant.Evaluate(constant.Bind(21));
            
            evaluate.Should().Throw<ArgumentException>();
        }
    }
}
