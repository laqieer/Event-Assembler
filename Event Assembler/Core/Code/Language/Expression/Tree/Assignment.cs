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
  public sealed class Assignment<T> : IExpression<T>
  {
    public readonly Symbol<T> Name;
    public readonly IExpression<T> Value;
    
    public EAExpressionType Type
    {
      get
      {
        return EAExpressionType.Assignment;
      }
    }

    public FilePosition Position => Name.Position;

    public Assignment(Symbol<T> name, IExpression<T> value)
    {
      this.Name = name;
      this.Value = value;
    }

    public IEnumerable<IExpression<T>> GetChildren()
    {
      yield return this.Name;
      yield return this.Value;
    }
  }
}
