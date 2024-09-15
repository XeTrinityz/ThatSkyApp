using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO.Compression;

namespace That_Sky_App;

public partial class MainWindow : Form
{
    private readonly string CurrentVersion = "1.4.1";

    private readonly string downloadUrl =
        "https://github.com/XeTrinityz/TSM-Installer/releases/latest/download/ThatSkyApp.exe";

    private readonly string executableName = "ThatSkyApp.exe";
    private readonly Dictionary<string, List<ItemData>> itemDict = [];
    private readonly string TSMVersion = "2.4.1";

    private readonly string versionFileUrl =
        "https://github.com/XeTrinityz/TSM-Installer/releases/latest/download/Version.json";

    private readonly string versionTSMFileUrl =
        "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/Version.json";

    private bool _dragging;
    private Point _dragStartPoint;
    private bool isMaximized;

    public MainWindow()
    {
        InitializeComponent();
        LoadData();
        PopulateListBoxes();
        InstallItems.KeyPress += InstallItems_KeyPress;
        TitleBar.MouseDown += TitleBar_MouseDown;
        TitleBar.MouseMove += TitleBar_MouseMove;
        TitleBar.MouseUp += TitleBar_MouseUp;
        Translations.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        Locations.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        Themes.SelectedIndexChanged += ListBox_SelectedIndexChanged;
        AboutPopup.SetToolTip(AboutButton, "TSM: XeTrinityz & TheSR\nSML: Lukas");
    }

    private void LoadData()
    {
        itemDict["Translations"] =
        [
            new ItemData
            {
                Name = "English", Author = "XeTrinityz",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1275813119012110470/1275813119352115220/English.json?ex=66e8368f&is=66e6e50f&hm=19765d933b2d9d5a4019c29e3fec11eb49b0e3d062c959e23146da34d3f53447&",
                Type = "Translation"
            },
            new ItemData
            {
                Name = "Russian", Author = "k0ra",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1275905422582222882/1284623770883063990/Russian.json?ex=66e7f75e&is=66e6a5de&hm=6d65b5d499b49c8acf1f70c5f0bdb528907628ac748e14de0e9f219a13952e24&",
                Type = "Translation"
            },
            new ItemData
            {
                Name = "Vietnamese", Author = "Havier",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1275909234579738635/1284428866798096464/Vietnamese.json?ex=66e7ea99&is=66e69919&hm=075fe83edef63717041bf4e59ee96fc86f31bf7d8c97c410542daf750f3f88e1&",
                Type = "Translation"
            }
        ];

        itemDict["Locations"] =
        [
            new ItemData
            {
                Name = "Points of Interest", Author = "Samuel Grey",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1276268133988172086/1276268134117937276/Teleport_Locations.zip?ex=66e88cd3&is=66e73b53&hm=3219e933eaa1abd2a9f02599ca25cee7ef5c87db113ffb8091b23e5b431e3a60&",
                Type = "Location"
            },
        ];

        itemDict["Themes"] =
        [
            new ItemData
            {
                Name = "Fighter Jet", Author = "Ling Ling",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1281768052844793887/1281768053364883517/Theme.json?ex=66e81fc6&is=66e6ce46&hm=fbb4a1bf16a15420d042ee48e8946882d9a55d6f33445cf7bd9c6f531861ff0b&",
                Type = "Theme"
            },
            new ItemData
            {
            Name = "Transparent and Dark", Author = "Havier",
            DownloadLink =
                "https://cdn.discordapp.com/attachments/1281361783646457929/1281361784644698133/Theme.json?ex=66e7f6e8&is=66e6a568&hm=a48749a219182a9689e720aa1524cd30fbf3dc7c38bce8abc4a698b7e3585fd9&",
            Type = "Theme"
            },
            new ItemData
            {
                Name = "Red", Author = "k0ra",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1281271466410905753/1281271467262345216/Theme.json?ex=66e84b8a&is=66e6fa0a&hm=9092d69d850fc9a5a10fcde0a61cba53f8b68a18ba5d17d82c5a09ff1e71df1c&",
                Type = "Theme"
            },
            new ItemData
            {
                Name = "Camouflage", Author = "Neutral",
                DownloadLink =
                    "https://cdn.discordapp.com/attachments/1281294768336404573/1281294768512438362/Theme.json?ex=66e8613e&is=66e70fbe&hm=0e2fa97097595a46dbffe13f990b3acae394be38348dbc21e740656a0952d049&",
                Type = "Theme"
            }
        ];
    }

    private void PopulateListBoxes()
    {
        foreach (ItemData item in itemDict["Translations"])
        {
            _ = Translations.Items.Add(item);
        }

        foreach (ItemData item in itemDict["Locations"])
        {
            _ = Locations.Items.Add(item);
        }

        foreach (ItemData item in itemDict["Themes"])
        {
            _ = Themes.Items.Add(item);
        }
    }

    private void ListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is ItemData selectedItem)
        {
            listBox.ClearSelected();
            DialogResult result = MessageBox.Show($"Do you want to install {selectedItem.Name}?", "Confirm Installation",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                InstallSelectedItem(selectedItem);
            }
        }
    }

    private async void InstallSelectedItem(ItemData selectedItem)
    {
        try
        {
            string? gameFolder = GetGameFolderFromRegistry();
            if (string.IsNullOrEmpty(gameFolder))
            {
                InfoLabel.Text = "Failed to locate game folder!";
                InfoLabel.ForeColor = Color.Red;
                return;
            }
            string modsFolder = Path.Combine(gameFolder, "mods", "TSM Resources");
            string destinationFolder = selectedItem.Type switch
            {
                "Theme" => Path.Combine(modsFolder, ""),
                "Translation" => Path.Combine(modsFolder, "Languages"),
                "Location" => Path.Combine(modsFolder, "Teleport Locations"),
                _ => throw new InvalidOperationException("Unknown item type")
            };
            if (!Directory.Exists(destinationFolder) && selectedItem.Type != "Theme")
            {
                _ = Directory.CreateDirectory(destinationFolder);
            }

            using (HttpClient client = new())
            {
                InfoLabel.Text = $"Downloading {selectedItem.Name}...";
                HttpResponseMessage response = await client.GetAsync(selectedItem.DownloadLink);
                _ = response.EnsureSuccessStatusCode();

                string tempFilePath = Path.GetTempFileName();
                using (FileStream tempFileStream = new(tempFilePath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(tempFileStream);
                }

                bool isZipFile = IsZipFile(tempFilePath);

                if (isZipFile)
                {
                    InfoLabel.Text = $"Extracting {selectedItem.Name}...";
                    System.IO.Compression.ZipFile.ExtractToDirectory(tempFilePath, destinationFolder, true);
                    File.Delete(tempFilePath);
                }
                else
                {
                    string destinationFile = selectedItem.Type == "Theme"
                        ? Path.Combine(destinationFolder, "Theme.json")
                        : Path.Combine(destinationFolder, $"{selectedItem.Name}.json");
                    File.Move(tempFilePath, destinationFile, true);
                }
            }
            InfoLabel.Text = $"{selectedItem.Name} installed successfully!";
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"Failed to install {selectedItem.Name}!";
            InfoLabel.ForeColor = Color.Red;
            _ = MessageBox.Show($"Error: {ex.Message}", "Installation Error", MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private bool IsZipFile(string filePath)
    {
        const int ZIP_SIGNATURE = 0x04034b50;
        using FileStream fs = new(filePath, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(fs);
        int signature = reader.ReadInt32();
        return signature == ZIP_SIGNATURE;
    }

    private void TitleBar_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragging = true;
            _dragStartPoint = e.Location;
        }
    }

    private void TitleBar_MouseMove(object sender, MouseEventArgs e)
    {
        if (_dragging)
        {
            Point newPoint = TitleBar.PointToScreen(e.Location);
            Location = new Point(
                newPoint.X - _dragStartPoint.X,
                newPoint.Y - _dragStartPoint.Y
            );
        }
    }

    private void TitleBar_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _dragging = false;
        }
    }

    private void InstallItems_KeyPress(object sender, KeyPressEventArgs e)
    {
        e.Handled = true;
    }

    private string? GetGameFolderFromRegistry()
    {
        string registryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 2325290";
        string registryValueName = "InstallLocation";
        string gameFolderName = "Sky Children of the Light";

        using RegistryKey? key = Registry.LocalMachine.OpenSubKey(registryKeyPath);
        if (key != null)
        {
            object? o = key.GetValue(registryValueName);
            if (o != null)
            {
                string? registryPath = o.ToString();
                if (IsCorrectGameFolder(registryPath, gameFolderName))
                {
                    return registryPath;
                }
            }
        }

        using FolderBrowserDialog folderBrowser = new();
        folderBrowser.Description = "Please select the 'Sky Children of the Light' installation folder.";
        folderBrowser.ShowNewFolderButton = false;

        if (folderBrowser.ShowDialog() == DialogResult.OK)
        {
            string selectedPath = folderBrowser.SelectedPath;

            if (IsCorrectGameFolder(selectedPath, gameFolderName))
            {
                return selectedPath;
            }

            string? foundFolder = ScanForGameFolder(selectedPath, gameFolderName);
            if (!string.IsNullOrEmpty(foundFolder))
            {
                return foundFolder;
            }
        }

        return null;
    }

    private bool IsCorrectGameFolder(string path, string gameFolderName)
    {
        return Path.GetFileName(path).Equals(gameFolderName, StringComparison.OrdinalIgnoreCase);
    }

    private string? ScanForGameFolder(string directory, string gameFolderName)
    {
        foreach (string subDir in Directory.GetDirectories(directory))
        {
            if (IsCorrectGameFolder(subDir, gameFolderName))
            {
                return subDir;
            }
        }

        string? parentDir = Directory.GetParent(directory)?.FullName;
        if (!string.IsNullOrEmpty(parentDir))
        {
            foreach (string subDir in Directory.GetDirectories(parentDir))
            {
                if (IsCorrectGameFolder(subDir, gameFolderName))
                {
                    return subDir;
                }
            }
        }

        return null;
    }

    private async void InstallButton_Click(object sender, EventArgs e)
    {
        InfoLabel.ForeColor = Color.White;
        if (InstallItems.SelectedItem != null && InstallItems.SelectedItem.ToString() == "TSM")
        {
            try
            {
                string downloadUrl = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/TSM.zip";
                string zipFilePath = Path.Combine(Path.GetTempPath(), "TSM.zip");

                using (HttpClient client = new())
                {
                    InfoLabel.Text = "Downloading TSM.zip...";
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    _ = response.EnsureSuccessStatusCode();

                    using FileStream fileStream = new(zipFilePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                }

                InfoLabel.Text = "Looking for game folder...";
                string? gameFolder = GetGameFolderFromRegistry();

                if (string.IsNullOrEmpty(gameFolder))
                {
                    InfoLabel.Text = "Failed to locate game folder!";
                    InfoLabel.ForeColor = Color.Red;
                    return;
                }

                InfoLabel.Text = "Game folder found!";

                string modsFolder = Path.Combine(gameFolder, "mods");
                if (!Directory.Exists(modsFolder))
                {
                    _ = Directory.CreateDirectory(modsFolder);
                    InfoLabel.Text = "Creating mods directory...";
                }

                InfoLabel.Text = "Extracting TSM.zip contents to the mods directory...";
                ZipFile.ExtractToDirectory(zipFilePath, modsFolder, true);
                InfoLabel.Text = "TSM installed successfully!";
            }
            catch (Exception)
            {
                InfoLabel.Text = "Failed to Install TSM!";
                InfoLabel.ForeColor = Color.Red;
            }
        }
        else if (InstallItems.SelectedItem != null && InstallItems.SelectedItem.ToString() == "SML")
        {
            try
            {
                string downloadUrl = "https://github.com/lukas0x1/sml-pc/releases/latest/download/sml-pc.zip";
                string zipFilePath = Path.Combine(Path.GetTempPath(), "sml-pc.zip");

                using (HttpClient client = new())
                {
                    InfoLabel.Text = "Downloading sml-pc.zip...";
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    _ = response.EnsureSuccessStatusCode();

                    using FileStream fileStream = new(zipFilePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                }

                InfoLabel.Text = "Looking for game folder...";
                string? gameFolder = GetGameFolderFromRegistry();

                if (string.IsNullOrEmpty(gameFolder))
                {
                    InfoLabel.Text = "Failed to locate game folder!";
                    InfoLabel.ForeColor = Color.Red;
                    return;
                }

                InfoLabel.Text = "Game folder found!";

                InfoLabel.Text = "Extracting sml-pc.zip contents to the mods directory...";
                ZipFile.ExtractToDirectory(zipFilePath, gameFolder, true);

                string modsFolder = Path.Combine(gameFolder, "mods");
                string demoFile = Path.Combine(modsFolder, "demo.dll");
                if (File.Exists(demoFile))
                {
                    File.Delete(demoFile);
                }

                InfoLabel.Text = "SML installed successfully!";
            }
            catch (Exception)
            {
                InfoLabel.Text = "Failed to install SML!";
                InfoLabel.ForeColor = Color.Red;
            }
        }
        else
        {
            try
            {
                string downloadUrl = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest/download/VC_redist.x64.exe";
                string exeFilePath = Path.Combine(Path.GetTempPath(), "VC_redist.x64.exe");

                using (HttpClient client = new())
                {
                    InfoLabel.Text = "Downloading VC_redist.x64.exe...";
                    HttpResponseMessage response = await client.GetAsync(downloadUrl);
                    _ = response.EnsureSuccessStatusCode();

                    using FileStream fileStream = new(exeFilePath, FileMode.Create);
                    await response.Content.CopyToAsync(fileStream);
                }

                InfoLabel.Text = "Starting process...";
                ProcessStartInfo startInfo = new()
                {
                    FileName = exeFilePath,
                    Arguments = "/install /quiet /norestart",
                    Verb = "runas",
                    UseShellExecute = true
                };

                using (Process? process = Process.Start(startInfo))
                {
                    process?.WaitForExit();
                }

                InfoLabel.Text = "VC Redist installed successfully!";
            }
            catch (Exception)
            {
                InfoLabel.Text = "Failed to install VC Redist!";
                InfoLabel.ForeColor = Color.Red;
            }
        }
    }

    private void OpenModsButton_Click(object sender, EventArgs e)
    {
        string? gameFolder = GetGameFolderFromRegistry();
        if (string.IsNullOrEmpty(gameFolder))
        {
            InfoLabel.Text = "Failed to locate game folder!";
            InfoLabel.ForeColor = Color.Red;
            return;
        }

        gameFolder = Path.Combine(gameFolder, "mods");
        _ = Process.Start("explorer.exe", gameFolder);
    }

    private void ChangelogButton_Click(object sender, EventArgs e)
    {
        string url = "https://github.com/XeTrinityz/ThatSkyMod/releases/latest";

        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occurred while trying to open the URL: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Exit_Click(object sender, EventArgs e)
    {
        Close();
    }

    private async void UpdateCheckButton_Click(object sender, EventArgs e)
    {
        await CheckForAppUpdatesAsync();
    }

    private async Task CheckForAppUpdatesAsync()
    {
        try
        {
            using HttpClient client = new();
            string json = await client.GetStringAsync(versionFileUrl);

            JObject versionInfo = JObject.Parse(json);
            string latestVersion = versionInfo["version"].ToString();

            Version currentVersion = new(CurrentVersion);
            Version latestVersionObj = new(latestVersion.TrimStart('v'));

            if (latestVersionObj > currentVersion)
            {
                DialogResult result = MessageBox.Show("A new version of the app is available. Do you want to update now?",
                    "Update Available", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    await DownloadAndUpdateAsync(downloadUrl);
                }
            }
            else
            {
                _ = MessageBox.Show("There are no new updates available.");
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occurred while checking for updates: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task CheckForTSMUpdatesAsync()
    {
        try
        {
            using HttpClient client = new();
            string json = await client.GetStringAsync(versionTSMFileUrl);

            JObject versionInfo = JObject.Parse(json);
            string latestVersion = versionInfo["version"].ToString();

            Version currentVersion = new(TSMVersion);
            Version latestVersionObj = new(latestVersion.TrimStart('v'));

            if (latestVersionObj > currentVersion)
            {
                DialogResult result = MessageBox.Show("A new version of TSM is available. Click `Install` to update.",
                    "Update Available", MessageBoxButtons.OK);
                await DownloadAndUpdateAsync(downloadUrl);
            }
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occurred while checking for TSM updates: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task DownloadAndUpdateAsync(string downloadUrl)
    {
        try
        {
            using HttpClient client = new();
            byte[] fileBytes = await client.GetByteArrayAsync(downloadUrl);
            string tempFilePath = Path.Combine(Path.GetTempPath(), executableName);

            await File.WriteAllBytesAsync(tempFilePath, fileBytes);

            string currentFilePath = Process.GetCurrentProcess().MainModule.FileName;
            string backupFilePath = currentFilePath + ".bak";

            if (File.Exists(backupFilePath))
            {
                File.Delete(backupFilePath);
            }

            File.Move(currentFilePath, backupFilePath);

            File.Move(tempFilePath, currentFilePath);

            _ = Process.Start(currentFilePath);

            Application.Exit();
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occurred while updating the application: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void MainWindow_Load(object sender, EventArgs e)
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string backupFilePath = Path.Combine(baseDirectory, "ThatSkyApp.exe.bak");

        if (File.Exists(backupFilePath))
        {
            try
            {
                await Task.Delay(3000);
                File.Delete(backupFilePath);
            }
            catch (Exception)
            {
            }
        }

        await CheckForTSMUpdatesAsync();
    }

    private void Discord_Click(object sender, EventArgs e)
    {
        string url = "https://discord.com/invite/z5Ub9a3QhU";

        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _ = MessageBox.Show($"An error occurred while trying to open the URL: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LaunchSkyButton_Click(object sender, EventArgs e)
    {
        string gameUrl = "steam://rungameid/2325290";
        try
        {
            _ = Process.Start(new ProcessStartInfo
            {
                FileName = gameUrl,
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            InfoLabel.Text = "Failed to start game!";
            InfoLabel.ForeColor = Color.Red;
        }
    }

    private void ResourceButton_Click(object sender, EventArgs e)
    {
        if (isMaximized)
        {
            Size = new Size(226, 207);
            TitleBar.Width = Size.Width;
            Exit.Location = new Point(201, 4);
            DiscordButton.Location = new Point(204, 188);
            AboutButton.Location = new Point(183, 186);
            isMaximized = false;
        }
        else
        {
            Size = new Size(644, 207);
            TitleBar.Width = Size.Width;
            Exit.Location = new Point(615, 4);
            DiscordButton.Location = new Point(619, 188);
            AboutButton.Location = new Point(601, 186);
            isMaximized = true;
        }
    }

    public class ItemData
    {
        public required string Name { get; set; }
        public required string Author { get; set; }
        public required string DownloadLink { get; set; }
        public required string Type { get; set; }

        public override string ToString()
        {
            return $"{Name} by {Author}";
        }
    }
}