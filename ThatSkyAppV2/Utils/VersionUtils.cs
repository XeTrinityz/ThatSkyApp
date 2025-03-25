using System;

namespace ThatSkyAppV2.Utils;

public static class VersionUtils
{
    public static bool IsNewerVersion(string latestVersion, string currentVersion)
    {
        // Clean up version strings by removing 'v' prefix if present
        latestVersion = latestVersion.TrimStart('v', 'V');
        currentVersion = currentVersion.TrimStart('v', 'V');

        Debug.WriteLine($"Comparing versions - Latest: {latestVersion}, Current: {currentVersion}");

        // Parse versions
        if (!Version.TryParse(latestVersion, out Version? latest) ||
            !Version.TryParse(currentVersion, out Version? current))
        {
            Debug.WriteLine("Failed to parse one or both versions");
            return false;
        }

        bool isNewer = latest > current;
        Debug.WriteLine($"Result: {isNewer} (Latest: {latest}, Current: {current})");
        return isNewer;
    }
}