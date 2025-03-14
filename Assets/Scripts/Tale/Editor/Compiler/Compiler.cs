#if UNITY_EDITOR
/*
 *                .:-=++.                             
 *           .*@@@##%@=.                              
 *       .=@@###:#@#.                 ...             
 *     .*@@@@=-%@@@@@@@@@@.           .@@.            
 *   .-@@@@%-%#=----@@=---.           .@@.            
 *   ...#@#-@@@@*. .@@.    -@@@@@=%@: .@@.  .=@@@@@-. 
 *  -@@@@@+@@@@-   .@@.  .%@%:..:%@@: .@@. .@@-...=@%.
 * .%@@@@#@#%@*.   .@@.  =@#.    :%@: .@@. @@######@@-
 * ..  .#*   .     .@@.  =@#.    :%@: .@@. @@-........
 *     :%          .@@.  .%@%.  :%@@: .@@. .@@+.  .*: 
 *    :@*          .@@.    =%@@@#=%@: .@@.  .+%@@@@*-.
 *   .#@=                                             
 *                   CarbonCopy Tale Script Compiler
 * ---------------------------------------------------
 * 
 * Compiles Markdown story scripts into Tale-compatible C# scripts.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Compilation;

namespace TaleUtil
{
    public partial class Editor
    {
        public static class StoryCompiler
        {
            enum State
            {
                SEARCHING_FOR_ENTRY_POINT,
                PARSING_SCRIPT
            };

            public static HashSet<string> Compile(string sourceFile, string workingDir)
            {
                var state = State.SEARCHING_FOR_ENTRY_POINT;

                string scene = null;
                StreamWriter sceneFile = null;

                var scenes = new HashSet<string>();
                var characters = new HashSet<string>();

                bool done = false;

                var regexSceneSpecialCharacters = new Regex(@"[\s\-,\.<>?;:\'""\[\]{}/\\|!@#$%^&*]", RegexOptions.Compiled);
                var regexSceneExtraUnderscores = new Regex(@"_+", RegexOptions.Compiled);

                var regexDialog = new Regex(@"^\s*([a-zA-Z0-9]+):(.*)$", RegexOptions.Compiled);

                // Used to map scene names from 'Scene_X' or 'Scena_X' to 'Story_X'
                var regexSceneNameMapping = new Regex(@"^(scen[ae])_.*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var sceneNameMapping = "Story";

                Log.Info("Compiler", "Compiling " + sourceFile);

                using (StreamReader sr = new StreamReader(sourceFile))
                {
                    string line;

                    while (!done && (line = sr.ReadLine()) != null)
                    {
                        switch (state)
                        {
                            case State.SEARCHING_FOR_ENTRY_POINT:
                            {
                                if (LineIsH1(line) && LineToH1(line).ToLowerInvariant() == "script")
                                {
                                    Log.Info("Compiler", "Found script entry point\n");
                                    state = State.PARSING_SCRIPT;
                                }

                                break;
                            }
                            case State.PARSING_SCRIPT:
                            {
                                if (LineIsH2(line))
                                {
                                    // Found scene
                                    var text = LineToH2(line);

                                    Log.Info("Compiler", "[SCENE] " + text);

                                    if (sceneFile != null)
                                    {
                                        // Finish writing old scene
                                        WriteSceneScriptEnd(sceneFile);
                                        sceneFile.Close();
                                        sceneFile = null;
                                    }

                                    // Normalize
                                    scene = regexSceneSpecialCharacters.Replace(text, "_");
                                    scene = regexSceneExtraUnderscores.Replace(scene, "_").Trim('_');

                                    // 'Scene_X' or 'Scena_X' ---> 'Story_X'
                                    scene = regexSceneNameMapping.Replace(scene, new MatchEvaluator(
                                        (match) => match.Value.Replace(match.Groups[1].Value, sceneNameMapping)));

                                    Log.Info("Compiler", "Renamed to ---> " + scene);

                                    if (scenes.Contains(scene))
                                    {
                                        Log.Warning("Compiler", "Scene " + scene + " already exists, this will overwrite the old one");
                                    }
                                    else
                                    {
                                        scenes.Add(scene);
                                    }

                                    Directory.CreateDirectory(System.IO.Path.Combine(workingDir, "Scenes"));

                                    sceneFile = new StreamWriter(System.IO.Path.Combine(workingDir, string.Format("Scenes/{0}.cs", scene)));

                                    WriteSceneScriptStart(sceneFile, scene);
                                }
                                else if (LineIsH1(line))
                                {
                                    // Reached the end of # Story because another H1 starts here
                                    done = true;
                                }
                                else if (scene != null)
                                {
                                    // Scene content
                                    var dialog = ParseDialog(line, regexDialog);

                                    if (dialog != null)
                                    {
                                        characters.Add(dialog.who);

                                        WriteSceneScriptDialog(sceneFile, dialog.who, dialog.what);
                                    }
                                    else if (line.Trim() == "---")
                                    {
                                        WriteSceneScriptWait(sceneFile);
                                    }
                                    else if (line.Trim().Length > 0)
                                    {
                                        WriteSceneScriptComment(sceneFile, line.Trim());
                                    }
                                }

                                break;
                            }
                        }
                    }
                }

                if (state == State.SEARCHING_FOR_ENTRY_POINT)
                {
                    Log.Error("Compiler", "Failed to find entry point; make sure a '# Script' heading exists");
                    return null;
                }

                if (sceneFile != null)
                {
                    WriteSceneScriptEnd(sceneFile);
                    sceneFile.Close();
                    sceneFile = null;
                }

                Console.WriteLine("---");

                Console.WriteLine("Writing Transition.cs");

                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(workingDir, "Transition.cs")))
                {
                    sw.Write(@"using UnityEngine;

public static class Transition {
    public static TaleUtil.Action FadeIn(float duration = 1f) =>
        Tale.Transition(""fade"", Tale.TransitionType.IN, duration);

    public static TaleUtil.Action FadeOut(float duration = 1f) =>
        Tale.Transition(""fade"", Tale.TransitionType.OUT, duration);
}
");
                }

                Console.WriteLine("Writing Dialog.cs");

                using (StreamWriter sw = new StreamWriter(System.IO.Path.Combine(workingDir, "Dialog.cs")))
                {
                    sw.Write(@"using System.IO;
using UnityEngine;
            
public static class Dialog {");

                    foreach (var character in characters)
                    {
                        sw.Write(string.Format(@"
    public static TaleUtil.Action {0}(string what, string voice = null, bool additive = false, bool reverb = false) =>
        Tale.Dialog(""{0}"", what, null, voice != null ? Path.Combine(""{0}"", voice) : null, voice != null && voice.ToLower().EndsWith(""loop""), additive, reverb);
", character));
                    }

                    sw.Write(@"}
");
                }

                Log.Info("Compiler", "Refreshing asset database");

                UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport | UnityEditor.ImportAssetOptions.ForceUpdate);

                Log.Info("Compiler", "Script compiled successfully");

                return scenes;
            }

            static void WriteSceneScriptStart(StreamWriter sw, string scene)
            {
                sw.Write(string.Format(@"using UnityEngine;

public class {0} : MonoBehaviour {{
    void Awake() {{
        Tale.Wait();
        Transition.FadeIn();
", scene));
            }

            static void WriteSceneScriptEnd(StreamWriter sw)
            {
                sw.Write(@"
        Tale.Wait();
        Transition.FadeOut();
        Tale.Scene();
    }
}
");
            }

            static void WriteSceneScriptDialog(StreamWriter sw, string who, string what)
            {
                sw.Write(string.Format(@"
        Dialog.{0}(""{1}""", who, what));

                if (what.StartsWith("(") && what.EndsWith(")"))
                {
                    sw.Write(", reverb: true");
                }

                sw.Write(@");
");
            }

            static void WriteSceneScriptWait(StreamWriter sw)
            {
                sw.Write(@"
        Tale.Wait();
");
            }

            static void WriteSceneScriptComment(StreamWriter sw, string what)
            {
                sw.Write(string.Format(@"
        // {0}
", what));
            }

            static bool LineIsH1(string line)
            {
                return line.StartsWith("# ");
            }

            static string LineToH1(string line)
            {
                return line.Substring(1).Trim();
            }

            static bool LineIsH2(string line)
            {
                return line.StartsWith("## ");
            }

            static string LineToH2(string line)
            {
                return line.Substring(2).Trim();
            }

            class ParseDialogResult
            {
                public string who;
                public string what;

                public ParseDialogResult(string who, string what)
                {
                    this.who = who;
                    this.what = what;
                }
            }

            static ParseDialogResult ParseDialog(string line, Regex compiledRegex)
            {
                var dialog = compiledRegex.Match(line);

                if (!dialog.Success)
                {
                    return null;
                }

                var who = dialog.Groups[1].Value.Trim();
                var what = dialog.Groups[2].Value.Trim(' ', '"').Replace("\"", "\\\"");

                if (what.Length == 0)
                {
                    return null;
                }

                return new ParseDialogResult(who, what);
            }
        }
    }
}
#endif