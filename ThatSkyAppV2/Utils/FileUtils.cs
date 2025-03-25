namespace ThatSkyAppV2.Utils;

public static class FileUtils
{
    private static readonly HashSet<string> ProtectedFiles = new()
    {
        "crash_reports",
        "data",
        "crashpad_handler.exe",
        "fmod.dll",
        "fmodstudio.dll",
        "Sky.exe",
        "Sky.lib",
        "Sky.log",
        "Sky.res",
        "steam_api64.dll"
    };

    public static void CleanupExtractionFiles(string gameFolder)
    {
        string[] filesToDelete = {
            Path.Combine(gameFolder, "mods", "demo.dll"),
            Path.Combine(gameFolder, "mods", "READ.txt"),
            Path.Combine(gameFolder, "libcurl.dll")
        };

        foreach (string file in filesToDelete)
        {
            try
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }
    }

    public static void RemoveNonProtectedFiles(string gameFolder)
    {
        try
        {
            // Delete non-protected files in root
            foreach (string filePath in Directory.GetFiles(gameFolder))
            {
                string fileName = Path.GetFileName(filePath);
                if (!ProtectedFiles.Contains(fileName))
                {
                    File.Delete(filePath);
                }
            }

            // Delete non-protected folders
            foreach (string dirPath in Directory.GetDirectories(gameFolder))
            {
                string dirName = Path.GetFileName(dirPath);
                if (!ProtectedFiles.Contains(dirName))
                {
                    Directory.Delete(dirPath, true);
                }
            }
        }
        catch (Exception)
        {
            // Log error if needed
        }
    }
}
