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
      return (IParser<Token, IExpression<T>>) new StatementParser<T>(Parsers.GetParameterParser<T>(eval)).Name<Token, IExpression<T>>("Statement");
    }

    private static IParser<Token, IExpression<T>> GetParameterParser<T>(Func<string, T> eval)
    {
      TokenTypeParser typeParser1 = TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.Comma);
      TokenTypeParser typeParser2 = TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.LeftSquareBracket);
      TokenTypeParser typeParser3 = TokenTypeParser.GetTypeParser(Nintenlord.Event_Assembler.Core.Code.Language.Lexer.TokenType.RightSquareBracket);
      NameParser<Token, IExpression<T>> nameParser = new MathParser<T>(eval).Name<Token, IExpression<T>>("Atom");
      IParser<Token, IExpression<T>> vectorParser = (IParser<Token, IExpression<T>>) null;
      vectorParser = (IParser<Token, IExpression<T>>) ((IParser<Token, IExpression<T>>) ((Nintenlord.Parser.Parser<Token, IExpression<T>>) ((Func<IParser<Token, IExpression<T>>>) (() => vectorParser)).Lazy<Token, IExpression<T>>() | (Nintenlord.Parser.Parser<Token, IExpression<T>>) nameParser)).SepBy1<Token, Token, IExpression<T>>((IParser<Token, Token>) typeParser1).Between<Token, List<IExpression<T>>, Token, Token>((IParser<Token, Token>) typeParser2, (IParser<Token, Token>) typeParser3).Transform<Token, List<IExpression<T>>, ExpressionList<T>>((Converter<List<IExpression<T>>, ExpressionList<T>>) (x => new ExpressionList<T>((IEnumerable<IExpression<T>>) x, x[0].Position))).Name<Token, ExpressionList<T>>("Vector");
      return (IParser<Token, IExpression<T>>) ((IParser<Token, IExpression<T>>) ((Nintenlord.Parser.Parser<Token, IExpression<T>>) nameParser | vectorParser)).Name<Token, IExpression<T>>("Parameter");
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
