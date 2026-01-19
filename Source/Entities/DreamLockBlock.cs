using Celeste.Mod.aonHelper.Helpers;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using System.Collections;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Celeste.Mod.DzhakeHelper;
using Celeste.Mod.DzhakeHelper.Entities;
using Celeste.Mod.Helpers;
using System.Runtime.CompilerServices;
using System;
using MonoMod.Utils;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.aonHelper.Entities;

[Tracked]
[CustomEntity("aonHelper/DreamLockBlock", "MoreLockBlocks/DreamLockBlock")]
public class DreamLockBlock : BaseLockBlock
{
    [TrackedAs(typeof(DreamBlock))]
    internal class DreamBlockDummy(Vector2 position, DreamLockBlock parent, bool below, bool ignoreInventory)
        : DreamBlock(position, 32, 32, null, false, false, below)
    {

        private const float ChargeUpDuration = 0.6f, UnlockDuration = 0.25f, ChargeDownDuration = 0.1f;

        private bool CanDashThrough
        {
            get
            {
                if (aonHelperModule.Session.DreamBlockDummyStates.TryGetValue(parent.ID, out bool value))
                    return value;
                
                aonHelperModule.Session.DreamBlockDummyStates[parent.ID] = false;
                if (ignoreInventory)
                    SetReverseHelperDummyState(false);
                
                return false;
            }

            set
            {
                aonHelperModule.Session.DreamBlockDummyStates[parent.ID] = value;
                if (ignoreInventory)
                    SetReverseHelperDummyState(value);
            }
        }

        private void SetReverseHelperDummyState(bool value)
        {
            aonHelperImports.ReverseHelper.ConfigureSetFromEnum?.Invoke(this, 1 << 1, value); // set `alwaysEnable`
            aonHelperImports.ReverseHelper.ConfigureSetFromEnum?.Invoke(this, 1 << 2, !value); // set `alwaysDisable`
        }

        private bool Unlocked => aonHelperModule.Session.UnlockedDreamLockBlocks.Contains(parent.ID); // whether we can change state

        private readonly bool ignoreInventory = ignoreInventory;

        public IEnumerator DummyUnlockRoutine()
        {
            Level level = SceneAs<Level>();
            Input.Rumble(RumbleStrength.Light, RumbleLength.Long);
            Add(shaker = new Shaker(true, s => shake = s));
            shaker.Interval = 0.02f;

            for (float p = 0f; p < 1f; p += Engine.DeltaTime / ChargeUpDuration)
            {
                whiteFill = Ease.CubeIn(p);
                yield return null;
            }
            ActivateNoRoutine(); // we can do this because the block is already registered as unlocked

            whiteHeight = 1f;
            whiteFill = 1f;
            for (float p = 1f; p > 0f; p -= Engine.DeltaTime / UnlockDuration)
            {
                whiteHeight = p;
                Glitch.Value = p * 0.2f;

                if (level.OnInterval(0.1f))
                {
                    level.Shake();
                    
                    for (int i = 0; i < Width; i += 4)
                        level.ParticlesFG.Emit(Strawberry.P_WingsBurst, new Vector2(X + i, Y + Height * whiteHeight + 1f));
                }
                
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Short);
                yield return null;
            }
            whiteHeight = Glitch.Value = 0f;

            while (whiteFill > 0f)
            {
                whiteFill -= Engine.DeltaTime / ChargeDownDuration;
                yield return null;
            }
        }

        #region DreamBlockDummy Hooks
        
        private static ILHook ilHook_Player_DashCoroutine;

        public static void Load()
        {
            On.Celeste.DreamBlock.Activate += DreamBlock_Activate;
            On.Celeste.DreamBlock.FastActivate += DreamBlock_FastActivate;
            On.Celeste.DreamBlock.ActivateNoRoutine += DreamBlock_ActivateNoRoutine;
            On.Celeste.DreamBlock.Deactivate += DreamBlock_Deactivate;
            On.Celeste.DreamBlock.FastDeactivate += DreamBlock_FastDeactivate;
            On.Celeste.DreamBlock.DeactivateNoRoutine += DreamBlock_DeactivateNoRoutine;
            
            IL.Celeste.DreamBlock.Added += DreamBlock_Added;

            if (!aonHelperDependencies.ReverseHelperLoaded)
            {
                IL.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;

                ilHook_Player_DashCoroutine = new ILHook(typeof(Player).GetMethod("DashCoroutine", HookHelper.Bind.NonPublicInstance)!.GetStateMachineTarget()!, Player_DashCoroutine);
            }
        }

        public static void Unload()
        {
            On.Celeste.DreamBlock.Activate -= DreamBlock_Activate;
            On.Celeste.DreamBlock.FastActivate -= DreamBlock_FastActivate;
            On.Celeste.DreamBlock.ActivateNoRoutine -= DreamBlock_ActivateNoRoutine;
            On.Celeste.DreamBlock.Deactivate -= DreamBlock_Deactivate;
            On.Celeste.DreamBlock.FastDeactivate -= DreamBlock_FastDeactivate;
            On.Celeste.DreamBlock.DeactivateNoRoutine -= DreamBlock_DeactivateNoRoutine;
            
            IL.Celeste.DreamBlock.Added -= DreamBlock_Added;

            if (!aonHelperDependencies.ReverseHelperLoaded)
            {
                IL.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;

                HookHelper.DisposeAndSetNull(ref ilHook_Player_DashCoroutine);
            }
        }

        private static void DreamBlock_Added(ILContext il)
        {
            ILCursor cursor = new(il);

            // IL_001d: stfld bool Celeste.DreamBlock::playerHasDreamDash
            if (!cursor.TryGotoNext(MoveType.Before, instr => instr.MatchStfld(typeof(DreamBlock), "playerHasDreamDash")))
                throw new HookHelper.HookException(il, "Unable to find assignment to `DreamBlock.playerHasDreamDash` to modify.");
                
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate(DetermineDreamBlockActive);
            
            return;

            static bool DetermineDreamBlockActive(bool orig, DreamBlock self)
            {
                if (self is not DreamBlockDummy dummy)
                    return orig;
            
                bool canDashThrough = dummy.CanDashThrough;
                if (dummy.ignoreInventory)
                    dummy.SetReverseHelperDummyState(canDashThrough);
                
                return canDashThrough;
            }
        }

        private static IEnumerator DreamBlock_Activate(On.Celeste.DreamBlock.orig_Activate orig, DreamBlock self) => DoNothingIfDummy(() => orig(self), self, true);
        private static IEnumerator DreamBlock_FastActivate(On.Celeste.DreamBlock.orig_FastActivate orig, DreamBlock self) => DoNothingIfDummy(() => orig(self), self, true);
        private static void DreamBlock_ActivateNoRoutine(On.Celeste.DreamBlock.orig_ActivateNoRoutine orig, DreamBlock self) => DoNothingIfDummy(() => orig(self), self, true);

        private static IEnumerator DreamBlock_Deactivate(On.Celeste.DreamBlock.orig_Deactivate orig, DreamBlock self) => DoNothingIfDummy(() => orig(self), self, false);
        private static IEnumerator DreamBlock_FastDeactivate(On.Celeste.DreamBlock.orig_FastDeactivate orig, DreamBlock self) => DoNothingIfDummy(() => orig(self), self, false);
        private static void DreamBlock_DeactivateNoRoutine(On.Celeste.DreamBlock.orig_DeactivateNoRoutine orig, DreamBlock self) => DoNothingIfDummy(() => orig(self), self, false);

        private static void DoNothingIfDummy(Action callOrig, DreamBlock self, bool canDashThrough)
        {
            if (self is DreamBlockDummy dummy)
            {
                if (!dummy.Unlocked)
                    return;
                
                dummy.CanDashThrough = canDashThrough;
            }
            
            callOrig();
        }
        private static IEnumerator DoNothingIfDummy(Func<IEnumerator> callOrig, DreamBlock self, bool canDashThrough)
        {
            if (self is DreamBlockDummy dummy)
            {
                if (!dummy.Unlocked)
                    yield break;
                
                dummy.CanDashThrough = canDashThrough;
            }
            
            yield return new SwapImmediately(callOrig());
        }

        private static void Player_DreamDashCheck(ILContext il)
        {
            ILCursor cursor = new(il);

            /*
             * IL_0000: ldarg.0
             * IL_0001: callvirt instance valuetype Celeste.PlayerInventory Celeste.Player::get_Inventory()
             * IL_0006: ldfld bool Celeste.PlayerInventory::DreamDash
             */
            if (!cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdarg0(),
                instr => instr.MatchCallvirt<Player>("get_Inventory"),
                instr => instr.MatchLdfld<PlayerInventory>("DreamDash")))
                throw new HookHelper.HookException(il, "Unable to find check for `Player.Inventory.DreamDash` to modify.");
            
            cursor.EmitLdarg0();
            cursor.EmitLdarg1();
            cursor.EmitDelegate(DetermineInventoryCheckOverride);
            cursor.EmitOr();
        }

        private static void Player_DashCoroutine(ILContext il)
        {
            ILCursor cursor = new(il);
            
            /*
             * IL_021c: ldloc.1
             * IL_021d: callvirt instance valuetype Celeste.PlayerInventory Celeste.Player::get_Inventory()
             * IL_0222: ldfld bool Celeste.PlayerInventory::DreamDash
             */
            if (!cursor.TryGotoNextBestFit(MoveType.After,
                instr => instr.MatchLdloc1(),
                instr => instr.MatchCallvirt<Player>("get_Inventory"),
                instr => instr.MatchLdfld<PlayerInventory>("DreamDash")))
                throw new HookHelper.HookException(il, "Unable to find check for `Player.Inventory.DreamDash` to modify.");
            
            cursor.EmitLdloc1();
            cursor.EmitCall(typeof(Vector2).GetProperty("UnitY", HookHelper.Bind.PublicStatic)!.GetGetMethod()!);
            cursor.EmitDelegate(DetermineInventoryCheckOverride);
            cursor.EmitOr();
        }

        private static bool DetermineInventoryCheckOverride(Player player, Vector2 dir)
            => player.CollideFirst<DreamBlock>(player.Position + dir) is DreamBlockDummy { CanDashThrough: true, ignoreInventory: true };

        #endregion
    }

    private DreamBlockDummy dummy;
    private readonly bool dummyBelow;
    private readonly bool dummyIgnoreInventory;

    public DreamLockBlock(
        EntityID id, Vector2 position,
        string spritePath,
        string unlockSfx, bool stepMusicProgress,
        OpeningSettingsData openingSettings,
        bool dummyBelow, bool dummyIgnoreInventory)
        : base(id, position, spritePath, unlockSfx, stepMusicProgress, openingSettings, defaultUnlockSfx: aonHelperSFX.game_lockblocks_dreamlockblock_key_unlock)
    {
        SurfaceSoundIndex = 11;

        this.dummyBelow = dummyBelow;
        this.dummyIgnoreInventory = dummyIgnoreInventory;
    }

    public DreamLockBlock(EntityData data, Vector2 offset, EntityID id)
        : this(id, data.Position + offset,
            data.Attr("spritePath"),
            data.Attr("unlockSfx"), data.Bool("stepMusicProgress"),
            ParseOpeningSettings(data.Bool("useVanillaKeys", true), data.Attr("dzhakeHelperKeySettings")),
            data.Bool("dummyBelow"), data.Bool("dummyIgnoreInventory", true))
    { }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        Scene.Add(dummy = new DreamBlockDummy(Position, this, dummyBelow, dummyIgnoreInventory));
        Depth = dummy.Depth - 1;
        if (aonHelperModule.Session.UnlockedDreamLockBlocks.Contains(ID))
            RemoveSelf();
    }

    #region TryOpen

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected override void TryOpen_DzhakeHelperLoaded(Player player, Follower fol)
    {
        Collidable = dummy.Collidable = false;
        
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
        
        Collidable = dummy.Collidable = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected override void TryOpen_DzhakeHelperUnloaded(Player player, Follower fol)
    {
        Collidable = dummy.Collidable = false;
        
        if (!Scene.CollideCheck<Solid>(player.Center, Center))
        {
            Opening = true;
            if (fol.Entity is Key key)
                key.StartedUsing = true;
            
            Add(new Coroutine(UnlockRoutine(fol)));
        }
        
        Collidable = dummy.Collidable = true;
    }

    #endregion
    
    #region UnlockRoutine

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected override IEnumerator UnlockRoutine_DzhakeHelperLoaded(Follower fol)
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
        aonHelperModule.Session.UnlockedDreamLockBlocks.Add(ID);
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
        dummy.Add(new Coroutine(dummy.DummyUnlockRoutine()));
        SurfaceSoundIndex = 12;
        yield return Sprite.PlayRoutine("open");

        level.Shake();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        yield return Sprite.PlayRoutine("burst");

        RemoveSelf();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    protected override IEnumerator UnlockRoutine_DzhakeHelperUnloaded(Follower fol)
    {
        SoundEmitter emitter = SoundEmitter.Play(UnlockSfx, this);
        emitter.Source.DisposeOnTransition = true;
        Level level = SceneAs<Level>();

        Key key = (fol.Entity as Key)!;
        
        Add(new Coroutine(key.UseRoutine(Center + new Vector2(0f, 2f))));
        yield return 1.2f;

        UnlockingRegistered = true;
        if (StepMusicProgress)
        {
            level.Session.Audio.Music.Progress++;
            level.Session.Audio.Apply();
        }
        aonHelperModule.Session.UnlockedDreamLockBlocks.Add(ID);
        key.RegisterUsed();
        while (key.Turning)
            yield return null;

        Tag |= Tags.TransitionUpdate;
        Collidable = false;
        emitter.Source.DisposeOnTransition = false;
        dummy.Add(new Coroutine(dummy.DummyUnlockRoutine()));
        SurfaceSoundIndex = 12;
        yield return Sprite.PlayRoutine("open");

        level.Shake();
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        yield return Sprite.PlayRoutine("burst");

        RemoveSelf();
    }

    #endregion
}