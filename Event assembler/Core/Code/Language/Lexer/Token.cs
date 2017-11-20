// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.Lexer.Token
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.IO;
using System;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Lexer
{
  public struct Token : IFilePositionable
  {
    private readonly FilePosition position;
    private readonly TokenType type;
    private readonly string value;

    public bool HasValue
    {
      get
      {
        return this.value != null;
      }
    }

    public TokenType Type
    {
      get
      {
        return this.type;
      }
    }

    public string Value
    {
      get
      {
        if (this.value != null)
          return this.value;
        throw new InvalidOperationException();
      }
    }

    public FilePosition Position
    {
      get
      {
        return this.position;
      }
    }

    private Token(FilePosition position, TokenType type)
    {
      this = new Token(position, type, (string) null);
    }

    public Token(FilePosition position, TokenType type, string value)
    {
      this.position = position;
      this.type = type;
      this.value = value;
    }

    public override string ToString()
    {
      return this.type.ToString() + (this.value != null ? "(" + this.value + ")" : "");
    }
  }
}
