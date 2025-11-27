using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ThatSkyAppV2.Constants;
using System.ComponentModel;
using ThatSkyAppV2.Utils;
using ThatSkyAppV2.Models;
using static ThatSkyAppV2.Services.DownloadService;
using System.Diagnostics;
using System.Linq;
using System.IO.Compression;

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

    private async Task<Process?> WaitForProcessAsync(string processName, TimeSpan timeout, System.Threading.CancellationToken ct)
    {
        DateTime end = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < end)
        {
            ct.ThrowIfCancellationRequested();
            var proc = Process.GetProcessesByName(processName).FirstOrDefault();
            if (proc != null)
            {
                try
                {
                    if (!proc.HasExited)
                        return proc;
                }
                catch { }
            }
            await Task.Delay(1000, ct);
        }
        return null;
    }

    // P/Invoke and injection helpers
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out IntPtr lpThreadId);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private bool InjectDll(int processId, string dllPath)
    {
        const uint PROCESS_CREATE_THREAD = 0x0002;
        const uint PROCESS_QUERY_INFORMATION = 0x0400;
        const uint PROCESS_VM_OPERATION = 0x0008;
        const uint PROCESS_VM_WRITE = 0x0020;
        const uint PROCESS_VM_READ = 0x0010;
        const uint MEM_COMMIT = 0x1000;
        const uint MEM_RESERVE = 0x2000;
        const uint PAGE_READWRITE = 0x04;

        IntPtr hProcess = IntPtr.Zero;
        try
        {
            hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, processId);
            if (hProcess == IntPtr.Zero) return false;

            byte[] dllBytes = System.Text.Encoding.ASCII.GetBytes(dllPath + "\0");
            IntPtr allocMem = VirtualAllocEx(hProcess, IntPtr.Zero, (uint)dllBytes.Length, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            if (allocMem == IntPtr.Zero) return false;

            if (!WriteProcessMemory(hProcess, allocMem, dllBytes, (uint)dllBytes.Length, out _)) return false;

            IntPtr hKernel32 = GetModuleHandle("kernel32.dll");
            if (hKernel32 == IntPtr.Zero) return false;
            IntPtr loadLibraryAddr = GetProcAddress(hKernel32, "LoadLibraryA");
            if (loadLibraryAddr == IntPtr.Zero) return false;

            IntPtr threadId;
            IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, loadLibraryAddr, allocMem, 0, out threadId);
            if (hThread == IntPtr.Zero) return false;
            CloseHandle(hThread);
            return true;
        }
        finally
        {
            if (hProcess != IntPtr.Zero) CloseHandle(hProcess);
        }
    }

    // New injection workflow: download/extract to ModInstallPath, launch Sky, inject TSM.dll
    public async Task InjectLatestAsync(System.Threading.CancellationToken cancellationToken)
    {
        var config = _configService.GetConfig();
        if (string.IsNullOrWhiteSpace(config.ModInstallPath))
        {
            throw new InvalidOperationException(_localizationService.GetString("Str.Error.ModPathNotSet"));
        }

        string installDir = config.ModInstallPath!;
        Directory.CreateDirectory(installDir);

        string tempZip = Path.Combine(Path.GetTempPath(), "TSM.zip");
        string dllPath = Path.Combine(installDir, config.InjectionMethod);
        
        try
        {
            // Check if game is already running
            Process? existingProcess = Process.GetProcessesByName("Sky").FirstOrDefault(p => {
                try { return !p.HasExited; }
                catch { return false; }
            });

            if (existingProcess != null)
            {
                // Game is already running, inject directly
                _updateInfoLabel(_localizationService.GetString("Str.Status.GameAlreadyRunning"));
                
                // Ensure we have the selected DLL (download if needed)
                bool needDownload = config.AlwaysDownloadLatestOnInject || !File.Exists(dllPath);
                if (needDownload)
                {
                    string url = config.GetDownloadUrl("TSM");
                    _updateInfoLabel(_localizationService.GetString("Str.Status.DownloadingTSM"));
                    var progress = new Progress<DownloadProgress>(p =>
                    {
                        var speed = p.SpeedBytesPerSecond / 1024 / 1024;
                        string fmt = _localizationService.GetString("Str.Status.DownloadingTSMProgress");
                        _updateInfoLabel(string.Format(fmt, p.ProgressPercentage, speed));
                    });

                    bool ok = await _downloadService.DownloadFileAsync(url, tempZip, progress, cancellationToken);
                    if (!ok) throw new Exception(_localizationService.GetString("Str.Error.DownloadTSMFailed"));

                    _updateInfoLabel(_localizationService.GetString("Str.Status.ExtractingTSM"));
                    System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, installDir, true);
                }
                else
                {
                    _updateInfoLabel(_localizationService.GetString("Str.Status.UsingLocalTSM"));
                }

                if (!File.Exists(dllPath)) throw new FileNotFoundException(_localizationService.GetString("Str.Error.DLLNotFound"), dllPath);

                // Optional delay before injection
                if (config.InjectDelayMs > 0)
                {
                    string waitFmt = _localizationService.GetString("Str.Status.WaitingBeforeInjection");
                    _updateInfoLabel(string.Format(waitFmt, config.InjectDelayMs));
                    await Task.Delay(config.InjectDelayMs, cancellationToken);
                }

                _updateInfoLabel(_localizationService.GetString("Str.Status.InjectingTSM"));
                if (!InjectDll(existingProcess.Id, dllPath))
                {
                    throw new Exception(_localizationService.GetString("Str.Error.InjectionFailed"));
                }

                _showPopup(_localizationService.GetString("Str.Message.InstallSuccess"));
                return;
            }

            // Game is not running - proceed with download/extract
            bool needDownload2 = config.AlwaysDownloadLatestOnInject || !File.Exists(dllPath);
            if (needDownload2)
            {
                // 1) Download latest TSM.zip
                string url = config.GetDownloadUrl("TSM");
                _updateInfoLabel(_localizationService.GetString("Str.Status.DownloadingTSM"));
                var progress = new Progress<DownloadProgress>(p =>
                {
                    var speed = p.SpeedBytesPerSecond / 1024 / 1024;
                    string fmt = _localizationService.GetString("Str.Status.DownloadingTSMProgress");
                    _updateInfoLabel(string.Format(fmt, p.ProgressPercentage, speed));
                });

                bool ok = await _downloadService.DownloadFileAsync(url, tempZip, progress, cancellationToken);
                if (!ok) throw new Exception(_localizationService.GetString("Str.Error.DownloadTSMFailed"));

                // 2) Extract to chosen location (overwrite)
                _updateInfoLabel(_localizationService.GetString("Str.Status.ExtractingTSM"));
                System.IO.Compression.ZipFile.ExtractToDirectory(tempZip, installDir, true);
            }
            else
            {
                _updateInfoLabel(_localizationService.GetString("Str.Status.UsingLocalTSM"));
            }

            // 3) Launch game via Steam (if AutoLaunchGame is enabled)
            if (config.AutoLaunchGame)
            {
                _updateInfoLabel(_localizationService.GetString("Str.Status.LaunchingGame"));
                Process.Start(new ProcessStartInfo
                {
                    FileName = "steam://rungameid/2325290",
                    UseShellExecute = true
                });
            }

            // 4) Wait for process window
            _updateInfoLabel(_localizationService.GetString("Str.Status.WaitingForGame"));
            var process = await WaitForProcessAsync("Sky", TimeSpan.FromMinutes(2), cancellationToken);
            if (process == null) throw new Exception(_localizationService.GetString("Str.Error.GameDidNotStart"));

            // 5) Inject selected DLL
            if (!File.Exists(dllPath)) throw new FileNotFoundException(_localizationService.GetString("Str.Error.DLLNotFound"), dllPath);

            // Optional delay before injection
            if (config.InjectDelayMs > 0)
            {
                string waitFmt = _localizationService.GetString("Str.Status.WaitingBeforeInjection");
                _updateInfoLabel(string.Format(waitFmt, config.InjectDelayMs));
                await Task.Delay(config.InjectDelayMs, cancellationToken);
            }

            _updateInfoLabel(_localizationService.GetString("Str.Status.InjectingTSM"));
            if (!InjectDll(process.Id, dllPath))
            {
                throw new Exception(_localizationService.GetString("Str.Error.InjectionFailed"));
            }

            _showPopup(_localizationService.GetString("Str.Message.InstallSuccess"));
        }
        finally
        {
            if (File.Exists(tempZip))
            {
                try { File.Delete(tempZip); } catch { }
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
                string fmt = _localizationService.GetString("Str.Status.DownloadingVCRedistProgress");
                _updateInfoLabel(string.Format(fmt, p.ProgressPercentage, speed));
            });

            if (!await _downloadService.DownloadFileAsync(vcRedistUrl, exePath, progress, cancellationToken))
            {
                throw new Exception(_localizationService.GetString("Str.Error.DownloadVCRedistFailed"));
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

    public void Dispose()
    {
        _downloadService?.Dispose();
    }
}