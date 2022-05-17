using Celeste.Mod.Entities;
using Celeste;
using Celeste.Mod.Meta;
using Celeste.Mod;
using Monocle;
using Microsoft.Xna.Framework;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace VivHelper.Entities.AsleepHeart {
    [CustomEntity("VivHelper/FakeHeartGem")]
    public class FakeHeartgem : Entity {


        public static ParticleType P_BlueShine = HeartGem.P_BlueShine;

        public static ParticleType P_RedShine = HeartGem.P_RedShine;

        public static ParticleType P_GoldShine = HeartGem.P_GoldShine;

        public static ParticleType P_FakeShine = HeartGem.P_FakeShine;

        public bool IsGhost;

        public const float GhostAlpha = 0.8f;

        public bool IsFake;

        private Sprite sprite;

        private Sprite white;

        public Wiggler ScaleWiggler;

        private Wiggler moveWiggler;

        private Vector2 moveWiggleDir;

        private BloomPoint bloom;

        private VertexLight light;

        private Poem poem;

        private BirdNPC bird;

        private float timer;
        //Nice.
        private bool collected;

        private bool autoPulse = true;

        private float bounceSfxDelay;

        private bool removeCameraTriggers;

        private SoundEmitter sfx;

        private List<InvisibleBarrier> walls = new List<InvisibleBarrier>();

        private HoldableCollider holdableCollider;

        private EntityID entityID;

        private InvisibleBarrier fakeRightWall;
        private string music;

        private bool shatterSpinners;
        private bool changeRespawnToNearest;

        public FakeHeartgem(Vector2 position)
            : base(position) {

            Add(holdableCollider = new HoldableCollider(OnHoldable));
            Add(new MirrorReflection());
        }

        public FakeHeartgem(EntityData data, Vector2 offset)
            : this(data.Position + offset) {
            removeCameraTriggers = data.Bool("removeCameraTriggers", false);
            IsFake = data.Bool("fake", true);
            music = data.Attr("music", "");
            entityID = new EntityID(data.Level.Name, data.ID);
            shatterSpinners = data.Bool("ShatterSpinnersOnBreak", false);
            changeRespawnToNearest = data.Bool("ChangeRespawnOnCollect", false);
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Level level = base.Scene as Level;
            AreaKey area = level.Session.Area;
            IsGhost = false;
            string id = "heartgem0";
            sprite = GFX.SpriteBank.Create(id);
            Add(sprite);
            sprite.Play("spin");
            sprite.OnLoop = delegate (string anim) {
                if (Visible && anim == "spin" && autoPulse) {
                    if (IsFake) {
                        Audio.Play("event:/new_content/game/10_farewell/fakeheart_pulse", Position);
                    } else {
                        Audio.Play("event:/game/general/crystalheart_pulse", Position);
                    }
                    ScaleWiggler.Start();
                    level.Displacement.AddBurst(Position, 0.35f, 8f, 48f, 0.25f);
                }
            };
            if (IsGhost) {
                sprite.Color = Color.White * 0.8f;
            }
            base.Collider = new Hitbox(16f, 16f, -8f, -8f);
            Add(new PlayerCollider(OnPlayer));
            Add(ScaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f) {
                sprite.Scale = Vector2.One * (1f + f * 0.25f);
            }));
            Add(bloom = new BloomPoint(0.75f, 16f));
            Color value;
            if (IsFake) {
                value = Calc.HexToColor("dad8cc");
            } else if (area.Mode == AreaMode.Normal) {
                value = Color.Aqua;
            } else if (area.Mode == AreaMode.BSide) {
                value = Color.Red;
            } else {
                value = Color.Gold;
            }
            value = Color.Lerp(value, Color.White, 0.5f);
            Add(light = new VertexLight(value, 1f, 32, 64));
            if (IsFake) {
                bloom.Alpha = 0f;
                light.Alpha = 0f;
            }
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            if (IsFake) {
                Player entity = base.Scene.Tracker.GetEntity<Player>();
                if ((entity != null && entity.X > base.X) || level.Session.GetFlag("fake_heart")) {
                    Visible = false;
                    Alarm.Set(this, 0.0001f, delegate {
                        if (removeCameraTriggers) {
                            FakeRemoveCameraTrigger();
                        }
                        RemoveSelf();
                    });
                } else {
                    scene.Add(fakeRightWall = new InvisibleBarrier(new Vector2(base.X + 160f, base.Y - 200f), 8f, 400f));
                }
            }
        }

        public override void Update() {
            bounceSfxDelay -= Engine.DeltaTime;
            timer += Engine.DeltaTime;
            sprite.Position = Vector2.UnitY * (float) Math.Sin(timer * 2f) * 2f + moveWiggleDir * moveWiggler.Value * -8f;
            if (white != null) {
                white.Position = sprite.Position;
                white.Scale = sprite.Scale;
                if (white.CurrentAnimationID != sprite.CurrentAnimationID) {
                    white.Play(sprite.CurrentAnimationID);
                }
                white.SetAnimationFrame(sprite.CurrentAnimationFrame);
            }
            if (collected && (base.Scene.Tracker.GetEntity<Player>()?.Dead ?? true)) {
                EndCutscene();
            }
            base.Update();
            if (!collected && base.Scene.OnInterval(0.1f)) {
                SceneAs<Level>().Particles.Emit(P_BlueShine, 1, base.Center, Vector2.One * 8f);
            }
        }

        public void OnHoldable(Holdable h) {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (!collected && entity != null && h.Dangerous(holdableCollider)) {
                Collect(entity);
            }
        }

        public void OnPlayer(Player player) {
            if (collected || (base.Scene as Level).Frozen) {
                return;
            }
            if (player.DashAttacking) {
                Collect(player);
                return;
            }
            if (bounceSfxDelay <= 0f) {
                if (IsFake) {
                    Audio.Play("event:/new_content/game/10_farewell/fakeheart_bounce", Position);
                } else {
                    Audio.Play("event:/game/general/crystalheart_bounce", Position);
                }
                bounceSfxDelay = 0.1f;
            }
            player.PointBounce(base.Center);
            moveWiggler.Start();
            ScaleWiggler.Start();
            moveWiggleDir = (base.Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        }

        private void Collect(Player player) {
            base.Scene.Tracker.GetEntity<AngryOshiro>()?.StopControllingTime();
            Coroutine coroutine = new Coroutine(CollectRoutine(player));
            coroutine.UseRawDeltaTime = true;
            Add(coroutine);
            collected = true;
            if (removeCameraTriggers) {
                List<CameraOffsetTrigger> list = base.Scene.Entities.FindAll<CameraOffsetTrigger>();
                foreach (CameraOffsetTrigger item in list) {
                    item.RemoveSelf();
                }
            }
        }

        private IEnumerator CollectRoutine(Player player) {
            Level level = base.Scene as Level;
            bool flag = false;
            MapMetaModeProperties mapMetaModeProperties = level?.Session.MapData.GetMeta();
            if (mapMetaModeProperties != null && mapMetaModeProperties.HeartIsEnd.HasValue) {
                flag = mapMetaModeProperties.HeartIsEnd.Value;
            }
            if (flag) {
                List<IStrawberry> list = new List<IStrawberry>();
                ReadOnlyCollection<Type> berryTypes = StrawberryRegistry.GetBerryTypes();
                foreach (Follower follower in player.Leader.Followers) {
                    if (berryTypes.Contains(follower.Entity.GetType()) && follower.Entity is IStrawberry) {
                        list.Add(follower.Entity as IStrawberry);
                    }
                }
                foreach (IStrawberry item in list) {
                    item.OnCollect();
                }
            }
            return orig_CollectRoutine(player);
        }

        private void EndCutscene() {
            Level level = base.Scene as Level;
            level.Frozen = false;
            level.CanRetry = true;
            level.FormationBackdrop.Display = false;
            Engine.TimeRate = 1f;
            if (poem != null) {
                poem.RemoveSelf();
            }
            foreach (InvisibleBarrier wall in walls) {
                wall.RemoveSelf();
            }
        }

        private void RegisterAsCollected(Level level, string poemID) {
            level.Session.HeartGem = true;
            level.Session.UpdateLevelStartDashes();
            int unlockedModes = SaveData.Instance.UnlockedModes;
            SaveData.Instance.RegisterHeartGem(level.Session.Area);
            if (!string.IsNullOrEmpty(poemID)) {
                SaveData.Instance.RegisterPoemEntry(poemID);
            }
            if (unlockedModes < 3 && SaveData.Instance.UnlockedModes >= 3) {
                level.Session.UnlockedCSide = true;
            }
            if (SaveData.Instance.TotalHeartGemsInVanilla >= 24) {
                Achievements.Register(Achievement.CSIDES);
            }
        }

        private IEnumerator DoFakeRoutineWithBird(Player player) {
            Level level = base.Scene as Level;
            int panAmount = 64;
            Vector2 panFrom = level.Camera.Position;
            Vector2 panTo = level.Camera.Position + new Vector2(-panAmount, 0f);
            Vector2 birdFrom = new Vector2(panTo.X - 16f, player.Y - 20f);
            Vector2 birdTo = new Vector2(panFrom.X + 320f + 16f, player.Y - 20f);
            yield return 2f;
            Glitch.Value = 0.75f;
            while (Glitch.Value > 0f) {
                Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 4f);
                level.Shake();
                yield return null;
            }
            yield return 1.1f;
            Glitch.Value = 0.75f;
            while (Glitch.Value > 0f) {
                Glitch.Value = Calc.Approach(Glitch.Value, 0f, Engine.RawDeltaTime * 4f);
                level.Shake();
                yield return null;
            }
            yield return 1.8f;
            //Bird was here 2020-2020, may she rest in peace.
            Engine.TimeRate = 0f;
            level.Frozen = false;
            player.Active = false;
            player.StateMachine.State = 11;
            while (Engine.TimeRate != 1f) {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 1f, 0.5f * Engine.RawDeltaTime);
                yield return null;
            }
            Engine.TimeRate = 1f;
            sfx.Source.Param("end", 1f);
            yield return 0.283f;
            level.FormationBackdrop.Display = false;
            for (float p3 = 0f; p3 < 1f; p3 += Engine.RawDeltaTime / 0.2f) {
                poem.TextAlpha = Ease.CubeIn(1f - p3);
                poem.ParticleSpeed = poem.TextAlpha;
                yield return null;
            }
            poem.Heart.Play("break");
            while (poem.Heart.Animating) {
                poem.Shake += Engine.DeltaTime;
                yield return null;
            }
            poem.RemoveSelf();
            poem = null;
            if (shatterSpinners) { ShatterSpinners(); }
            for (int i = 0; i < 10; i++) {
                Vector2 position = level.Camera.Position + new Vector2(320f, 180f) * 0.5f;
                Vector2 value = level.Camera.Position + new Vector2(160f, -64f);
                base.Scene.Add(new AbsorbOrb(position, null, value));
            }
            level.Shake();
            Glitch.Value = 0.8f;
            while (Glitch.Value > 0f) {
                Glitch.Value -= Engine.DeltaTime * 4f;
                yield return null;
            }
            yield return 0.25f;
            if (music != "") {
                level.Session.Audio.Music.Event = music;
                level.Session.Audio.Apply();
            }
            player.Active = true;
            player.Depth = 0;
            player.StateMachine.State = 11;
            while (!player.OnGround() && player.Bottom < (float) level.Bounds.Bottom) {
                yield return null;
            }
            player.Facing = Facings.Right;
            yield return 0.5f;
            SkipFakeHeartCutscene(level);
            level.EndCutscene();
        }

        private IEnumerator PlayerStepForward() {
            yield return 0.1f;
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player?.CollideCheck<Solid>(player.Position + new Vector2(12f, 1f)) ?? false) {
                yield return player.DummyWalkToExact((int) player.X + 10);
            }
            yield return 0.2f;
        }

        private void SkipFakeHeartCutscene(Level level) {
            Engine.TimeRate = 1f;
            Glitch.Value = 0f;
            Vector2 Target = level.GetSpawnPoint(level.Tracker.GetEntity<Player>().Position);
            Vector2 point = Target + Vector2.UnitY * -4f;
            if (changeRespawnToNearest && ((base.Scene.CollideCheck<Solid>(point) ? base.Scene.CollideCheck<FloatySpaceBlock>(point) : true) && (!level.Session.RespawnPoint.HasValue || level.Session.RespawnPoint.Value != Target))) {
                level.Session.HitCheckpoint = true;
                level.Session.RespawnPoint = Target;
                level.Session.UpdateLevelStartDashes();
            }
            if (sfx != null) {
                sfx.Source.Stop();
            }
            level.Session.SetFlag("fake_heart");
            level.Frozen = false;
            level.FormationBackdrop.Display = false;
            if (music != "") {
                level.Session.Audio.Music.Event = music;
                level.Session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (entity != null) {
                entity.Sprite.Play("idle");
                entity.Active = true;
                entity.StateMachine.State = 0;
                entity.Dashes = 1;
                entity.Speed = Vector2.Zero;
                for (int i = 0; i < 10; i++) {
                    entity.UpdateHair(applyGravity: true);
                }
            }
            foreach (AbsorbOrb item in base.Scene.Entities.FindAll<AbsorbOrb>()) {
                item.RemoveSelf();
            }
            if (poem != null) {
                poem.RemoveSelf();
            }
            if (fakeRightWall != null) {
                fakeRightWall.RemoveSelf();
            }
            if (removeCameraTriggers) {
                FakeRemoveCameraTrigger();
            }
            foreach (InvisibleBarrier wall in walls) {
                wall.RemoveSelf();
            }
            if (shatterSpinners) { ShatterSpinners(); }
            RemoveSelf();
        }

        private void FakeRemoveCameraTrigger() {
            CameraTargetTrigger cameraTargetTrigger = CollideFirst<CameraTargetTrigger>();
            if (cameraTargetTrigger != null) {
                cameraTargetTrigger.LerpStrength = 0f;
            }
        }

        private IEnumerator orig_CollectRoutine(Player player) {
            IsFake = true;
            Level level = Scene as Level;
            Vector2 Target = level.GetSpawnPoint(player.Position);
            Vector2 point = Target + Vector2.UnitY * -4f;
            if (changeRespawnToNearest && ((base.Scene.CollideCheck<Solid>(point) ? base.Scene.CollideCheck<FloatySpaceBlock>(point) : true) && (!level.Session.RespawnPoint.HasValue || level.Session.RespawnPoint.Value != Target))) {
                level.Session.HitCheckpoint = true;
                level.Session.RespawnPoint = Target;
                level.Session.UpdateLevelStartDashes();
            }
            AreaKey area = level.Session.Area;
            string poemID = AreaData.Get(level).Mode[(int) area.Mode].PoemID;
            bool completeArea = false;
            if (IsFake) {
                level.StartCutscene(SkipFakeHeartCutscene);
            } else {
                level.CanRetry = false;
            }
            if (completeArea || IsFake) {
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
            }
            if (completeArea) {
                List<Strawberry> strawbs = new List<Strawberry>();
                foreach (Follower follower in player.Leader.Followers) {
                    if (follower.Entity is Strawberry) {
                        strawbs.Add(follower.Entity as Strawberry);
                    }
                }
                foreach (Strawberry strawb in strawbs) {
                    strawb.OnCollect();
                }
            }
            string sfxEvent = "event:/new_content/game/10_farewell/fakeheart_get";
            sfx = SoundEmitter.Play(sfxEvent, this);
            Add(new LevelEndingHook(delegate {
                sfx.Source.Stop();
            }));
            walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Right, level.Bounds.Top), 8f, level.Bounds.Height));
            walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left - 8, level.Bounds.Top), 8f, level.Bounds.Height));
            walls.Add(new InvisibleBarrier(new Vector2(level.Bounds.Left, level.Bounds.Top - 8), level.Bounds.Width, 8f));
            foreach (InvisibleBarrier wall in walls) {
                Scene.Add(wall);
            }
            Add(white = GFX.SpriteBank.Create("heartGemWhite"));
            Depth = -2000000;
            yield return null;
            Celeste.Celeste.Freeze(0.2f);
            yield return null;
            Engine.TimeRate = 0.5f;
            player.Depth = -2000000;
            for (int i = 0; i < 10; i++) {
                Scene.Add(new AbsorbOrb(Position));
            }
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            level.Flash(Color.White);
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 1f;
            light.Alpha = (bloom.Alpha = 0f);
            Visible = false;
            for (float t3 = 0f; t3 < 2f; t3 += Engine.RawDeltaTime) {
                Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
                yield return null;
            }
            yield return null;
            if (player.Dead) {
                yield return 100f;
            }
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            string poemText = null;
            if (!string.IsNullOrEmpty(poemID)) {
                poemText = Dialog.Clean("poem_" + poemID);
            }
            poem = new Poem(poemText, 0, 1f);
            poem.Alpha = 0f;
            poem.Heart = VivHelperModule.spriteBank.Create("FakeRealHeart");
            Scene.Add(poem);
            poem.Heart.Play("spin");
            for (float t2 = 0f; t2 < 1f; t2 += Engine.RawDeltaTime) {
                poem.Alpha = Ease.CubeOut(t2);
                yield return null;
            }
            if (IsFake) {
                yield return DoFakeRoutineWithBird(player);
                yield break;
            }
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed) {
                yield return null;
            }
            sfx.Source.Stop();
            if (!completeArea) {
                level.FormationBackdrop.Display = false;
                for (float t = 0f; t < 1f; t += Engine.RawDeltaTime * 2f) {
                    poem.Alpha = Ease.CubeIn(1f - t);
                    yield return null;
                }
                player.Depth = 0;
                EndCutscene();

            } else {
                yield return new FadeWipe(level, wipeIn: false) {
                    Duration = 3.25f
                }.Duration;

            }

        }

        public void ShatterSpinners() {
            Level level = SceneAs<Level>();
            foreach (CrystalStaticSpinner entity3 in level.Tracker.GetEntities<CrystalStaticSpinner>()) {
                Audio.Play("event:/game/06_reflection/fall_spike_smash", Position);
                entity3.Destroy(boss: false);
            }
        }
    }
}
