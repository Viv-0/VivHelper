using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Microsoft.Xna.Framework;
using Celeste.Mod.Entities;

namespace VivHelper.Entities {
    [Tracked]
    public class CollectibleController : Entity {
        public struct GroupDef {
            /// <summary>
            /// Name of the group
            /// </summary>
            public string groupName;
            /// <summary>
            /// Defines the groups it triggers
            /// </summary>
            public string[] triggeredGroups;
            /// <summary>
            /// If true, this coin is collectable at the start
            /// </summary>
            public bool enabledAtStart;
            /// <summary>
            /// By default, triggers when all coins of the group have been collected, Key Coins are set by a boolean in the Coin object as a key of its group
            /// </summary>
            public bool triggerOnKeyCoins;

            public int maximum;
            public int totalKeyCoins;

            public GroupDef(string gN, string tG, bool eAS, bool tOKC) {
                groupName = gN;
                enabledAtStart = eAS;
                triggerOnKeyCoins = tOKC;
                if (tG == null)
                    triggeredGroups = null;
                else {
                    triggeredGroups = tG.Split(',');
                    for (int i = 0; i < triggeredGroups.Length; i++)
                        triggeredGroups[i] = triggeredGroups[i].Trim(); //Cleans up trimming
                }
                maximum = totalKeyCoins = 0;
            }

            public void AddOne(bool keyCoin) {
                maximum++;
                if (keyCoin)
                    totalKeyCoins++;
            }
            public void RemoveOne(bool keyCoin) {
                maximum--;
                if (keyCoin)
                    totalKeyCoins--;
            }

            public override string ToString() {
                return $"{groupName}: max {maximum}  enabledAtStart {enabledAtStart}";
            }
        }



        // CollectibleController uses Session, nothing is technically stored in controller other than Group instructions which is also important
        private Dictionary<string, GroupDef> GroupDefinitions;

        private List<Collectible> CollectibleSet;

        public CollectibleController(List<EntityData> datas) {
            GroupDefinitions = new Dictionary<string, GroupDef>();
            GroupDefinitions[""] = new GroupDef("", null, true, false);
            foreach (EntityData data in datas) {
                string g = data.Attr("group");
                if (!string.IsNullOrWhiteSpace(g)) {
                    GroupDefinitions[g] = new GroupDef(g, data.Attr("groupsTriggered", ""), data.Bool("enabledOnRoomLoad", true), data.Bool("triggeredOnKeyCoins", false));


                } else
                    throw new Exception($"Collectible Group Identifier in room {data.Level.Name} at position {data.Position} had no group name.");
            }
            if (CollectibleSet == null)
                CollectibleSet = new List<Collectible>();
            Tag = Tags.Global;
        }

        public void Track(Collectible coin) {
            if (CollectibleSet == null) {
                CollectibleSet = new List<Collectible>();
            }
            CollectibleSet.Add(coin);
            if (coin.group == null) {
                GroupDefinitions[""].AddOne(false);
                coin.Enable(false);
            } else {
                GroupDef gd = GroupDefinitions[coin.group];
                gd.AddOne(coin.isKeyCoin);
                if (gd.enabledAtStart) {
                    coin.Enable(false);
                }
            }


        }

        public void Untrack(Collectible coin) {
            CollectibleSet.Remove(coin);
            if (coin.group == null)
                GroupDefinitions[""].RemoveOne(false);
            else
                GroupDefinitions[coin.group].RemoveOne(coin.isKeyCoin);
        }

        public bool AddCollectedCoin(Collectible coin) {
            var g = coin.group;
            if (!VivHelperModule.Session.CollectedCoins.ContainsKey(g))
                VivHelperModule.Session.CollectedCoins.Add(g, new HashSet<EntityID>());
            bool b = VivHelperModule.Session.CollectedCoins[g].Add(coin.ID);
            GroupDef gd = GroupDefinitions[g];
            if ((VivHelperModule.Session.CollectedCoins[g].Count == gd.maximum) && gd.triggeredGroups != null) {
                foreach (string h in GroupDefinitions[g].triggeredGroups) {
                    foreach (Collectible c in CollectibleSet.Where(a => a.group == h && a.enabled)) {
                        c.Enable(true);
                    }
                }
            }
            return b;
        }

    }
}
