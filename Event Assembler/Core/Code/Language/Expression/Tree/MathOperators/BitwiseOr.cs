// -----------------------------------------------------------------------
// <copyright file="BitwiseOr.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree.MathOperators
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class BitwiseOr<T> : BinaryOperator<T>
    {
        public BitwiseOr(IExpression<T> first, IExpression<T> second, FilePosition position)
            : base(first, second, EAExpressionType.OR, position)
        {

        }
    }
}
