using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RcMissileCamera
{
    // Builds the same status block NOXMFD's own TelemetryServer.RcBlock used to build
    // server-side (PR #45), now owned entirely by this extension and pushed via
    // Api.PublishSlice instead of spliced into NOXMFD's own frame builder.
    //
    // ponytail: the aim reticle rides this normal 10 Hz slice (aimX/aimY fields) rather than its
    // own high-rate SSE channel the way PR #45's version did — Api.PublishEvent exists for that,
    // but no browser page currently subscribes to a per-extension high-rate event automatically
    // (docs/extensions-api.md, Deferred). Ceiling: the reticle updates at 10 Hz instead of ~60 Hz,
    // which will read as slightly less smooth during a fast drag. Upgrade path: once the
    // browser-side generic listener registration lands, switch this back to PublishEvent.
    internal static class RcTelemetry
    {
        internal static void Publish(RcFeed feed)
        {
            NOXMFD.Api.PublishSlice(Plugin.ExtId, Build(feed));
        }

        private static string Build(RcFeed feed)
        {
            if (!RcBridge.Available) return "{\"available\":false}";

            // "tele"/"markers" are spliced in verbatim (see McBridge.TelemetryJson/MarkersJson)
            // rather than built via string.Format — they're already valid, pre-escaped JSON
            // straight from the base MissileCamera mod, and re-threading their fields through
            // format args here would just be a second, easier-to-drift copy.
            string tele = string.IsNullOrEmpty(McBridge.TelemetryJson) ? "null" : McBridge.TelemetryJson!;
            string markers = string.IsNullOrEmpty(McBridge.MarkersJson) ? "[]" : McBridge.MarkersJson;
            UnityEngine.Vector2 reticle = RcBridge.ReticleViewport;

            return string.Format(CultureInfo.InvariantCulture,
                "{{\"available\":true,\"fsActive\":{0},\"controlling\":{1},\"missile\":\"{2}\"," +
                "\"thr\":{3:0.000},\"boost\":{4},\"link\":\"{5}\",\"formation\":{6},\"pool\":{7}," +
                "\"aimX\":{8:0.000},\"aimY\":{9:0.000},\"tele\":{10},\"markers\":{11}}}",
                feed.Active ? "true" : "false",
                RcBridge.IsControlling ? "true" : "false",
                Escape(RcBridge.ControlledMissileName ?? string.Empty),
                RcBridge.Throttle01,
                RcBridge.BoostActive ? "true" : "false",
                Escape(RcBridge.LinkQuality ?? string.Empty),
                RcBridge.FormationFollowActive ? "true" : "false",
                StringArray(RcBridge.ControllablePool),
                reticle.x, reticle.y,
                tele, markers);
        }

        private static string StringArray(IReadOnlyList<string> items)
        {
            if (items == null || items.Count == 0) return "[]";
            var sb = new StringBuilder("[");
            for (int i = 0; i < items.Count; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append('"').Append(Escape(items[i] ?? string.Empty)).Append('"');
            }
            return sb.Append(']').ToString();
        }

        // Own tiny copy — TelemetryServer.EscapeJson is internal to NOXMFD, not part of the
        // public Api surface, and this extension's JSON needs are small enough not to justify
        // asking for it to be exposed.
        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return s ?? string.Empty;
            var sb = new StringBuilder(s.Length + 8);
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"':  sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
