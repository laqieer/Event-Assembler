// -----------------------------------------------------------------------
// <copyright file="Assingment.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Nintenlord.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    sealed public class Assingment<T> : IExpression<T>
    {
        public readonly Symbol<T> Name;
        readonly Symbol<T>[] variables;

        public readonly IExpression<T> Result;

        public Symbol<T> this[int index]
        {
            get 
            {
                return variables[index];
            }
        }
        public int VariableCount
        {
            get 
            {
                return variables.Length;
            }
        }

        public Assingment(Symbol<T> name, IEnumerable<Symbol<T>> variables, IExpression<T> result, FilePosition position)
        {
            this.Name = name;
            this.variables = variables.ToArray();
            this.Result = result;
            this.Position = position;
        }

        #region IExpression<T> Members

        public EAExpressionType Type
        {
            get { return EAExpressionType.Assignment; }
        }

        public FilePosition Position
        {
            get;
            private set;
        }
        
        #endregion

        #region ITree<IExpression<T>> Members

        public IEnumerable<IExpression<T>> GetChildren()
        {
            yield return Name;
            foreach (var item in variables)
            {
                yield return item;
            }
            yield return Result;
        }

        #endregion
    }
}
