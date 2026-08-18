using System;
using System.IO;
using System.Reflection;

namespace RcMissileCamera
{
    // The resolver passed to Api.RegisterExtension — same suffix-match idiom NOXMFD's own
    // ServeAssetRel uses against its manifest, just against this project's own embedded
    // resources instead. "" (the page itself) maps to missile-camera.html.
    internal static class MissileCameraAssets
    {
        private static readonly Assembly _asm = typeof(MissileCameraAssets).Assembly;

        internal static byte[]? Resolve(string relPath)
        {
            string name = string.IsNullOrEmpty(relPath) ? "missile-camera.html" : relPath;
            string suffix = "." + ("web." + name).Replace('/', '.');

            string? resourceName = null;
            foreach (string n in _asm.GetManifestResourceNames())
            {
                if (n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) { resourceName = n; break; }
            }
            if (resourceName == null) return null;

            using Stream? s = _asm.GetManifestResourceStream(resourceName);
            if (s == null) return null;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
