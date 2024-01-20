using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using Unity.Netcode;

namespace SecretLabs {
    [Serializable]
    public class SyncedInstance<T> {
        internal static CustomMessagingManager MessageManager => NetworkManager.Singleton.CustomMessagingManager;
        internal static bool IsClient => NetworkManager.Singleton.IsClient;
        internal static bool IsHost => NetworkManager.Singleton.IsHost;

        [NonSerialized] protected static int IntSize = sizeof(int);

        public static T Default { get; set; }
        public static T Instance { get; set; }

        public static bool Synced { get; internal set; }

        protected void InitInstance(T instance) {
            Default = instance;
            Instance = instance;
        }

        internal static void SyncInstance(byte[] data) {
            Instance = DeserializeFromBytes(data);
            Synced = true;
        }

        internal static void RevertSync() {
            Instance = Default;
            Synced = false;
        }

        public static byte[] SerializeToBytes(T val) {
            BinaryFormatter bf = new();
            using MemoryStream stream = new();

            try {
                bf.Serialize(stream, val);
                return stream.ToArray();
            }
            catch (Exception e) {
                Plugin.Logger.LogError($"Error serializing instance: {e}");
                return null;
            }
        }

        public static T DeserializeFromBytes(byte[] data) {
            BinaryFormatter bf = new();
            using MemoryStream stream = new(data);

            try {
                return (T)bf.Deserialize(stream);
            }
            catch (Exception e) {
                Plugin.Logger.LogError($"Error deserializing instance: {e}");
                return default;
            }
        }
    }
}