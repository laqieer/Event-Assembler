// -----------------------------------------------------------------------
// <copyright file="Division.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree.MathOperators
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class Division<T> : BinaryOperator<T>
    {
        public Division(IExpression<T> first, IExpression<T> second, FilePosition position)
            : base(first, second, EAExpressionType.Division, position)
        {

        }
    }
}
