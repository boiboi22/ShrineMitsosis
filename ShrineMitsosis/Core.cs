using Il2Cpp;
using Il2CppAssets.Scripts.Managers;
using Il2CppAssets.Scripts.Steam;
using Il2CppInterop.Runtime;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(ShrineMitsosis.Core), "ShrineMitsosis", "1.0.0", "Potatoe_Man", null)]
[assembly: MelonGame("Vedinad", "Megabonk Demo")]

namespace ShrineMitsosis
{
    public class Core : MelonMod
    {
        public static string ConfigPath = Path.Combine(MelonEnvironment.UserDataDirectory, "ShrineMitosis", "ShrineMitosisConfig.cfg");

        public static int ShrineMultiplier = 2;
        public static string LogPrefix = "[ShrineMitosis]";

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    Save(); // Create default config if missing
                    return;
                }

                foreach (var line in File.ReadAllLines(ConfigPath))
                {
                    var trimmed = line.Trim();
                    if (trimmed.StartsWith("#") || string.IsNullOrWhiteSpace(trimmed))
                        continue;

                    var split = trimmed.Split('=', 2);
                    if (split.Length != 2)
                        continue;

                    string key = split[0].Trim();
                    string value = split[1].Trim();

                    switch (key)
                    {
                        case "ShrineMultiplier":
                            int.TryParse(value, out ShrineMultiplier);
                            break;
                        case "LogPrefix":
                            LogPrefix = value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error loading config: {ex}");
            }
        }
        public override void OnInitializeMelon()
        {
            Load();
            HarmonyInstance.PatchAll();
            MelonLogger.Warning("THIS IS A CHEAT MOD!!!");
            MelonLogger.Warning("Leaderboards are disabled.");
        }

        private static bool inRound = false;
        private static bool DupedShrines = false;

        private static string roundStartObjectName = "Chest(Clone)";
        private static string roundEndObjectName = "B_ChestRigged";

        public static class Globals
        {
            public static GameObject FoundChest;
        }

        public override void OnUpdate()
        {
            // 1. Detect round start
            if (GameObject.Find(roundStartObjectName) != null)
            {
                inRound = true;
                MelonLogger.Msg(DupedShrines);
                if (DupedShrines == false)
                {
                    MelonLogger.Msg(DupedShrines);
                    //if (Globals.FoundChest != null)
                    //{
                    //MelonLogger.Msg(Globals.FoundChest);
                    var opens = SceneManager.GetActiveScene().GetRootGameObjects();
                    foreach (var open in opens)
                    {
                        //MelonLogger.Msg(open);
                        if (open == null) continue;
                        var chest = open.GetComponent<SpawnInteractables>();
                        if (chest != null)
                        {
                            for (int i = 0; i < ShrineMultiplier; i++)
                            {
                                chest.SpawnShrines();
                            }
                            MelonLogger.Msg(ShrineMultiplier);
                            MelonLogger.Msg(chest.shrineDensity);
                            DupedShrines = true;
                        }
                    }
                    //}
                    /*else
                    {
                        var chest = Globals.FoundChest.GetComponent<SpawnInteractables>();
                        for (int i = 0; i < ShrineMultiplier - 1; i++)
                        {
                            MelonLogger.Msg(DupedShrines);
                            MelonLogger.Msg(Globals.FoundChest);
                            chest.SpawnShrines();
                        }
                        DupedShrines = true;
                    }*/
                }
            }

            if (GameObject.Find(roundEndObjectName) != null)
            {
                inRound = false;
                DupedShrines = false;
            }
        }

        // Harmony patch to disable UploadScore, since your a CHEATER   
        [HarmonyLib.HarmonyPatch(typeof(Leaderboards), "UploadScore")]
        internal class UploadScore_DisablePatch
        {
            public static bool Prefix(int score)
            {
                // Skip original method
                MelonLogger.Msg($"Blocked UploadScore({score}) call.");
                return false;
            }
        }
        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

                using (StreamWriter writer = new StreamWriter(ConfigPath))
                {
                    writer.WriteLine("# ShrineMitosis Config");
                    writer.WriteLine(" ");
                    writer.WriteLine("ShrineMultiplier=" + ShrineMultiplier);
                    writer.WriteLine("The Multiplication amount.");
                    writer.WriteLine(" ");
                    writer.WriteLine("LogPrefix=" + LogPrefix);
                }
            }
            catch (Exception ex)
            {
                MelonLoader.MelonLogger.Error($"Error saving config: {ex}");
            }
        }
    }
}