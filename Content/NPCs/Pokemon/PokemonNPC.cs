using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hjson;
using Newtonsoft.Json.Linq;
using ReLogic.Content;
using Terramon.Content.AI;
using Terramon.Content.Dusts;
using Terramon.Content.Items.Mechanical;
using Terramon.Core.NPCComponents;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace Terramon.Content.NPCs.Pokemon;

[Autoload(false)]
public class PokemonNPC : ModNPC
{
    private static Dictionary<ushort, JToken> SchemaCache;
    public readonly ushort useId;
    private readonly string useName;
    public PokemonData data;
    private Asset<Texture2D> glowTexture;
    private bool hasGenderDifference;
    private bool isDestroyed;
    private int shinySparkleTimer;

    public PokemonNPC(ushort useId, string useName)
    {
        this.useId = useId;
        this.useName = useName;
        Name = useName + "NPC";
        Texture = "Terramon/Assets/Pokemon/" + useName;
    }

    protected override bool CloneNewInstances => true;

    public override string Name { get; }

    public override LocalizedText DisplayName => Terramon.DatabaseV2.GetLocalizedPokemonName(useId);

    private AIController Behaviour { get; set; }

    public override string Texture { get; }

    public override void SetDefaults()
    {
        NPC.defense = int.MaxValue;
        NPC.lifeMax = 100;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.value = 0f;
        NPC.knockBackResist = 0.75f;
        NPC.despawnEncouraged = true;
        NPC.friendly = true;

        // Load gender-specific texture if it exists.
        hasGenderDifference = ModContent.HasAsset(Texture + "F");

        // Load glowmask texture if it exists.
        if (ModContent.RequestIfExists<Texture2D>(Texture + "_Glow", out var glowTex))
            glowTexture = glowTex;

        // TODO: Optimize.
        if (!SchemaCache.TryGetValue(useId, out var npcSchema))
        {
            var hjsonStream = Mod.GetFileStream($"Content/Pokemon/{useName}.hjson");
            using var hjsonReader = new StreamReader(hjsonStream);
            var jsonText = HjsonValue.Load(hjsonReader).ToString();
            hjsonReader.Close();
            var schema = JObject.Parse(jsonText);
            if (!schema.ContainsKey("NPC")) return;
            npcSchema = schema["NPC"];
            SchemaCache.Add(useId, npcSchema);
        }

        var mi = typeof(NPCComponentExtensions).GetMethod("EnableComponent");
        foreach (var component in npcSchema!.Children<JProperty>())
        {
            var componentType = Mod.Code.GetType($"Terramon.Content.NPCs.NPC{component.Name}");
            if (componentType == null) continue;
            var enableComponentRef = mi!.MakeGenericMethod(componentType);
            var instancedComponent = enableComponentRef.Invoke(null, new object[] { NPC, null });
            foreach (var prop in component.Value.Children<JProperty>())
            {
                var fieldInfo = componentType.GetRuntimeField(prop.Name);
                if (fieldInfo == null) continue;
                fieldInfo.SetValue(instancedComponent, prop.Value.ToObject(fieldInfo.FieldType));
            }
        }
    }

    public override void OnSpawn(IEntitySource source)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        var spawningPlayer = Player.FindClosest(NPC.Center, NPC.width, NPC.height);
        data = PokemonData.Create(Main.player[spawningPlayer], useId, 5);
        NPC.netUpdate = true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var path = Texture;
        if (hasGenderDifference && data?.Gender == Gender.Female)
            path += "F";
        if (data?.Variant != null)
            path += "_" + data.Variant;
        if (data is { IsShiny: true })
            path += "_S";

        var frameSize = NPC.frame.Size();
        var texture = ModContent.Request<Texture2D>(path).Value;
        var effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        spriteBatch.Draw(texture,
            NPC.Center - screenPos +
            new Vector2(0f, NPC.gfxOffY + DrawOffsetY + (int)Math.Ceiling(NPC.height / 2f) + 4),
            NPC.frame, drawColor, NPC.rotation,
            frameSize / new Vector2(2, 1), NPC.scale, effects, 0f);

        if (glowTexture == null) return false;
        spriteBatch.Draw(glowTexture.Value,
            NPC.Center - screenPos +
            new Vector2(0f, NPC.gfxOffY + DrawOffsetY + (int)Math.Ceiling(NPC.height / 2f) + 4),
            NPC.frame, Color.White, NPC.rotation,
            frameSize / new Vector2(2, 1), NPC.scale, effects, 0f);

        return false;
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((byte)data.Gender);
        writer.Write(data.IsShiny);
        Behaviour?.SendExtraAI(writer);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        data.Gender = (Gender)reader.ReadByte();
        data.IsShiny = reader.ReadBoolean();
        Behaviour?.ReceiveExtraAI(reader);
    }

    public override void AI()
    {
        if (NPC.life < NPC.lifeMax) NPC.life = NPC.lifeMax;
        if (data.IsShiny) ShinyEffect();
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

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        return projectile.ModProjectile is BasePkballProjectile;
    }

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        return false;
    }

    public void Destroy()
    {
        if (isDestroyed) return;

        //TODO: Add shader animation (I already made this shader in my mod source but I couldn't figure out how to apply it properly)
        var dust = ModContent.DustType<SummonCloud>();
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, 0, 1);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, 0, -1);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, 1);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, -1);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, 0.5f, 0.5f);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, 0.5f, -0.5f);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, -0.5f, 0.5f);
        Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, dust, -0.5f, -0.5f);

        NPC.netUpdate = true;
        NPC.active = false;
        isDestroyed = true;
    }

    public override void Load()
    {
        SchemaCache = new Dictionary<ushort, JToken>();
    }

    public override void Unload()
    {
        SchemaCache = null;
    }
}