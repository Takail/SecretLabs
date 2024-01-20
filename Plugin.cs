using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using SecretLabs.Patches;

namespace SecretLabs
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony _harmony;
        public static new Config Config { get; internal set; }
        public static new ManualLogSource Logger;
        
        private void Awake() {
            Logger = base.Logger;
            _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            Config = new Config(base.Config);
            _harmony.PatchAll(typeof(RoutePatches));
            _harmony.PatchAll(typeof(Config));
            Logger.LogDebug($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}