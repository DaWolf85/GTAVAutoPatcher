namespace AutoPatcher
{
    partial class mainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.grpSelectPatch = new System.Windows.Forms.GroupBox();
            this.rbRestore = new System.Windows.Forms.RadioButton();
            this.rbDowngrade = new System.Windows.Forms.RadioButton();
            this.btnStart = new System.Windows.Forms.Button();
            this.tbGTAVPath = new System.Windows.Forms.TextBox();
            this.btnFindGTAV = new System.Windows.Forms.Button();
            this.tbOutput = new System.Windows.Forms.TextBox();
            this.progress = new System.Windows.Forms.ProgressBar();
            this.fbFindGTAV = new System.Windows.Forms.FolderBrowserDialog();
            this.lbGTAVPath = new System.Windows.Forms.Label();
            this.grpSelectPatch.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpSelectPatch
            // 
            this.grpSelectPatch.Controls.Add(this.rbRestore);
            this.grpSelectPatch.Controls.Add(this.rbDowngrade);
            this.grpSelectPatch.Location = new System.Drawing.Point(12, 37);
            this.grpSelectPatch.Name = "grpSelectPatch";
            this.grpSelectPatch.Size = new System.Drawing.Size(111, 69);
            this.grpSelectPatch.TabIndex = 0;
            this.grpSelectPatch.TabStop = false;
            // 
            // rbRestore
            // 
            this.rbRestore.AutoSize = true;
            this.rbRestore.Location = new System.Drawing.Point(9, 39);
            this.rbRestore.Name = "rbRestore";
            this.rbRestore.Size = new System.Drawing.Size(62, 17);
            this.rbRestore.TabIndex = 1;
            this.rbRestore.Text = "Restore";
            this.rbRestore.UseVisualStyleBackColor = true;
            // 
            // rbDowngrade
            // 
            this.rbDowngrade.AutoSize = true;
            this.rbDowngrade.Location = new System.Drawing.Point(9, 16);
            this.rbDowngrade.Name = "rbDowngrade";
            this.rbDowngrade.Size = new System.Drawing.Size(80, 17);
            this.rbDowngrade.TabIndex = 0;
            this.rbDowngrade.Text = "Downgrade";
            this.rbDowngrade.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Enabled = false;
            this.btnStart.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStart.Location = new System.Drawing.Point(425, 37);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(164, 69);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            // 
            // tbGTAVPath
            // 
            this.tbGTAVPath.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.tbGTAVPath.Location = new System.Drawing.Point(12, 160);
            this.tbGTAVPath.Name = "tbGTAVPath";
            this.tbGTAVPath.Size = new System.Drawing.Size(501, 20);
            this.tbGTAVPath.TabIndex = 2;
            this.tbGTAVPath.Text = "Enter GTA V folder location or click Find";
            // 
            // btnFindGTAV
            // 
            this.btnFindGTAV.Location = new System.Drawing.Point(519, 159);
            this.btnFindGTAV.Name = "btnFindGTAV";
            this.btnFindGTAV.Size = new System.Drawing.Size(70, 21);
            this.btnFindGTAV.TabIndex = 3;
            this.btnFindGTAV.Text = "Find";
            this.btnFindGTAV.UseVisualStyleBackColor = true;
            // 
            // tbOutput
            // 
            this.tbOutput.Location = new System.Drawing.Point(12, 212);
            this.tbOutput.Multiline = true;
            this.tbOutput.Name = "tbOutput";
            this.tbOutput.ReadOnly = true;
            this.tbOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbOutput.Size = new System.Drawing.Size(577, 200);
            this.tbOutput.TabIndex = 5;
            // 
            // progress
            // 
            this.progress.Location = new System.Drawing.Point(12, 418);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(577, 20);
            this.progress.TabIndex = 6;
            // 
            // fbFindGTAV
            // 
            this.fbFindGTAV.Description = "Select the GTA V folder";
            this.fbFindGTAV.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.fbFindGTAV.ShowNewFolderButton = false;
            // 
            // lbGTAVPath
            // 
            this.lbGTAVPath.AutoSize = true;
            this.lbGTAVPath.Location = new System.Drawing.Point(12, 139);
            this.lbGTAVPath.Name = "lbGTAVPath";
            this.lbGTAVPath.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.lbGTAVPath.Size = new System.Drawing.Size(118, 18);
            this.lbGTAVPath.TabIndex = 7;
            this.lbGTAVPath.Text = "GTA V Folder Location:";
            // 
            // mainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 450);
            this.Controls.Add(this.lbGTAVPath);
            this.Controls.Add(this.progress);
            this.Controls.Add(this.tbOutput);
            this.Controls.Add(this.btnFindGTAV);
            this.Controls.Add(this.tbGTAVPath);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.grpSelectPatch);
            this.Name = "mainWindow";
            this.Text = "GTA V AutoPatcher v2.2";
            this.grpSelectPatch.ResumeLayout(false);
            this.grpSelectPatch.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox grpSelectPatch;
        private System.Windows.Forms.RadioButton rbRestore;
        private System.Windows.Forms.RadioButton rbDowngrade;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox tbGTAVPath;
        private System.Windows.Forms.Button btnFindGTAV;
        private System.Windows.Forms.TextBox tbOutput;
        private System.Windows.Forms.ProgressBar progress;
        private System.Windows.Forms.FolderBrowserDialog fbFindGTAV;
        private System.Windows.Forms.Label lbGTAVPath;
    }
}

