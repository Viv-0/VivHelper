using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monocle;
using Celeste;
using Microsoft.Xna.Framework;
using VivHelper.Entities.Boosters;
using VivHelper.Entities;

namespace VivHelper {
    public enum FastBubbleState {
        Normal = 0,
        NoFastBubble = -1,
        FastBubbleOrKill = 1
    }

    public enum DashAngleState {
        ForceDashDirection = 1,
        OffsetByAngle = 0,
        ForcePreviousSpeedAngle = 3
    }
    public enum RefillTiming {
        BoostBegin,
        BoostEnd,
        DashBegin,
        DashEnd,
    }
    public enum DashSolidContact {
        Ignore = -1,
        Default = 0,
        Kill = 1,
        Bounce = 2
    }

    public enum SpeedMagnitudeCheck {
        Default = 0,
        MomentumRetain = 1
    }

    public enum CustomDashActionTypes {
        OnCustomDashBegin, OnCustomDashEnd, OnCustomDashRefill, OnCustomBoostBegin, OnCustomBoostEnd
    }

    public enum CornerCorrect {
        PostHit,
        PreHit,
        NoCornerCorrection
    }

    [Flags]
    public enum DashEndSlowdownTypes {
        Down = 1,
        Right = 2,
        Up = 4,
        Left = 8
    }
    public struct CustomDashActions {
        public Action<Player> onCustomDashBegin;
        public Action<Player> onCustomDashEnd;
        public Action<Player> onCustomDashRefill;
        public Action<Player> onCustomBoostBegin;
        public Action<Player> onCustomBoostEnd;
        public Action<Vector2> onCustomBoosterRender;
    }

    public class CustomDashStateCh {
        public static Dictionary<string, EntityData> presetCustomBoosterStates = new Dictionary<string, EntityData>();
        // Whether or not the dash ends when hitting the boundary of a room (that isn't a transition)
        public bool BreakOnBounds = true;
        // Whether or not the dash can end by entering a dream block
        public bool CanEnterDreamBlock = true;
        // How long to freeze the game after executing the dash behavior in seconds
        public float FreezeFrames = 0.05f;

        //Determines whether or not to force the Initial Dash Direction to a specific value
        public DashAngleState ForceDashDirection = DashAngleState.OffsetByAngle;

        //the speed the player goes while dashing, default value 240(px/s). If negative, acts as Inverse Dash.
        public float DashSpeed = 240f;
        public bool InvertDash = false;

        //The Angles that correspond 
        public int DashEndType = -2;
        //The multipier from DashSpeed to get DashEndSpeed.
        public float DashEndMultiplier = 0.66666f;

        //the time the dash takes. The "length" the dash is, is just DashSpeed * DashDuration. Default value 0.15.
        //If the value is set to <0, this becomes identical to a RedDash, and combined with HeldDash boolean acts as the default behavior for the HeldDash variant, (superdash version will allow for steering)
        public float DashDuration = 0.15f;
        // Dashes for as long as you hold the dash button. Defaults to Infinite delay time inside a bubble, and candashexit = false
        public bool HeldDash = false;
        // Determines whether or not you can leave a bubble via dashing. Held Dash is forced false, because you dash exit upon releasing dash
        public bool CanDashExit = true;

        //offsets the player-input direction by AngleOffset degrees, default value 0
        public float AngleOffset = 0f;

        //changes the steering speed of the dash in degrees per frame, assuming a normal 60fps gamerate. If set to 0, SuperDashing is disabled. Default null.
        public float SuperDashSteerSpeed = 0f;
        //Technical. Ask Viv about it if you need it
        public bool PrioritizeCornerCorrection = false;
        //What to do when the player hits a solid from the dash state
        public DashSolidContact DashSolidEffect = DashSolidContact.Default;
        // If DashSolidEffect = Kill, at what angle does it count as "crashing" into the wall = kill, versus a "graze" = no kill. Dot Limit is defined by the DotProduct
        public float DashKillDotLimit = 0.3f;

        //Uses speed magnitude in some speed-relevant checks which can enable speed retention through alternate angles.
        public SpeedMagnitudeCheck SpeedMagnitudeCheck = SpeedMagnitudeCheck.Default;

        //Default value false, see DashRefill for more info.
        public bool DashRefillModifier = false;
        public int DashRefill = -1;
        /* if DashRefillMod is false:
             If DashRefill <0, then it will set the player's Dash Count to the Inventory Dashes, otherwise it will set the player's current dashes to DashRefill.
           if DashRefillMod is true: 
             Adds DashRefill to the player's Dash Count */

        //Default value false, see StamRefill for more info
        public bool StamRefillModifier = false;
        public float StamRefill = -1f;
        /* if StamValueMod is false:
             If StamValue is <0, then it will set the player's Dash Count to the Inventory Stamina (default 110), otherwise it will set the player's current stamina to StamValue.
           if StamValueMod is true: 
             Adds StamValue to the player's Stamina */

        // Dash Bonus Handlers
        // Time you can dash after doing a custom dash/bubble, in seconds
        public float dashCooldownTime = 0.2f;
        // Time you can regain a dash after doing a custom dash/bubble, in seconds
        public float dashRefillCooldownTime = 0.1f;
        // Timer you can get a boost from the dash in seconds *after* dash ends.
        public float gliderBoostTime = 0.55f;

        //Refill Params
        // Normal = Green Bubble, Kill = Yellow Bubble, No FastBubble = Grey Bubble with Timer (timer set to 0, == grey bubble)
        public FastBubbleState FastBubbleState = FastBubbleState.Normal;
        //if set <0, fastbubbleTimer will wait for dash input, regardless of FastBubbleState. If ==0, the bubble will dash immediately i.e. Gray Booster.
        public float FastBubbleTimer = 0.25f;
        // Whether or not to drop the holdable (jelly, theo) when entering the bubble
        public bool DropHoldable = true;
        // Order of operations for when to Refill the player's dashes while in a bubble
        public RefillTiming RefillTiming = RefillTiming.BoostBegin;
        // If FastBubbleKill, flashes this color.
        public Color FlashColor = Color.Red;
        // WIP: Disables Bubble State Retention
        public bool DisableRetentionTech = false;
        // If true, can break dash blocks, hit kevins, etc.
        public bool ImpactsObjectsAsDash = true;
        // If true, the dash direction does not change if you hit ground. Dashes do this, but RedDashes do not.
        public bool OverrideDashDirResetOnGround = true;
        // If true, when moving in a direction not straight down, you can jump on the ground to get a wavedash. RedBubbles do not do this, but regular dashes do
        public bool CanWavedashOut = true;

        // Voodoo Magic
        // Allows for mods to use APIs to hook in extra behaviors, such as Shadow Dash, Sun Dash, Time Crystal, etc.
        public string ExtraParameters = string.Empty;

        public bool IsRedDashEsque => HeldDash || DashDuration < 0;

        public void HandleParameters(CustomDashActionTypes actionForm, Player player) {
            foreach (string q in ExtraParameters.Split(',')) {
                if (UltraCustomDash.customDashSpecialHandlers.TryGetValue(q.Trim(), out CustomDashActions value)) {
                    switch (actionForm) {
                        case CustomDashActionTypes.OnCustomBoostBegin:
                            value.onCustomBoostBegin?.Invoke(player);
                            break;
                        case CustomDashActionTypes.OnCustomBoostEnd:
                            value.onCustomBoostEnd?.Invoke(player);
                            break;
                        case CustomDashActionTypes.OnCustomDashRefill:
                            value.onCustomDashRefill?.Invoke(player);
                            break;
                        case CustomDashActionTypes.OnCustomDashBegin:
                            value.onCustomDashBegin?.Invoke(player);
                            break;
                        case CustomDashActionTypes.OnCustomDashEnd:
                            value.onCustomDashEnd?.Invoke(player);
                            break;
                    }
                }
            }
        }
        public void HandleParameterRender(Vector2 position) {
            foreach (string q in ExtraParameters.Split(',')) {
                if (UltraCustomDash.customDashSpecialHandlers.TryGetValue(q.Trim(), out CustomDashActions value)) {
                    value.onCustomBoosterRender?.Invoke(position);
                }
            }
        }

        /*  
         *  if True: DashEnd will be performed, which equates to reducing the player's speed to 75% of the dash speed.
         *  For a given number N, if the number N is broken into binary notation
         *  -N = invert the given value.
         *  0b0000 : 0 = always false
         *  0b0001 : 1 = true if going up at all
         *  0b0010 : 2 = true if going down at all (-2 is true if not going down at all, which is the default for regular dashes.)
         *  0b0100 : 4 = true if going left at all
         *  0b1000 : 8 = true if going right at all
         */
        public static bool DashEndControl(int slowdownType, Vector2 playerDashDir) {
            if (slowdownType == 0 || slowdownType == -15)
                return false;
            else if (slowdownType == 15)
                return true;
            int q = Math.Abs(slowdownType);
            bool b = false;
            if ((q & 1) > 0)
                b |= playerDashDir.Y < 0;
            if ((q & 2) > 0)
                b |= playerDashDir.Y > 0;
            if ((q & 4) > 0)
                b |= playerDashDir.X < 0;
            if ((q & 8) > 0)
                b |= playerDashDir.X > 0;
            return slowdownType < 0 ? !b : b;

        }

        /// <summary>
        /// Handles Refilling dashes and the refill dash action set.
        /// </summary>
        /// <param name="player">the player, dummy :p</param>
        public void HandleRefill(Player player) {
            if (DashRefillModifier) {
                player.Dashes += DashRefill;
            } else {
                if (DashRefill < 0)
                    player.RefillDash();
                else
                    player.Dashes = DashRefill;
            }

            if (StamRefillModifier) {
                player.Stamina += StamRefill;
            } else {
                if (DashRefill < 0)
                    player.RefillStamina();
                else
                    player.Stamina = StamRefill;
            }
            HandleParameters(CustomDashActionTypes.OnCustomDashRefill, player);
        }

        public Vector2 CustomDashDir(Vector2 pDashDir, Vector2 pSpeed) {
            switch (ForceDashDirection) {
                case DashAngleState.ForcePreviousSpeedAngle:
                    return Vector2.Normalize(pSpeed);
                case DashAngleState.ForceDashDirection:
                    return Vector2.UnitX.Rotate(AngleOffset);
                default:
                    pDashDir.Rotate(AngleOffset);
                    return pDashDir;
            }
        }

        public static void LoadPresets() {
            presetCustomBoosterStates.Add("Default", new EntityData {
                Values = new Dictionary<string, object>
                {
                    {"SuperDashSteerSpeed", ""},
                    {"UseSpritesFromXML", false },
                    {"SpriteInfo", "VivHelper/boosters/hiCustomBooster" },
                    {"ColorSet", "000000,8cf7cf,4acfc6,1c7856,0e4a36,172b21,0e1c15,ffffff,291c33" }
                }
            });
            presetCustomBoosterStates.Add("Superdash", new EntityData {
                Values = new Dictionary<string, object>
                {
                    {"SuperDashSteerSpeed", "4"},
                    {"DashDuration", 0.3f },{"DashSpeed", 240f},
                    {"DashRefillAmount", -1f },{"DashRefillType", false },
                    {"UseSpritesFromXML", false },
                    {"SpriteInfo", "VivHelper/boosters/hiCustomBooster" },
                    {"ColorSet", "000000,5cd4ff,5ca6e5,005c7b,003146,192d33,191922,ffffff,291c33" },
                }
            });
            presetCustomBoosterStates.Add("FragileDash", new EntityData {
                Values = new Dictionary<string, object>
                {
                    {"DashIntoSolidEffect", "Kill" },{"ExtraParameters", "" },
                    {"UseSpritesFromXML", false },
                    {"SpriteInfo", "VivHelper/boosters/hiCustomBooster" },
                    {"ColorSet", "000000,ffffa7,b2ef5f,84912e,555f1f,3e3e28,2e2e1f,ffffff,291c33" },
                }
            });
            presetCustomBoosterStates.Add("HeldDash", new EntityData {
                Values = new Dictionary<string, object>
                {
                    {"HeldDash", true },
                    {"DashDuration", -1f }, {"FastBubbleTimer", -0.25f },
                    {"UseSpritesFromXML", false },
                    {"SpriteInfo", "VivHelper/boosters/hiCustomBooster" },
                    {"ColorSet", "000000,f1ceab,e5a565,9c5b1a,5f3f10,3d230a,271707,ffffff,291c33" },
                }
            });
            presetCustomBoosterStates.Add("DoubleSpeed", new EntityData {
                Values = new Dictionary<string, object>
                {
                    {"DashSpeed", 480f},
                    {"UseSpritesFromXML", false },
                    {"SpriteInfo", "VivHelper/boosters/hiCustomBooster" },
                    {"ColorSet", "000000,ceffff,62ffff,16ad75,006241,0e2f1e,00160b,ffffff,291c33" },
                }
            });
            presetCustomBoosterStates.Add("HeldSuperdash", new EntityData {
                Values = new Dictionary<string, object>
                {
                    {"HeldDash", true },
                    {"SuperDashSteerSpeed", "4"},
                    {"DashDuration", -1f },{"DashSpeed", 240 },
                    {"UseSpritesFromXML", false },
                    {"SpriteInfo", "VivHelper/boosters/hiCustomBooster" },
                    {"ColorSet", "000000,5cd4ff,e5a565,005c7b,5f3f10,192d33,271707,ffffff,291c33" },
                }
            });
        }
    }

    //
    //{"ColorSet", "000000,f78ced,cf4a9a,781c6f,4a0e42,2a172b,1c0e1c,ffffff,291c33" },
    //{"ColorSet", "000000,a43e00,e54e00,480000,350000,9e000e,dc6d00,ffffff,291c33" },
    //{"ColorSet", "000000,84ff9e,f6ff1a,3e8014,0e4a36,172b21,0e1c15,ffffff,291c33" },
    //{"ColorSet", "000000
}
