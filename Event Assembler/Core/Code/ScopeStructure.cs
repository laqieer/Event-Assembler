// -----------------------------------------------------------------------
// <copyright file="ScopeStructure.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Nintenlord.Event_Assembler.Core.Code
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Nintenlord.Collections.Trees;
    using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
    using Nintenlord.Utility;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class ScopeStructure<T> : ITree<ScopeStructure<T>>
    {
        public readonly ScopeStructure<T> ParentScope;
        readonly List<ScopeStructure<T>> childScopes;
        readonly Dictionary<string, IExpression<T>> definedSymbols;

        public ScopeStructure(ScopeStructure<T> parentScope)
        {
            this.ParentScope = parentScope;
            this.childScopes = new List<ScopeStructure<T>>();
            this.definedSymbols = new Dictionary<string, IExpression<T>>();
        }

        public void AddChildScope(ScopeStructure<T> newChildScope)
        {
            childScopes.Add(newChildScope);
        }

        public CanCauseError<IExpression<T>> GetSymbolValue(string symbol)
        {
            IExpression<T> value;
            if (definedSymbols.TryGetValue(symbol, out value))
            {
                return CanCauseError<IExpression<T>>.NoError(value);
            }
            else if (ParentScope != null)
            {
                return ParentScope.GetSymbolValue(symbol);
            }
            else
            {
                return CanCauseError<IExpression<T>>.Error("Symbol {0} not defined", symbol);
            }
        }

        public CanCauseError AddNewSymbol(string symbol, IExpression<T> value)
        {
            //Detect if adds cycles
            if (definedSymbols.ContainsKey(symbol))
            {
                return CanCauseError.Error("Symbol already exists.");
            }
            else
            {
                definedSymbols[symbol] = value;
                return CanCauseError.NoError;
            }
        }


        #region ITree<ScopeStructure> Members

        public IEnumerable<ScopeStructure<T>> GetChildren()
        {
            return childScopes;
        }

        #endregion
    }
}
