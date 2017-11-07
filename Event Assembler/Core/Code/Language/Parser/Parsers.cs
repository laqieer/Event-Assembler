// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.Parser.Parsers
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.Parser;
using Nintenlord.Parser.ParserCombinators.UnaryParsers;
using System;
using System.Collections.Generic;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Parser
{
  internal static class Parsers
  {
    public static IParser<Token, IExpression<T>> GetStatementParser<T>(Func<string, T> eval)
    {
      return new StatementParser<T>(GetParameterParser(eval)).Name("Statement");
    }

    private static IParser<Token, IExpression<T>> GetParameterParser<T>(Func<string, T> eval)
    {
      TokenTypeParser typeParser1 = TokenTypeParser.GetTypeParser(TokenType.Comma);
      TokenTypeParser typeParser2 = TokenTypeParser.GetTypeParser(TokenType.LeftSquareBracket);
      TokenTypeParser typeParser3 = TokenTypeParser.GetTypeParser(TokenType.RightSquareBracket);
      NameParser<Token, IExpression<T>> nameParser = new MathParser<T>(eval).Name("Atom");
      IParser<Token, IExpression<T>> vectorParser = null;
      vectorParser = ((IParser<Token, IExpression<T>>)(((Func<IParser<Token, IExpression<T>>>)(() => vectorParser)).Lazy() | nameParser)).SepBy1(typeParser1).Between(typeParser2, typeParser3).Transform(x => new ExpressionList<T>(x, x[0].Position)).Name("Vector");
      return ((IParser<Token, IExpression<T>>)(nameParser | vectorParser)).Name("Parameter");
    }

    public static IParser<Token, IExpression<T>> GetMathParser<T>(Func<string, T> eval)
    {
      TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.Symbol);
      TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.IntegerLiteral);
      TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.LeftParenthesis);
      TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.RightParenthesis);
      TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.MathOperator);
      throw new NotImplementedException();
    }
  }
}
