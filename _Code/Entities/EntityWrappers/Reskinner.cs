using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Celeste.Mod.Entities;
using Monocle;
using Microsoft.Xna.Framework;
using System.Reflection;
using MonoMod.Utils;
using MonoMod;

namespace VivHelper.Entities
{
    [Tracked]
    public class GlobalReskinner : Entity
    {
        #region Hooks

        public static bool firstRender = false;

        public static void Load()
        {
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
            On.Monocle.Scene.BeforeUpdate += Scene_BeforeUpdate;
        }

       

        public static void Unload()
        {
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static void Scene_BeforeUpdate(On.Monocle.Scene.orig_BeforeUpdate orig, Scene self)
        {
            orig(self);
            if (firstRender)
            {
                //We know that if firstRender is true, then there is a GlobalReskinner in the current Level scene
                firstRender = false;
                if (self is Level) //Just a good check to have in general
                {
                    GlobalReskinner gr = self.Tracker.GetEntity<GlobalReskinner>();
                    foreach(Entity e in self.Entities.Where((e) => !e.Components.Contains<ReskinnerComponent>() && gr.e.GetType())
                    {
                        if(e.Components.Contains<ReskinnerComponent>())
                    }
                }
            }
            
           
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
        {
            orig(self);
            GlobalReskinner gr = null;
            DynData<LevelLoader> dyn = new DynData<LevelLoader>(self);
            foreach(LevelData ld in dyn.Get<Session>("session").MapData.Levels)
            {
                foreach(EntityData entityData in ld.Entities)
                {
                    if(entityData.Name == "VivHelper/GlobalReskinner")
                    {
                        if (gr == null)
                        {
                            self.Level.Add(gr = new GlobalReskinner());
                            firstRender = true;
                        }

                    }
                }
            }
        }
        #endregion

        public 
    }

    

    public class ReskinnerComponent : Component
    {
        
    }

    public class GlobalReskinAdder : Entity
    {
        public Type type;
        public List<string> spriteSet;
    }

    public class Reskinner : Entity
    {

    }
}
