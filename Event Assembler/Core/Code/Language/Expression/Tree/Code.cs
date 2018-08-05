using System.Collections.Generic;
using Nintenlord.Collections;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    sealed class Code<T> : IExpression<T>
    {
        readonly Symbol<T> codeName;
        readonly IExpression<T>[] parameters;
        readonly FilePosition position;


        public Symbol<T> CodeName
        {
            get { return codeName; }
        }
        public IExpression<T>[] Parameters
        {
            get { return parameters; }
        }
        public IExpression<T> this[int index]
        {
            get { return parameters[index]; }
        }
        public FilePosition Position
        {
            get { return position; }
        }
        public bool IsEmpty
        {
            get { return codeName == null; }
        }

        public int ParameterCount { get { return parameters.Length; } }

        private Code(FilePosition position, Symbol<T> codeName, List<IExpression<T>> parameters)
        {
            this.codeName = codeName;
            this.parameters = parameters.ToArray();
            this.position = position;
        }
        public Code(Symbol<T> codeName, List<IExpression<T>> parameters)
        {
            this.codeName = codeName;
            this.parameters = parameters.ToArray();
            this.position = codeName.Position;
        }
        
        #region IExpression<T> Members

        public EAExpressionType Type
        {
            get { return EAExpressionType.Code; }
        }
        
        public IEnumerable<IExpression<T>> GetChildren()
        {
            if (this.IsEmpty)
            {
                yield break;
            }
            else
            {
                yield return codeName;
                foreach (var item in parameters)
                {
                    yield return item;
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return codeName + parameters.ToElementWiseString(" ", " ", "");
        }

        public static Code<T> EmptyCode(FilePosition position)
        {
            return new Code<T>(position, null, new List<IExpression<T>>());
        }
    }
}
