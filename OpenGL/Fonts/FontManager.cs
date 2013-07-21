using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NLog;

namespace GraphicLib.OpenGL.Fonts
{
  internal static class FontManager
  {
    private static readonly List<MyFont> Fonts = new List<MyFont>();
    internal static Texture FontTexture;

    internal static void LoadFonts(string path)
    {
      Fonts.Clear();
      DirectoryInfo diForListLoad = new DirectoryInfo(path);
      FileInfo[] fileList = diForListLoad.GetFiles();
      foreach(FileInfo i in fileList.Where(i => i.Extension == ".fnt"))
      {
        ParseFontFile(path, i.FullName);
      }
      if(Fonts.Count == 0)
      {
        throw new ArgumentException(String.Format("No valid font files in {0} directory", path));
      }
    }

    private static void ParseBasicFontInfo(string[] line, out FontInfo fontInfo)
    {
      fontInfo = new FontInfo {Name = "", Bold = false, Italic = false, Pt = -1};
      for(int i = line.Count() - 1; i >= 0; i--)
      {
        switch(line[i])
        {
          case "italic":
            fontInfo.Italic = true;
            break;
          case "bold":
            fontInfo.Bold = true;
            break;
          default:
            fontInfo.Pt = Int32.Parse(line[i].Substring(0, line[i].Length - 2));
            for(int j = 0; j < i; j++)
            {
              fontInfo.Name += line[j] + "";
            }
            fontInfo.Name = fontInfo.Name.Trim();
            break;
        }
        if(fontInfo.Name != "")
        {
          break;
        }
      }
    }

    private static void ParseFontFile(string directory, string path)
    {
      try
      {
        using(StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open)))
        {
          string textureFileName = reader.ReadLine().Substring("textures: ".Length);
          FontTexture = new Texture(new Bitmap(directory + textureFileName));
          string[] line = reader.ReadLine().Split(' ');
          while(true)
          {
            FontInfo fontInfo;
            ParseBasicFontInfo(line, out fontInfo);
            string str; //, showing = "";
            Fonts.Add(new MyFont(fontInfo));
            while(true)
            {
              str = reader.ReadLine();
              if(string.IsNullOrWhiteSpace(str) 
                || str == "kerning pairs:")
              {
                break;
              }
              line = str.Split('\t');
              //showing += Char.ConvertFromUtf32(Int32.Parse(line[0]));
              Fonts[Fonts.Count - 1].AddSymbol(Char.ConvertFromUtf32(Int32.Parse(line[0]))[0],
                                               new GlyphData
                                                 {
                                                   XPos = Int32.Parse(line[1]),
                                                   YPos = Int32.Parse(line[2]),
                                                   Width = Int32.Parse(line[3]),
                                                   Height = Int32.Parse(line[4]),
                                                   XOffset = Int32.Parse(line[5]),
                                                   YOffset = Int32.Parse(line[6]),
                                                   OrigW = Int32.Parse(line[7]),
                                                   OrigH = Int32.Parse(line[8])
                                                 });
            }
            Fonts[Fonts.Count - 1].FontInfo.Height = Int32.Parse(line[8]);
            while(true)
            {
              str = reader.ReadLine();
              if(string.IsNullOrWhiteSpace(str))
              {
                return;
              }
              line = str.Split('\t');
              int check;
              if(!Int32.TryParse(line[0], out check))
              {
                line = str.Split(' ');
                break;
              }
              Fonts[Fonts.Count - 1].AddKerningPair(Char.ConvertFromUtf32(Int32.Parse(line[0]))[0],
                                                    Char.ConvertFromUtf32(Int32.Parse(line[1]))[0],
                                                    Int32.Parse(line[2]));
            }
          }
        }
      }
      catch(IOException exc)
      {
        LogManager.GetCurrentClassLogger().Error(String.Format("IOexception: {0}", exc.Message));
      }
      catch(Exception exc)
      {
        LogManager.GetCurrentClassLogger().Error(String.Format("Unexpected exception, InnerException: {0}",
                                                               exc.InnerException));
        LogManager.GetCurrentClassLogger().Error(String.Format("\tUnexpected exception, message: {0}", exc.Message));
      }
    }

    private static int CompareFonts(int myFontIndex, Font font)
    {
      int result = 0;
      if(font.Name.Equals(Fonts[myFontIndex].FontInfo.Name, StringComparison.OrdinalIgnoreCase))
      {
        result++;
      }
      int fontPt = Fonts[myFontIndex].FontInfo.Pt <= 12 ? 12 : 72;
      if(Math.Abs(fontPt - font.SizeInPoints) <= 0.1)
      {
        result++;
      }
      if(Fonts[myFontIndex].FontInfo.Bold == font.Bold)
      {
        result++;
      }
      if(Fonts[myFontIndex].FontInfo.Italic == font.Italic)
      {
        result++;
      }
      return result;
    }

    public static int FindClosestFont(Font font)
    {
      int result = 0;
      int comparePoints = CompareFonts(0, font);
      if(comparePoints == 4)
      {
        return 0;
      }
      for(int i = 1; i < Fonts.Count; i++)
      {
        int currentComparePoints = CompareFonts(i, font);
        if(currentComparePoints <= comparePoints)
        {
          continue;
        }
        result = i;
        comparePoints = currentComparePoints;
        if(comparePoints == 4)
        {
          return result;
        }
      }
      return result;
    }

    public static MyFont GetFont(int index)
    {
      return Fonts[index];
    }

    public static float TextureXCoord(float x)
    {
      return x / FontTexture.Width;
    }

    public static float TextureYCoord(float y)
    {
      return y / FontTexture.Height;
    }
  }
}