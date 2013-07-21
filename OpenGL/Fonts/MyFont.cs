using System.Collections.Generic;

namespace GraphicLib.OpenGL.Fonts
{

  internal class MyFont
  {
    internal FontInfo FontInfo;
    internal readonly Dictionary<char, GlyphData> Symbols;
    private readonly Dictionary<KeyValuePair<char, char>, int> _kerningPairs;

    internal MyFont(FontInfo fontInfo)
    {
      this.FontInfo = fontInfo;
      Symbols = new Dictionary<char, GlyphData>();
      _kerningPairs = new Dictionary<KeyValuePair<char, char>, int>(new MyCharCharComparer());
    }

    internal void AddSymbol(char symbol, GlyphData glyphData)
    {
      if(Symbols.ContainsKey(symbol))
        return;
      Symbols.Add(symbol, glyphData);
    }

    internal void AddKerningPair(char symbol1, char symbol2, int delta)
    {
      if(_kerningPairs.ContainsKey(new KeyValuePair<char, char>(symbol1, symbol2)))
        return;
      _kerningPairs.Add(new KeyValuePair<char, char>(symbol1, symbol2), delta);
    }
  }
}
