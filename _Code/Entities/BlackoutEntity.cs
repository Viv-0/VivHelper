using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using VHM = VivHelper.VivHelperModule;
using Celeste.Mod;
using MonoMod.Utils;

namespace VivHelper.Entities {
    public class Blockout {
        public static float alphaFade = 1f;
        private static Dictionary<Entity, int> origDepths = new Dictionary<Entity, int>();
        //All the hooks are the same, because I need to do the same
        #region Useless Hooks
        public static void BlackoutLK(On.Celeste.Lookout.orig_Update orig, Lookout self) {
            if (VHM.Session.Blackout) {
                int origDepth = self.Depth;
                self.Depth = -250001;
                orig(self);
                self.Depth = origDepth;
            } else {
                orig(self);
            }
        }

        public static void BlackoutPP(On.Celeste.PlayerPlayback.orig_Update orig, PlayerPlayback self) {
            if (VHM.Session.Blackout) {
                int origDepth = self.Depth;
                self.Depth = -250001;
                orig(self);
                self.Depth = origDepth;
            } else {
                orig(self);
            }
        }

        public static void BlackoutP(On.Celeste.Player.orig_Update orig, Player self) {
            if (VHM.Session.Blackout) {
                int origDepth = self.Depth;
                self.Depth = -250001;
                orig(self);
                self.Depth = origDepth;
            } else {
                orig(self);
            }
        }
        public static void BlackoutG(On.Celeste.Glider.orig_Update orig, Glider self) {
            if (VHM.Session.Blackout) {
                int origDepth = self.Depth;
                self.Depth = -250001;
                orig(self);
                self.Depth = origDepth;
            } else {
                orig(self);
            }
        }
        #endregion

        public static void Blackout(On.Monocle.Entity.orig_Update orig, Entity self) {
            if (self is Lookout ||
                self is BadPlaybackWatchtower || self is CustomPlaybackWatchtower || self is PlatinumWatchtower ||
                self is Player ||
                self is PlayerPlayback ||
                self.Components.Get<Holdable>() != null) {
                if (self.Depth != -250001 && VHM.Session.Blackout && alphaFade > 0.5f) {
                    origDepths.Add(self, self.Depth);
                    self.Depth = -250001;
                } else if (self.Depth == -250001 && !VHM.Session.Blackout && alphaFade < 0.5f) {
                    self.Depth = origDepths[self];
                    origDepths.Remove(self);
                }
            }
            orig(self);
        }


        //Code Borrowed From Spring Collab, I would've written this myself but there's no other way to do it.
        private static void BloomRendererHook(On.Celeste.BloomRenderer.orig_Apply orig, BloomRenderer self, VirtualRenderTarget target, Scene scene) {
            if (alphaFade < 1f) {
                // multiply all alphas by alphaFade, and back up original values.
                List<BloomPoint> affectedBloomPoints = new List<BloomPoint>();
                List<float> originalAlpha = new List<float>();
                foreach (BloomPoint bloomPoint in scene.Tracker.GetComponents<BloomPoint>().ToArray()) {
                    if (bloomPoint.Visible && !(bloomPoint.Entity is Payphone)) {
                        affectedBloomPoints.Add(bloomPoint);
                        originalAlpha.Add(bloomPoint.Alpha);
                        bloomPoint.Alpha *= alphaFade;
                    }
                }

                // render the bloom.
                orig(self, target, scene);

                // restore original alphas.
                int index = 0;
                foreach (BloomPoint bloomPoint in affectedBloomPoints) {
                    bloomPoint.Alpha = originalAlpha[index++];
                }
            } else {
                // alpha multiplier is 1: nothing to modify, go on with vanilla.
                orig(self, target, scene);
            }
        }
        //Code Borrowed From Spring Collab, I would've written this myself but there's no other way to do it.
        private static void LightHook(On.Celeste.LightingRenderer.orig_BeforeRender orig, LightingRenderer self, Scene scene) {
            if (alphaFade < 1f) {
                // multiply all alphas by alphaFade, and back up original values.
                List<VertexLight> affectedVertexLights = new List<VertexLight>();
                List<float> originalAlpha = new List<float>();
                foreach (VertexLight vertexLight in scene.Tracker.GetComponents<VertexLight>().ToArray()) {
                    if (vertexLight.Visible && !vertexLight.Spotlight) {
                        affectedVertexLights.Add(vertexLight);
                        originalAlpha.Add(vertexLight.Alpha);
                        vertexLight.Alpha *= alphaFade;
                    }
                }

                // render the lighting.
                orig(self, scene);

                // restore original alphas.
                int index = 0;
                foreach (VertexLight vertexLight in affectedVertexLights) {
                    vertexLight.Alpha = originalAlpha[index++];
                }
            } else {
                // alpha multiplier is 1: nothing to modify, go on with vanilla.
                orig(self, scene);
            }
        }

    }

    [Tracked]
    [CustomEntity("VivHelper/BlackoutController")]
    public class BlackoutController : Entity {
        public enum States {
            On,
            Flashing,
            Flag,
            Off
        }
        public States state;
        public float delay = 0f;
        private float timer;
        private MTexture blackout;
        private int checkCount = -1;

        public BlackoutController(EntityData data, Vector2 offset) : base(data.Position + offset) {
            state = data.Enum<States>("StartingState", States.Off);
            if (state == States.Flashing) { delay = data.Float("Delay", 3f); timer = delay; }
            base.Depth = -249900;
            blackout = GFX.Game["VivHelper/entities/Blackout"];
        }

        public override void Awake(Scene scene) {
            if (scene.Tracker.CountEntities<BlackoutController>() > 1) { checkCount = 0; }
            base.Awake(scene);
            VHM.Session.Blackout = state == States.On;
            Add(new TransitionListener { OnOutBegin = delegate { VHM.Session.Blackout = false; } });

        }

        public override void Update() {
            if (checkCount > -1 && checkCount < 10) {
                if (Scene.Tracker.CountEntities<BlackoutController>() > 1) {
                    /**/

                    checkCount++;
                }
            }
            if (checkCount > 9) {
                throw new Exception("Multiple Blackout Controllers found within the scene.");
            }
            base.Update();
            if (state == States.Flashing) {
                if (timer > 0f) { timer -= Engine.DeltaTime; } else { VHM.Session.Blackout = !VHM.Session.Blackout; timer = delay; }
            } else if (state == States.Flag) {
                VHM.Session.Blackout = (Scene as Level).Session.GetFlag("VH_Blackout");
            }
        }

        public override void Render() {
            base.Render();
            blackout.DrawCentered((Scene as Level).Camera.Position + new Vector2(160f, 90f), Color.White * (1 - Blockout.alphaFade));
        }
    }
}
