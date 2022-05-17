using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Celeste.Mod;
using System.Collections;

namespace VivHelper.Entities.SeekerStuff {
    [Tracked]
    [CustomEntity("VivHelper/CustomSeekerGenerator")]
    public class CustomSeekerGenerator : Entity {
        public List<CustomSeeker> seekers;
        public EntityData[] seekerDataList;
        public string fileName;

        public float delayBetweenSpawning;
        public bool spawnAfterKill;
        public int spawnMax;
        public bool startOn;
        public string flagEnable, flagDisable;
        public int N, count; public string[] flagsOnN;
        public int clusterAmount;
        public Vector2 exitSpeed;
        public Vector2 offset;

        public bool loadFailed;

        public bool enabled;

        public CustomSeekerGenerator(EntityData data, Vector2 offset) : base(data.Position + offset) {
            this.offset = offset;
            fileName = data.Attr("FileName", "");
            if (fileName == "") { fileName = "Seeker"; }
            CustomSeekerGeneratorYaml genYaml = LoadYaml(fileName);
            loadFailed = genYaml == null;
            if (!loadFailed) {
                count = 0;
                seekers = new List<CustomSeeker>();
                seekerDataList = new EntityData[genYaml.Seekers.Count];
                foreach (CustomSeekerYaml csy in genYaml.Seekers) {
                    seekerDataList[csy.Order] = csy.SeekerDataFromYaml(data.Position);
                }
                delayBetweenSpawning = data.Float("DelayBetweenSpawning", 2f);
                spawnAfterKill = data.Bool("SpawnAfterKill", true);
                spawnMax = data.Int("SpawnMax", 3);
                startOn = data.Bool("StartOn", true);
                flagEnable = data.Attr("FlagEnable", "");
                flagDisable = data.Attr("FlagDisable", "");
                N = data.Int("SpawnLimit", 10);
                flagsOnN = data.Attr("FlagsOnLimit", "").Split(',');
                Array.Resize<string>(ref flagsOnN, flagsOnN.Length + 1);
                flagsOnN[flagsOnN.Length - 1] = flagDisable;
                exitSpeed = new Vector2(data.Float("ExitSpeed") * (float) Math.Cos(Math.PI * data.Float("ExitDirection") / 180), data.Float("ExitSpeed") * (float) Math.Sin(Math.PI * data.Float("ExitDirection") / 180));
            }
        }

        public override void Awake(Scene scene) {
            base.Awake(scene);
            Add(new Coroutine(Sequence()));
        }

        public IEnumerator Sequence() {
            yield return 1;
            while (count < N) {
                if (Scene.Tracker.CountEntities<CustomSeeker>() < spawnMax && !(spawnAfterKill && seekers.Count != 0)) {
                    Spawn(count);
                    yield return delayBetweenSpawning;
                }
            }
            foreach (string f in flagsOnN)
                SceneAs<Level>().Session.SetFlag(f);
            RemoveSelf();
        }

        public void Spawn(int num) {
            Audio.Play("event:/char/badeline/appear", Position);
            CustomSeeker cs = new CustomSeeker(seekerDataList[num], offset);
            cs.Speed = exitSpeed;
            Scene.Add(cs);

        }

        public static CustomSeekerGeneratorYaml LoadYaml(string path) {
            String fullPath = "Seekers/" + path;
            if (!Everest.Content.TryGet(fullPath, out ModAsset asset)) {
                Logger.Log("VivHelper", "Failed loading Seeker Generator file \"" + path + "\": The file could not be found.");
                Engine.Commands.Log("VivHelper: The game tried to load the file at \"" + fullPath + "\" but couldn't find it. You may have input the name of the file wrong.");
                return null;
            } else {
                try {
                    using (StreamReader reader = new StreamReader(asset.Stream)) {
                        return YamlHelper.Deserializer.Deserialize<CustomSeekerGeneratorYaml>(reader);
                    }
                } catch (Exception e) {
                    Logger.Log("VivHelper", "Failed loading Seeker Generator file \"" + path + $"\": {e.Message}");
                    return null;
                }
            }
        }
    }

    public class CustomSeekerGeneratorYaml {
        public List<CustomSeekerYaml> Seekers;
    }
}
