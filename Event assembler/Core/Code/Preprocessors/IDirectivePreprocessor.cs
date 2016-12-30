// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Preprocessors.IDirectivePreprocessor
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Event_Assembler.Core.Code.Preprocessors.BuiltInMacros;
using Nintenlord.Event_Assembler.Core.Collections;
using Nintenlord.Event_Assembler.Core.IO.Input;
using Nintenlord.Event_Assembler.Core.IO.Logs;
using System;
using System.Collections.Generic;

namespace Nintenlord.Event_Assembler.Core.Code.Preprocessors
{
  internal interface IDirectivePreprocessor : IPreprocessor, IDisposable
  {
    Stack<bool> Include { get; }

    IDefineCollection DefCol { get; }

    Pool Pool { get; }

    IInputStream Input { get; }

    ILog Log { get; }

    bool IsValidToDefine(string name);

    bool IsPredefined(string name);
  }
}
