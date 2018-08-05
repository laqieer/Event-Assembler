using Nintenlord.Collections.Trees;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression
{
    /// <summary>
    /// Expression of EA language.
    /// </summary>
    /// <typeparam name="T">The primitive integer type.</typeparam>
    public interface IExpression<out T> : ITree<IExpression<T>>
    {
        /// <summary>
        /// Type of the expressions.
        /// </summary>
        EAExpressionType Type { get; }
        /// <summary>
        /// Position of the start of the expression.
        /// </summary>
        FilePosition Position { get; } 
    }
}
