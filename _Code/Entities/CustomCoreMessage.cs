﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.VivHelper;

namespace VivHelper.Entities {
    [Tracked]
    [CustomEntity("VivHelper/CustomCoreMessage = Load0", "VivHelper/CustomCoreMessage2 = Load1")]
    public class ColoredCustomCoreMessage : Entity {
        public enum PauseRenderTypes {
            Hidden = 0,
            Shown = 1,
            Fade = 2
        }
        public static Entity Load0(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new ColoredCustomCoreMessage(entityData, offset, 0);
        public static Entity Load1(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new ColoredCustomCoreMessage(entityData, offset, 1);

        private string text;
        public float alpha, defaultFadedValue;
        private bool outline, alwaysRender, lockPosition;
        private float RenderDistance, alphaMult;
        private Vector2 scale;
        private Ease.Easer EaseType;
        private Color color, outlineColor;
        private Vector2[] nodes;
        private bool CustomPositionRange;
        private float MoveSpeed;
        //AlwaysHidden = 0, AlwaysShown = 1, Fade = 2 
        private PauseRenderTypes pausetype;

        public ColoredCustomCoreMessage(EntityData data, Vector2 offset, int legacy)
            : base(data.Position + offset) {
            base.Tag = data.Bool("ShowInTransition", false) ? Tags.HUD | Tags.PauseUpdate | Tags.TransitionUpdate : Tags.HUD | Tags.PauseUpdate;
            var t1 = data.Attr("dialog", "app_ending");
            var b = false;
            if (t1.StartsWith("*§")) { text = t1.Substring(2); b = true; } else text = Dialog.Clean(t1);
            if (text.Contains("\n") || text.Contains("\r")) {
                var t2 = text.Split(new char[2]
                {
                '\n',
                '\r'
                }, StringSplitOptions.RemoveEmptyEntries);
                if (t2.Length > 0)
                    text = t2[data.Int("line")];
                else if (!b)
                    text = "{" + t1 + "}";
            }
            
            outlineColor = VivHelper.ColorFix(data.Attr("OutlineColor", "Black"), 1f);
            outline = data.Has("outline") ? data.Bool("outline") : outlineColor != Color.Transparent;
            pausetype = data.Enum<PauseRenderTypes>("PauseType", PauseRenderTypes.Hidden);
            scale = Vector2.One * data.Float("Scale", 1.25f);
            RenderDistance = data.Float("RenderDistance", 128f);
            if (!VivHelper.TryGetEaser(data.Attr("EaseType", "CubeInOut"), out EaseType))
                EaseType = Ease.CubeInOut;
            color = VivHelper.ColorFix(data.Attr("TextColor1", "White"), 1f);
            alwaysRender = data.Bool("AlwaysRender");

            lockPosition = data.Bool("LockPosition", false);
            nodes = data.NodesOffset(offset);

            switch (legacy) {
                case 0:
                    CustomPositionRange = false;
                    break;
                case 1:
                    CustomPositionRange = nodes.Length % 2 == 0 && nodes.Length > 1;
                    break;
            }

            defaultFadedValue = data.Float("DefaultFadedValue", 0f);
            alphaMult = Calc.Clamp(data.Float("AlphaMultiplier", 1f), 0f, 1f);
        }



        public override void Update() {
            Player entity = base.Scene.Tracker.GetEntity<Player>();
            if (base.Scene.Paused) {
                switch (pausetype) {
                    case PauseRenderTypes.Hidden:
                        alpha = 0f;
                        break;
                    case PauseRenderTypes.Shown:
                        break;
                    case PauseRenderTypes.Fade:
                        alpha = Calc.Approach(alpha, 0f, 0.05f);
                        break;
                }
            } else {
                float q;
                if (alwaysRender) { q = alphaMult; } else if (!CustomPositionRange) {
                    if (entity != null)
                        q = alphaMult * (defaultFadedValue + (1 - defaultFadedValue) * EaseType(Calc.ClampedMap(Math.Abs(base.X - entity.X), 0f, RenderDistance, 1f, 0f)));
                    else { q = alpha; }
                } else {
                    List<float> f = new List<float>();
                    f.Add(Calc.ClampedMap(Math.Abs(base.X - entity.X), 0f, RenderDistance, 1f, 0f));
                    for (int i = 0; i < nodes.Length; i += 2) {
                        Vector2 v = Vector2.Lerp(nodes[i], nodes[i + 1], 0.5f);
                        f.Add(Calc.ClampedMap(Math.Abs(v.X - entity.X), 0f, nodes[i + 1].X - v.X, 1f, 0f));
                    }
                    q = alphaMult * (defaultFadedValue + (1 - defaultFadedValue) * EaseType(Calc.Max(f.ToArray())));
                }
                if (pausetype == PauseRenderTypes.Fade) {
                    alpha = Calc.Approach(alpha, q, 0.05f);
                } else {
                    alpha = q;
                }
            }
            base.Update();
        }

        public override void Render() {
            Vector2 position = ((Level) base.Scene).Camera.Position;
            Vector2 value = position + new Vector2(160f, 90f);
            Vector2 position2 = lockPosition ? (Position - position) * 6f : (Position - position + (Position - value) * 0.2f) * 6f;
            if (SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode) {
                position2.X = 1920f - position2.X;
            }
            if (outline) {
                ActiveFont.DrawOutline(text, position2, new Vector2(0.5f, 0.5f), scale, color * alpha, 2f, outlineColor * alpha);
            } else {
                ActiveFont.Draw(text, position2, new Vector2(0.5f, 0.5f), scale, color * alpha);
            }
        }
    }
}
