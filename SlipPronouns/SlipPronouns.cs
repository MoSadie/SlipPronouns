using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using System;
using MoCore;
using Subpixel.Events;
using SlipPronouns.pronouns;
using HarmonyLib;

namespace SlipPronouns
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("com.mosadie.mocore", BepInDependency.DependencyFlags.HardDependency)]
    [BepInProcess("Slipstream_Win.exe")]
    public class SlipPronouns : BaseUnityPlugin, MoPlugin
    {
        private static ConfigEntry<string> url;

        private static ConfigEntry<bool> debugLogs;

        internal static ManualLogSource Log;

        public static PronounFetcher pronounFetcher = new PronounFetcher();

        public static readonly string COMPATIBLE_GAME_VERSION = "4.1579";
        public static readonly string GAME_VERSION_URL = "https://raw.githubusercontent.com/MoSadie/SlipPronouns/refs/heads/main/versions.json";

        private void Awake()
        {
            try
            {
                SlipPronouns.Log = base.Logger;

                if (!MoCore.MoCore.RegisterPlugin(this))
                {
                    Log.LogError("Failed to register plugin with MoCore. Please check the logs for more information.");
                    return;
                }

                url = Config.Bind("API Settings", "API URL", "https://api.pronouns.alejo.io/v1", "Url for Alejo's Twitch Pronouns API");

                debugLogs = Config.Bind("Debug", "DebugLogs", false, "Enable additional logging for debugging");

                pronounFetcher.init(url.Value);

                Harmony.CreateAndPatchAll(typeof(SlipPronouns));

                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            } catch (Exception e)
            {
                Log.LogError("An error occurred while starting the plugin.");
                Log.LogError(e.Message);
            }

        }

        internal static void debugLogInfo(string message)
        {
            if (debugLogs.Value)
            {
                Log.LogInfo(message);
            }
        }

        internal static void debugLogWarn(string message)
        {
            if (debugLogs.Value)
            {
                Log.LogWarning(message);
            }
        }

        internal static void debugLogError(string message)
        {
            if (debugLogs.Value)
            {
                Log.LogError(message);
            }
        }

        internal static void debugLogDebug(string message)
        {
            if (debugLogs.Value)
            {
                Log.LogDebug(message);
            }
        }

        internal void crewTest(CrewmateCreatedEvent e)
        {
            try
            {
                if (e.Crewmate != null && e.Crewmate.Client != null && e.Crewmate.Client.Player != null)
                {
                    string twitchName = e.Crewmate.Client.Player.TwitchUserName != null ? e.Crewmate.Client.Player.TwitchUserName : "null";
                    string twitchId = e.Crewmate.Client.Player.TwitchUserId != null ? e.Crewmate.Client.Player.TwitchUserId : "null";
                    Log.LogInfo($"Crewmate created: {e.Crewmate.Client.Player.DisplayName} Twitch Info: name:{twitchName} id:{twitchId}");
                }
            } catch (Exception ex)
            {
                Log.LogError("An error occurred while doing the crewmate created event test.");
                Log.LogError(ex.Message);
            }
        }

        public string GetCompatibleGameVersion()
        {
            return COMPATIBLE_GAME_VERSION;
        }

        public string GetVersionCheckUrl()
        {
            return GAME_VERSION_URL;
        }

        public BaseUnityPlugin GetPluginObject()
        {
            return this;
        }

        [HarmonyPatch(typeof(Player), "DisplayName", MethodType.Getter)]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        static void addToDisplayName(ref Player __instance, ref string __result)
        {
            if (__instance.TwitchUserName != null)
            {
                __result += SlipPronouns.pronounFetcher.getSuffix(__result); // Someday change back to __instance.TwitchUserName
            }
        }
    }
}
