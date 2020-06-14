using System;
using CompactMPC.Circuits;
using CompactMPC.ExpressionsNew;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests.ExpressionsNew
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

            int aValue = a.Evaluate(a.Bind(113));
            Assert.AreEqual(113, aValue);
            
            int dValue = d.Evaluate(a.Bind(42), b.Bind(137), c.Bind(true));
            Assert.AreEqual(180, dValue);

            bool eValue = e.Evaluate(a.Bind(42), b.Bind(137), c.Bind(true));
            Assert.IsTrue(eValue);
            
            bool fValue = f.Evaluate(a.Bind(42), b.Bind(137), c.Bind(false));
            Assert.IsFalse(fValue);
        }
        
        [TestMethod]
        [ExpectedException(typeof(CircuitEvaluationException))]
        public void TestEvaluateWithMissingInputValues()
        {
            IntegerExpression a = IntegerExpression.FromInput(120);
            IntegerExpression b = IntegerExpression.FromInput(140);
            IntegerExpression c = a + b;

            int cValue = c.Evaluate(a.Bind(113));
            Assert.AreEqual(113, cValue);
        }
        
        [TestMethod]
        public void TestEvaluateWithConstants()
        {
            IntegerExpression input = IntegerExpression.FromInput(4);
            IntegerExpression constant = IntegerExpression.FromConstant(42);
            IntegerExpression sum = input + constant;

            int sumValue = sum.Evaluate(input.Bind(3));
            Assert.AreEqual(45, sumValue);

            int constantValue = constant.Evaluate();
            Assert.AreEqual(42, constantValue);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestConstantWithConstantBinding()
        {
            IntegerExpression constant = IntegerExpression.FromConstant(42);
            constant.Evaluate(constant.Bind(21));
        }
    }
}
