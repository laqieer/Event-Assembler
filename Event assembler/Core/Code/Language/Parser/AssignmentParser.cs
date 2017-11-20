using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nintenlord.IO.Scanners;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Parser
{
    class AssignmentParser<T> : Parser<Token, IExpression<T>>
    {
        private IParser<Token, IExpression<T>> valueParser;

        public AssignmentParser(IParser<Token, IExpression<T>> valueParser)
        {
            this.valueParser = valueParser;
        }

        protected override IExpression<T> ParseMain(IScanner<Token> scanner, out Match<Token> match)
        {
            match = new Match<Token>(scanner);

            Token first = scanner.Current;
            long currentIndex = scanner.Offset;

            if (first.Type == TokenType.Symbol)
            {
                string labelName = first.Value;

                ++match;
                scanner.MoveNext();

                if (scanner.Current.Type == TokenType.Equal)
                {
                    ++match;
                    scanner.MoveNext();

                    IExpression<T> result = valueParser.Parse(scanner, out Match<Token> resultMatch);

                    if (resultMatch.Success)
                    {
                        match += resultMatch;
                        return new Assignment<T>(first.Value, result, first.Position);
                    }
                }
            }

            scanner.Offset = currentIndex;
            match = new Match<Token>(scanner, "failed to parse assignment");

            return null;
        }
    }
}
