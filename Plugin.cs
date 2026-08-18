using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RcMissileCamera
{
    // A real, separate BepInEx plugin — not part of NOXMFD.dll. Registers itself with NOXMFD's
    // public Api at Awake() (docs/extensions-api.md) instead of anything in src/plugin/ knowing
    // this mod exists.
    [BepInPlugin("com.roque.rc-missile-camera", "RC Missile Camera", MyPluginInfo.PLUGIN_VERSION)]
    // No MinimumVersion pin yet — this is the extension API's own first real consumer, still on
    // the same branch as the API itself. A real release should pin one (Api.ApiVersion is the
    // thing to check it against), the same discipline docs/extensions-api.md's Versioning
    // section describes.
    [BepInDependency("com.roque.NOXMFD")]
    [BepInProcess("NuclearOption.exe")]
    [BepInProcess("NuclearOptionServer.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal const string ExtId = "rc-missile-camera";
        internal static ManualLogSource? Log;

        // Same reasoning as NOXMFD's own Plugin.cs: this GameObject doesn't survive the
        // boot -> MainMenu scene transition in this Unity version, so the actual per-frame work
        // (RcLifecycle) runs on a self-spawned, DontDestroyOnLoad GameObject instead.
        private static RcLifecycle? _lifecycle;
        private static bool _sceneSubscribed;

        private void Awake()
        {
            Log = Logger;

            bool ok = NOXMFD.Api.RegisterExtension(ExtId, "RC CAM", RcAssets.Resolve, RcCommands.Handle);
            if (!ok)
            {
                Log.LogError("[RCCAM] failed to register with NOXMFD (id already taken?) — extension disabled.");
                return;
            }

            if (!_sceneSubscribed)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                _sceneSubscribed = true;
            }
            Log.LogInfo("RC Missile Camera loaded.");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_lifecycle != null) return;
            var go = new GameObject("RcMissileCamera_Lifecycle");
            Object.DontDestroyOnLoad(go);
            _lifecycle = go.AddComponent<RcLifecycle>();
            Log?.LogInfo("[RCCAM] RcLifecycle attached (scene='" + scene.name + "').");
        }
    }
}
