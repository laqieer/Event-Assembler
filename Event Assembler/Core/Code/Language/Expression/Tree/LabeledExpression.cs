// -----------------------------------------------------------------------
// <copyright file="LabelDefinition.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    sealed class LabeledExpression<T> : IExpression<T>
    {
        readonly string labelName;
        readonly FilePosition position;
        readonly IExpression<T> labeledExpression;

        public string LabelName
        {
            get { return labelName; }
        }

        public LabeledExpression(FilePosition position, string labelName, IExpression<T> labeledExpression)
        {
            this.position = position;
            this.labelName = labelName;
            this.labeledExpression = labeledExpression;
        }

        #region IExpression<T> Members

        public EAExpressionType Type
        {
            get { return EAExpressionType.Labeled; }
        }

        public FilePosition Position
        {
            get { return position; }
        }

        #endregion

        #region ITree<IExpression<T>> Members

        public IEnumerable<IExpression<T>> GetChildren()
        {
            yield return labeledExpression;
        }

        #endregion
    }
}
