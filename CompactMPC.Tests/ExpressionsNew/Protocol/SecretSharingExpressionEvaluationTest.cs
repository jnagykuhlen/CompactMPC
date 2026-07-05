using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.ExpressionsNew.Protocol
{
    [TestClass]
    public class SecretSharingExpressionEvaluationTest
    {
        // private readonly SecretSharingExpressionEvaluation _evaluation = new SecretSharingExpressionEvaluation();

        [TestMethod]
        public void TestEvaluate()
        {
            IntegerExpression a = IntegerExpression.AssignableUpTo(120);
            IntegerExpression b = IntegerExpression.AssignableUpTo(140);
            BooleanExpression c = BooleanExpression.Assignable();
            IntegerExpression d = IntegerExpression.Sum(a, b, IntegerExpression.FromBoolean(c));
            BooleanExpression e = b > a;
            BooleanExpression f = c && e;
            
            // _evaluation.
            
            // CompiledCircuit compiledCircuit = CompiledCircuit.From(d, e, f);
            // _evaluator.EvaluateAsync(compiledCircuit, )
        }
    }
    
    
}