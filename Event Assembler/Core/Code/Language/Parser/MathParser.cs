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
    using System.Collections.Generic;
    using Nintenlord.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    sealed class MathParser<T> : Parser<Token, IExpression<T>>
    {
        readonly private Func<string, T> evaluate;

        private Dictionary<MathTokenType, IPrefixParselet> prefixParselets;
        private Dictionary<MathTokenType, IInfixParselet> infixParselets;

        public MathParser(Func<string, T> evaluate)
        {
            this.evaluate = evaluate;

            prefixParselets = new Dictionary<MathTokenType, IPrefixParselet>
            {
                { MathTokenType.Identifer,       new PrefixIdentifierParselet() },
                { MathTokenType.IntLiteral,      new PrefixLiteralParselet() },
                { MathTokenType.LeftParenthesis, new PrefixGroupParselet() },
                { MathTokenType.Plus,            new PrefixPlusParselet() },
                { MathTokenType.Minus,           new PrefixMinusParselet() }
            };

            infixParselets = new Dictionary<MathTokenType, IInfixParselet>
            {
                { MathTokenType.Plus,        new InfixSumParselet() },
                { MathTokenType.Minus,       new InfixDifferenceParselet() },
                { MathTokenType.Multiply,    new InfixProductParselet() },
                { MathTokenType.Divide,      new InfixDivideParselet() },
                { MathTokenType.Modulo,      new InfixModulusParselet() },
                { MathTokenType.BitwiseAnd,  new InfixBitwiseAndParselet() },
                { MathTokenType.BitwiseOr,   new InfixBitwiseOrParselet() },
                { MathTokenType.BitwiseXor,  new InfixBitwiseXorParselet() },
                { MathTokenType.LeftShift,   new InfixLShiftParselet() },
                { MathTokenType.RightShift,  new InfixRShiftParselet() },
                { MathTokenType.ARightShift, new InfixARShiftParselet() }
            };
        }

        protected override IExpression<T> ParseMain(IScanner<Token> scanner, out Match<Token> match)
        {
            return ParsePrecedence(scanner, out match, (int) Precedences.NoPrecedence);
        }

        private IExpression<T> ParsePrecedence(IScanner<Token> scanner, out Match<Token> match, int precedence)
        {
            match = new Match<Token>(scanner);

            if (prefixParselets.TryGetValue(GetMathTokenType(scanner.Current), out var prefixParselet))
            {
                IExpression<T> current = prefixParselet.Parse(this, scanner, out Match<Token> prefixMatch);
                match += prefixMatch;

                if (!match.Success)
                    return null;

                while (infixParselets.TryGetValue(GetMathTokenType(scanner.Current), out var infixParselet))
                {
                    if (precedence >= infixParselet.Precedence)
                        break;

                    current = infixParselet.Parse(this, scanner, out Match<Token> infixMatch, current);
                    match += infixMatch;

                    if (!match.Success)
                        return null;
                }

                return current;
            }

            match = new Match<Token>(scanner, "couldn't parse math expression");
            return null;
        }

        private MathTokenType GetMathTokenType(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Symbol:
                    return MathTokenType.Identifer;

                case TokenType.IntegerLiteral:
                    return MathTokenType.IntLiteral;

                case TokenType.LeftParenthesis:
                    return MathTokenType.LeftParenthesis;

                case TokenType.RightParenthesis:
                    return MathTokenType.RightParenthesis;

                case TokenType.MathOperator:
                    switch (token.Value)
                    {
                        case "|":
                            return MathTokenType.BitwiseOr;

                        case "&":
                            return MathTokenType.BitwiseAnd;

                        case "^":
                            return MathTokenType.BitwiseXor;

                        case "<<":
                            return MathTokenType.LeftShift;

                        case ">>":
                            return MathTokenType.RightShift;

                        case ">>>":
                            return MathTokenType.ARightShift;

                        case "+":
                            return MathTokenType.Plus;

                        case "-":
                            return MathTokenType.Minus;

                        case "*":
                            return MathTokenType.Multiply;

                        case "/":
                            return MathTokenType.Divide;

                        case "%":
                            return MathTokenType.Modulo;

                        default:
                            return MathTokenType.Unknown;
                    }

                default:
                    return MathTokenType.Unknown;
            }
        }

        private interface IPrefixParselet
        {
            IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match);
        }

        private interface IInfixParselet
        {
            int Precedence { get; }
            IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match, IExpression<T> previous);
        }

        private enum MathTokenType
        {
            Identifer,
            IntLiteral,

            LeftParenthesis,
            RightParenthesis,

            Plus,
            Minus,
            Multiply,
            Divide,
            Modulo,
            LeftShift,
            RightShift,
            ARightShift,
            BitwiseOr,
            BitwiseAnd,
            BitwiseXor,
            Unknown,
        }

        private enum Precedences : int
        {
            NoPrecedence = 0,
            Or,
            Xor,
            And,
            Shifts,
            Sums,
            Products,
            Prefixes,
        }

        private class PrefixIdentifierParselet : IPrefixParselet
        {
            public IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match)
            {
                match = new Match<Token>(scanner);
                IExpression<T> result = new Symbol<T>(scanner.Current.Value, scanner.Current.Position);

                ++match;
                scanner.MoveNext();

                return result;
            }
        }

        private class PrefixLiteralParselet : IPrefixParselet
        {
            public IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match)
            {
                match = new Match<Token>(scanner);

                try
                {
                    IExpression<T> result = new ValueExpression<T>(parser.evaluate(scanner.Current.Value), scanner.Current.Position);

                    ++match;
                    scanner.MoveNext();

                    return result;
                }
                catch (Exception)
                {
                    match = new Match<Token>(scanner, "failed to parse integer literal \"{0}\"", scanner.Current.Value);
                    return null;
                }
            }
        }

        private class PrefixPlusParselet : IPrefixParselet
        {
            public IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match)
            {
                match = new Match<Token>(scanner);

                ++match;
                scanner.MoveNext();

                IExpression<T> result = parser.ParsePrecedence(scanner, out Match<Token> outMatch, (int) Precedences.Prefixes);
                match += outMatch;

                return result;
            }
        }

        private class PrefixMinusParselet : IPrefixParselet
        {
            public IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match)
            {
                match = new Match<Token>(scanner);
                FilePosition pos = scanner.Current.Position;

                ++match;
                scanner.MoveNext();

                IExpression<T> result = parser.ParsePrecedence(scanner, out Match<Token> outMatch, (int)Precedences.Prefixes);
                match += outMatch;

                return new Minus<T>(null, result, pos);
            }
        }

        private class PrefixGroupParselet : IPrefixParselet
        {
            public IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match)
            {
                match = new Match<Token>(scanner);

                ++match;
                scanner.MoveNext();

                IExpression<T> result = parser.ParsePrecedence(scanner, out Match<Token> outMatch, (int)Precedences.NoPrecedence);

                if (scanner.Current.Type != TokenType.RightParenthesis)
                {
                    match = new Match<Token>(scanner, "failed to parse closing parenthesis");
                    return null;
                }

                match += outMatch;

                ++match;
                scanner.MoveNext();

                return result;
            }
        }

        private abstract class InfixBinaryOperatorParselet : IInfixParselet
        {
            public abstract int Precedence { get; }

            public abstract IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos);

            public IExpression<T> Parse(MathParser<T> parser, IScanner<Token> scanner, out Match<Token> match, IExpression<T> previous)
            {
                match = new Match<Token>(scanner);

                ++match;
                scanner.MoveNext();

                IExpression<T> right = parser.ParsePrecedence(scanner, out Match<Token> rightMatch, Precedence);

                match += rightMatch;

                if (match.Success)
                    return MakeExpression(previous, right, previous.Position);

                return null;
            }
        }

        private class InfixSumParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Sums; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new Sum<T>(left, right, pos);
            }
        }

        private class InfixDifferenceParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Sums; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new Minus<T>(left, right, pos);
            }
        }

        private class InfixProductParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Products; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new Multiply<T>(left, right, pos);
            }
        }

        private class InfixDivideParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Products; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new Division<T>(left, right, pos);
            }
        }

        private class InfixModulusParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Products; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new Modulus<T>(left, right, pos);
            }
        }

        private class InfixBitwiseOrParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Or; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new BitwiseOr<T>(left, right, pos);
            }
        }

        private class InfixBitwiseXorParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Xor; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new BitwiseXor<T>(left, right, pos);
            }
        }

        private class InfixBitwiseAndParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.And; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new BitwiseAnd<T>(left, right, pos);
            }
        }

        private class InfixLShiftParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Shifts; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new BitShiftLeft<T>(left, right, pos);
            }
        }

        private class InfixRShiftParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Shifts; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new BitShiftRight<T>(left, right, pos);
            }
        }

        private class InfixARShiftParselet : InfixBinaryOperatorParselet
        {
            public override int Precedence { get => (int)Precedences.Shifts; }

            public override IExpression<T> MakeExpression(IExpression<T> left, IExpression<T> right, FilePosition pos)
            {
                return new ArithmeticShiftRight<T>(left, right, pos);
            }
        }
    }
}
