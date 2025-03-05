using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace Terramon.Content.GUI.Common;

public class UIHoverImage : UIImage
{
    private readonly object _hoverText;

    public UIHoverImage(Asset<Texture2D> texture, string hoverText) : base(texture)
    {
        _hoverText = hoverText;
    }

    public UIHoverImage(Asset<Texture2D> texture, LocalizedText hoverText) : base(texture)
    {
        _hoverText = hoverText;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        if (ContainsPoint(Main.MouseScreen)) Main.LocalPlayer.mouseInterface = true;
        if (!IsMouseHovering) return;
        if (Main.inFancyUI) Main.instance.MouseText(_hoverText.ToString());
        else
            Main.hoverItemName = _hoverText.ToString();
    }
}