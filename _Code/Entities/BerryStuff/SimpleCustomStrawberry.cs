using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;
using Celeste.Mod;
using MonoMod.Utils;
using System.Reflection;
using System.Collections;

namespace VivHelper {
    [CustomEntity("VivHelper/CustomStrawberry")]
    public class CustomStrawberry : Strawberry {
        private static MethodInfo collectroutine = typeof(Strawberry).GetMethod("CollectRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
        public DynData<Strawberry> dyn;
        public string xmlKey;
        public bool Fake;
        private bool isGhostBerry;



        public CustomStrawberry(EntityData e, Vector2 v, EntityID id) : base(e, v, id) {
            dyn = new DynData<Strawberry>(this);
            dyn.Set<bool>("Golden", false);
            xmlKey = e.Attr("Directory", "");
            if (xmlKey == "")
                xmlKey = "strawberry";
            isGhostBerry = SaveData.Instance.CheckStrawberry(ID);

        }

        public override void Added(Scene scene) {
            base.Added(scene);
            Remove(dyn.Get<Sprite>("sprite"));
            dyn.Set<Sprite>("sprite", GFX.SpriteBank.Create(xmlKey));
            Add(dyn.Get<Sprite>("sprite"));


        }

        public void OnCollect2() {
            if (!dyn.Get<bool>("collected")) {
                int collectIndex = 0;
                dyn.Set<bool>("collected", true);
                if (Follower.Leader != null) {
                    Player obj = Follower.Leader.Entity as Player;
                    collectIndex = obj.StrawberryCollectIndex;
                    obj.StrawberryCollectIndex++;
                    obj.StrawberryCollectResetTimer = 2.5f;
                    Follower.Leader.LoseFollower(Follower);
                }
                Session session = (base.Scene as Level).Session;
                session.DoNotLoad.Add(ID);
                session.UpdateLevelStartDashes();
                Add(new Coroutine(CollectRoutine(collectIndex)));
            }
        }


        private IEnumerator CollectRoutine(int collectIndex) {
            _ = Scene;
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;
            int num = 0;
            if (Moon) {
                num = 3;
            } else if (isGhostBerry) {
                num = 1;
            } else if (Golden) {
                num = 2;
            }
            Audio.Play(SFX.game_gen_strawberry_get, Position, "colour", num, "count", collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            dyn.Get<Sprite>("sprite").Play("collect");
            while (dyn.Get<Sprite>("sprite").Animating) {
                yield return null;
            }
            StrawberryPoints sp = new StrawberryPoints(Position, isGhostBerry, collectIndex, Moon);
            DynData<StrawberryPoints> d = new DynData<StrawberryPoints>(sp);
            sp.Remove(d.Get<Sprite>("sprite"));
            d.Set<Sprite>("sprite", GFX.SpriteBank.Create(xmlKey));
            sp.Add(d.Get<Sprite>("sprite"));
            Scene.Add(sp);
            RemoveSelf();
        }

    }
    /**
        [CustomEntity("VivHelper/FakeStrawberry")]
        public class FakeStrawberry : Strawberry
        {
            public DynData<Strawberry> dyn;
            public string xmlKey;
            public bool Fake;
            private bool isGhostBerry;



            public FakeStrawberry(EntityData e, Vector2 v, EntityID id) : base(e, v, id)
            {
                dyn = new DynData<Strawberry>(this);
                dyn.Set<bool>("Golden", false);
                xmlKey = e.Attr("Directory", ""); if (xmlKey == "") xmlKey = "strawberry";
                isGhostBerry = SaveData.Instance.CheckStrawberry(ID);

            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Remove(dyn.Get<Sprite>("sprite"));
                dyn.Set<Sprite>("sprite", GFX.SpriteBank.Create(xmlKey));
                Add(dyn.Get<Sprite>("sprite"));


            }

            public void OnCollect2()
            {
                if (!dyn.Get<bool>("collected"))
                {
                    int collectIndex = 0;
                    dyn.Set<bool>("collected", true);
                    if (Follower.Leader != null)
                    {
                        Player obj = Follower.Leader.Entity as Player;
                        collectIndex = obj.StrawberryCollectIndex;
                        obj.StrawberryCollectIndex++;
                        obj.StrawberryCollectResetTimer = 2.5f;
                        Follower.Leader.LoseFollower(Follower);
                    }
                    Add(new Coroutine(FakeCollectRoutine(collectIndex)));
                }
            }

            private IEnumerator FakeCollectRoutine(int collectIndex)
            {
                _ = Scene;
                Tag = Tags.TransitionUpdate;
                Depth = -2000010;
                Audio.Play(SFX.game_gen_strawberry_get, Position, "colour", 1, "count", collectIndex);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                dyn.Get<Sprite>("sprite").Play("collect");
                while (dyn.Get<Sprite>("sprite").Animating)
                {
                    yield return null;
                }
                StrawberryPoints sp = new StrawberryPoints(Position, true, collectIndex, Moon);
                DynData<StrawberryPoints> d = new DynData<StrawberryPoints>(sp);
                sp.Remove(d.Get<Sprite>("sprite"));
                d.Set<Sprite>("sprite", GFX.SpriteBank.Create(xmlKey));
                sp.Add(d.Get<Sprite>("sprite"));
                Scene.Add(sp);
                RemoveSelf();
            }


        }**/
}
