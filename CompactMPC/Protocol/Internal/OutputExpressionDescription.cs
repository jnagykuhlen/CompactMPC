using CompactMPC.Expressions;

namespace CompactMPC.Protocol.Internal;

public record OutputExpressionDescription(IOutput<IExpression> Output, IExpression Expression);
