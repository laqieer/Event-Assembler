// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Code.Language.FE8CodeLanguage
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using System;
using System.Collections.Generic;

namespace Nintenlord.Event_Assembler.Core.Code.Language
{
  public static class FE8CodeLanguage
  {
    public static readonly string Name = "FE8";
    public static readonly Tuple<string, List<Priority>>[][] PointerList = new Tuple<string, List<Priority>>[10][]{ new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("TurnBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("CharacterBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("LocationBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[1]{ new Tuple<string, List<Priority>>("MiscBasedEvents", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[4]{ new Tuple<string, List<Priority>>("Dunno1", EACodeLanguage.MainPriorities), new Tuple<string, List<Priority>>("Dunno2", EACodeLanguage.MainPriorities), new Tuple<string, List<Priority>>("Dunno3", EACodeLanguage.MainPriorities), new Tuple<string, List<Priority>>("Tutorial", EACodeLanguage.MainPriorities) }, new Tuple<string, List<Priority>>[2]{ new Tuple<string, List<Priority>>("Traps1", EACodeLanguage.TrapPriorities), new Tuple<string, List<Priority>>("Traps2", EACodeLanguage.TrapPriorities) }, new Tuple<string, List<Priority>>[2]{ new Tuple<string, List<Priority>>("Units1", EACodeLanguage.UnitPriorities), new Tuple<string, List<Priority>>("Units2", EACodeLanguage.UnitPriorities) }, new Tuple<string, List<Priority>>[3]{ new Tuple<string, List<Priority>>("SkirmishUnitsAlly1", EACodeLanguage.UnitPriorities), new Tuple<string, List<Priority>>("SkirmishUnitsAlly2", EACodeLanguage.UnitPriorities), new Tuple<string, List<Priority>>("SkirmishUnitsAlly3", EACodeLanguage.UnitPriorities) }, new Tuple<string, List<Priority>>[3]{ new Tuple<string, List<Priority>>("SkirmishUnitsEnemy1", EACodeLanguage.UnitPriorities), new Tuple<string, List<Priority>>("SkirmishUnitsEnemy2", EACodeLanguage.UnitPriorities), new Tuple<string, List<Priority>>("SkirmishUnitsEnemy3", EACodeLanguage.UnitPriorities) }, new Tuple<string, List<Priority>>[2]{ new Tuple<string, List<Priority>>("BeginningScene", EACodeLanguage.NormalPriorities), new Tuple<string, List<Priority>>("EndingScene", EACodeLanguage.NormalPriorities) } };
    public static readonly string[] Types = new string[32]{ "Offset", "Character", "Class", "Item", "AI", "MiscUnitData", "UnitAffiliation", "Frames", "Text", "TileXCoord", "TileYCoord", "Turn", "TurnMoment", "EventID", "ConditionalID", "MapChangeID", "ChapterID", "Background", "Cutscene", "Music", "Weather", "VisionRange", "BubbleType", "AmountOfMoney", "VillageOrMoney", "MenuCommand", "ChestData", "BallistaType", "MoveManualAction", "WorldMapID", "PixelXCoord", "PixelYCoord" };
  }
}
