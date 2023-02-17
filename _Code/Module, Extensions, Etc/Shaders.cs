using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VivHelper.Entities.Spinner2;

namespace VivHelper {

    public static class Shaders {
        public static Dictionary<string, Effect> Effects = new();

        public static void Load() {
            On.Celeste.GameplayBuffers.Create += GameplayBuffers_Create;
            Celeste.Mod.Everest.Content.OnUpdate += Content_OnUpdate;
        }


        public static void Unload() {
            On.Celeste.GameplayBuffers.Create -= GameplayBuffers_Create;
            Celeste.Mod.Everest.Content.OnUpdate -= Content_OnUpdate;
        }

        private static void GameplayBuffers_Create(On.Celeste.GameplayBuffers.orig_Create orig) {
            orig();
            SpinnerGrouper.SetTargets();
        }


        public static void Content_OnUpdate(ModAsset from, ModAsset to) {
            if (to.Format == "cso" || to.Format == ".cso") {
                try {
                    AssetReloadHelper.Do("VivHelper - Reloading Shader", () => {
                        string effectName = to.PathVirtual.Substring(8, to.PathVirtual.Length - 12);

                        if (Effects.TryGetValue(effectName, out Effect effect)) {
                            if (!effect.IsDisposed)
                                effect.Dispose();
                            Effects.Remove(effectName);
                        }

                        Logger.Log(LogLevel.Info, "VivHelper", $"Reloaded Effect: {effectName}");
                    });

                } catch (Exception e) {
                    Logger.LogDetailed(e);
                }

            }
        }

        public static Effect GetEffect(string id) {
            Effect effect;
            if (Effects.TryGetValue(id, out effect)) { return effect; }
            if (Everest.Content.TryGet($"Effects/{id}.cso", out ModAsset effectAsset, true)) {
                try {
                    effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
                    Effects.Add(id, effect);
                    effect.Parameters["Dimensions"].SetValue(new Vector2(Engine.Graphics.GraphicsDevice.Viewport.Width, Engine.Graphics.GraphicsDevice.Viewport.Height));
                    return effect;
                } catch (Exception ex) {
                    Logger.Log(LogLevel.Error, "VivHelper", "Failed to load the shader " + id);
                    Logger.Log(LogLevel.Error, "VivHelper", "Exception: \n" + ex.ToString());
                }
            }

            throw new MissingShaderException(id);
        }

    }


    public class MissingShaderException : Exception {
        private string id;
        public MissingShaderException(string id) : base() {
            this.id = id;
        }

        public override string Message => $"Shader not found: {id}";
    }
}
