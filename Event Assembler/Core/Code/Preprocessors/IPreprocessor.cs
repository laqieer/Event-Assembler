﻿// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Preprocessors.IPreprocessor
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Event_Assembler.Core.IO.Input;
using System;
using System.Collections.Generic;

namespace Nintenlord.Event_Assembler.Core.Code.Preprocessors
{
  public interface IPreprocessor : IDisposable
  {
    string Process(string line, IInputStream inputStream);

    void AddDefined(IEnumerable<string> original);

    void AddReserved(IEnumerable<string> reserved);
  }
}
