// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.Parser.StatementParser`1
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.IO.Scanners;
using Nintenlord.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Parser
{
  public sealed class StatementParser<T> : Nintenlord.Parser.Parser<Token, IExpression<T>>
  {
    private readonly IParser<Token, IExpression<T>> parameterParser;

    public StatementParser(IParser<Token, IExpression<T>> parameterParser)
    {
      this.parameterParser = parameterParser;
    }

    protected override IExpression<T> ParseMain(IScanner<Token> scanner, out Match<Token> match)
    {
      match = new Match<Token>(scanner);
      Token current = scanner.Current;
      if (IsStatementEnding(current.Type))
      {
        ++match;
        scanner.MoveNext();
        return Code<T>.EmptyCode(current.Position);
      }
      if(current.Type == TokenType.StringLiteral)
      {
            ++match;
            scanner.MoveNext();
            Match<Token> match2;
            IExpression<T> expression = this.Statement(scanner, new Symbol<T>(current.Value.Substring(1, current.Value.Length-1), current.Position), out match2);
            match += match2;
            if (!match.Success)
                return null;
            return expression;
      }
      if (current.Type == TokenType.Symbol)
      {
        ++match;
        scanner.MoveNext();
        if (scanner.Current.Type == TokenType.Colon)
        {
          ++match;
          scanner.MoveNext();
          Match<Token> match1;
          IExpression<T> labeledExpression = this.Parse(scanner, out match1);
          match += match1;
          if (!match.Success)
            return null;
          return new LabeledExpression<T>(current.Position, current.Value, labeledExpression);
        }
        Match<Token> match2;
        IExpression<T> expression = this.Statement(scanner, new Symbol<T>(current.Value, current.Position), out match2);
        match += match2;
        if (!match.Success)
          return null;
        return expression;
      }
      match = new Match<Token>(scanner, "Expected statement or label, got {0}", new object[1]
      {
        current
      });
      return null;
    }

    private IExpression<T> Statement(IScanner<Token> scanner, Symbol<T> name, out Match<Token> match)
    {
      List<IExpression<T>> parameters = new List<IExpression<T>>();
      bool flag = false;
      match = new Match<Token>(scanner);
      Token current;
      while (true)
      {
        current = scanner.Current;
        if (current.Type != Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.Equal)
        {
          if (!StatementParser<T>.IsStatementEnding(current.Type))
          {
            Match<Token> match1;
            IExpression<T> expression = this.parameterParser.Parse(scanner, out match1);
            match += match1;
            if (match.Success)
              parameters.Add(expression);
            else
              goto label_9;
          }
          else
            goto label_6;
        }
        else
          break;
      }
      ++match;
      scanner.MoveNext();
      flag = true;
      current = scanner.Current;
      if (current.Type == Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.Symbol)
      {
        ++match;
        scanner.MoveNext();
        Symbol<T> symbol = new Symbol<T>(current.Value, current.Position);
        goto label_9;
      }
      else
      {
        Match<Token> match1;
        this.parameterParser.Parse(scanner, out match1);
        match += match1;
        goto label_9;
      }
label_6:
      ++match;
      scanner.MoveNext();
label_9:
      if (!match.Success)
        return (IExpression<T>) null;
      if (flag)
        throw new ArgumentException();
      return (IExpression<T>) new Code<T>(name, parameters);
    }

    private static bool IsStatementEnding(TokenType type)
    {
      return type == TokenType.CodeEnder || type == TokenType.NewLine || type == TokenType.LeftCurlyBracket || type == TokenType.RightCurlyBracket;
    }
  }
}
