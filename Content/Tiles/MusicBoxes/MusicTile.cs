using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace Terramon.Content.Tiles.MusicBoxes;

public abstract class MusicTile : ModTile
{
    public override string Texture => "Terramon/Assets/Tiles/MusicBoxes/" + GetType().Name;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.newTile.StyleLineSkip = 2;
        TileObjectData.addTile(Type);
        DustType = DustID.Titanium;

        AddMapEntry(new Color(191, 142, 111), Language.GetText("ItemName.MusicBox"));
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return true;
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (Lighting.UpdateEveryFrame && new FastRandom(Main.TileFrameSeed).WithModifier(i, j).Next(4) != 0) return;
        var tile = Main.tile[i, j];
        if (tile.TileFrameX != 36 || tile.TileFrameY % 36 != 0 || (int)Main.timeForVisualEffects % 7 != 0 ||
            !Main._rand.NextBool(3)) return;
        var musicNote = Main._rand.Next(570, 573);
        Vector2 spawnPosition = new(i * 16 + 8, j * 16 - 8);
        Vector2 noteMovement = new(Main.WindForVisuals * 2f, -0.5f);
        noteMovement.X *= 1f + Main._rand.Next(-50, 51) * 0.01f;
        noteMovement.Y *= 1f + Main._rand.Next(-50, 51) * 0.01f;
        switch (musicNote)
        {
            case 572:
                spawnPosition.X -= 8f;
                break;
            case 571:
                spawnPosition.X -= 4f;
                break;
        }

        Gore.NewGore(new EntitySource_TileUpdate(i, j), spawnPosition, noteMovement, musicNote, 0.8f);
    }
}