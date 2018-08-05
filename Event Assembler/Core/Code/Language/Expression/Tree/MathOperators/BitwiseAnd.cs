// -----------------------------------------------------------------------
// <copyright file="BitwiseAnd.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree.MathOperators
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class BitwiseAnd<T> : BinaryOperator<T>
    {
        public BitwiseAnd(IExpression<T> first, IExpression<T> second, FilePosition position)
            : base(first, second, EAExpressionType.AND, position)
        {

        }
    }
}
