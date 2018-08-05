// -----------------------------------------------------------------------
// <copyright file="BinaryOperator.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree.MathOperators
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class BinaryOperator<T> : IExpression<T>
    {
        readonly private EAExpressionType type;
        readonly private IExpression<T> first;
        readonly private IExpression<T> second;
        readonly private FilePosition position;

        public IExpression<T> Second
        {
            get { return second; }
        } 
        public IExpression<T> First
        {
            get { return first; }
        }

        protected BinaryOperator(IExpression<T> first, IExpression<T> second, EAExpressionType type, FilePosition position)
        {
            this.first = first;
            this.second = second;
            this.type = type;
            this.position = position;
        }

        #region ITree<IExpression<T>> Members

        public EAExpressionType Type
        {
            get { return type; }
        }

        public FilePosition Position
        {
            get { return position; }
        } 

        public IEnumerable<IExpression<T>> GetChildren()
        {
            yield return first;
            yield return second;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("({0} {2} {1})", first, second, type);
        }
    }
}
