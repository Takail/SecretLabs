using System;

using BepInEx.Configuration;

using GameNetcodeStuff;

using HarmonyLib;

using Unity.Collections;
using Unity.Netcode;

namespace SecretLabs {
    [Serializable]
    public class Config : SyncedInstance<Config>
    { 
        public bool customPriceEnabled;
        public int moonPrice;
        public Config(ConfigFile cfg)
        {
            InitInstance(this);
            customPriceEnabled = cfg.Bind("General", "CustomPriceEnabled", false, "Enable this if you wish to configure the moons route price below.").Value;
            moonPrice = cfg.Bind("General", "MoonPrice", 700, "The route price of the Secret Labs moon.").Value;
        }
        
        public static void RequestSync() {
            if (!IsClient) {
                return;
            }

            using FastBufferWriter stream = new(IntSize, Allocator.Temp);
            MessageManager.SendNamedMessage($"{PluginInfo.PLUGIN_NAME}_OnRequestConfigSync", 0uL, stream);
        }
        
        public static void OnRequestSync(ulong clientId, FastBufferReader _) {
            if (!IsHost) {
                return;
            }

            var array = SerializeToBytes(Instance);
            var value = array.Length;

            using FastBufferWriter stream = new(value + IntSize, Allocator.Temp);

            try {
                stream.WriteValueSafe(in value);
                stream.WriteBytesSafe(array);

                MessageManager.SendNamedMessage($"{PluginInfo.PLUGIN_NAME}_OnReceiveConfigSync", clientId, stream);
            } catch(Exception e) {
                Plugin.Logger.LogDebug($"Error occurred syncing config with client: {clientId}\n{e}");
            }
        }
        
        public static void OnReceiveSync(ulong _, FastBufferReader reader) {
            if (!reader.TryBeginRead(IntSize)) {
                Plugin.Logger.LogError("Config sync error: Could not begin reading buffer.");
                return;
            }

            reader.ReadValueSafe(out int val);
            if (!reader.TryBeginRead(val)) {
                Plugin.Logger.LogError("Config sync error: Host could not sync.");
                return;
            }

            var data = new byte[val];
            reader.ReadBytesSafe(ref data, val);

            SyncInstance(data);

            Plugin.Logger.LogDebug("Successfully synced config with host.");
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer() {
            if (IsHost) {
                MessageManager.RegisterNamedMessageHandler($"{PluginInfo.PLUGIN_NAME}_OnRequestConfigSync", OnRequestSync);
                Synced = true;

                return;
            }

            Synced = false;
            MessageManager.RegisterNamedMessageHandler($"{PluginInfo.PLUGIN_NAME}_OnReceiveConfigSync", OnReceiveSync);
            RequestSync();
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        public static void PlayerLeave() {
            RevertSync();
        }
    }
}