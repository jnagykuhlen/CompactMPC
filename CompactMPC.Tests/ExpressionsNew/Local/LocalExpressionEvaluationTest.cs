using System;
using CompactMPC.Circuits;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew.Local
{
    [TestClass]
    public class LocalExpressionEvaluationTest
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

            new LocalExpressionEvaluation().Input(a, 113).ExecuteFor(a)
                .Should().Be(113);

            new LocalExpressionEvaluation()
                .Input(a, 42).Input(b, 137).Input(c, true)
                .Output(d).Output(e)
                .Execute()
                .Value(d, out int dValue)
                .Value(e, out bool eValue);

            dValue.Should().Be(180);
            eValue.Should().BeTrue();
            
            new LocalExpressionEvaluation().Input(a, 42).Input(b, 137).Input(c, false).ExecuteFor(f)
                .Should().BeFalse();
        }

        [TestMethod]
        public void TestEvaluateWithMissingInputValues()
        {
            IntegerExpression a = IntegerExpression.FromInput(120);
            IntegerExpression b = IntegerExpression.FromInput(140);
            IntegerExpression c = a + b;

            Action evaluate = () => new LocalExpressionEvaluation().Input(a, 113).ExecuteFor(c);

            evaluate.Should().Throw<CircuitEvaluationException>();
        }

        [TestMethod]
        public void TestEvaluateWithConstants()
        {
            IntegerExpression input = IntegerExpression.FromInput(4);
            IntegerExpression constant = IntegerExpression.FromConstant(42);
            IntegerExpression sum = input + constant;

            new LocalExpressionEvaluation().ExecuteFor(constant).Should().Be(42);
            new LocalExpressionEvaluation().Input(input, 3).ExecuteFor(sum).Should().Be(45);
        }

        [TestMethod]
        public void TestConstantWithConstantBinding()
        {
            IntegerExpression constant = IntegerExpression.FromConstant(42);

            Action evaluate = () => new LocalExpressionEvaluation().Input(constant, 21).Execute();

            evaluate.Should().Throw<ArgumentException>();
        }
    }
}
