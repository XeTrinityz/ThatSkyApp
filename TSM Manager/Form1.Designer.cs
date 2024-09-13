using System.Drawing.Drawing2D;

namespace TSM_Manager
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button installVcRedistButton;
        private System.Windows.Forms.Button installModButton;
        private System.Windows.Forms.Button uninstallModButton;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.RichTextBox logTextBox;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            installVcRedistButton = new Button();
            installModButton = new Button();
            uninstallModButton = new Button();
            titleLabel = new Label();
            ExitButton = new PictureBox();
            panel1 = new Panel();
            installSMLButton = new Button();
            uninstallSMLButton = new Button();
            logTextBox = new RichTextBox();
            ((System.ComponentModel.ISupportInitialize)ExitButton).BeginInit();
            SuspendLayout();
            // 
            // installVcRedistButton
            // 
            installVcRedistButton.BackColor = Color.FromArgb(45, 45, 48);
            installVcRedistButton.Cursor = Cursors.Hand;
            installVcRedistButton.FlatAppearance.BorderSize = 0;
            installVcRedistButton.FlatStyle = FlatStyle.Flat;
            installVcRedistButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            installVcRedistButton.ForeColor = Color.White;
            installVcRedistButton.Location = new Point(159, 201);
            installVcRedistButton.Name = "installVcRedistButton";
            installVcRedistButton.Padding = new Padding(10);
            installVcRedistButton.Size = new Size(476, 50);
            installVcRedistButton.TabIndex = 7;
            installVcRedistButton.Text = "Install VC Redist";
            installVcRedistButton.UseVisualStyleBackColor = false;
            installVcRedistButton.Click += installVcRedistButton_Click;
            // 
            // installModButton
            // 
            installModButton.BackColor = Color.FromArgb(45, 45, 48);
            installModButton.Cursor = Cursors.Hand;
            installModButton.FlatAppearance.BorderSize = 0;
            installModButton.FlatStyle = FlatStyle.Flat;
            installModButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            installModButton.ForeColor = Color.White;
            installModButton.Location = new Point(159, 134);
            installModButton.Name = "installModButton";
            installModButton.Padding = new Padding(10);
            installModButton.Size = new Size(235, 50);
            installModButton.TabIndex = 8;
            installModButton.Text = "Install Mod";
            installModButton.UseVisualStyleBackColor = false;
            installModButton.Click += installModButton_Click;
            // 
            // uninstallModButton
            // 
            uninstallModButton.BackColor = Color.FromArgb(45, 45, 48);
            uninstallModButton.Cursor = Cursors.Hand;
            uninstallModButton.FlatAppearance.BorderSize = 0;
            uninstallModButton.FlatStyle = FlatStyle.Flat;
            uninstallModButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            uninstallModButton.ForeColor = Color.White;
            uninstallModButton.Location = new Point(400, 134);
            uninstallModButton.Name = "uninstallModButton";
            uninstallModButton.Padding = new Padding(10);
            uninstallModButton.Size = new Size(235, 50);
            uninstallModButton.TabIndex = 9;
            uninstallModButton.Text = "Uninstall Mod";
            uninstallModButton.UseVisualStyleBackColor = false;
            uninstallModButton.Click += uninstallModButton_Click;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new Font("Segoe UI", 36F, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(230, 31);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new Size(341, 65);
            titleLabel.TabIndex = 6;
            titleLabel.Text = "That Sky Mod";
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ExitButton
            // 
            ExitButton.Cursor = Cursors.Hand;
            ExitButton.Image = (Image)resources.GetObject("ExitButton.Image");
            ExitButton.Location = new Point(749, 12);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new Size(39, 35);
            ExitButton.SizeMode = PictureBoxSizeMode.Zoom;
            ExitButton.TabIndex = 5;
            ExitButton.TabStop = false;
            ExitButton.Click += ExitButton_Click;
            // 
            // panel1
            // 
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Location = new Point(260, 277);
            panel1.Name = "panel1";
            panel1.Size = new Size(300, 2);
            panel1.TabIndex = 2;
            // 
            // installSMLButton
            // 
            installSMLButton.BackColor = Color.FromArgb(45, 45, 48);
            installSMLButton.Cursor = Cursors.Hand;
            installSMLButton.FlatAppearance.BorderSize = 0;
            installSMLButton.FlatStyle = FlatStyle.Flat;
            installSMLButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            installSMLButton.ForeColor = Color.White;
            installSMLButton.Location = new Point(159, 306);
            installSMLButton.Name = "installSMLButton";
            installSMLButton.Padding = new Padding(10);
            installSMLButton.Size = new Size(235, 50);
            installSMLButton.TabIndex = 1;
            installSMLButton.Text = "Install SML";
            installSMLButton.UseVisualStyleBackColor = false;
            installSMLButton.Click += installSMLButton_Click;
            // 
            // uninstallSMLButton
            // 
            uninstallSMLButton.BackColor = Color.FromArgb(45, 45, 48);
            uninstallSMLButton.Cursor = Cursors.Hand;
            uninstallSMLButton.FlatAppearance.BorderSize = 0;
            uninstallSMLButton.FlatStyle = FlatStyle.Flat;
            uninstallSMLButton.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            uninstallSMLButton.ForeColor = Color.White;
            uninstallSMLButton.Location = new Point(400, 306);
            uninstallSMLButton.Name = "uninstallSMLButton";
            uninstallSMLButton.Padding = new Padding(10);
            uninstallSMLButton.Size = new Size(235, 50);
            uninstallSMLButton.TabIndex = 0;
            uninstallSMLButton.Text = "Uninstall SML";
            uninstallSMLButton.UseVisualStyleBackColor = false;
            uninstallSMLButton.Click += uninstallSMLButton_Click;
            // 
            // logTextBox
            // 
            logTextBox.BackColor = Color.FromArgb(29, 29, 31);
            logTextBox.BorderStyle = BorderStyle.None;
            logTextBox.Font = new Font("Segoe UI", 12F);
            logTextBox.ForeColor = Color.LimeGreen;
            logTextBox.Location = new Point(50, 381);
            logTextBox.Name = "logTextBox";
            logTextBox.ReadOnly = true;
            logTextBox.ScrollBars = RichTextBoxScrollBars.None;
            logTextBox.Size = new Size(700, 138);
            logTextBox.TabIndex = 0;
            logTextBox.TabStop = false;
            logTextBox.Text = "";
            // 
            // Form1
            // 
            BackColor = Color.FromArgb(29, 29, 31);
            ClientSize = new Size(800, 553);
            ControlBox = false;
            Controls.Add(logTextBox);
            Controls.Add(uninstallSMLButton);
            Controls.Add(installSMLButton);
            Controls.Add(panel1);
            Controls.Add(ExitButton);
            Controls.Add(titleLabel);
            Controls.Add(installVcRedistButton);
            Controls.Add(installModButton);
            Controls.Add(uninstallModButton);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "That Sky Mod";
            ((System.ComponentModel.ISupportInitialize)ExitButton).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private Region CreateRoundedRegion(Control control)
        {
            GraphicsPath path = new GraphicsPath();
            path.AddArc(new Rectangle(0, 0, 20, 20), 180, 90);
            path.AddArc(new Rectangle(control.Width - 20, 0, 20, 20), 270, 90);
            path.AddArc(new Rectangle(control.Width - 20, control.Height - 20, 20, 20), 0, 90);
            path.AddArc(new Rectangle(0, control.Height - 20, 20, 20), 90, 90);
            path.CloseFigure();
            return new Region(path);
        }

        #endregion

        private PictureBox ExitButton;
        private Panel panel1;
        private Button installSMLButton;
        private Button uninstallSMLButton;
    }
}