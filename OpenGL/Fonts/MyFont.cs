using System.Collections.Generic;
using System.Drawing;

namespace GraphicLib.OpenGL.Fonts
{

  internal class MyFont
  {
    internal FontInfo FontInfo;
    internal readonly Dictionary<char, RectangleF> Symbols;
    private readonly Dictionary<KeyValuePair<char, char>, int> _kerningPairs;

    internal MyFont(FontInfo fontInfo)
    {
      this.FontInfo = fontInfo;
      Symbols = new Dictionary<char, RectangleF>();
      _kerningPairs = new Dictionary<KeyValuePair<char, char>, int>(new MyCharCharComparer());
    }

    internal void AddSymbol(char symbol, float x, float y, float width, float height)
    {
      if(Symbols.ContainsKey(symbol))
        return;
      Symbols.Add(symbol, new RectangleF(x, y, width, height));
    }

    internal void AddKerningPair(char symbol1, char symbol2, int delta)
    {
      if(_kerningPairs.ContainsKey(new KeyValuePair<char, char>(symbol1, symbol2)))
        return;
      _kerningPairs.Add(new KeyValuePair<char, char>(symbol1, symbol2), delta);
    }
  }
}
