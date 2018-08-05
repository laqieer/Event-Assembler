using System.Collections.Generic;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    sealed class ValueExpression<T> : IExpression<T>
    {
        readonly private FilePosition position;

        public T Value
        {
            get;
            private set;
        }

        public ValueExpression(T value, FilePosition position)
        {
            this.Value = value;
            this.position = position;
        }
        
        #region IExpression<T> Members

        public EAExpressionType Type
        {
            get { return EAExpressionType.Value; }
        }

        public FilePosition Position
        {
            get { return position; }
        } 

        #endregion

        #region ITree<IExpression<T>> Members

        public IEnumerable<IExpression<T>> GetChildren()
        {
            yield break;
        }

        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
