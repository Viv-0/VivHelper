using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {

    public class BronzeBerry : Entity {


        private static Vector2 controlPoint = new Vector2(28,55);
        private static IDetour[] detours;
        internal static MTexture bronzeGui;

        public static void Load() {
            using(new DetourContext(){ Before = { "*" } }){
                detours = new IDetour[] {
                    new ILHook(typeof(OuiChapterPanel).GetNestedType("Option", BindingFlags.Instance|BindingFlags.NonPublic).GetMethod("Render"), ModifyRender),
                    new ILHook(typeof(OuiChapterPanel).GetMethod("SwapRoutine",BindingFlags.NonPublic|BindingFlags.Instance).GetStateMachineTarget(), CacheOptionData)
                };
            }
        }

        public static void Unload() {
            foreach(var detour in detours) detour?.Dispose();
        }

        public static void CacheOptionData(ILContext ctx) {
            ILCursor cursor = new ILCursor(ctx);
            while(cursor.TryGotoNext(MoveType.After, instr=>instr.MatchStfld(typeof(OuiChapterPanel).GetNestedType("Option", BindingFlags.Instance|BindingFlags.NonPublic).GetField("Siblings")) && instr.Next.OpCode == OpCodes.Callvirt)) {
                cursor.Emit(OpCodes.Dup);
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Call, typeof(BronzeBerry).GetMethod("bronzeCacheData", BindingFlags.Static|BindingFlags.NonPublic));
            }
        }
        private static void bronzeCacheData(object temp, OuiChapterPanel panel) {
            DynamicData d = DynamicData.For(temp);
            if(d.Get("CheckpointLevelName") == null && !d.Get<bool>("Large"))
                d.Set("VH_CustomCLN", panel.Area.SID);
        }

        public static void ModifyRender(ILContext ctx) {
            ILCursor cursor = new ILCursor(ctx);
            VariableDefinition v = new VariableDefinition(ctx.Import(typeof(float)));
            ctx.Body.Variables.Add(v);

            if (cursor.TryGotoNext(instr => instr.MatchRet()) && cursor.TryGotoPrev(MoveType.Before, instr => instr.MatchCallvirt<MTexture>("DrawCentered"))) {
      
                cursor.Emit(OpCodes.Stloc, v);
                cursor.Emit(OpCodes.Ldloc, v);
                cursor.Index++;
                cursor.Emit(OpCodes.Ldarg_0);
                cursor.Emit(OpCodes.Ldloc_1);
                cursor.Emit(OpCodes.Ldloc,v);
                cursor.Emit(OpCodes.Call, typeof(BronzeBerry).GetMethod("bronzeRenderFromCache", BindingFlags.Static|BindingFlags.NonPublic));
            } 
        }

        private static void bronzeRenderFromCache(object temp, Vector2 renderPoint, float scale) {
            DynamicData d = DynamicData.For(temp); //Idk how to get the Option type so we're using DynamicData
            string value;
            if(!d.TryGet("VH_CustomCLN", out value))
                value = d.Get<string>("CheckpointLevelName");
            if(VivHelperModule.SaveData.Bronzes.Contains(value)){
                if(bronzeGui == null) bronzeGui = GFX.Gui["VivHelper/bronzeberry"];
                bronzeGui.DrawCentered(renderPoint + controlPoint, Color.White, scale * 0.666f);
            }
        }

        public BronzeBerry(EntityData data, Vector2 offset) : base(data.Position + offset) {

        }
    }
}
