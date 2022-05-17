using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod.Entities;
using System.Reflection;
using System.Collections;
using Microsoft.Xna.Framework;

namespace VivHelper.Entities
{ 

    //The main reason I'm using a class here instead of a Session variable is because I want this to cleanly ignore maps when it is not used in the map, for potential lag concerns.
    /// <summary>
    /// The GlobalRespriter is a sort of blank entity which contains a Dictionary, and all it does is run a hook to add an EntityRespritingComponent to any
    /// entity added in the scene which matches the type and description for the given sprite to be resprited.
    /// </summary>
    [Tracked]
    public class GlobalRespriter : Entity
    {
        public enum RespriteType
        {
            Invalid = 0,
            Sprite = 1,
            Image = 2,
            MTexture = 3,
            NineSlice = 4,
            ImageList_Numbered = 5,
            PNGtoImageList = 6
        }

        
        public struct Resprite
        {
            public Type type;
            public string key;

            public bool Int_FieldInfo; //true for int, false for FieldInfo (default)
            public FieldInfo respriteFieldInfo;
            public int numInCompList;
            /// <summary>
            /// Dependent on the resprite type, this will be one of a number of objects, see the RespriteType name for this.
            /// </summary>
            public object RespriteSet;

            //For Debugging and Detailed Exceptions.
            public string LevelName;
            public Vector2 LevelPos;

           
        }
        /// <summary>
        /// <TKey> string mapped as ClassName:VarName </TKey>
        /// <Value> struct - enum defines the type of the RespriteSet object</Value>
        /// </summary>
        public Dictionary<string, Resprite> RespriteDictionary;

        private static Type[] typeSet = new Type[]
        {
            null, //Just doing this to be clean
            typeof(Sprite),
            typeof(Image),
            typeof(MTexture),
            typeof(MTexture[,]),
            typeof(List<Image>),
            typeof(List<Image>)
        };

        public GlobalRespriter(List<EntityData> mapboundResprites) : base(Vector2.Zero)
        {
            RespriteDictionary = new Dictionary<string, Resprite>();
            foreach(EntityData e in mapboundResprites)
            {
                CheckRespriteViability(e, out Resprite r);
                RespriteDictionary[r.key] = r;
            }
        }

        public void CheckRespriteViability(EntityData e, out Resprite resprite)
        {
            string classname = e.Attr("ClassName");
            if (string.IsNullOrWhiteSpace(classname))
            {
                throw new InvalidPropertyException("the `ClassName` parameter in one of your MapResprites is empty or whitespaces. This one can be found in room " + e.Level.Name + "at position: " + e.Position + ".");
            }
            Type Class = VivHelper.GetType(classname, false);
            if (Class == null)
            {
                throw new InvalidPropertyException("The `ClassName` parameter in one of the MapResprites is not a valid class that the game can find.\n" +
                                                   "This one can be found in room " + e.Level.Name + " at position: " + e.Position + ", and the current parameter is " + classname + ".\n" +
                                                   "If you're seeing this as a player of a map, please report this error log to the creator. This might be caused by not having a given helper.");
            }
            string varname = e.Attr("VariableName");
            if (string.IsNullOrWhiteSpace(varname))
            {
                throw new InvalidPropertyException("the `VariableName` parameter in one of your MapResprites is empty or whitespaces. This one can be found in room " + e.Level.Name + "at position: " + e.Position + ".");
            }
            RespriteType a = e.Enum<RespriteType>("VariableType", RespriteType.Invalid);
            Type varType;
            if (a != RespriteType.Invalid)
            {
                varType = typeSet[(int)a];
            }
            else
            {
                throw new InvalidPropertyException("The `VariableType` parameter in one of the MapResprites is not a viable parameter type. if you need another structure for other objects that create an image, please let Viv know @Viv#1113." +
                    "This one can be found in room " + e.Level.Name + " at position: " + e.Position + ", and the current parameter is " + a.ToString() + ".");
            }
            string replacement = e.Attr("Replacement");
            if (string.IsNullOrWhiteSpace(replacement))
            {
                throw new InvalidPropertyException("The `Replacement` parameter in one of your MapResprites is empty or whitespace.\nThis one can be found in room " + e.Level.Name + " at position: " + e.Position + ".");
            }
            if (varname[0] == '#') //This is the use case for Components
            {
                string setname = varname.Substring(1);
                if (!int.TryParse(setname, out int variable))
                {
                    throw new InvalidPropertyException("The `VariableName` parameter in one of the MapResprites is not defined as a valid variable, or identified as the nth instance of the added components of the object as #<Number from list of components of the given type from `VariableType`>.\n" +
                        "This one can be found in room " + e.Level.Name + " at position: " + e.Position + ", and the current parameter is " + varname + ".");
                }
                //We cannot check/determine if this is valid *yet* so we'll now make the things work.
                resprite = new Resprite
                {
                    numInCompList = variable,
                    Int_FieldInfo = true
                };

            }
            else
            {
                FieldInfo varF0 = Class.GetField(varname, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (varF0 == null)
                {
                    throw new InvalidPropertyException("The `VariableName` parameter in one of the MapResprites is not a valid class field (in other words, the name you put in is not the name of a variable in the class.)\n" +
                        "This one can be found in room " + e.Level.Name + " at position: " + e.Position + ", and the current parameter is " + varname + ".");
                }
                if (varF0.FieldType != varType)
                {
                    throw new InvalidPropertyException("The `VariableName` parameter in one of your MapResprites does not match the given parameter aligning with your `VariableType` parameter. Your VariableName found a Field matching type `" + varF0.FieldType.ToString() + "` while your `VariableType` parameter aligns with type `" + varType.ToString() + "`.\n" +
                        "This one can be found in room " + e.Level.Name + " at position: " + e.Position + ".");
                }
                //At this point I think every check has been cleared, other than Match Sprites check which I'll just say, the MapResprite matching Q doesn't match with the sprite correctly
                resprite = new Resprite
                {
                    respriteFieldInfo = varF0,
                    Int_FieldInfo = false
                };
            }
            resprite.key = classname + ":" + varname;
            resprite.LevelName = e.Level.Name;
            resprite.LevelPos = e.Level.Position;
            switch (a)
            {
                case RespriteType.Sprite:
                    resprite.RespriteSet = VivHelperModule.RespriteBank.Create(replacement);
                    break;
                case RespriteType.Image:
                    resprite.RespriteSet = new Image(GFX.Game[replacement]);
                    break;
                case RespriteType.MTexture:
                    resprite.RespriteSet = GFX.Game[replacement];
                    break;
                case RespriteType.NineSlice:
                    MTexture[,] m = new MTexture[3, 3];
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            m[i, j] = GFX.Game[replacement].GetSubtexture(i * 8, j * 8, 8, 8, null);
                        }
                    }
                    break;
                case RespriteType.ImageList_Numbered:
                    List<Image> l = new List<Image>();
                    foreach (MTexture m1 in GFX.Game.GetAtlasSubtextures(replacement))
                    {
                        l.Add(new Image(m1));
                    }
                    break;
                case RespriteType.PNGtoImageList:
                    MTexture source = GFX.Game[replacement];
                    List<Image> list = new List<Image>();
                    int num = source.Width / 8;
                    int num2 = source.Height / 8;
                    for (int i = 0; (float)i < base.Width; i += 8)
                    {
                        for (int j = 0; (float)j < base.Height; j += 8)
                        {
                            int num3 = ((i != 0) ? ((!((float)i >= base.Width - 8f)) ? Calc.Random.Next(1, num - 1) : (num - 1)) : 0);
                            int num4 = ((j != 0) ? ((!((float)j >= base.Height - 8f)) ? Calc.Random.Next(1, num2 - 1) : (num2 - 1)) : 0);
                            Image image = new Image(source.GetSubtexture(num3 * 8, num4 * 8, 8, 8));
                            image.Position = new Vector2(i, j);
                            list.Add(image);
                        }
                    }
                    break;
                default:
                    throw new InvalidPropertyException("This should never appear.");
            }
        }

        public bool RespriteEntity(Entity entity, Resprite resprite)
        {
            return true;
        }
    }
    
}
