// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.FE6CodeLanguage
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using System;
using System.Collections.Generic;

namespace Nintenlord.Event_Assembler.Core.Code.Language
{
  public static class FE6CodeLanguage
  {
    public static readonly string Name = "FE6";
    public static readonly Tuple<string, List<Priority>>[][] PointerList = new Tuple<string, List<Priority>>[6][]{ new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("TurnBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("CharacterBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("LocationBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("MiscBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[2]{ new Tuple<string, List<Priority>>("EnemyUnits", EACodeLanguage.UnitPriorities), new Tuple<string, List<Priority>>("AllyUnits", EACodeLanguage.UnitPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("EndingScene", EACodeLanguage.NormalPriorities) } };
  }
}
