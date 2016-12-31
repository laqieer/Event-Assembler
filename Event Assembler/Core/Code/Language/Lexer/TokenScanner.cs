// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenScanner
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Event_Assembler.Core.IO.Input;
using Nintenlord.IO.Scanners;
using System;
using System.Collections.Generic;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Lexer
{
  internal sealed class TokenScanner : IStoringScanner<Token>, IScanner<Token>
  {
    private List<Token> readTokens;
    private IPositionableInputStream input;
    private int tokenOffset;

    public bool IsAtEnd { get; private set; }

    public long Offset
    {
      get
      {
        return (long) this.tokenOffset;
      }
      set
      {
        throw new NotSupportedException();
      }
    }

    public Token Current
    {
      get
      {
        if (this.tokenOffset > this.readTokens.Count)
          throw new InvalidOperationException("End of tokens to read.");
        if (this.tokenOffset == this.readTokens.Count)
          return new Token();
        return this.readTokens[this.tokenOffset];
      }
    }

    public bool CanSeek
    {
      get
      {
        return false;
      }
    }

    public Token this[int offset]
    {
      get
      {
        return this.AtOffset(offset);
      }
    }

    public TokenScanner(IPositionableInputStream input)
    {
      this.input = input;
      this.readTokens = new List<Token>(4096);
      this.tokenOffset = -1;
      this.IsAtEnd = false;
    }

    public Token AtOffset(int offset)
    {
      return this.readTokens[offset];
    }

    public bool MoveNext()
    {
      if (this.IsAtEnd)
        return false;
      ++this.tokenOffset;
      while (this.tokenOffset >= this.readTokens.Count)
      {
        string line = this.input.ReadLine();
        if (line != null)
          this.readTokens.AddRange(Tokeniser.TokeniseLine(line, this.input.CurrentFile, this.input.LineNumber));
        else
          break;
      }
      this.IsAtEnd = this.tokenOffset >= this.readTokens.Count;
      return !this.IsAtEnd;
    }
  }
}
