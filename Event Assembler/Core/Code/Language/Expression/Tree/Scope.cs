using System.Collections.Generic;
using System.Linq;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    class Scope<T> : IExpression<T>
    {
        readonly private List<IExpression<T>> expressions;
        readonly private FilePosition position;

        public Scope(List<IExpression<T>> expressions, FilePosition position)
        {
            this.expressions = expressions;
            this.position = position;
        }

        #region IExpression<T> Members

        public FilePosition Position
        {
            get { return position; }
        } 

        public EAExpressionType Type
        {
            get { return EAExpressionType.Scope; }
        }

        public IEnumerable<IExpression<T>> GetChildren()
        {
            return expressions.AsEnumerable();
        }

        #endregion
    }
}
