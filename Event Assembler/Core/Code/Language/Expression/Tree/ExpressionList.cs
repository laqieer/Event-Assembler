// -----------------------------------------------------------------------
// <copyright file="List.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Nintenlord.Collections;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class ExpressionList<T> : IExpression<T>
    {
        readonly FilePosition filePosition;
        readonly List<IExpression<T>> expressions;

        public int ComponentCount
        {
            get { return expressions.Count; }
        }
        public IExpression<T> this[int index]
        {
            get { return expressions[index]; }
        }

        public ExpressionList(IEnumerable<IExpression<T>> expressions, FilePosition startPosition)
        {
            this.expressions = new List<IExpression<T>>(expressions);
            this.filePosition = startPosition;
        }

        #region IExpression<T> Members

        public EAExpressionType Type
        {
            get { return EAExpressionType.List; }
        }

        public FilePosition Position
        {
            get { return filePosition; }
        }

        #endregion

        #region ITree<IExpression<T>> Members

        public IEnumerable<IExpression<T>> GetChildren()
        {
            return this.expressions;
        }

        #endregion

        public override string ToString()
        {
            return expressions.ToElementWiseString(", ","[","]");
        }
    }
}
