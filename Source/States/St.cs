using Microsoft.Xna.Framework;

namespace Celeste.Mod.aonHelper.States
{
    public static class St
    {
        public static int DarkerMatter { get; private set; }

        internal static void Initialize()
        {
            States.DarkerMatter.Initialize();
        }

        internal static void Load()
        {
            On.Celeste.Player.ctor += Mod_Player_ctor;

            States.DarkerMatter.Load();
        }

        internal static void Unload()
        {
            On.Celeste.Player.ctor -= Mod_Player_ctor;

            States.DarkerMatter.Unload();
        }

        private static void Mod_Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);
            DarkerMatter = self.StateMachine.AddState("StDarkerMatter", self.DarkerMatterUpdate, self.DarkerMatterRoutine, self.DarkerMatterBegin, self.DarkerMatterEnd);
        }
    }
}