// -----------------------------------------------------------------------
// <copyright file="Minus.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree.MathOperators
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class Minus<T> : BinaryOperator<T>
    {
        public Minus(IExpression<T> first, IExpression<T> second, FilePosition position)
            : base(first, second, EAExpressionType.Minus, position)
        {

        }
    }
}
