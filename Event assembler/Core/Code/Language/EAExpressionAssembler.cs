// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.EAExpressionAssembler
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Collections;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.Event_Assembler.Core.Code.Templates;
using Nintenlord.Event_Assembler.Core.IO.Input;
using Nintenlord.Event_Assembler.Core.IO.Logs;
using Nintenlord.IO;
using Nintenlord.Parser;
using Nintenlord.Utility;
using Nintenlord.Utility.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nintenlord.Event_Assembler.Core.Code.Language
{
    internal sealed class EAExpressionAssembler
    {
        private const string currentOffsetCode = "CURRENTOFFSET";
        private const string messagePrinterCode = "MESSAGE";
        private const string errorPrinterCode = "ERROR";
        private const string warningPrinterCode = "WARNING";
        private const string offsetAligner = "ALIGN";
        private const string offsetChanger = "ORG";
        private const string offsetPusher = "PUSH";
        private const string offsetPopper = "POP";
        private const string assertion = "ASSERT";
        private const string protectCode = "PROTECT";
        private readonly IParser<Token, IExpression<int>> parser;
        private readonly ICodeTemplateStorer storer;
        private ILog log;
        private Dictionary<IExpression<int>, ScopeStructure<int>> scopeStructures;
        private Dictionary<Code<int>, Tuple<int, ICodeTemplate>> codeOffsets;
        private int currentOffset;
        private Stack<int> offsetHistory;
        private List<Tuple<int, int>> protectedRegions;

        public EAExpressionAssembler(ICodeTemplateStorer storer, IParser<Token, IExpression<int>> parser)
        {
            this.parser = parser;
            this.storer = storer;
            this.offsetHistory = new Stack<int>();
            this.protectedRegions = new List<Tuple<int, int>>();
        }

        public void Assemble(IPositionableInputStream input, BinaryWriter output, ILog log)
        {
            this.log = log;
            this.scopeStructures = new Dictionary<IExpression<int>, ScopeStructure<int>>();
            this.codeOffsets = new Dictionary<Code<int>, Tuple<int, ICodeTemplate>>();
            TokenScanner tokenScanner = new TokenScanner(input);
            if (!tokenScanner.MoveNext())
                return;
            Match<Token> match;
            IExpression<int> expression = parser.Parse(tokenScanner, out match);
            if (!match.Success)
                log.AddError(match.Error);
            else if (!tokenScanner.IsAtEnd && tokenScanner.Current.Type != TokenType.EndOfStream)
            {
                log.AddError(tokenScanner.Current.Position.ToString() + ": Didn't reach end, currently at " + tokenScanner.Current.ToString());
            }
            else
            {
                this.currentOffset = (int)output.BaseStream.Position;
                foreach (Tuple<Code<int>, int, ICodeTemplate> tuple in FirstPass(expression, null))
                {
                    int codeLength = tuple.Item3.GetLengthBytes(tuple.Item1.Parameters.ToArray());
                    if (IsProtected(tuple.Item2, codeLength)) //Offset, length
                    {
                        log.AddError(tuple.Item1.Position.ToString() + ": Attempting to modify protected memory at " + tuple.Item2.ToHexString("$") + " with code of length " + codeLength);
                    }
                    else
                    {
                        this.codeOffsets.Add(tuple.Item1, Tuple.Create(tuple.Item2, tuple.Item3));
                    }
                }
                this.currentOffset = (int)output.BaseStream.Position;
                this.offsetHistory.Clear();
                this.SecondPass(output, expression, null);
            }
        }

        public IEnumerable<KeyValuePair<string, int>> GetGlobalSymbols()
        {
            foreach (ScopeStructure<int> scope in scopeStructures.Values)
            {
                if (scope.IsGlobalScope())
                {
                    foreach (KeyValuePair<string, IExpression<int>> pair in scope.GetSymbols())
                    {
                        int? value = GetSymbolVal(scope, pair.Key);

                        if (value.HasValue)
                            yield return new KeyValuePair<string, int>(pair.Key, value.Value);
                    }
                }
            }
        }

        private IEnumerable<Tuple<Code<int>, int, ICodeTemplate>> FirstPass(IExpression<int> expression, ScopeStructure<int> scope)
        {
            switch (expression.Type)
            {
                case EAExpressionType.Scope:
                    Scope<int> newScope = (Scope<int>)expression;
                    ScopeStructure<int> newStr = new ScopeStructure<int>(scope);
                    scopeStructures[newScope] = newStr;

                    foreach (IExpression<int> child in expression.GetChildren())
                        foreach (Tuple<Code<int>, int, ICodeTemplate> tuple in FirstPass(child, newStr))
                            yield return tuple;

                    break;

                case EAExpressionType.Code:
                    Code<int> code = expression as Code<int>;

                    if (code.IsEmpty || HandleBuiltInCodeFirstPass(code, scope))
                        break;

                    Types.Type[] paramTypes = ((IEnumerable<IExpression<int>>)code.Parameters).Select(new Func<IExpression<int>, Types.Type>(Types.Type.GetType<int>)).ToArray();

                    CanCauseError<ICodeTemplate> templateError = this.storer.FindTemplate(code.CodeName.Name, paramTypes);

                    if (templateError.CausedError)
                    {
                        AddError<int, ICodeTemplate>((IExpression<int>)code, templateError);
                        break;
                    }

                    ICodeTemplate template = templateError.Result;

                    int oldOffset = currentOffset;
                    currentOffset += template.GetLengthBytes(((IEnumerable<IExpression<int>>)code.Parameters).ToArray());

                    yield return Tuple.Create(code, oldOffset, template);
                    break;

                case EAExpressionType.Labeled:
                    {
                        CanCauseError err = scope.AddNewSymbol(((LabelExpression<int>)expression).LabelName, new ValueExpression<int>(this.currentOffset, new FilePosition()));

                        if (err.CausedError)
                        {
                            AddWarning(expression, err.ErrorMessage);
                        }

                        break;
                   }

                case EAExpressionType.Assignment:
                    {
                        Assignment<int> assingment = (Assignment<int>)expression;
                        CanCauseError<int> value = Folding.Fold(assingment.Value, x => this.GetSymbolVal(scope, x));

                        CanCauseError err = null;

                        if (value.CausedError)
                            err = scope.AddNewSymbol(assingment.Name, assingment.Value);
                        else
                            err = scope.AddNewSymbol(assingment.Name, new ValueExpression<int>(value.Result, new FilePosition()));

                        if (err.CausedError)
                        {
                            AddWarning(expression, err.ErrorMessage);
                        }

                        break;
                    }

                default:
                    throw new ArgumentException("Badly formed tree.");
            }
        }

        private void SecondPass(BinaryWriter output, IExpression<int> expression, ScopeStructure<int> parentScope)
        {
            if (expression.Type == EAExpressionType.Scope)
            {
                ScopeStructure<int> parentScope1 = this.scopeStructures[expression];
                foreach (IExpression<int> child in expression.GetChildren())
                    this.SecondPass(output, child, parentScope1);
            }
            else if (expression.Type == EAExpressionType.Code)
            {
                Code<int> code = expression as Code<int>;

                if (code.IsEmpty || HandleBuiltInCodeSecondPass(code, parentScope) || !codeOffsets.TryGetValue(code, out Tuple<int, ICodeTemplate> tupple))
                    return;

                this.currentOffset = tupple.Item1;
                if (tupple.Apply((Func<int, ICodeTemplate, bool>)((x, y) => x % y.OffsetMod != 0)))
                    this.AddError<int>((IExpression<int>)code, "Code {0}'s offset {1} is not divisible by {2}", (object)tupple.Item2.Name, (object)this.currentOffset.ToHexString("$"), (object)tupple.Item2.OffsetMod);
                if (output.BaseStream.Position != (long)this.currentOffset)
                {
                    if (!output.BaseStream.CanSeek)
                        this.AddError<int>((IExpression<int>)code, "Stream cannot be seeked.");
                    else
                        output.BaseStream.Seek((long)this.currentOffset, SeekOrigin.Begin);
                }
                CanCauseError<byte[]> data = tupple.Item2.GetData(code.Parameters, x => this.GetSymbolVal(parentScope, x));
                if (data.CausedError)
                    this.AddError<int, byte[]>((IExpression<int>)code, data);
                else
                    output.Write(data.Result);
                this.currentOffset = (int)output.BaseStream.Position;
            }
            else if (expression.Type == EAExpressionType.Labeled)
            {
                foreach (IExpression<int> child in expression.GetChildren())
                    this.SecondPass(output, child, parentScope);
            }
            else if (expression.Type != EAExpressionType.Assignment)
                throw new ArgumentException("Badly formed tree.");
        }

        private string ExpressionToString(IExpression<int> exp, ScopeStructure<int> scope)
        {
            switch (exp.Type)
            {
                case EAExpressionType.Code:
                    Code<int> code = exp as Code<int>;
                    return code.CodeName.Name + ((IEnumerable<IExpression<int>>)code.Parameters).Select((x => this.ExpressionToString(x, scope))).ToElementWiseString<string>(" ", " ", "");
                case EAExpressionType.XOR:
                case EAExpressionType.AND:
                case EAExpressionType.OR:
                case EAExpressionType.LeftShift:
                case EAExpressionType.RightShift:
                case EAExpressionType.ArithmeticRightShift:
                case EAExpressionType.Division:
                case EAExpressionType.Multiply:
                case EAExpressionType.Modulus:
                case EAExpressionType.Minus:
                case EAExpressionType.Sum:
                case EAExpressionType.Value:
                case EAExpressionType.Symbol:
                    CanCauseError<int> canCauseError = Folding.Fold(exp, (y => this.GetSymbolVal(scope, y)));
                    if (canCauseError.CausedError)
                        return exp.ToString();
                    return canCauseError.Result.ToHexString("0x");
                case EAExpressionType.List:
                    return exp.GetChildren().ToElementWiseString<IExpression<int>>(", ", "[", "]");
                default:
                    throw new ArgumentException("malformed tree");
            }
        }

        private int? GetSymbolVal(ScopeStructure<int> scope, string symbolName)
        {
            if (symbolName.Equals(currentOffsetCode, StringComparison.OrdinalIgnoreCase) || symbolName.Equals(offsetChanger, StringComparison.OrdinalIgnoreCase))
                return new int?(this.currentOffset);
            CanCauseError<IExpression<int>> symbolValue = scope.GetSymbolValue(symbolName);
            if (symbolValue.CausedError)
                return new int?();
            CanCauseError<int> canCauseError = Folding.Fold(symbolValue.Result, (x => this.GetSymbolVal(scope, x)));
            if (canCauseError.CausedError)
                return new int?();
            return new int?(canCauseError.Result);
        }

        private bool HandleBuiltInCodeFirstPass(Code<int> code, ScopeStructure<int> scope)
        {
            switch (code.CodeName.Name)
            {
                case messagePrinterCode:
                case errorPrinterCode:
                case warningPrinterCode:
                    return true;
                case currentOffsetCode:
                case offsetAligner:
                    if (code.ParameterCount.IsInRange(1, 1) && !(code[0] is ExpressionList<int>))
                    {
                        CanCauseError<int> canCauseError = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                        if (!canCauseError.CausedError)
                            this.currentOffset = this.currentOffset.ToMod(canCauseError.Result);
                    }
                    return true;
                case offsetChanger:
                    if (code.ParameterCount.IsInRange(1, 1) && !(code[0] is ExpressionList<int>))
                    {
                        CanCauseError<int> canCauseError = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                        if (!canCauseError.CausedError)
                            this.currentOffset = canCauseError.Result;
                    }
                    return true;
                case offsetPusher:
                    if (code.ParameterCount.IsInRange(0, 0))
                    {
                        this.offsetHistory.Push(this.currentOffset);
                    }
                    return true;
                case offsetPopper:
                    if (code.ParameterCount.IsInRange(0, 0))
                    {
                        if (this.offsetHistory.Count > 0)
                            this.currentOffset = this.offsetHistory.Pop();
                    }
                    return true;
                case assertion:
                    return true;
                case protectCode:
                    if (code.ParameterCount.IsInRange(1, 1))
                    {
                        CanCauseError<int> canCauseError = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                        if (!canCauseError.CausedError)
                            this.protectedRegions.Add(new Tuple<int, int>(canCauseError.Result, 4));
                    }
                    else if (code.ParameterCount.IsInRange(2, 2))
                    {
                        CanCauseError<int> firstParam = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                        CanCauseError<int> secondParam = Folding.Fold(code[1], (x => this.GetSymbolVal(scope, x)));
                        if (!firstParam.CausedError && !secondParam.CausedError)
                        {
                            if (firstParam.Result < secondParam.Result)
                                this.protectedRegions.Add(new Tuple<int, int>(firstParam.Result, secondParam.Result - firstParam.Result));
                            else
                                this.log.AddWarning("Protected region not valid (end offset not after start offset). No region protected.");
                        }
                    }
                    else
                    {
                        this.AddNotCorrectParameters<int>(code, 2);
                    }
                    return true;
                default:
                    return false;
            }
        }

        private bool HandleBuiltInCodeSecondPass(Code<int> code, ScopeStructure<int> scope)
        {
            switch (code.CodeName.Name)
            {
                case messagePrinterCode:
                    this.log.AddMessage(this.ExpressionToString((IExpression<int>)code, scope).Substring(code.CodeName.Name.Length + 1));
                    return true;
                case errorPrinterCode:
                    this.log.AddError(this.ExpressionToString((IExpression<int>)code, scope).Substring(code.CodeName.Name.Length + 1));
                    return true;
                case warningPrinterCode:
                    this.log.AddWarning(this.ExpressionToString((IExpression<int>)code, scope).Substring(code.CodeName.Name.Length + 1));
                    return true;
                case currentOffsetCode:
                case offsetAligner:
                    if (code.ParameterCount.IsInRange(1, 1))
                    {
                        if (code[0] is ExpressionList<int>)
                        {
                            this.AddNotAtomTypeParameter<int>(code[0]);
                        }
                        else
                        {
                            CanCauseError<int> error = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                            if (error.CausedError)
                                this.AddError<int, int>((IExpression<int>)code, error);
                            else
                                this.currentOffset = this.currentOffset.ToMod(error.Result);
                        }
                    }
                    else
                        this.AddNotCorrectParameters<int>(code, 1);
                    return true;
                case offsetChanger:
                    if (code.ParameterCount.IsInRange(1, 1))
                    {
                        if (code[0] is ExpressionList<int>)
                        {
                            this.AddNotAtomTypeParameter<int>(code[0]);
                        }
                        else
                        {
                            CanCauseError<int> error = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                            if (error.CausedError)
                                this.AddError<int, int>((IExpression<int>)code, error);
                            else if (error.Result >= 0x2000000)
                                this.AddError((IExpression<int>)code, "Tried to set offset to " + error.Result.ToHexString("0x"));
                            else
                                this.currentOffset = error.Result;
                        }
                    }
                    else
                        this.AddNotCorrectParameters<int>(code, 1);
                    return true;
                case offsetPusher:
                    if (code.ParameterCount.IsInRange(0, 0))
                        this.offsetHistory.Push(this.currentOffset);
                    else
                        this.AddNotCorrectParameters<int>(code, 0);
                    return true;
                case offsetPopper:
                    if (code.ParameterCount.IsInRange(0, 0))
                    {
                        if (this.offsetHistory.Count > 0)
                            this.currentOffset = this.offsetHistory.Pop();
                        else
                            this.AddError((IExpression<int>)code, "Tried to pop while offset stack was empty.");
                    }
                    else
                        this.AddNotCorrectParameters<int>(code, 0);
                    return true;
                case assertion:
                    if (code.ParameterCount.IsInRange(1, 1))
                    {
                        if (code[0] is ExpressionList<int>)
                        {
                            this.AddNotAtomTypeParameter<int>(code[0]);
                        }
                        else
                        {
                            CanCauseError<int> error = Folding.Fold(code[0], (x => this.GetSymbolVal(scope, x)));
                            if (error.CausedError)
                                this.AddError<int, int>((IExpression<int>)code, error);
                            else if (error.Result < 0)
                                this.AddError(code, "Assertion failed.");
                        }
                    }
                    else
                        this.AddNotCorrectParameters<int>(code, 1);
                    return true;

                case protectCode:
                    return true;
                default:
                    return false;
            }
        }

        private bool IsProtected(int offset, int length)
        {
            foreach (Tuple<int, int> protectedRegion in protectedRegions)
            {
                //They intersect if the last offset in the given region is after the start of this one
                //and the first offset in the given region is before the last of this one
                if (offset + length > protectedRegion.Item1 && offset < protectedRegion.Item1 + protectedRegion.Item2)
                    return true;
            }
            return false;
        }

        private void AddWarning<T>(IExpression<T> code, string error)
        {
            this.log.AddWarning(code.Position.ToString() + ": " + error);
        }

        private void AddWarning<T>(IExpression<T> code, string format, params object[] args)
        {
            this.log.AddWarning(code.Position.ToString() + ": " + string.Format(format, args));
        }

        private void AddError<T, TResult>(IExpression<T> code, CanCauseError<TResult> error)
        {
            this.log.AddError(code.Position.ToString() + ": " + error.ErrorMessage);
        }

        private void AddError<T>(IExpression<T> code, string error)
        {
            this.log.AddError(code.Position.ToString() + ": " + error);
        }

        private void AddError<T>(IExpression<T> code, string format, params object[] args)
        {
            this.log.AddError(code.Position.ToString() + ": " + string.Format(format, args));
        }

        private void AddNotAtomTypeParameter<T>(IExpression<T> parameter)
        {
            this.log.AddError("{1}: Parameter {0} doesn't have correct type.", new object[2]
            {
        (object) parameter,
        (object) parameter.Position
            });
        }

        private void AddNotCorrectParameters<T>(Nintenlord.Event_Assembler.Core.Code.Language.Expression.Code<T> code, int paramCount)
        {
            this.log.AddError("{3}: Code {0} doesn't have {2} parameters, but has {1} parameters", (object)code.CodeName, (object)paramCount, (object)code.Parameters.Length, (object)code.Position);
        }

        private void AddNotCorrectParameters<T>(Nintenlord.Event_Assembler.Core.Code.Language.Expression.Code<T> code, int paramMin, int paramMax)
        {
            this.log.AddError("{4}: Code {0} doesn't have {3} parameters, but has {1}-{2} parameters", (object)code.CodeName, (object)paramMin, (object)paramMax, (object)code.Parameters.Length, (object)code.Position);
        }
    }
}
