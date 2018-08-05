// -----------------------------------------------------------------------
// <copyright file="StatementParser.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.IO.Scanners;
using Nintenlord.Parser;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Parser
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public sealed class StatementParser<T> : Parser<Token, IExpression<T>>
    {
        readonly IParser<Token, IExpression<T>> parameterParser;

        public StatementParser(IParser<Token, IExpression<T>> parameterParser)
        {
            this.parameterParser = parameterParser;
        }

        protected override IExpression<T> ParseMain(IScanner<Token> scanner, out Match<Token> match)
        {
            match = new Match<Token>(scanner);

            Token first = scanner.Current;

            if (IsStatementEnding(first.Type))
            {
                match++; scanner.MoveNext();
                return Code<T>.EmptyCode(first.Position);
            }
            else if (first.Type == TokenType.Symbol)
            {
                match++; scanner.MoveNext();

                Token second = scanner.Current;

                if (second.Type == TokenType.Colon)//Label
                {
                    match++; scanner.MoveNext();
                    
                    Match<Token> latestMatch;
                    var expression = this.Parse(scanner, out latestMatch);
                    match += latestMatch;
                    return match.Success ? new LabeledExpression<T>(first.Position, first.Value, expression) : null;
                }
                else //assignment or code
                {
                    Match<Token> latestMatch;
                    var result = Statement(scanner, new Symbol<T>(first.Value, first.Position) , out latestMatch);
                    match += latestMatch;
                    return match.Success ? result : null;
                }
            }
            else
            {
                match = new Match<Token>(scanner, "Expected statement or label, got {0}", first);
                return null;
            }
        }

        private IExpression<T> Statement(IScanner<Token> scanner, Symbol<T> name, out Match<Token> match)
        {
            List<IExpression<T>> parameters = new List<IExpression<T>>();
            IExpression<T> expressionToReplace = null;
            bool assignment = false;
            match = new Match<Token>(scanner);

            #region Parse rest of the statement
            while (true)
            {
                Token next = scanner.Current;
                if (next.Type == TokenType.Equal)
                {
                    match++; scanner.MoveNext();
                    assignment = true;

                    next = scanner.Current;
                    if (next.Type == TokenType.Symbol)
                    {
                        match++; scanner.MoveNext();
                        expressionToReplace = new Symbol<T>(next.Value, next.Position);
                    }
                    else
                    {
                        Match<Token> latestMatch;
                        expressionToReplace = parameterParser.Parse(scanner, out latestMatch);
                        match += latestMatch;
                    }
                    break;
                }
                else if (IsStatementEnding(next.Type))
                {
                    match++; scanner.MoveNext();
                    break;
                }
                else
                {
                    Match<Token> latestMatch;
                    var parameter = parameterParser.Parse(scanner, out latestMatch);
                    match += latestMatch;
                    if (!match.Success) break;

                    parameters.Add(parameter);
                }
            }
            #endregion

            if (!match.Success)
                return null;

            #region Construct and return statement
            if (assignment)
            {
                throw new ArgumentException();
                //List<Symbol<T>> variables = new List<Symbol<T>>();
                //foreach (var parameter in parameters)
                //{
                //    if (parameter.CompCount == 1 &&
                //        parameter.Only.Type == EAExpressionType.Symbol)
                //    {
                //        variables.Add(parameter.Only as Symbol<T>);
                //    }
                //    else
                //    {
                //        match = new Match<Token>(scanner, "Assignment parameter {0} isn't valid", parameter);
                //    }

                //}
                //return new Assingment<T>(name, variables, expressionToReplace, name.Position);
            }
            else
            {
                return new Code<T>(name, parameters);
            }
            #endregion
        }

        private static bool IsStatementEnding(TokenType type)
        {
            return type == TokenType.CodeEnder || type == TokenType.NewLine;
        }
    }
}
