// -----------------------------------------------------------------------
// <copyright file="MathParser.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Nintenlord.Event_Assembler.Core.Code.Language.Parser
{
    using System;
    using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
    using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
    using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
    using Nintenlord.IO.Scanners;
    using Nintenlord.Parser;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    sealed class MathParser<T> : Parser<Token, IExpression<T>>
    {
        readonly private Func<string, T> evaluate;

        public MathParser(Func<string, T> evaluate)
        {
            this.evaluate = evaluate;
        }

        protected override IExpression<T> ParseMain(IScanner<Token> scanner, out Match<Token> match)
        {
            return Parse10(scanner, out match);
        }

        //Operators of the lowest precedence.
        //Parses all operators of precedence <=10
        private IExpression<T> Parse10(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse9(scanner, out match);
            if (match.Success)
            {
                Match<Token> secondMatch;
                var result = Parse10opt(scanner, temp, out secondMatch);
                match += secondMatch;
                return result;
            }
            else return null;
        }

        private IExpression<T> Parse10opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            Match<Token> tempMatch;
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "|")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse9(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new BitwiseOr<T>(expression, second, token.Position);
                            var opt = Parse10opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else
                    {
                        match = new Match<Token>(scanner, 0);
                    }
                    break;
                default:
                    match = new Match<Token>(scanner, 0);
                    break;
            }
            return expression;
        }

        //Parses all operators of precedence <=9 
        private IExpression<T> Parse9(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse8(scanner, out match);
            if (match.Success)
            {
                Match<Token> secondMatch;
                var result = Parse9opt(scanner, temp, out secondMatch);
                match += secondMatch;
                return result;
            }
            else return null;
        }
        private IExpression<T> Parse9opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            Match<Token> tempMatch;
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "^")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse8(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new BitwiseXor<T>(expression, second, token.Position);
                            var opt = Parse9opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else
                    {
                        match = new Match<Token>(scanner, 0);
                    }
                    break;
                default:
                    match = new Match<Token>(scanner, 0);
                    break;
            }
            return expression;
        }

        //Parses all operators of precedence <=8 
        private IExpression<T> Parse8(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse5(scanner, out match);
            if (match.Success)
            {
                Match<Token> secondMatch;
                var result = Parse8opt(scanner, temp, out secondMatch);
                match += secondMatch;
                return result;
            }
            else return null;
        }
        private IExpression<T> Parse8opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            Match<Token> tempMatch;
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "&")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse5(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new BitwiseAnd<T>(expression, second, token.Position);
                            var opt = Parse8opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else
                    {
                        match = new Match<Token>(scanner, 0);
                    }
                    break;
                default:
                    match = new Match<Token>(scanner, 0);
                    break;
            }
            return expression;
        }

        //Precidences 6 and 7 contain < <= > >= == and !=, which aren't used in EA.

        //Parses all operators of precedence <=5 
        private IExpression<T> Parse5(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse4(scanner, out match);
            if (match.Success)
            {
                Match<Token> secondMatch;
                var result = Parse5opt(scanner, temp, out secondMatch);
                match += secondMatch;
                return result;
            }
            else return null;
        }
        private IExpression<T> Parse5opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            Match<Token> tempMatch;
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "<<")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse4(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new BitShiftLeft<T>(expression, second, token.Position);
                            var opt = Parse5opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else if (token.Value == ">>")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse4(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new BitShiftRight<T>(expression, second, token.Position);
                            var opt = Parse5opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else if (token.Value == ">>>")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse4(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new ArithmeticShiftRight<T>(expression, second, token.Position);
                            var opt = Parse5opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else
                    {
                        match = new Match<Token>(scanner, 0);
                    }
                    break;
                default:
                    match = new Match<Token>(scanner, 0);
                    break;
            }
            return expression;
        }

        private IExpression<T> Parse4(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse3(scanner, out match);
            if (match.Success)
            {
                Match<Token> secondMatch;
                var result = Parse4opt(scanner, temp, out secondMatch);
                match += secondMatch;
                return result;
            }
            else return null;
        }

        private IExpression<T> Parse4opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            Match<Token> tempMatch;
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "+")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse3(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new Sum<T>(expression, second, token.Position);
                            var opt = Parse4opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else if (token.Value == "-")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse3(scanner, out tempMatch);
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new Minus<T>(expression, second, token.Position);
                            var opt = Parse4opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                    }
                    else
                    {
                        match = new Match<Token>(scanner, 0);
                    }
                    break;
                default:
                    match = new Match<Token>(scanner, 0);
                    break;
            }
            return expression;
        }


        private IExpression<T> Parse3(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse1(scanner, out match); //TODO
            if (match.Success)
            {
                Match<Token> tempMatch;
                var result = Parse3opt(scanner, temp, out tempMatch);
                match += tempMatch;
                return result;
            }
            else return null;
        }

        private IExpression<T> Parse3opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            Match<Token> tempMatch;
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "*")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse1(scanner, out tempMatch); //TODO
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new Multiply<T>(expression, second, token.Position);
                            var opt = Parse3opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                        else return null;
                    }
                    else if (token.Value == "/")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse1(scanner, out tempMatch); //TODO
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new Division<T>(expression, second, token.Position);
                            var opt = Parse3opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                        else return null;
                    }
                    else if (token.Value == "%")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var second = Parse1(scanner, out tempMatch); //TODO
                        match += tempMatch;
                        if (match.Success)
                        {
                            expression = new Modulus<T>(expression, second, token.Position);
                            var opt = Parse3opt(scanner, expression, out tempMatch);
                            match += tempMatch;
                            return opt;
                        }
                        else return null;
                    }
                    else
                    {
                        match = new Match<Token>(scanner, 0);
                    }
                    break;
                default:
                    match = new Match<Token>(scanner, 0);
                    break;
            }
            return expression;
        }


        /* At some other time, separate unary + and - into their own precidence level. For now, allow it to be done the way nintenlord originally did it. */
        /*
        private IExpression<T> Parse2(IScanner<Token> scanner, out Match<Token> match)
        {
            IExpression<T> temp = Parse1(scanner, out match);
            if (match.Success)
            {
                Match<Token> tempMatch;
                var result = Parse2opt(scanner, temp, out tempMatch);
                match += tempMatch;
                return result;
            }
            else return null;
        }
        private IExpression<T> Parse2opt(IScanner<Token> scanner, IExpression<T> expression, out Match<Token> match)
        {
            bool negative = false;
            Match<Token> tempMatch = new Match<Token>(scanner, 0);
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    

                    if (token.Value == "-")
                    {
                        match = new Match<Token>(scanner, 1);
                        scanner.MoveNext();
                        var operand = Parse1(scanner, tempMatch);
                        token = scanner.Current;
                        negative = true;
                        goto case TokenType.IntegerLiteral;
                    }
                    else if (token.Value == "+")
                    {
                        scanner.MoveNext(); match++;
                        token = scanner.Current;
                        negative = false;
                        goto case TokenType.IntegerLiteral;
                    }
                    else
                    {
                        goto default;
                    }
                case TokenType.Symbol:
                    scanner.MoveNext();match++;
                    return new Symbol<T>(token.Value, token.Position);
                    
                case TokenType.IntegerLiteral:
                    scanner.MoveNext();match++;
                    if (negative)
                    {
                        return new ValueExpression<T>(evaluate("-" + token.Value), token.Position);
                    }
                    else
                    {
                        return new ValueExpression<T>(evaluate(token.Value), token.Position);
                    }
                    
                case TokenType.LeftParenthesis:
                    scanner.MoveNext(); match++;

                    Match<Token> tempMatch;
                    var res = Parse2opt(scanner, out tempMatch);
                    match += tempMatch;
                    if (match.Success)
                    {
                        if (scanner.Current.Type == TokenType.RightParenthesis)
                        {
                            scanner.MoveNext();
                            match++;
                            return res;
                        }
                        else
                        {
                            match = new Match<Token>(scanner, "Unclosed parenthesis");
                        }
                    }
                    return null;

                default:
                    match = new Match<Token>(scanner, "Operator instead of math value");
                    return null;
            }
        }
        */

        private IExpression<T> Parse1(IScanner<Token> scanner, out Match<Token> match)
        {
            bool negative = false;
            match = new Match<Token>(scanner, 0);
            var token = scanner.Current;
            switch (token.Type)
            {
                case TokenType.MathOperator:
                    if (token.Value == "-")
                    {
                        scanner.MoveNext(); match++;
                        token = scanner.Current;
                        negative = true;
                        goto case TokenType.IntegerLiteral;
                    }
                    else if (token.Value == "+")
                    {
                        scanner.MoveNext(); match++;
                        token = scanner.Current;
                        negative = false;
                        goto case TokenType.IntegerLiteral;
                    }
                    else
                    {
                        goto default;
                    }
                case TokenType.Symbol:
                    scanner.MoveNext(); match++;
                    return new Symbol<T>(token.Value, token.Position);

                case TokenType.IntegerLiteral:
                    scanner.MoveNext(); match++;
                    if (negative)
                    {
                        return new ValueExpression<T>(evaluate("-" + token.Value), token.Position);
                    }
                    else
                    {
                        return new ValueExpression<T>(evaluate(token.Value), token.Position);
                    }

                case TokenType.LeftParenthesis:
                    scanner.MoveNext(); match++;

                    Match<Token> tempMatch;
                    var res = Parse10(scanner, out tempMatch);
                    match += tempMatch;
                    if (match.Success)
                    {
                        if (scanner.Current.Type == TokenType.RightParenthesis)
                        {
                            scanner.MoveNext();
                            match++;
                            return res;
                        }
                        else
                        {
                            match = new Match<Token>(scanner, "Unclosed parentheses");
                        }
                    }
                    return null;

                default:
                    match = new Match<Token>(scanner, "Operator instead of math value");
                    return null;
            }
        }

    }
}
