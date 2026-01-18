using Microsoft.Xna.Framework;
using Monocle;
using System.Collections;
using System.Runtime.CompilerServices;
using Celeste.Mod.DzhakeHelper;
using Celeste.Mod.DzhakeHelper.Entities;
using System.Linq;

namespace Celeste.Mod.aonHelper.Entities;

public abstract class BaseLockBlock : Solid
{
    protected EntityID ID;

    public readonly Sprite Sprite;

    public struct OpeningSettingsData
    {
        public bool VanillaKeys;

        public bool DzhakeHelperKeysNone;
        public bool DzhakeHelperKeysAll;
        public int? DzhakeHelperKeyGroup;
    }
    private OpeningSettingsData OpeningSettings;

    protected bool Opening;
    public bool UnlockingRegistered;

    protected readonly bool StepMusicProgress;

    protected readonly string UnlockSfx;

    protected BaseLockBlock(
        EntityID id, Vector2 position,
        string spritePath,
        string unlockSfx, bool stepMusicProgress,
        OpeningSettingsData openingSettings,
        string defaultSpriteId = "aonHelper_lockBlockLock", string defaultUnlockSfx = SFX.game_03_key_unlock)
        : base(position, 32f, 32f, false)
    {
        DisableLightsInside = false;
        
        ID = id;
        OpeningSettings = openingSettings;
        
        Add(new PlayerCollider(OnPlayer, new Circle(60f, 16f, 16f)));

        Add(Sprite = string.IsNullOrWhiteSpace(spritePath)
            ? aonHelperGFX.SpriteBank.Create(defaultSpriteId)
            : BuildCustomSprite(spritePath));
        Sprite.Play("idle");
        Sprite.Position = new Vector2(Width / 2f, Height / 2f);

        StepMusicProgress = stepMusicProgress;
        UnlockSfx = string.IsNullOrWhiteSpace(unlockSfx)
            ? defaultUnlockSfx
            : SFX.EventnameByHandle(unlockSfx);
    }

    private static Sprite BuildCustomSprite(string spritePath)
    {
        Sprite sprite = new(GFX.Game, spritePath);

        // <Loop id="idle" delay="0.1" frames="0"/>
        sprite.AddLoop("idle", "", 0.1f, 0);
        // <Anim id="open" delay="0.06" frames="0-9"/>
        sprite.Add("open", "", 0.06f, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        // <Anim id="burst" delay="0.06" frames="10-18"/>
        sprite.Add("burst", "", 0.06f, 10, 11, 12, 13, 14, 15, 16, 17, 18);

        sprite.JustifyOrigin(0.5f, 0.5f);
        return sprite;
    }

    protected static OpeningSettingsData ParseOpeningSettings(bool useVanillaKeys, string settings)
    {
        bool groupSpecified = int.TryParse(settings, out int dzhakeHelperKeyGroup);
        return new OpeningSettingsData
        {
            VanillaKeys = useVanillaKeys,
            DzhakeHelperKeysNone = settings == "",
            DzhakeHelperKeysAll = settings == "*",
            DzhakeHelperKeyGroup = groupSpecified ? dzhakeHelperKeyGroup : null
        };
    }

    #region OnPlayer

    private void OnPlayer(Player player)
    {
        if (aonHelperModule.Instance.DzhakeHelperLoaded)
            OnPlayer_DzhakeHelperLoaded(player);
        else
            OnPlayer_DzhakeHelperUnloaded(player);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected virtual void OnPlayer_DzhakeHelperLoaded(Player player)
    {
        if (Opening)
            return;
        
        foreach (Follower follower in player.Leader.Followers)
        {
            if (follower.Entity is Key { StartedUsing: false } && OpeningSettings.VanillaKeys)
            {
                TryOpen(player, follower);
                break;
            }
            
            if (follower.Entity is CustomKey { StartedUsing: false } key2
                && !OpeningSettings.DzhakeHelperKeysNone
                && (OpeningSettings.DzhakeHelperKeysAll || key2.OpenAny || key2.Group == OpeningSettings.DzhakeHelperKeyGroup))
            {
                TryOpen(player, follower);
                break;
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected virtual void OnPlayer_DzhakeHelperUnloaded(Player player)
    {
        if (Opening)
            return;

        foreach (Follower follower in player.Leader.Followers.Where(follower => follower.Entity is Key { StartedUsing: false } && OpeningSettings.VanillaKeys))
        {
            TryOpen(player, follower);
            break;
        }
    }

    #endregion
        
    #region TryOpen

    private void TryOpen(Player player, Follower fol)
    {
        if (aonHelperModule.Instance.DzhakeHelperLoaded)
            TryOpen_DzhakeHelperLoaded(player, fol);
        else
            TryOpen_DzhakeHelperUnloaded(player, fol);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected virtual void TryOpen_DzhakeHelperLoaded(Player player, Follower fol)
    {
        Collidable = false;
        
        if (!Scene.CollideCheck<Solid>(player.Center, Center))
        {
            Opening = true;
            switch (fol.Entity)
            {
                case Key key:
                    key.StartedUsing = true;
                    break;
                case CustomKey key2:
                    key2.StartedUsing = true;
                    break;
            }
            
            Add(new Coroutine(UnlockRoutine(fol)));
        }
        
        Collidable = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected virtual void TryOpen_DzhakeHelperUnloaded(Player player, Follower fol)
    {
        Collidable = false;
        
        if (!Scene.CollideCheck<Solid>(player.Center, Center))
        {
            Opening = true;
            if (fol.Entity is Key key)
                key.StartedUsing = true;
            
            Add(new Coroutine(UnlockRoutine(fol)));
        }
        
        Collidable = true;
    }

    #endregion
        
    #region UnlockRoutine

    protected IEnumerator UnlockRoutine(Follower fol)
        => aonHelperModule.Instance.DzhakeHelperLoaded
            ? UnlockRoutine_DzhakeHelperLoaded(fol)
            : UnlockRoutine_DzhakeHelperUnloaded(fol);

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected virtual IEnumerator UnlockRoutine_DzhakeHelperLoaded(Follower fol)
    {
        SoundEmitter emitter = SoundEmitter.Play(UnlockSfx, this);
        emitter.Source.DisposeOnTransition = true;
        Level level = SceneAs<Level>();

        Key key = fol.Entity as Key;
        CustomKey key2 = fol.Entity as CustomKey;
        
        if (key is not null)
            Add(new Coroutine(key.UseRoutine(Center + new Vector2(0f, 2f))));
        else if (key2 is not null)
            Add(new Coroutine(key2.UseRoutine(Center + new Vector2(0f, 2f))));
        yield return 1.2f;

        UnlockingRegistered = true;
        if (StepMusicProgress)
        {
            level.Session.Audio.Music.Progress++;
            level.Session.Audio.Apply();
        }
        level.Session.DoNotLoad.Add(ID);
        if (key is not null)
        {
            key.RegisterUsed();

            while (key.Turning)
                yield return null;
        }
        else if (key2 is not null)
        {
            key2.RegisterUsed();
            DzhakeHelperModule.Session.CurrentKeys.RemoveAll(info => info.ID.ID == key2.ID.ID);

            while (key2.Turning)
                yield return null;
        }

        Tag |= Tags.TransitionUpdate;
        Collidable = false;
        emitter.Source.DisposeOnTransition = false;
        yield return Sprite.PlayRoutine("open");

        level.Shake();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        yield return Sprite.PlayRoutine("burst");

        RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected virtual IEnumerator UnlockRoutine_DzhakeHelperUnloaded(Follower fol)
    {
        SoundEmitter emitter = SoundEmitter.Play(UnlockSfx, this);
        emitter.Source.DisposeOnTransition = true;
        Level level = SceneAs<Level>();

        Key key = fol.Entity as Key;
        
        Add(new Coroutine(key.UseRoutine(Center + new Vector2(0f, 2f))));
        yield return 1.2f;

        UnlockingRegistered = true;
        if (StepMusicProgress)
        {
            level.Session.Audio.Music.Progress++;
            level.Session.Audio.Apply();
        }
        level.Session.DoNotLoad.Add(ID);
        key.RegisterUsed();
        while (key.Turning)
            yield return null;

        Tag |= Tags.TransitionUpdate;
        Collidable = false;
        emitter.Source.DisposeOnTransition = false;
        yield return Sprite.PlayRoutine("open");

        level.Shake();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        yield return Sprite.PlayRoutine("burst");

        RemoveSelf();
    }

    #endregion
}