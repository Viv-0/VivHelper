using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Celeste;
using Monocle;
using Celeste.Mod;
using VivHelper;
using VivHelper.Entities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoMod.Utils;
using System.Reflection;
using VivHelper.Module__Extensions__Etc;

namespace VivHelper {
    public class VivHelperCommands {

        [Command("get_curve_length", "Gets the length of a Curve Entity in the loaded area. Specified by the identifier of the Curve Entity. (Viv's Helper)")]
        private static void GetCurveTotalLength(string Identifier) {

            CurveEntity.GetCurveTotalLength(Identifier);

        }

        [Command("get_curve_ids", "Gets all IDs of curves in the loaded area. (Viv's Helper)")]
        private static void GetCurveIDs() {
            CurveEntity.GetCurveIDs();
        }

        [Command("get_seeker_ids", "Gets all IDs from seekers in the loaded area (and gives positions at time of Command).\n" +
                                   "Used in tandem with write_seeker_data to produce a YAML file used for Seeker Generators.")]
        private static void GetSeekerIDs() {
            if (CustomSeeker.CustomSeekersList.Count == 0) { Engine.Commands.Log("There are no Custom Seekers currently loaded."); return; }
            for (int i = 0; i < CustomSeeker.CustomSeekersList.Count; i++) {
                string t = "ID- " + i + "\nPosition- (" + CustomSeeker.CustomSeekersList[i].Position.X + ", " + CustomSeeker.CustomSeekersList[i].Position.Y + ")";
                Engine.Commands.Log(t);
            }
        }

        private static string RandText() {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < random.Next(1, 50); i++) { random.Next(); }

            for (int i = 0; i < stringChars.Length; i++) {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new String(stringChars);
        }

        [Command("write_seeker_data", "Produces a text file in Celeste/Mods/VivHelper_YAMLData in the proper YAML format for one seeker. Copy and paste\nthe text" +
                                      " into your Seeker Generator YAML file or use write_generator_data to create a full generator with that one seeker.\n" +
                                        "Leaving the ID blank will produce a Default Seeker (without annotations)")]
        private static void GenerateSeekerYAML(int ID = -1) {
            List<CustomSeeker> customSeekers = CustomSeeker.CustomSeekersList;
            if (customSeekers.Count == 0 && ID > -1) { Engine.Commands.Log("There are no Custom Seekers currently loaded."); return; }
            if (ID < -1 || ID >= customSeekers.Count) { Engine.Commands.Log("There are no Custom Seekers with that ID currently loaded."); return; }

            //Here we go. The real coding stuff.
            string path = VivHelperModule.SeekerFolderPath;
            string randText = RandText();
            string filePath = System.IO.Path.Combine(path, "seeker" + (ID == -1 ? "Default" : ID.ToString()) + "_" + randText + ".yaml");
            bool b = System.IO.File.Exists(filePath);
            System.IO.FileStream fs = System.IO.File.Create(filePath);
            fs.Dispose();
            string[] Text = customSeekers[ID].GetYAMLText(ID == -1).ToArray();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filePath)) {
                file.WriteLine("# Add this to the end of your Seekers list in the Seeker Generator file. Make sure the line after the end of the previous seeker has the \"-\" in it as well.");
                file.WriteLine("- Order: " + ID + "\n");
                for (int i = 0; i < Text.Length; i++) {
                    file.WriteLine("  " + Text[i]);
                }
            }
            Engine.Commands.Log("Seeker file " + (b ? "overwrote file at " : "written to ") + filePath + ".\n" +
                                "Copy and paste the text into your generator file.");
        }

        [Command("write_generator_data", "Produces a text file in Celeste/Mods/VivHelper_YAMLData in the proper YAML format for one generator.\nTake the file and place" +
                                      " it into your SeekerGenerators folder in your mod - if you don't have that folder, just add a folder to your mod titled\"SeekerGenerators\"\n" +
                                        "Parameters: seekerID = the seeker ID that the generator puts in automatically, leaving blank adds a default seeker")]
        private static void GenerateGeneratorYAML(int seekerID = -1) {
            List<CustomSeeker> customSeekers = CustomSeeker.CustomSeekersList;
            if (customSeekers.Count == 0) { Engine.Commands.Log("There are no Custom Seekers currently loaded."); return; }
            if (seekerID < -1 || seekerID >= customSeekers.Count) { Engine.Commands.Log("There are no Custom Seekers with that ID currently loaded, and you have not let it default."); return; }
            string randText = RandText();
            string path = VivHelperModule.SeekerFolderPath;
            string filePath = System.IO.Path.Combine(path, "generator" + (seekerID == -1 ? 0 : seekerID) + RandText() + ".yaml");
            bool b = System.IO.File.Exists(filePath);
            System.IO.FileStream fs = System.IO.File.Create(filePath);
            fs.Dispose();
            string[] Text = customSeekers[seekerID].GetYAMLText(seekerID == -1).ToArray();
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@filePath)) {
                file.WriteLine("- Seekers:");

                //Seeker write-in
                if (seekerID != -1) {
                    file.WriteLine("  - Order: " + seekerID + "\n");
                    for (int i = 1; i < Text.Length; i++) {
                        file.WriteLine("    " + Text[i]);
                    }
                }
            }
            Engine.Commands.Log("Generator file " + (b ? "overwrote file at " : "written to ") + filePath + ".\n" +
                               "Move the YAML file to your Seekers folder.");
        }

        [Command("set_colorgrade", "Sets the colorgrade.")]
        private static void SetColorGrade(string name) {
            Level level = Engine.Scene as Level;
            if (level == null) { Engine.Commands.Log("Level set to null, no color grade changed."); } else {
                level.Session.ColorGrade = name;
                Engine.Commands.Log("Color grade changed.");
            }
        }

        [Command("get_entity_types", "Retrieves all entities that the mouse is currently touching and prints the entity type for each.\n[identifier] - Specifies to the subset of all entities that contain the identifier in their type name. It's recommended to keep the identifiers to helper names.\n[ignoreCollidable] - true or false, if true, ignores whether or not the entity currently collidable.")]
        private static void GetType(string identifier = null, bool ignoreCollidable = false) {
            Level level = Engine.Scene as Level;
            if (level == null) {
                Engine.Commands.Log("Current Scene is currently not a level.");
                return;
            }
            MouseState state = Mouse.GetState();
            Vector2 vector = new Vector2(state.X, state.Y);
            Camera camera = level.Camera;
            int num2 = (int) Math.Round((float) Engine.Instance.GraphicsDevice.PresentationParameters.BackBufferWidth / (float) camera.Viewport.Width);
            Vector2 value = vector;
            value = (value / num2).Floor();
            value = camera.ScreenToCamera(value);
            value /= 8;
            value = value.Floor();
            value *= 8;
            Engine.Commands.Log(string.Format("Entities currently {0} the mouse:", ignoreCollidable ? "at the position of" : "colliding with"));
            Console.WriteLine(string.Format("Entities currently {0} the mouse:", ignoreCollidable ? "at the position of" : "colliding with"));
            foreach (Entity e in level.Entities) {
                if (e.Collider == null)
                    continue;
                if (ignoreCollidable && !e.Collidable) {
                    e.Collidable = true;
                    if (!e.CollidePoint(value)) {
                        e.Collidable = false;
                        continue;
                    }
                    e.Collidable = false;
                } else {
                    if (!e.CollidePoint(value)) {
                        continue;
                    }
                }
                string s = e.GetType().ToString();
                if (string.IsNullOrWhiteSpace(identifier)) {
                    Engine.Commands.Log(s);
                    Console.WriteLine(s);
                } else {
                    if (s.Contains(identifier)) {
                        Engine.Commands.Log(s);
                        Console.WriteLine(s);
                    }
                }
            }
        }
    }
}

