using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RcMissileCamera
{
    // A real, separate BepInEx plugin — not part of NOXMFD.dll. Registers itself with NOXMFD's
    // public Api at Awake() (docs/extensions-api.md) instead of anything in src/plugin/ knowing
    // this mod exists.
    [BepInPlugin("com.roque.rc-missile-camera", "NOXMFD: RC Missile Camera Extension", MyPluginInfo.PLUGIN_VERSION)]
    // Pinned to the version Api.cs (the only surface this extension touches) first shipped in.
    // That surface hasn't changed shape since, so this is the true floor — anything at or above
    // it has the shape this extension was written against, per docs/extensions-api.md's
    // Versioning section.
    [BepInDependency("com.roque.NOXMFD", "0.23.0")]
    [BepInProcess("NuclearOption.exe")]
    [BepInProcess("NuclearOptionServer.exe")]
    public class Plugin : BaseUnityPlugin
    {
        internal const string ExtId = "rc-missile-camera";
        internal static ManualLogSource? Log;

        // Same reasoning as NOXMFD's own Plugin.cs: this GameObject doesn't survive the
        // boot -> MainMenu scene transition in this Unity version, so the actual per-frame work
        // (MissileCameraLifecycle) runs on a self-spawned, DontDestroyOnLoad GameObject instead.
        private static MissileCameraLifecycle? _lifecycle;
        private static bool _sceneSubscribed;

        private void Awake()
        {
            Log = Logger;

            bool ok = NOXMFD.Api.RegisterExtension(ExtId, "MISSILE CAMERA", MissileCameraAssets.Resolve, MissileCameraCommands.Handle);
            if (!ok)
            {
                Log.LogError("[MISSILE CAMERA] failed to register with NOXMFD (id already taken?) — extension disabled.");
                return;
            }

            if (!_sceneSubscribed)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                _sceneSubscribed = true;
            }
            Log.LogInfo("MISSILE CAMERA loaded.");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_lifecycle != null) return;
            var go = new GameObject("MissileCamera_Lifecycle");
            Object.DontDestroyOnLoad(go);
            _lifecycle = go.AddComponent<MissileCameraLifecycle>();
            Log?.LogInfo("[MISSILE CAMERA] MissileCameraLifecycle attached (scene='" + scene.name + "').");
        }
    }
}
