namespace ThatSkyAppV2.Models;

public class AppConfig
{
    public string Language { get; set; } = "English";

    // Hard-coded download URLs - can be accessed via GetDownloadUrl method
    private static readonly Dictionary<string, Dictionary<string, string>> DownloadUrls = new()
    {
        ["English"] = new()
        {
            ["TSM"] = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip",
            ["SML"] = "https://github.com/XeTrinityz/ThatSkyModLoader/releases/latest/download/TSML.zip",
            ["VCRedist"] = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
        },
        ["Chinese"] = new()
        {
            ["TSM"] = "https://gitee.com/xiao-zhu245/TSMinstall/releases/download/TSM/TSM.zip",
            ["SML"] = "https://gitee.com/xiao-zhu245/TSMinstall/releases/download/TSM/sml-pc.zip",
            ["VCRedist"] = "https://gitee.com/xiao-zhu245/TSMinstall/releases/download/TSM/vc_redist.x64.exe"
        },
        ["Russian"] = new()
        {
            ["TSM"] = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip",
            ["SML"] = "https://github.com/XeTrinityz/ThatSkyModLoader/releases/latest/download/TSML.zip",
            ["VCRedist"] = "https://aka.ms/vs/17/release/vc_redist.x64.exe"
        }
    };

    public string GetDownloadUrl(string modName)
    {
        if (DownloadUrls.TryGetValue(Language, out var urls) && urls.TryGetValue(modName, out var url))
        {
            return url;
        }

        // Fallback to English URLs if language not found
        return DownloadUrls["English"][modName];
    }
}