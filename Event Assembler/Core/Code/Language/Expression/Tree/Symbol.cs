using System.Collections.Generic;
using Nintenlord.IO;
using Nintenlord.Utility;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    sealed public class Symbol<T> : IExpression<T>, INamed<string>
    {
        readonly string name;
        readonly FilePosition position;

        public string Name
        {
            get { return name; }
        }
        public FilePosition Position
        {
            get { return position; }
        }

        public Symbol(string name, FilePosition position)
        {
            this.name = name;
            this.position = position;
        }

        #region IExpression<T> Members

        public EAExpressionType Type
        {
            get { return EAExpressionType.Symbol; }
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
            return name;
        }
    }
}
