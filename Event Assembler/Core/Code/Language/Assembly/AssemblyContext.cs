using System;
using System.Collections.Generic;
using System.IO;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression.Tree;
using Nintenlord.Event_Assembler.Core.Code.Templates;
using Nintenlord.Event_Assembler.Core.IO.Logs;
using Nintenlord.Utility;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Assembly
{
    sealed class AssemblyContext<T>
    {
        private struct CodeData
        {
            public int Offset;
            public ICodeTemplate CodeTemplate;

            public CodeData(int offset, ICodeTemplate codeTemplate)
            {
                this.Offset = offset;
                this.CodeTemplate = codeTemplate;
            }
        }

        private Dictionary<IExpression<T>, ScopeStructure<T>> scopeStructures;
        private Dictionary<Code<T>, CodeData> codeOffsets;


        public AssemblyContext(ILog log, BinaryWriter output)
        {
            this.Log = log;
            this.scopeStructures = new Dictionary<IExpression<T>, ScopeStructure<T>>();
            this.codeOffsets = new Dictionary<Code<T>, CodeData>();
            this.Output = output;
        }

        public ILog Log { get; private set; }
        public ScopeStructure<T> CurrentScope { get; private set; }
        public int CurrentOffset { get; set; }
        public BinaryWriter Output { get; private set; }

        public void SetScopeStructure(IExpression<T> expression)
        {
            ScopeStructure<T> newCurrent;
            if (!scopeStructures.TryGetValue(expression, out newCurrent))
            {
                newCurrent = new ScopeStructure<T>(CurrentScope);
                scopeStructures[expression] = newCurrent;
            }
            CurrentScope = newCurrent;
        }

        public void SetToNoScope()
        {
            CurrentScope = null;
        }

        public void PopScope()
        {
            CurrentScope = CurrentScope.ParentScope;
        }


        
        public void AddCodeData(Code<T> code, int offset, ICodeTemplate template)
        {
            codeOffsets.Add(code, new CodeData(offset, template));
        }

        public bool TryGetCodeData(Code<T> code, out int codeOffset, out ICodeTemplate template)
        {
            CodeData data;
            var result = codeOffsets.TryGetValue(code, out data);
            codeOffset = data.Offset;
            template = data.CodeTemplate;
            return result;
        }


        public void AddError<TResult>(IExpression<T> code, CanCauseError<TResult> error)
        {
            Log.AddError(code.Position + ": " + error.ErrorMessage);
        }

        public void AddError(IExpression<T> code, string error)
        {
            Log.AddError(code.Position + ": " + error);
        }

        public void AddError(IExpression<T> code, string format, params object[] args)
        {
            Log.AddError(code.Position + ": " + string.Format(format, args));
        }

        public void AddNotAtomTypeParameter(IExpression<T> parameter)
        {
            Log.AddError("{1}: Parameter {0} doesn't have correct type.",
                         parameter,
                         parameter.Position);
        }

        public void AddNotCorrectParameters(Code<T> code, int paramCount)
        {
            Log.AddError("{3}: Code {0} doesn't have {2} parameters, but has {1} parameters",
                         code.CodeName,
                         paramCount,
                         code.Parameters.Length,
                         code.Position);
        }

        public void AddNotCorrectParameters(Code<T> code, int paramMin, int paramMax)
        {
            Log.AddError("{4}: Code {0} doesn't have {3} parameters, but has {1}-{2} parameters",
                         code.CodeName,
                         paramMin,
                         paramMax,
                         code.Parameters.Length,
                         code.Position);
        }
    }
}