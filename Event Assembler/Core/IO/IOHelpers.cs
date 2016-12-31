// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.IO.IOHelpers
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

using Nintenlord.Event_Assembler.Core.Collections;
using System;
using System.IO;

namespace Nintenlord.Event_Assembler.Core.IO
{
  internal static class IOHelpers
  {
    public static string FindFile(string currentFile, string newFile)
    {
      newFile = newFile.Trim('"');
      if (File.Exists(newFile))
        return newFile;
      if (!string.IsNullOrEmpty(currentFile))
      {
        string path = Path.Combine(Path.GetDirectoryName(currentFile), newFile);
        if (File.Exists(path))
          return path;
      }
      return string.Empty;
    }

    public static void DefineFile(string path, IDefineCollection defCol)
    {
      StreamReader streamReader = new StreamReader(path);
      while (!streamReader.EndOfStream)
      {
        if (streamReader.ReadLine().Length > 0)
        {
          string[] strArray = (string[]) null;
          for (int index = 1; index < strArray.Length; ++index)
            defCol.Add(strArray[index], strArray[0]);
        }
      }
      streamReader.Close();
    }

    public static char? ReadCharacter(this TextReader reader)
    {
      int num = reader.Read();
      if (num == -1)
        return new char?();
      return new char?(Convert.ToChar(num));
    }

    public static char? PeekCharacter(this TextReader reader)
    {
      int num = reader.Peek();
      if (num == -1)
        return new char?();
      return new char?(Convert.ToChar(num));
    }
  }
}
