using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nintenlord.Collections;
using Nintenlord.Collections.Trees;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.Event_Assembler.Core.Code.Language.Types.IntegerRepresentations;
using Nintenlord.Event_Assembler.Core.Code.Templates;
using Nintenlord.Event_Assembler.Core.IO.Input;
using Nintenlord.Event_Assembler.Core.IO.Logs;
using Nintenlord.IO;
using Nintenlord.Parser;
using Nintenlord.Utility;
using Nintenlord.Utility.Primitives;
using EAType = Nintenlord.Event_Assembler.Core.Code.Language.Types.Type;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Assembly
{
    public sealed class EAExpressionAssembler<T>
    {
        private readonly IParser<Token, IExpression<T>> parser;
        private readonly ICodeTemplateStorer storer;
        private readonly StringComparison stringComparison;
        private readonly IIntegerType<T> intType;
        private readonly IPointerMaker<T> pointerMaker;
        private readonly Dictionary<EAExpressionType, 
            Func<IExpression<T>, AssemblyContext<T>, FirstPassResult>> firstPassHandlers;
        private readonly Dictionary<EAExpressionType,
            Action<IExpression<T>, AssemblyContext<T>>> secondPassHandlers;

        public EAExpressionAssembler(ICodeTemplateStorer storer, IParser<Token, IExpression<T>> parser, IIntegerType<T> intType, IPointerMaker<T> pointerMaker)
        {
            this.parser = parser;
            this.storer = storer;
            this.intType = intType;
            this.pointerMaker = pointerMaker;
            this.stringComparison = StringComparison.OrdinalIgnoreCase;

            firstPassHandlers = new Dictionary<EAExpressionType, Func<IExpression<T>, AssemblyContext<T>, FirstPassResult>>();

            firstPassHandlers[EAExpressionType.Code] = HandleCodeFirstPass;
            firstPassHandlers[EAExpressionType.Scope] = HandleScopeFirstPass;
            firstPassHandlers[EAExpressionType.Labeled] = HandleLabelFirstPass;
            firstPassHandlers[EAExpressionType.Assignment] = HandleAssignmentFirstPass;

            secondPassHandlers = new Dictionary<EAExpressionType, Action<IExpression<T>, AssemblyContext<T>>>();

            secondPassHandlers[EAExpressionType.Code] = HandleCodeSecondPass;
            secondPassHandlers[EAExpressionType.Scope] = HandleScopeSecondPass;
            secondPassHandlers[EAExpressionType.Labeled] = HandleLabelSecondPass;
            secondPassHandlers[EAExpressionType.Assignment] = HandleAssignmentSecondPass;
        }

        public void Assemble(IPositionableInputStream input, BinaryWriter output, ILog log)
        {
            var assemblyContext = new AssemblyContext<T>(log, output);

            var scanner = new TokenScanner(input);

            if (!scanner.MoveNext())
            {
                return;
            }

            Match<Token> match;
            var tree = parser.Parse(scanner, out match);
            if (!match.Success)
            {
                log.AddError(match.Error);// + " " + inputStream.PeekOriginalLine()
                return;
            }

            if (scanner.IsAtEnd)
            {
                log.AddError("Consumed all input. Shouldn't have happened.");
                return;
            }

            if (scanner.Current.Type != TokenType.EndOfStream)
            {
                log.AddError(scanner.Current.Position + ": Didn't reach end, currently at " + scanner.Current);
                return;
            }
            assemblyContext.CurrentOffset = (int)output.BaseStream.Position;
            foreach (var item in FirstPass(tree, assemblyContext))
            {
                assemblyContext.AddCodeData(item.code, item.offset, item.template);
            }
            assemblyContext.CurrentOffset = (int)output.BaseStream.Position;
            SecondPass(tree, assemblyContext);
        }

        #region First pass

        private struct FirstPassResult
        {
            public bool valid;
            public Code<T> code;
            public int offset;
            public ICodeTemplate template;

            private FirstPassResult(
                bool valid,
                Code<T> code,
                int offset,
                ICodeTemplate template)
            {
                this.valid = valid;
                this.code = code;
                this.offset = offset;
                this.template = template;
            }

            public static FirstPassResult Invalid()
            {
                return new FirstPassResult(false, null, 0, null);
            }

            public static FirstPassResult Valid(Code<T> code,
                int offset,
                ICodeTemplate template)
            {
                return new FirstPassResult(true, code, offset, template);
            }

        }

        private IEnumerable<FirstPassResult> FirstPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            return from expression1 in expression.BreadthFirstEnumerator()
                   let func = firstPassHandlers.TryGetValue(expression1.Type)
                   where !func.CausedError
                   let res = func.Result(expression1, assemblyContext)
                   where res.valid
                   select res;
        }

        private FirstPassResult HandleAssignmentFirstPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            Assingment<T> assignment = (Assingment<T>)expression;
            if (assignment.VariableCount != 0)
            {
                assemblyContext.AddError(assignment, "Assignments with parameters aren't supported.");
            }
            else
            {
                assemblyContext.CurrentScope.AddNewSymbol(assignment.Name.Name, assignment.Result);
            }
            return FirstPassResult.Invalid();
        }

        private FirstPassResult HandleLabelFirstPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            LabeledExpression<T> labelExp = (LabeledExpression<T>)expression;
            assemblyContext.CurrentScope.AddNewSymbol((labelExp).LabelName,
                               new ValueExpression<T>(intType.FromInt(assemblyContext.CurrentOffset), default(FilePosition)));
            return FirstPassResult.Invalid();
        }

        private FirstPassResult HandleScopeFirstPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            assemblyContext.SetScopeStructure(expression);
            return FirstPassResult.Invalid();
        }

        private FirstPassResult HandleCodeFirstPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            Code<T> code = (Code<T>)expression;

            if (code.IsEmpty)
                return FirstPassResult.Invalid();

            if (HandleBuiltInCode(code, assemblyContext, false))
                return FirstPassResult.Invalid();

            var paramTypes = code.Parameters.Select(EAType.GetType).ToArray();
            var templateError = storer.FindTemplate(code.CodeName.Name, paramTypes);

            if (templateError.CausedError)
            {
                assemblyContext.AddError(code, templateError);
                return FirstPassResult.Invalid();
            }
            else
            {
                var template = templateError.Result;
                int oldOffset = assemblyContext.CurrentOffset;
                assemblyContext.CurrentOffset += template.GetLengthBytes(code.Parameters);
                return FirstPassResult.Valid(code, oldOffset, template);
            }
        }

        #endregion

        #region Second pass

        private void SecondPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            foreach (var expression1 in expression.BreadthFirstEnumerator())
            {
                var action = secondPassHandlers.TryGetValue(expression1.Type);
                if (!action.CausedError)
                {
                    action.Result(expression1, assemblyContext);
                }
            }
        }

        private void HandleAssignmentSecondPass(
            IExpression<T> assignment,
            AssemblyContext<T> assemblyContext)
        {
            throw new NotImplementedException();
        }

        private void HandleScopeSecondPass(
            IExpression<T> newScope,
            AssemblyContext<T> assemblyContext)
        {
            assemblyContext.SetScopeStructure(newScope);
            foreach (var child in newScope.GetChildren())
            {
                SecondPass(child, assemblyContext);
            }
        }

        private void HandleLabelSecondPass(
            IExpression<T> label,
            AssemblyContext<T> assemblyContext)
        {
            foreach (var child in label.GetChildren())
            {
                SecondPass(child, assemblyContext);
            }
        }

        private void HandleCodeSecondPass(
            IExpression<T> expression,
            AssemblyContext<T> assemblyContext)
        {
            Code<T> code = (Code<T>)expression;
            if (code.IsEmpty)
                return;
            
            if (HandleBuiltInCode(code, assemblyContext, true))
                return;
            
            int codeOffset;
            ICodeTemplate codeTemplate;
            if (!assemblyContext.TryGetCodeData(code, out codeOffset, out codeTemplate))
            {
                return;
            }
            assemblyContext.CurrentOffset = codeOffset;

            if (codeOffset % codeTemplate.OffsetMod != 0)
            {
                assemblyContext.AddError(code,
                                         "Code {0}'s offset {1} is not divisible by {2}",
                                         codeTemplate.Name, codeOffset.ToHexString("$"),
                                         codeTemplate.OffsetMod);
            }

            if (assemblyContext.Output.BaseStream.Position != assemblyContext.CurrentOffset)
            {
                if (!assemblyContext.Output.BaseStream.CanSeek)
                {
                    assemblyContext.AddError(code, "Stream cannot be seeked.");
                }
                else
                {
                    assemblyContext.Output.BaseStream.Seek(assemblyContext.CurrentOffset, SeekOrigin.Begin);
                }
            }

            CanCauseError<byte[]> rawData = codeTemplate.GetData(code.Parameters, 
                x => GetSymbolVal(x, assemblyContext), intType, pointerMaker);

            if (rawData.CausedError)
            {
                assemblyContext.AddError(code, rawData);
            }
            else
            {
                assemblyContext.Log.AddMessage(code.ToString() + rawData.Result.ToElementWiseString(" ", "[", "]"));
                assemblyContext.Output.Write(rawData.Result);
            }
            assemblyContext.CurrentOffset = (int)assemblyContext.Output.BaseStream.Position;
        }

        #endregion

        #region Getting string

        private string ExpressionToString(IExpression<T> exp, AssemblyContext<T> assemblyContext)
        {
            switch (exp.Type)
            {
                case EAExpressionType.List:
                    return exp.GetChildren().ToElementWiseString(", ", "[", "]");

                case EAExpressionType.Code:
                    var code = (Code<T>)exp;

                    return code.CodeName.Name + code.Parameters.Select(
                        x => ExpressionToString(x, assemblyContext)).ToElementWiseString(" ", " ", "");
                case EAExpressionType.XOR:
                case EAExpressionType.AND:
                case EAExpressionType.OR:
                case EAExpressionType.LeftShift:
                case EAExpressionType.RightShift:
                case EAExpressionType.Division:
                case EAExpressionType.Multiply:
                case EAExpressionType.Modulus:
                case EAExpressionType.Minus:
                case EAExpressionType.Sum:
                case EAExpressionType.Value:
                case EAExpressionType.Symbol:
                    return (from val in Folding.Fold(exp, y => GetSymbolVal(y, assemblyContext), intType)
                            select intType.ToString(val, 16)).ValueOrDefault(exp.ToString);
                //case EAExpressionType.Scope:
                //case EAExpressionType.Labeled:
                //case EAExpressionType.Assignment:
                default:
                    throw new ArgumentException("malformed tree");
            }
        }

        private CanCauseError<T> GetSymbolVal(string symbolName, AssemblyContext<T> assemblyContext)
        {
            if (symbolName.Equals(currentOffsetCode, stringComparison) ||
                symbolName.Equals(offsetChanger, stringComparison))
            {
                return intType.FromInt(assemblyContext.CurrentOffset);
            }
            else
            {
                return from expression in assemblyContext.CurrentScope.GetSymbolValue(symbolName)
                       from foldResult in Folding.Fold(expression, x => GetSymbolVal(x, assemblyContext), intType)
                       select foldResult;
            }
        }

        #endregion
        
        #region Built-in codes

        private const string currentOffsetCode = "CURRENTOFFSET";
        private const string messagePrinterCode = "MESSAGE";
        private const string errorPrinterCode = "ERROR";
        private const string warningPrinterCode = "WARNING";
        private const string offsetAlinger = "ALIGN";
        private const string offsetChanger = "ORG";

        private bool HandleBuiltInCode(Code<T> code, AssemblyContext<T> assemblyContext, bool addToLog)
        {
            string text;
            switch (code.CodeName.Name)
            {
                case messagePrinterCode:
                    if (addToLog)
                    {
                        text = ExpressionToString(code, assemblyContext);
                        assemblyContext.Log.AddMessage(text.Substring(code.CodeName.Name.Length + 1));
                    }
                    return true;
                case errorPrinterCode:
                    if (addToLog)
                    {
                        text = ExpressionToString(code, assemblyContext);
                        assemblyContext.Log.AddError(text.Substring(code.CodeName.Name.Length + 1));
                    }
                    return true;
                case warningPrinterCode:
                    if (addToLog)
                    {
                        text = ExpressionToString(code, assemblyContext);
                        assemblyContext.Log.AddWarning(text.Substring(code.CodeName.Name.Length + 1));
                    }
                    return true;
                case currentOffsetCode:
                case offsetAlinger:
                    if (code.ParameterCount.IsInRange(1, 1))
                    {
                        if (code[0] is ExpressionList<T>)
                        {
                            if (addToLog)
                            {
                                assemblyContext.AddNotAtomTypeParameter(code[0]);
                            }
                        }
                        else
                        {
                            var align = Folding.Fold(code[0], x => CanCauseError<T>.Error("No symbols available."), intType);
                            if (align.CausedError)
                            {
                                if (addToLog)
                                {
                                    assemblyContext.AddError(code, align);
                                }
                            }
                            else
                            {
                                assemblyContext.CurrentOffset = assemblyContext.CurrentOffset.ToMod(intType.GetIntValue(align.Result));
                            }
                        }
                    }
                    else
                    {
                        if (addToLog)
                        {
                            assemblyContext.AddNotCorrectParameters(code, 1);
                        }
                    }
                    return true;
                case offsetChanger:
                    if (code.ParameterCount.IsInRange(1, 1))
                    {
                        if (code[0] is ExpressionList<T>)
                        {
                            if (addToLog)
                            {
                                assemblyContext.AddNotAtomTypeParameter(code[0]);
                            }
                        }
                        else
                        {
                            var newOffset = Folding.Fold(code[0], x => CanCauseError<T>.Error("No symbols available."), intType);
                            if (newOffset.CausedError)
                            {
                                if (addToLog)
                                {
                                    assemblyContext.AddError(code, newOffset);
                                }
                            }
                            else
                            {
                                assemblyContext.CurrentOffset = intType.GetIntValue(newOffset.Result);
                            }
                        }
                    }
                    else
                    {
                        if (addToLog)
                        {
                            assemblyContext.AddNotCorrectParameters(code, 1);
                        }
                    }
                    return true;
                default:
                    return false;
            }
        }

        #endregion
    }

    interface IPass<TContext,TResult>
    {
        
    }
}
