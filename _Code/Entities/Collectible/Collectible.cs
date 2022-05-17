using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod;

namespace VivHelper.Entities
{
    [CustomEntity("VivHelper/SimplestCoin = SimplestLoad",
                  "VivHelper/SimpleCoin = SimpleLoad",
                  "VivHelper/AdvancedCoin = AdvancedLoad",
                  "VivHelper/ComplicatedCoin = ComplexLoad",
                  "VivHelper/CustomCoin = CustomLoad")]
    [Tracked]
    public class Collectible : Entity
    {
        public static void Load()
        {
            Everest.Events.Level.OnExit += Level_OnExit;
        }
        public static void Unload()
        {
            Everest.Events.Level.OnExit += Level_OnExit;
        }

        private static void Level_OnExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            Controller = null;
        }

        public static Entity SimplestLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new Collectible(entityData, offset, new EntityID(levelData.Name, entityData.ID), 0);
        public static Entity SimpleLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new Collectible(entityData, offset, new EntityID(levelData.Name, entityData.ID), 1);
        public static Entity AdvancedLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new Collectible(entityData, offset, new EntityID(levelData.Name, entityData.ID), 2);
        public static Entity ComplexLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new Collectible(entityData, offset, new EntityID(levelData.Name, entityData.ID), 3);
        public static Entity CustomLoad(Level level, LevelData levelData, Vector2 offset, EntityData entityData) => new Collectible(entityData, offset, new EntityID(levelData.Name, entityData.ID), 4);

        public enum PersistenceType { None = 0, Permanent = 1, OnFlag = 2, OnNewRoomTransition = 3 }

        public static CollectibleMasterController Controller;

        public static ParticleType sparkles = new ParticleType(NPC03_Oshiro_Lobby.P_AppearSpark) { Color2 = Color.Transparent };

        //Collection Formats
        public Action<Entity> OnCollect;
        public bool PlayerCollect = true;
        public bool HoldableCollect = false;
        public bool TheoCollect = false;
        public bool JellyCollect = false;
        public bool SeekerCollect = false;
        public string FlagCollect = null;
        public Type[] CollectTypes = null;

        public bool GroupMaster;
        public string[] triggeredGroups;

        public Sprite sprite;
        public readonly bool CollectAnim, CollectOnFastIdle;

        public bool AddSparkles;
        public bool Collected;
        public bool Completed;
        public PersistenceType Persistent;
        public EntityID ID;

        public Collectible(EntityData e, Vector2 v, EntityID id, int r) : base(e.Position + v)
        {
            //Base mechanics
            ID = id;
            Persistent = e.Has("persistent") ? e.Enum<PersistenceType>("persistent") : PersistenceType.None;
            switch (Persistent)
            {
                case PersistenceType.OnNewRoomTransition:
                    Add(new TransitionListener() { OnOutBegin = delegate { if (SceneAs<Level>().Session.Level != VivHelperModule.Session.PreviousLevel) StopSpawning(); } });
                    break;
            }

            switch (r)
            {
                case -1:
                    break;
                //Simplest
                case 0:
                    string sp = e.Attr("SpritePath");
                    sprite = VivHelperModule.spriteBank.Create(sp);
                    switch (sp)
                    {
                        case "goldcoin":
                            PlayerCollect = true;
                            Add(new PlayerCollider(Collect));
                            TheoCollect = false;
                            JellyCollect = false;
                            HoldableCollect = false;
                            SeekerCollect = false;
                            break;
                    }

                    break;
                default:
                    break;
            }
            
            if (e.Bool("FromSpriteBank"))
            {
                sprite = GFX.SpriteBank.Create(e.Attr("SpritePath").Trim());
                
            }
            else
            {
                CollectOnFastIdle = e.Bool("CollectIsFastIdle");
                sprite = new Sprite(GFX.Game, e.Attr("SpritePath").Trim().TrimEnd('/') + "/");
                sprite.AddLoop("idle", "idle", 0.06f);
                if (CollectAnim) {
                    if (CollectOnFastIdle)
                    {
                        List<MTexture> l = new List<MTexture>();
                        for (int _ = 0; _ < 4; _++) l.AddRange(sprite.Animations["idle"].Frames);
                        sprite.Add("collect", 0.03f, "idle", l.ToArray());
                        sprite.OnFinish = delegate(string q)
                        {
                            if(q == "collect")
                            {
                                sprite.Visible = false;
                                (sprite.Entity as Collectible).Completed = true;
                            }
                        };
                    }

                }
               
            }
            AddSparkles = e.Bool("AddParticles");


            
        }

       

        public void Collect(Entity e)
        {

        }

        public void StopSpawning()
        {
            (Scene as Level).Session.DoNotLoad.Add(this.ID);
        }

    }
}
