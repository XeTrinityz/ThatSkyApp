using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ThatSkyAppV2.Constants;
using System.ComponentModel;
using ThatSkyAppV2.Utils;
using ThatSkyAppV2.Models;
using static ThatSkyAppV2.Services.DownloadService;

namespace ThatSkyAppV2.Services;

public class InstallationService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ConfigurationService _configService;
    private readonly LocalizationService _localizationService;
    private readonly Action<string> _showPopup;
    private readonly Action<string> _updateInfoLabel;
    private readonly DownloadService _downloadService;

    public InstallationService(
        HttpClient httpClient,
        ConfigurationService configService,
        LocalizationService localizationService,
        Action<string> showPopup,
        Action<string> updateInfoLabel)
    {
        _httpClient = httpClient;
        _configService = configService;
        _localizationService = localizationService;
        _showPopup = showPopup;
        _updateInfoLabel = updateInfoLabel;
        _downloadService = new DownloadService(httpClient);
    }

    public async Task InstallModsAsync(string gameFolder, ModInstallInfo[] mods)
    {
        try
        {
            bool isUpdate = File.Exists(Path.Combine(gameFolder, "mods", "TSM.dll"));
            if (isUpdate)
            {
                RemoveExistingModFiles(gameFolder);
            }

            var config = _configService.GetConfig();
            
            foreach (var mod in mods)
            {
                string downloadUrl;
                
                // Force correct URL based on setting for SML
                if (mod.ModName == "SML")
                {
                    if (config.UseNewModLoader)
                    {
                        downloadUrl = "https://github.com/XeTrinityz/ThatSkyModLoader/releases/latest/download/TSML.zip";
                    }
                    else
                    {
                        downloadUrl = "https://github.com/lukas0x1/sml-pc/releases/latest/download/sml-pc.zip";
                    }
                }
                else
                {
                    downloadUrl = config.GetDownloadUrl(mod.ModName);
                }
                
                await InstallModAsync(mod, downloadUrl, gameFolder);
            }

            string message = _localizationService.GetString(
                isUpdate ? "Str.Message.UpdateSuccess" : "Str.Message.InstallSuccess");
            _showPopup(message);
        }
        catch (Exception ex)
        {
            string errorMessage = string.Format(
                _localizationService.GetString(
                    File.Exists(Path.Combine(gameFolder, "mods", "TSM.dll"))
                        ? "Str.Message.UpdateFailed"
                        : "Str.Message.InstallFailed"
                ),
                ex.Message
            );
            _showPopup(errorMessage);
            throw;
        }
    }

    private async Task InstallModAsync(ModInstallInfo mod, string downloadUrl, string gameFolder)
    {
        string tempFile = Path.Combine(Path.GetTempPath(), mod.FileName);
        try
        {
            _updateInfoLabel($"Downloading {mod.FileName}...");

            var progress = new Progress<DownloadProgress>(p =>
            {
                var speed = p.SpeedBytesPerSecond / 1024 / 1024; // Convert to MB/s
                _updateInfoLabel($"Downloading {mod.FileName}: {p.ProgressPercentage:F1}% ({speed:F1} MB/s)");
            });

            if (!await _downloadService.DownloadFileAsync(downloadUrl, tempFile, progress))
            {
                throw new Exception($"Failed to download {mod.FileName}");
            }

            string extractPath = mod.ExtractToMods ? Path.Combine(gameFolder, "mods") : gameFolder;
            Directory.CreateDirectory(extractPath);

            if (mod.ExtractToMods)
            {
                string existingMod = Path.Combine(extractPath, "TSM.dll");
                if (File.Exists(existingMod)) File.Delete(existingMod);
            }

            _updateInfoLabel($"Extracting {mod.FileName}...");
            ZipFile.ExtractToDirectory(tempFile, extractPath, true);

            if (mod.ExtractToMods)
            {
                //_updateInfoLabel("Patching TSM.dll...");
                string dllPath = Path.Combine(extractPath, "TSM.dll");
                //if (!SecurityUtils.PatchDllHash(dllPath))
                //{
                //    throw new Exception("Failed to patch TSM.dll");
                //}
            }

            if (!mod.ExtractToMods)
            {
                FileUtils.CleanupExtractionFiles(gameFolder);
            }
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    public async Task InstallVCRedistAsync(CancellationToken cancellationToken)
    {
        string exePath = Path.Combine(Path.GetTempPath(), "VC_redist.x64.exe");
        try
        {
            var config = _configService.GetConfig();
            string vcRedistUrl = config.GetDownloadUrl("VCRedist");

            _updateInfoLabel(_localizationService.GetString("Str.Message.DownloadingVCRedist"));

            var progress = new Progress<DownloadProgress>(p =>
            {
                var speed = p.SpeedBytesPerSecond / 1024 / 1024; // Convert to MB/s
                _updateInfoLabel($"Downloading VC Redist: {p.ProgressPercentage:F1}% ({speed:F1} MB/s)");
            });

            if (!await _downloadService.DownloadFileAsync(vcRedistUrl, exePath, progress, cancellationToken))
            {
                throw new Exception("Failed to download VC Redist");
            }

            _updateInfoLabel(_localizationService.GetString("Str.Message.InstallingVCRedist"));
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = "/install /quiet /norestart",
                    UseShellExecute = true,
                    Verb = "runas"
                }
            };

            try
            {
                process.Start();
                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode == 0)
                {
                    _showPopup(_localizationService.GetString("Str.Message.VCRedistSuccess"));
                }
                else
                {
                    string error = string.Format(
                        _localizationService.GetString("Str.Message.VCRedistFailed"),
                        process.ExitCode
                    );
                    _showPopup(error);
                }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                _showPopup(_localizationService.GetString("Str.Message.VCRedistCancelled"));
                throw new OperationCanceledException(_localizationService.GetString("Str.Message.VCRedistCancelled"), ex);
            }
        }
        finally
        {
            if (File.Exists(exePath))
            {
                File.Delete(exePath);
            }
        }
    }

    private void RemoveExistingModFiles(string gameFolder)
    {
        string[] filesToRemove = {
            Path.Combine(gameFolder, "mods", "TSM.dll"),
            Path.Combine(gameFolder, "powrprof.dll")
        };

        foreach (string file in filesToRemove)
        {
            if (File.Exists(file))
            {
                _updateInfoLabel($"Removing {Path.GetFileName(file)}...");
                File.Delete(file);
            }
        }
    }

    public void Dispose()
    {
        _downloadService?.Dispose();
    }
}