using System.Collections.Generic;
namespace ThatSkyAppV2.Models;

public class AppConfig
{
    public string Language { get; set; } = "English";
    public string? ModInstallPath { get; set; }
    public bool AlwaysDownloadLatestOnInject { get; set; } = true;
    public int InjectDelayMs { get; set; } = 0;
    public bool AutoLaunchGame { get; set; } = true;

    // Hard-coded download URLs - can be accessed via GetDownloadUrl method
    private static readonly Dictionary<string, Dictionary<string, string>> DownloadUrls = new()
    {
        ["English"] = new()
        {
            ["TSM"] = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip",
            ["VCRedist"] = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
        },
        ["Chinese"] = new()
        {
            ["TSM"] = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip",
            ["VCRedist"] = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
        },
        ["Russian"] = new()
        {
            ["TSM"] = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip",
            ["VCRedist"] = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
        }
    };

    public string GetDownloadUrl(string modName)
    {
        // For other mods, use the traditional language-based lookup
        if (DownloadUrls.TryGetValue(Language, out var urls) && urls.TryGetValue(modName, out var url))
        {
            return url;
        }

        // Fallback to English URLs if language not found
        return DownloadUrls["English"][modName];
    }
}