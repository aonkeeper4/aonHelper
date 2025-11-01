using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.aonHelper.Entities;

[TrackedAs(typeof(HeartGem))]
[CustomEntity("aonHelper/ResizableHeart")]
public class ResizableHeart : HeartGem
{
    private readonly int width, height;
    
    private readonly string spriteID;
    private Sprite spriteOutline;
    private Color color;
    private readonly bool disableGhost;

    private readonly float respawnTime;
    private float respawnTimer;

    private readonly bool fake;

    public ResizableHeart(EntityData data, Vector2 offset) : base(data, offset)
    {
        width = data.Width;
        height = data.Height;
        
        spriteID = data.Attr("path");
        color = data.HexColor("color", Calc.HexToColor("00a81f"));
        disableGhost = data.Bool("disableGhost");
        
        respawnTime = data.Float("respawnTimer", 3f);
        
        fake = data.Bool("isFake");
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        Level level = SceneAs<Level>();
        
        Collider = new Hitbox(width, height, -width / 2, -height / 2);
        
        AreaKey area = level.Session.Area;
        IsGhost = !IsFake && !fake && SaveData.Instance.Areas_Safe[area.ID].Modes[(int)area.Mode].HeartGem;
        
        Remove(sprite);
        if (!string.IsNullOrWhiteSpace(spriteID))
        {
            sprite = GFX.SpriteBank.Create(spriteID);
            
            sprite.Play("spin");
            if (IsGhost && !disableGhost)
                sprite.Color = Color.White * 0.8f;
            
            switch (spriteID)
            {
                case "heartgem0":
                    color = Color.Aqua;
                    shineParticle = P_BlueShine;
                    break;
                
                case "heartgem1":
                    color = Color.Red;
                    shineParticle = P_RedShine;
                    break;
                
                case "heartgem2":
                    color = Color.Gold;
                    shineParticle = P_GoldShine;
                    break;
                
                case "heartgem3":
                    color = Calc.HexToColor("dad8cc");
                    shineParticle = P_FakeShine;
                    break;
                
                default:
                    shineParticle = new ParticleType(P_BlueShine) { Color = color };
                    break;
            }
        }
        else
        {
            sprite = aonHelperModule.SpriteBank.Create("aonHelper_resizableHeart");
            spriteOutline = aonHelperModule.SpriteBank.Create("aonHelper_resizableHeartOutline");
            sprite.Color = IsGhost && !fake && !disableGhost ? Color.Lerp(color, Color.White, 0.8f) * 0.8f : color;
            shineParticle = new ParticleType(P_BlueShine) { Color = color };
        }
        sprite.OnLoop = anim =>
        {
            if (!Visible || anim != "spin" || !autoPulse)
                return;
            
            Audio.Play(IsFake ? "event:/new_content/game/10_farewell/fakeheart_pulse" : "event:/game/general/crystalheart_pulse", Position);

            ScaleWiggler.Start();
            level.Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
        };
        Add(sprite);
        if (spriteOutline is not null)
            Add(spriteOutline);
        
        Remove(ScaleWiggler);
        ScaleWiggler = Wiggler.Create(0.5f, 4f, f => sprite.Scale = Vector2.One * (1f + f * 0.25f));
        Add(ScaleWiggler);
        
        light.Color = Color.Lerp(color, Color.White, 0.5f);
    }

    public override void Update()
    {
        if (fake)
            collected = respawnTimer > 0f;
        
        base.Update();

        if (respawnTimer > 0f)
        {
            respawnTimer -= Engine.DeltaTime;
            if (respawnTimer <= 0f)
            {
                Collidable = Visible = true;
                ScaleWiggler.Start();
            }
        }
        
        if (spriteOutline is not null)
        {
            spriteOutline.Position = sprite.Position;
            spriteOutline.Scale = sprite.Scale;
            if (spriteOutline.CurrentAnimationID != sprite.CurrentAnimationID)
            {
                spriteOutline.Play(sprite.CurrentAnimationID);
            }
            spriteOutline.SetAnimationFrame(sprite.CurrentAnimationFrame);
        }
    }

    private void CollectFake(Player player, float angle)
    {
        if (!Collidable)
            return;
        
        Collidable = Visible = false;
        respawnTimer = respawnTime;
        
        Celeste.Freeze(0.05f);
        SceneAs<Level>().Shake();
        SlashFx.Burst(Position, angle);
        
        player?.RefillDash();
    }
    
    #region Hooks
    
    internal static void Load()
    {
        On.Celeste.HeartGem.OnPlayer += HeartGem_OnPlayer;
        On.Celeste.HeartGem.OnHoldable += HeartGem_OnHoldable;
    }

    internal static void Unload()
    {
        On.Celeste.HeartGem.OnPlayer -= HeartGem_OnPlayer;
        On.Celeste.HeartGem.OnHoldable -= HeartGem_OnHoldable;
    }
    
    private static void HeartGem_OnHoldable(On.Celeste.HeartGem.orig_OnHoldable orig, HeartGem self, Holdable holdable)
    {
        if (self is not ResizableHeart resizable)
        {
            orig(self, holdable);
            return;
        }
        
        Player entity = resizable.Scene.Tracker.GetEntity<Player>();
        if (resizable.fake && resizable.Visible && holdable.Dangerous(resizable.holdableCollider))
            resizable.CollectFake(entity, holdable.GetSpeed().Angle());
        else
            orig(self, holdable);
    }

    private static void HeartGem_OnPlayer(On.Celeste.HeartGem.orig_OnPlayer orig, HeartGem self, Player player)
    {
        if (self is not ResizableHeart { fake: true } resizable)
        {
            orig(self, player);
            return;
        }

        Level level = resizable.SceneAs<Level>();
        
        if (!resizable.Visible || level.Frozen)
            return;
        
        if (player.DashAttacking)
        {
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            resizable.CollectFake(player, player.Speed.Angle());
            return;
        }
        
        player.PointBounce(resizable.Center);
        if (resizable.bounceSfxDelay <= 0f)
        {
            Audio.Play("event:/game/general/crystalheart_bounce", resizable.Position);
            resizable.bounceSfxDelay = 0.1f;
        }
        resizable.moveWiggler.Start();
        resizable.ScaleWiggler.Start();
        resizable.moveWiggleDir = (resizable.Center - player.Center).SafeNormalize(Vector2.UnitY);
        
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
    }
    
    #endregion
}