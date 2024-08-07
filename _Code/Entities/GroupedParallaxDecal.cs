﻿using Celeste;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Monocle;
using Celeste.Mod;
using static Celeste.Mod.DecalRegistry;
using Mono.Cecil.Cil;

namespace VivHelper.Entities {
    public class GroupedParallaxDecal : Entity {

        private float parallaxAmount;

        // GroupedParallaxDecal class should have a constructor with params LevelData ld and DecalData dd,
        // And be placed in the center of the room
        public GroupedParallaxDecal(DecalData dd, bool isFG, Rectangle roomBounds) : base(new Vector2(roomBounds.X + roomBounds.Width / 2, roomBounds.Y + roomBounds.Height / 2)) {
            Depth = isFG ? Depths.FGDecals : Depths.BGDecals; //Set this here incase there is no Depth value in the DecalRegistry

            string path = dd.Texture.Substring(0, dd.Texture.Length - 4).Trim().ToLower();
            //all decals in a group should have the same properties, so we can just load the details for the first one.

            if (DecalRegistry.RegisteredDecals.TryGetValue(path, out DecalInfo dInfo)) {
                //there's two relevant attributes to parallaxing: depth and parallax amount
                //most parallaxed decals have exactly two decal attributes, and we need both of them (and we only support those two anyway)
                //which means looping through the list isn't a bad way to do this
                foreach (KeyValuePair<string, XmlAttributeCollection> xmlAC in dInfo.CustomProperties) {
                    if (xmlAC.Key.Equals("parallax")) {
                        parallaxAmount = float.Parse(xmlAC.Value["amount"].Value);
                    } else if (xmlAC.Key.Equals("depth")) {
                        Depth = int.Parse(xmlAC.Value["value"].Value);
                    }
                }
            } else {
                //if the decal registry info is missing just set it to zero and log the error
                parallaxAmount = 0;
                Logger.Log("Grouped Parrallax Decal", string.Format("Decal Registry data for {0} not found.", path));
            }

            AddDecalToGroup(this, dd, roomBounds);
        }

        public override void Render() {
            //adapted from Celeste.Decal.Render()
            Vector2 position = Position;
            Vector2 vector = (base.Scene as Level).Camera.Position + new Vector2(160f, 90f); //magic numbers explicitly taken from original parallaxing code in Celeste.Decal
            Vector2 vector2 = (Position - vector) * parallaxAmount;
            Position += vector2;
            base.Render();
            Position = position;
        }

        private static ILHook hook_Level_orig_LoadLevel;
        private static Dictionary<string, GroupedParallaxDecal> ParallaxDecalByGroup;

        public static void Load() {
            ParallaxDecalByGroup = new Dictionary<string, GroupedParallaxDecal>();
            hook_Level_orig_LoadLevel = new ILHook(typeof(Level).GetMethod("orig_LoadLevel", BindingFlags.Public | BindingFlags.Instance), MakeParallaxGroupsIL);
            On.Celeste.Level.UnloadLevel += ClearParallaxDecalsDict;
            On.Celeste.Level.End += ClearParallaxDecalsDict;
            On.Celeste.Level.TransitionTo += ClearParallaxDecalsDict;
        }

        private static void ClearParallaxDecalsDict(On.Celeste.Level.orig_TransitionTo orig, Level self, LevelData next, Vector2 direction) {
            orig(self, next, direction);
            ParallaxDecalByGroup.Clear();
        }

        private static void ClearParallaxDecalsDict(On.Celeste.Level.orig_UnloadLevel orig, Level self) {
            orig(self);
            ParallaxDecalByGroup.Clear();
        }

        private static void ClearParallaxDecalsDict(On.Celeste.Level.orig_End orig, Level self) {
            orig(self);
            ParallaxDecalByGroup.Clear();
        }

        public static void Unload() {
            hook_Level_orig_LoadLevel?.Dispose();
            On.Celeste.Level.UnloadLevel -= ClearParallaxDecalsDict;
            On.Celeste.Level.End -= ClearParallaxDecalsDict;
            On.Celeste.Level.TransitionTo -= ClearParallaxDecalsDict;
        }

        //Viv wrote this part. Viv named the method "NoTouchy" so I haven't.
        private static void MakeParallaxGroupsIL(ILContext il) {
            ILCursor cursor = new ILCursor(il);
            ILLabel target = null; //required because out target is not always responsive.
            int lIndex = -1; //LevelData Index
            int dIndex = -1; //DecalData Index
            if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Session>("get_LevelData"), i2 => i2.MatchStloc(out lIndex))) {
                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<LevelData>("FgDecals")) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out int _), instr => instr.MatchBr(out target))) {
                    //brtrue <target> is now our free "continue" operator
                    if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out dIndex))) //it is safe to assume that since we have retrieved the stloc.s X, that that will remain consistent, since it is within the context of the function running it.
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, dIndex); //dIndex is absolutely set
                        cursor.Emit(OpCodes.Ldloc, lIndex); //lIndex is absolutely set by first if 
                        cursor.Emit(OpCodes.Ldc_I4_1);
                        cursor.EmitDelegate<Func<Level, DecalData, LevelData, bool, bool>>(MakeParallaxGroup);
                        cursor.Emit(OpCodes.Brtrue, target);
                    }
                }
                if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<LevelData>("BgDecals")) && cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out int _), instr => instr.MatchBr(out target))) {
                    //brtrue <target> is now our free "continue" operator
                    if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchStloc(out dIndex))) //it is safe to assume that since we have retrieved the stloc.s X, that that will remain consistent, since it is within the context of the function running it.
                    {
                        cursor.Emit(OpCodes.Ldarg_0);
                        cursor.Emit(OpCodes.Ldloc, dIndex); //dIndex is absolutely set
                        cursor.Emit(OpCodes.Ldloc, lIndex); //lIndex is absolutely set by first if 
                        cursor.Emit(OpCodes.Ldc_I4_0);
                        cursor.EmitDelegate<Func<Level, DecalData, LevelData, bool, bool>>(MakeParallaxGroup);
                        cursor.Emit(OpCodes.Brtrue, target);
                    }
                }
            }
        }

        private static void AddDecalToGroup(GroupedParallaxDecal group, DecalData dd, Rectangle roomBounds) {
            Image i = new(GFX.Game["decals/" + dd.Texture.Substring(0, dd.Texture.Length - 4)]);
            i.Position = dd.Position + new Vector2(roomBounds.X, roomBounds.Y) - group.Position;
            i.Scale = dd.Scale;
            i.CenterOrigin();
            group.Add(i);
        }

        private static bool MakeParallaxGroup(Level level, DecalData dd, LevelData ld, bool isFG) {
            //If the conditions are not met to add this to the Grouped Parallax Decal, return false, otherwise determine its group,
            //If its group is found in the ParallaxDecalByGroup dictionary already, run AddDecalToGroup, otherwise construct the GroupedParallaxDecal with that DecalData and add it to the Dictionary by group
            if (!dd.Texture.Contains("vhgroupedparallaxdecals"))
                return false;

            string groupName = dd.Texture.Substring(dd.Texture.IndexOf("vhgroupedparallaxdecals/") + 24).ToLower(); //len("vhgroupedparallaxdecals/") = 24
            groupName = groupName.Substring(0, groupName.LastIndexOf("/"));
            if (ParallaxDecalByGroup.ContainsKey(groupName)) {
                Rectangle roomBounds = ld.Bounds;
                AddDecalToGroup(ParallaxDecalByGroup[groupName], dd, roomBounds);
            } else {
                GroupedParallaxDecal groupeddecal = new(dd, isFG, ld.Bounds);
                ParallaxDecalByGroup.Add(groupName, groupeddecal);
                level.Add(groupeddecal);
            }

            return true;
        }
    }
}
