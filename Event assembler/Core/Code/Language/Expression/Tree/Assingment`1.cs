// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree.Assingment`1
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Collections.Trees;
using Nintenlord.IO;
using System.Collections.Generic;
using System.Linq;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree
{
  public sealed class Assingment<T> : IExpression<T>, ITree<IExpression<T>>
  {
    public readonly Symbol<T> Name;
    private readonly Symbol<T>[] variables;
    public readonly IExpression<T> Result;

    public Symbol<T> this[int index]
    {
      get
      {
        return this.variables[index];
      }
    }

    public int VariableCount
    {
      get
      {
        return this.variables.Length;
      }
    }

    public EAExpressionType Type
    {
      get
      {
        return EAExpressionType.Assignment;
      }
    }

    public FilePosition Position { get; private set; }

    public Assingment(Symbol<T> name, IEnumerable<Symbol<T>> variables, IExpression<T> result, FilePosition position)
    {
      this.Name = name;
      this.variables = variables.ToArray<Symbol<T>>();
      this.Result = result;
      this.Position = position;
    }

    public IEnumerable<IExpression<T>> GetChildren()
    {
      yield return (IExpression<T>) this.Name;
      foreach (Symbol<T> variable in this.variables)
        yield return (IExpression<T>) variable;
      yield return this.Result;
    }
  }
}
