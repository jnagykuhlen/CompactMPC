using CompactMPC.ExpressionsNew;
using CompactMPC.Protocol.New;

namespace CompactMPC.Protocol.Internal;

public record OutputExpressionDescription(IOutput<IExpression> Output, IExpression Expression);
