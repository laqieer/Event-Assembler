using System;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.IO.Scanners;
using Nintenlord.Parser;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Parser
{
    internal sealed class TokenParser<T> : Parser<Token, IExpression<T>>
    {
        readonly IParser<Token, IExpression<T>> mainParser;

        public TokenParser(Func<string, T> eval)
        {
            var codeParser = Parsers.GetStatementParser(eval);

            var scopeParser = new ScopeParser<T>(
                codeParser.Many(),
                TokenTypeParser.GetTypeParser(TokenType.LeftCurlyBracket),
                TokenTypeParser.GetTypeParser(TokenType.RightCurlyBracket));

            mainParser = scopeParser.Transform(x => (IExpression<T>)x);
        }


        void ParseEvent2<T1, T2>(object sender, ParsingEventArgs<T1, T2> e)
        {
            Console.WriteLine("Parser {0}, matched {1}", sender, e.Match);
        }

        protected override IExpression<T> ParseMain(IScanner<Token> scanner, out Match<Token> match)
        {
            return mainParser.Parse(scanner, out match);
        }
    }
}
