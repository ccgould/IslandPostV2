namespace IslandPostPOS.Models;

public class IconButtonData
{
    public string IconGlyph { get; set; }
    public string Text { get; set; }

    public IconButtonData(string iconGlyph, string text)
    {
        IconGlyph = iconGlyph;
        Text = text;
    }
}
