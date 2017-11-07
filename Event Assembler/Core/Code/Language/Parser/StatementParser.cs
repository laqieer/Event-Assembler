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
  public sealed class StatementParser<T> : Parser<Token, IExpression<T>>
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

      if (current.Type == TokenType.StringLiteral)
      {
        ++match;
        scanner.MoveNext();

        IExpression<T> expression = ParseCode(scanner, new Symbol<T>(current.Value.Substring(1, current.Value.Length - 1), current.Position), out Match<Token> match2);
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

          return new LabelExpression<T>(current.Position, current.Value);
        }
        else
        {
          IExpression<T> expression = ParseCode(scanner, new Symbol<T>(current.Value, current.Position), out Match<Token> match2);
          match += match2;

          if (!match.Success)
            return null;

          return expression;
        }
      }

      match = new Match<Token>(scanner, "Expected statement or label, got {0}", current);
      return null;
    }

    private IExpression<T> ParseCode(IScanner<Token> scanner, Symbol<T> name, out Match<Token> match)
    {
      List<IExpression<T>> parameters = new List<IExpression<T>>();
      Token current;

      match = new Match<Token>(scanner);

      while (true)
      {
        current = scanner.Current;
        
        if (!IsStatementEnding(current.Type))
        {
          IExpression<T> expression = parameterParser.Parse(scanner, out Match<Token> parameterMatch);

          if (parameterMatch.Success)
          {
            // Parameter matched
            match += parameterMatch;
            parameters.Add(expression);
          }
          else
          {
            // Next Token can't parsed to parameter, so we let it be handled by caller's logic

            // If parameter parser consumed some tokens, err
            if (parameterMatch.Length >= 0)
              match = new Match<Token>(scanner, "Expected parameter, got {0}", current);

            break;
          }
        }
        else
        {
          // Statement ender: we can safely ignore it, and break
          ++match;
          scanner.MoveNext();
          break;
        }
      }

      if (!match.Success)
        return null;
      
      return new Code<T>(name, parameters);
    }

    private static bool IsStatementEnding(TokenType type)
    {
      return type == TokenType.CodeEnder || type == TokenType.NewLine;
    }
  }
}
