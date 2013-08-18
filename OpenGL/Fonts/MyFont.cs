using System.Collections.Generic;

namespace GraphicLib.OpenGL.Fonts
{

  internal class MyFont
  {
    internal FontInfo FontInfo;
    internal Texture FontTexture;
    internal readonly Dictionary<char, GlyphData> Symbols;
    private readonly Dictionary<KeyValuePair<char, char>, float> _kerningPairs;

    internal MyFont(FontInfo fontInfo, Texture texture)
    {
      FontInfo = fontInfo;
      FontTexture = texture;
      Symbols = new Dictionary<char, GlyphData>();
      _kerningPairs = new Dictionary<KeyValuePair<char, char>, float>(new MyCharCharComparer());
    }

    internal void AddSymbol(char symbol, GlyphData glyphData)
    {
      if(Symbols.ContainsKey(symbol))
        return;
      Symbols.Add(symbol, glyphData);
    }

    internal void AddKerningPair(char symbol1, char symbol2, float delta)
    {
      if(_kerningPairs.ContainsKey(new KeyValuePair<char, char>(symbol1, symbol2)))
        return;
      _kerningPairs.Add(new KeyValuePair<char, char>(symbol1, symbol2), delta);
    }

    internal float GetKerningDelta(char leftChar, char rightChar)
    {
      KeyValuePair<char, char> pair = new KeyValuePair<char, char>(leftChar, rightChar);
      if(_kerningPairs.ContainsKey(pair))
      {
        return _kerningPairs[pair];
      }
      return 0.0f;
    }
  }
}
