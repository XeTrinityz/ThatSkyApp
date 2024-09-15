namespace That_Sky_App
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            TitleBar = new Panel();
            Exit = new Button();
            TitleBarIcon = new PictureBox();
            TitleBarLabel = new Label();
            InstallButton = new Button();
            InstallItems = new ComboBox();
            InfoLabel = new Label();
            UpdateCheckButton = new Button();
            ChangelogButton = new Button();
            OpenModsButton = new Button();
            DiscordButton = new PictureBox();
            ResourcesButton = new Button();
            AboutPopup = new ToolTip(components);
            AboutButton = new PictureBox();
            ResourceButton = new Button();
            TranslationsGroup = new GroupBox();
            Translations = new ListBox();
            LocationsGroup = new GroupBox();
            Locations = new ListBox();
            ThemesGroup = new GroupBox();
            Themes = new ListBox();
            TitleBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)TitleBarIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)DiscordButton).BeginInit();
            ((System.ComponentModel.ISupportInitialize)AboutButton).BeginInit();
            TranslationsGroup.SuspendLayout();
            LocationsGroup.SuspendLayout();
            ThemesGroup.SuspendLayout();
            SuspendLayout();
            // 
            // TitleBar
            // 
            TitleBar.BackColor = Color.FromArgb(26, 32, 38);
            TitleBar.Controls.Add(Exit);
            TitleBar.Controls.Add(TitleBarIcon);
            TitleBar.Controls.Add(TitleBarLabel);
            TitleBar.Location = new Point(0, 0);
            TitleBar.Name = "TitleBar";
            TitleBar.Size = new Size(226, 30);
            TitleBar.TabIndex = 0;
            // 
            // Exit
            // 
            Exit.BackColor = Color.FromArgb(26, 32, 38);
            Exit.FlatAppearance.BorderSize = 0;
            Exit.FlatStyle = FlatStyle.Flat;
            Exit.Font = new Font("Arial", 8F);
            Exit.ForeColor = Color.White;
            Exit.Location = new Point(201, 4);
            Exit.Name = "Exit";
            Exit.Size = new Size(18, 18);
            Exit.TabIndex = 2;
            Exit.Text = "X";
            Exit.UseVisualStyleBackColor = false;
            Exit.Click += Exit_Click;
            // 
            // TitleBarIcon
            // 
            TitleBarIcon.Image = (Image)resources.GetObject("TitleBarIcon.Image");
            TitleBarIcon.Location = new Point(6, 3);
            TitleBarIcon.Name = "TitleBarIcon";
            TitleBarIcon.Size = new Size(24, 24);
            TitleBarIcon.TabIndex = 1;
            TitleBarIcon.TabStop = false;
            // 
            // TitleBarLabel
            // 
            TitleBarLabel.AutoSize = true;
            TitleBarLabel.BackColor = Color.FromArgb(26, 32, 38);
            TitleBarLabel.Font = new Font("Arial", 10F);
            TitleBarLabel.ForeColor = Color.White;
            TitleBarLabel.Location = new Point(32, 5);
            TitleBarLabel.Name = "TitleBarLabel";
            TitleBarLabel.Size = new Size(91, 16);
            TitleBarLabel.TabIndex = 0;
            TitleBarLabel.Text = "That Sky App";
            // 
            // InstallButton
            // 
            InstallButton.BackColor = Color.FromArgb(26, 32, 38);
            InstallButton.Cursor = Cursors.Hand;
            InstallButton.FlatAppearance.BorderSize = 0;
            InstallButton.FlatStyle = FlatStyle.Flat;
            InstallButton.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            InstallButton.ForeColor = Color.White;
            InstallButton.Location = new Point(6, 39);
            InstallButton.Name = "InstallButton";
            InstallButton.Size = new Size(124, 23);
            InstallButton.TabIndex = 1;
            InstallButton.Text = "Install";
            InstallButton.UseVisualStyleBackColor = false;
            InstallButton.Click += InstallButton_Click;
            // 
            // InstallItems
            // 
            InstallItems.BackColor = Color.FromArgb(26, 32, 38);
            InstallItems.FlatStyle = FlatStyle.Flat;
            InstallItems.Font = new Font("Arial", 9F);
            InstallItems.ForeColor = Color.White;
            InstallItems.FormattingEnabled = true;
            InstallItems.Items.AddRange(new object[] { "TSM", "SML", "VC Redist" });
            InstallItems.Location = new Point(136, 39);
            InstallItems.Name = "InstallItems";
            InstallItems.Size = new Size(83, 23);
            InstallItems.TabIndex = 2;
            InstallItems.Tag = "";
            InstallItems.Text = "TSM";
            // 
            // InfoLabel
            // 
            InfoLabel.AutoSize = true;
            InfoLabel.BackColor = Color.FromArgb(17, 17, 17);
            InfoLabel.Font = new Font("Arial", 9F);
            InfoLabel.ForeColor = Color.White;
            InfoLabel.Location = new Point(6, 186);
            InfoLabel.Name = "InfoLabel";
            InfoLabel.Size = new Size(120, 15);
            InfoLabel.TabIndex = 3;
            InfoLabel.Text = "Created by XeTrinityz";
            // 
            // UpdateCheckButton
            // 
            UpdateCheckButton.BackColor = Color.FromArgb(26, 32, 38);
            UpdateCheckButton.Cursor = Cursors.Hand;
            UpdateCheckButton.FlatAppearance.BorderSize = 0;
            UpdateCheckButton.FlatStyle = FlatStyle.Flat;
            UpdateCheckButton.Font = new Font("Arial", 9F);
            UpdateCheckButton.ForeColor = Color.White;
            UpdateCheckButton.Location = new Point(6, 68);
            UpdateCheckButton.Name = "UpdateCheckButton";
            UpdateCheckButton.Size = new Size(124, 23);
            UpdateCheckButton.TabIndex = 4;
            UpdateCheckButton.Text = "Check For Updates";
            UpdateCheckButton.UseVisualStyleBackColor = false;
            UpdateCheckButton.Click += UpdateCheckButton_Click;
            // 
            // ChangelogButton
            // 
            ChangelogButton.BackColor = Color.FromArgb(26, 32, 38);
            ChangelogButton.Cursor = Cursors.Hand;
            ChangelogButton.FlatAppearance.BorderSize = 0;
            ChangelogButton.FlatStyle = FlatStyle.Flat;
            ChangelogButton.Font = new Font("Arial", 9F);
            ChangelogButton.ForeColor = Color.White;
            ChangelogButton.Location = new Point(136, 68);
            ChangelogButton.Name = "ChangelogButton";
            ChangelogButton.Size = new Size(83, 23);
            ChangelogButton.TabIndex = 5;
            ChangelogButton.Text = "Changelog";
            ChangelogButton.UseVisualStyleBackColor = false;
            ChangelogButton.Click += ChangelogButton_Click;
            // 
            // OpenModsButton
            // 
            OpenModsButton.BackColor = Color.FromArgb(26, 32, 38);
            OpenModsButton.Cursor = Cursors.Hand;
            OpenModsButton.FlatAppearance.BorderSize = 0;
            OpenModsButton.FlatStyle = FlatStyle.Flat;
            OpenModsButton.Font = new Font("Arial", 9F);
            OpenModsButton.ForeColor = Color.White;
            OpenModsButton.Location = new Point(6, 98);
            OpenModsButton.Name = "OpenModsButton";
            OpenModsButton.Size = new Size(213, 23);
            OpenModsButton.TabIndex = 6;
            OpenModsButton.Text = "Open Mods Folder";
            OpenModsButton.UseVisualStyleBackColor = false;
            OpenModsButton.Click += OpenModsButton_Click;
            // 
            // DiscordButton
            // 
            DiscordButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            DiscordButton.Cursor = Cursors.Hand;
            DiscordButton.Image = (Image)resources.GetObject("DiscordButton.Image");
            DiscordButton.Location = new Point(204, 188);
            DiscordButton.Name = "DiscordButton";
            DiscordButton.Size = new Size(15, 15);
            DiscordButton.TabIndex = 3;
            DiscordButton.TabStop = false;
            DiscordButton.Click += Discord_Click;
            // 
            // ResourcesButton
            // 
            ResourcesButton.BackColor = Color.FromArgb(26, 32, 38);
            ResourcesButton.Cursor = Cursors.Hand;
            ResourcesButton.FlatAppearance.BorderSize = 0;
            ResourcesButton.FlatStyle = FlatStyle.Flat;
            ResourcesButton.Font = new Font("Arial", 9F);
            ResourcesButton.ForeColor = Color.White;
            ResourcesButton.Location = new Point(6, 127);
            ResourcesButton.Name = "ResourcesButton";
            ResourcesButton.Size = new Size(213, 23);
            ResourcesButton.TabIndex = 7;
            ResourcesButton.Text = "Launch Sky";
            ResourcesButton.UseVisualStyleBackColor = false;
            ResourcesButton.Click += LaunchSkyButton_Click;
            // 
            // AboutPopup
            // 
            AboutPopup.AutoPopDelay = 5000;
            AboutPopup.BackColor = SystemColors.ControlDarkDark;
            AboutPopup.ForeColor = SystemColors.Control;
            AboutPopup.InitialDelay = 100;
            AboutPopup.ReshowDelay = 100;
            AboutPopup.ToolTipTitle = "Credits";
            // 
            // AboutButton
            // 
            AboutButton.Image = (Image)resources.GetObject("AboutButton.Image");
            AboutButton.Location = new Point(183, 186);
            AboutButton.Name = "AboutButton";
            AboutButton.Size = new Size(15, 15);
            AboutButton.TabIndex = 8;
            AboutButton.TabStop = false;
            // 
            // ResourceButton
            // 
            ResourceButton.BackColor = Color.FromArgb(26, 32, 38);
            ResourceButton.Cursor = Cursors.Hand;
            ResourceButton.FlatAppearance.BorderSize = 0;
            ResourceButton.FlatStyle = FlatStyle.Flat;
            ResourceButton.Font = new Font("Arial", 9F);
            ResourceButton.ForeColor = Color.White;
            ResourceButton.Location = new Point(6, 156);
            ResourceButton.Name = "ResourceButton";
            ResourceButton.Size = new Size(213, 23);
            ResourceButton.TabIndex = 9;
            ResourceButton.Text = "Resources";
            ResourceButton.UseVisualStyleBackColor = false;
            ResourceButton.Click += ResourceButton_Click;
            // 
            // TranslationsGroup
            // 
            TranslationsGroup.Controls.Add(Translations);
            TranslationsGroup.FlatStyle = FlatStyle.Flat;
            TranslationsGroup.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TranslationsGroup.ForeColor = Color.White;
            TranslationsGroup.Location = new Point(227, 39);
            TranslationsGroup.Name = "TranslationsGroup";
            TranslationsGroup.Size = new Size(200, 70);
            TranslationsGroup.TabIndex = 15;
            TranslationsGroup.TabStop = false;
            TranslationsGroup.Text = "Translations";
            // 
            // Translations
            // 
            Translations.BackColor = Color.FromArgb(17, 17, 17);
            Translations.BorderStyle = BorderStyle.None;
            Translations.Font = new Font("Arial", 8.25F);
            Translations.ForeColor = Color.White;
            Translations.FormattingEnabled = true;
            Translations.ItemHeight = 14;
            Translations.Location = new Point(6, 21);
            Translations.Name = "Translations";
            Translations.Size = new Size(188, 42);
            Translations.TabIndex = 12;
            // 
            // LocationsGroup
            // 
            LocationsGroup.Controls.Add(Locations);
            LocationsGroup.FlatStyle = FlatStyle.Flat;
            LocationsGroup.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LocationsGroup.ForeColor = Color.White;
            LocationsGroup.Location = new Point(433, 39);
            LocationsGroup.Name = "LocationsGroup";
            LocationsGroup.Size = new Size(200, 70);
            LocationsGroup.TabIndex = 16;
            LocationsGroup.TabStop = false;
            LocationsGroup.Text = "Locations";
            // 
            // Locations
            // 
            Locations.BackColor = Color.FromArgb(17, 17, 17);
            Locations.BorderStyle = BorderStyle.None;
            Locations.Font = new Font("Arial", 8.25F);
            Locations.ForeColor = Color.White;
            Locations.FormattingEnabled = true;
            Locations.ItemHeight = 14;
            Locations.Location = new Point(6, 21);
            Locations.Name = "Locations";
            Locations.Size = new Size(189, 42);
            Locations.TabIndex = 13;
            // 
            // ThemesGroup
            // 
            ThemesGroup.Controls.Add(Themes);
            ThemesGroup.FlatStyle = FlatStyle.Flat;
            ThemesGroup.Font = new Font("Arial", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ThemesGroup.ForeColor = Color.White;
            ThemesGroup.Location = new Point(227, 111);
            ThemesGroup.Name = "ThemesGroup";
            ThemesGroup.Size = new Size(406, 68);
            ThemesGroup.TabIndex = 17;
            ThemesGroup.TabStop = false;
            ThemesGroup.Text = "Themes";
            // 
            // Themes
            // 
            Themes.BackColor = Color.FromArgb(17, 17, 17);
            Themes.BorderStyle = BorderStyle.None;
            Themes.Font = new Font("Arial", 8.25F);
            Themes.ForeColor = Color.White;
            Themes.FormattingEnabled = true;
            Themes.ItemHeight = 14;
            Themes.Location = new Point(6, 21);
            Themes.Name = "Themes";
            Themes.Size = new Size(394, 42);
            Themes.TabIndex = 14;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(17, 17, 17);
            ClientSize = new Size(226, 207);
            Controls.Add(ThemesGroup);
            Controls.Add(LocationsGroup);
            Controls.Add(TranslationsGroup);
            Controls.Add(ResourceButton);
            Controls.Add(AboutButton);
            Controls.Add(ResourcesButton);
            Controls.Add(DiscordButton);
            Controls.Add(OpenModsButton);
            Controls.Add(ChangelogButton);
            Controls.Add(UpdateCheckButton);
            Controls.Add(InfoLabel);
            Controls.Add(InstallItems);
            Controls.Add(InstallButton);
            Controls.Add(TitleBar);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MaximumSize = new Size(644, 207);
            MinimizeBox = false;
            MinimumSize = new Size(226, 207);
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Load += MainWindow_Load;
            TitleBar.ResumeLayout(false);
            TitleBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)TitleBarIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)DiscordButton).EndInit();
            ((System.ComponentModel.ISupportInitialize)AboutButton).EndInit();
            TranslationsGroup.ResumeLayout(false);
            LocationsGroup.ResumeLayout(false);
            ThemesGroup.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel TitleBar;
        private Label TitleBarLabel;
        private PictureBox TitleBarIcon;
        private Button Exit;
        private Button InstallButton;
        private ComboBox InstallItems;
        private Label InfoLabel;
        private Button UpdateCheckButton;
        private Button ChangelogButton;
        private Button OpenModsButton;
        private PictureBox DiscordButton;
        private Button ResourcesButton;
        private ToolTip AboutPopup;
        private PictureBox AboutButton;
        private Button ResourceButton;
        private ListBox Translations;
        private ListBox Locations;
        private ListBox Themes;
        private GroupBox TranslationsGroup;
        private GroupBox LocationsGroup;
        private GroupBox ThemesGroup;
    }
}
