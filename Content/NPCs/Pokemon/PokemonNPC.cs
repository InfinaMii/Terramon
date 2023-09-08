using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terramon.Content.AI;
using Terramon.ID;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.Utilities;

namespace Terramon.Content.NPCs.Pokemon;

public class PokemonNPC : ModNPC
{
    private readonly object[] useAiParams;
    private readonly Type useAiType;
    private readonly int useHeight;
    private readonly string useName;
    private readonly float useSpawnChance;
    private readonly byte[] useSpawnConditions;
    private readonly int useWidth;
    private bool isShiny;
    private int shinySparkleTimer;

    public PokemonNPC(string useName, int useWidth, int useHeight, Type useAiType, object[] useAiParams,
        byte[] useSpawnConditions, float useSpawnChance)
    {
        this.useName = useName;
        this.useWidth = useWidth;
        this.useHeight = useHeight;
        this.useAiType = useAiType;
        this.useAiParams = useAiParams;
        this.useSpawnConditions = useSpawnConditions;
        this.useSpawnChance = useSpawnChance;
    }

    protected override bool CloneNewInstances => true;

    // ReSharper disable once ConvertToAutoProperty
    public override string Name => useName + "NPC";

    private AIController Behaviour { get; set; }

    public override string Texture => "Terramon/Assets/Pokemon/" + useName;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;
    }

    public override void SetDefaults()
    {
        NPC.width = useWidth;
        NPC.height = useHeight;
        NPC.defense = int.MaxValue;
        NPC.lifeMax = 100;
        NPC.aiStyle = -1;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.value = 0f;
        NPC.knockBackResist = 1f;
        NPC.despawnEncouraged = true;
        NPC.friendly = true;
        NPC.gfxOffY = -2;
        var npcArg = new object[] { NPC };
        Behaviour = (AIController)Activator.CreateInstance(useAiType, BindingFlags.OptionalParamBinding, null,
            npcArg.Concat(useAiParams).ToArray(), CultureInfo.CurrentCulture);
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        isShiny = Terramon.RollShiny();
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return !isShiny;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!isShiny) return;
        var path = useName + "_S";
        var texture = ModContent.Request<Texture2D>($"Terramon/Assets/Pokemon/{path}").Value;
        var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        var frameHeight = texture.Height / Main.npcFrameCount[NPC.type];
        spriteBatch.Draw(texture, NPC.position - screenPos + new Vector2(texture.Width / 2f, texture.Height / 2f + 2),
            new Rectangle(0, NPC.frame.Y, texture.Width, frameHeight), drawColor, NPC.rotation,
            new Vector2(texture.Width / 2f, frameHeight), NPC.scale, effects, 0f);
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (useSpawnConditions == null) return 0f;
        var chance = 1f;
        foreach (var c in useSpawnConditions)
            switch (c)
            {
                case SpawnConditionID.SurfaceJungle:
                    chance *= SpawnCondition.SurfaceJungle.Chance;
                    break;
                case SpawnConditionID.Cavern:
                    chance *= SpawnCondition.Cavern.Chance;
                    break;
                case SpawnConditionID.ZoneBeach:
                    chance *= spawnInfo.Player.ZoneBeach ? 1f : 0f;
                    break;
                case SpawnConditionID.ZoneForest:
                    chance *= spawnInfo.Player.ZoneForest ? 1f : 0f;
                    break;
                case SpawnConditionID.ZoneSnow:
                    chance *= spawnInfo.Player.ZoneSnow ? 1f : 0f;
                    break;
                case SpawnConditionID.DayTime:
                    chance *= Main.dayTime ? 1f : 0f;
                    break;
                case SpawnConditionID.NightTime:
                    chance *= Main.dayTime ? 0f : 1f;
                    break;
            }

        return chance * useSpawnChance;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(isShiny);
        Behaviour?.SendExtraAI(writer);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        isShiny = reader.ReadBoolean();
        Behaviour?.ReceiveExtraAI(reader);
    }

    public override void AI()
    {
        if (NPC.life < NPC.lifeMax) NPC.life = NPC.lifeMax;
        if (isShiny) ShinyEffect();
        Behaviour?.AI();
    }

    private void ShinyEffect()
    {
        Lighting.AddLight(NPC.position, 0.5f, 0.5f, 0.5f);
        shinySparkleTimer++;
        if (shinySparkleTimer < 6) return;
        for (var i = 0; i < 2; i++)
        {
            const short dustType = 204;
            var dust = Dust.NewDustDirect(
                NPC.position + new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3)), NPC.width,
                NPC.height, dustType);
            dust.velocity = NPC.velocity;
            dust.noGravity = true;
            dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
        }

        shinySparkleTimer = 0;
    }

    public override void FindFrame(int frameHeight)
    {
        Behaviour?.FindFrame(frameHeight);
    }
}