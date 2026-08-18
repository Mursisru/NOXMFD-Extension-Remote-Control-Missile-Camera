using UnityEngine;

namespace RcMissileCamera
{
    // Per-frame driver, living on its own persistent GameObject (see Plugin.cs's comment on why).
    // Commands don't need draining here — NOXMFD's own MissionLifecycle drains the extension
    // command queue and calls MissileCameraCommands.Handle directly (docs/extensions-api.md
    // surface #3). This only owns the capture feed's tick and the periodic telemetry publish.
    internal class MissileCameraLifecycle : MonoBehaviour
    {
        private readonly RcFeed _feed = new RcFeed();
        private float _telemetryTimer;
        private const float TelemetryInterval = 0.1f;   // 10 Hz — matches NOXMFD's own frame cadence

        private void Update()
        {
            float dt = Time.deltaTime;
            _feed.Tick(dt);

            _telemetryTimer += dt;
            if (_telemetryTimer >= TelemetryInterval)
            {
                _telemetryTimer = 0f;
                MissileCameraTelemetry.Publish(_feed);
            }
        }

        private void OnDestroy()
        {
            _feed.Disengage();
        }
    }
}
