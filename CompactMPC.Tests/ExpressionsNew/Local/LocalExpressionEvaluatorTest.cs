using System;
using CompactMPC.Circuits;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew.Local
{
    [TestClass]
    public class LocalExpressionEvaluatorTest
    {
        private readonly LocalExpressionEvaluator _localExpressionEvaluator = new LocalExpressionEvaluator();

        [TestMethod]
        public void TestEvaluateFromInputs()
        {
            IntegerExpression a = IntegerExpression.FromInput(120);
            IntegerExpression b = IntegerExpression.FromInput(140);
            BooleanExpression c = BooleanExpression.FromInput();
            IntegerExpression d = IntegerExpression.Sum(a, b, IntegerExpression.FromBoolean(c));
            BooleanExpression e = b > a;
            BooleanExpression f = c && e;

            _localExpressionEvaluator.Evaluate(a, a.Bind(113)).Should().Be(113);
            _localExpressionEvaluator.Evaluate(d, a.Bind(42), b.Bind(137), c.Bind(true)).Should().Be(180);
            _localExpressionEvaluator.Evaluate(e, a.Bind(42), b.Bind(137), c.Bind(true)).Should().BeTrue();
            _localExpressionEvaluator.Evaluate(f, a.Bind(42), b.Bind(137), c.Bind(false)).Should().BeFalse();
        }

        [TestMethod]
        public void TestEvaluateWithMissingInputValues()
        {
            IntegerExpression a = IntegerExpression.FromInput(120);
            IntegerExpression b = IntegerExpression.FromInput(140);
            IntegerExpression c = a + b;

            Action evaluate = () => _localExpressionEvaluator.Evaluate(c, a.Bind(113));

            evaluate.Should().Throw<CircuitEvaluationException>();
        }

        [TestMethod]
        public void TestEvaluateWithConstants()
        {
            IntegerExpression input = IntegerExpression.FromInput(4);
            IntegerExpression constant = IntegerExpression.FromConstant(42);
            IntegerExpression sum = input + constant;

            _localExpressionEvaluator.Evaluate(constant).Should().Be(42);
            _localExpressionEvaluator.Evaluate(sum, input.Bind(3)).Should().Be(45);
        }

        [TestMethod]
        public void TestConstantWithConstantBinding()
        {
            IntegerExpression constant = IntegerExpression.FromConstant(42);

            Action evaluate = () => _localExpressionEvaluator.Evaluate(constant, constant.Bind(21));

            evaluate.Should().Throw<ArgumentException>();
        }
    }
}
