using Microsoft.Win32;
using System.Diagnostics;
using System.IO.Compression;

namespace TSM_Manager
{
    public partial class Form1 : Form
    {
        private bool _dragging = false;
        private Point _dragStartPoint;

        public Form1()
        {
            InitializeComponent();
            MouseDown += Form1_MouseDown;
            MouseMove += Form1_MouseMove;
            MouseUp += Form1_MouseUp;
        }

        private void LogMessage(string message, Color color)
        {
            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.SelectionLength = 0;

            logTextBox.SelectionColor = color;
            logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}\n");
            logTextBox.SelectionColor = logTextBox.ForeColor;
            logTextBox.ScrollToCaret();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = true;
                _dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragging)
            {
                Point newPoint = PointToScreen(e.Location);
                Location = new Point(newPoint.X - _dragStartPoint.X, newPoint.Y - _dragStartPoint.Y);
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragging = false;
            }
        }

        private string GetGameFolderFromRegistry()
        {
            string registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2325290";
            string registryValueName = "InstallLocation";

            using RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKeyPath);
            if (key != null)
            {
                object o = key.GetValue(registryValueName);
                if (o != null)
                {
                    return o.ToString();
                }
            }

            return null;
        }

        private async void installModButton_Click(object sender, EventArgs e)
        {
            try
            {
                string downloadUrl = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip";
                string zipFilePath = Path.Combine(Path.GetTempPath(), "TSM.zip");

                using (HttpClient client = new())
                {
                    LogMessage("Downloading TSM.dll...", Color.LimeGreen);
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    _ = response.EnsureSuccessStatusCode();

                    using FileStream fileStream = new(zipFilePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                }

                string gameFolder = GetGameFolderFromRegistry();

                if (string.IsNullOrEmpty(gameFolder))
                {
                    LogMessage("Could not find the game folder in the registry.", Color.Red);
                    return;
                }

                LogMessage($"Game Folder Located: {gameFolder}", Color.LimeGreen);

                string modsFolder = Path.Combine(gameFolder, "mods");
                if (!Directory.Exists(modsFolder))
                {
                    _ = Directory.CreateDirectory(modsFolder);
                    LogMessage("Created Directory: mods", Color.LimeGreen);
                }

                LogMessage("Extracting TSM.zip contents to the mods directory...", Color.LimeGreen);
                ZipFile.ExtractToDirectory(zipFilePath, modsFolder, true);

                LogMessage("Mod installed successfully!", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred: {ex.Message}", Color.Red);
            }
        }

        private void uninstallModButton_Click(object sender, EventArgs e)
        {
            try
            {
                string gameFolder = GetGameFolderFromRegistry();

                if (string.IsNullOrEmpty(gameFolder))
                {
                    LogMessage("Could not find the game folder in the registry.", Color.Red);
                    return;
                }

                LogMessage($"Game Folder Located: {gameFolder}", Color.LimeGreen);

                string modsFolder = Path.Combine(gameFolder, "mods");
                if (!Directory.Exists(modsFolder))
                {
                    LogMessage("Mods folder does not exist. Nothing to uninstall.", Color.Orange);
                    return;
                }

                string tsmDllPath = Path.Combine(modsFolder, "TSM.dll");
                string tsmResourcesFolderPath = Path.Combine(modsFolder, "TSM Resources");

                bool filesDeleted = false;

                if (File.Exists(tsmDllPath))
                {
                    File.Delete(tsmDllPath);
                    filesDeleted = true;
                    LogMessage("Deleted: TSM.dll", Color.LimeGreen);
                }

                if (Directory.Exists(tsmResourcesFolderPath))
                {
                    Directory.Delete(tsmResourcesFolderPath, true);
                    filesDeleted = true;
                    LogMessage("Deleted: TSM Resources", Color.LimeGreen);
                }

                if (!filesDeleted)
                {
                    LogMessage("No mod files were found to uninstall.", Color.Orange);
                    return;
                }

                LogMessage("Mod uninstalled successfully!", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred: {ex.Message}", Color.Red);
            }
        }

        private async void installVcRedistButton_Click(object sender, EventArgs e)
        {
            try
            {
                string downloadUrl = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/VC_redist.x64.exe";
                string exeFilePath = Path.Combine(Path.GetTempPath(), "VC_redist.x64.exe");

                using (HttpClient client = new())
                {
                    LogMessage("Downloading VC_redist.x64.exe...", Color.LimeGreen);
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    _ = response.EnsureSuccessStatusCode();

                    using FileStream fileStream = new(exeFilePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                }

                LogMessage("Starting process...", Color.LimeGreen);
                ProcessStartInfo startInfo = new()
                {
                    FileName = exeFilePath,
                    Arguments = "/install /quiet /norestart",
                    Verb = "runas",
                    UseShellExecute = true
                };

                using (Process process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }

                LogMessage("VC Redist installed successfully!", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred: {ex.Message}", Color.Red);
            }
        }

        private async void installSMLButton_Click(object sender, EventArgs e)
        {
            try
            {
                string downloadUrl = "https://github.com/lukas0x1/sml-pc/releases/latest/download/sml-pc.zip";
                string zipFilePath = Path.Combine(Path.GetTempPath(), "sml-pc.zip");

                using (HttpClient client = new())
                {
                    LogMessage("Downloading sml-pc.zip...", Color.LimeGreen);
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    _ = response.EnsureSuccessStatusCode();

                    using FileStream fileStream = new(zipFilePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                }

                string gameFolder = GetGameFolderFromRegistry();

                if (string.IsNullOrEmpty(gameFolder))
                {
                    LogMessage("Could not find the game folder in the registry.", Color.Red);
                    return;
                }

                LogMessage($"Game Folder Located: {gameFolder}", Color.LimeGreen);

                LogMessage("Extracting sml-pc.zip contents to the games directory...", Color.LimeGreen);
                ZipFile.ExtractToDirectory(zipFilePath, gameFolder, true);

                string modsFolder = Path.Combine(gameFolder, "mods");
                string demoFile = Path.Combine(modsFolder, "demo.dll");
                if (File.Exists(demoFile))
                {
                    File.Delete(demoFile);
                }

                LogMessage("SML installed successfully!", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred: {ex.Message}", Color.Red);
            }
        }

        private void uninstallSMLButton_Click(object sender, EventArgs e)
        {
            try
            {

                string gameFolder = GetGameFolderFromRegistry();

                if (string.IsNullOrEmpty(gameFolder))
                {
                    LogMessage("Could not find the game folder in the registry.", Color.Red);
                    return;
                }

                LogMessage($"Game Folder Located: {gameFolder}", Color.LimeGreen);

                string modsFolder = Path.Combine(gameFolder, "mods");
                string fontsFolder = Path.Combine(gameFolder, "fonts");
                string powrprofDllPath = Path.Combine(gameFolder, "powrprof.dll");
                string smlConfigPath = Path.Combine(gameFolder, "sml_config.json");

                bool filesDeleted = false;

                if (Directory.Exists(modsFolder))
                {
                    Directory.Delete(modsFolder, true);
                    filesDeleted = true;
                    LogMessage("Deleted: mods", Color.LimeGreen);
                }

                if (Directory.Exists(fontsFolder))
                {
                    Directory.Delete(fontsFolder, true);
                    filesDeleted = true;
                    LogMessage("Deleted: fonts", Color.LimeGreen);
                }

                if (File.Exists(powrprofDllPath))
                {
                    File.Delete(powrprofDllPath);
                    filesDeleted = true;
                    LogMessage("Deleted: powrprof.dll", Color.LimeGreen);
                }

                if (File.Exists(smlConfigPath))
                {
                    File.Delete(smlConfigPath);
                    filesDeleted = true;
                    LogMessage("Deleted: sml_config.json", Color.LimeGreen);
                }

                if (!filesDeleted)
                {
                    LogMessage("No files or folders were found to uninstall.", Color.Red);
                    return;
                }

                LogMessage("SML uninstalled successfully!", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                LogMessage($"An error occurred: {ex.Message}", Color.Red);
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}