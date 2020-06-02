﻿namespace Dull.WinformApp
{
    partial class Form1
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.rtbDownloadingInfo = new System.Windows.Forms.RichTextBox();
            this.tbOutputDirSelected = new System.Windows.Forms.TextBox();
            this.tbFileSelected = new System.Windows.Forms.TextBox();
            this.btnDownloadFiles = new System.Windows.Forms.Button();
            this.btnSelectOutputDir = new System.Windows.Forms.Button();
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rtbDownloadingInfo);
            this.panel1.Controls.Add(this.tbOutputDirSelected);
            this.panel1.Controls.Add(this.tbFileSelected);
            this.panel1.Controls.Add(this.btnDownloadFiles);
            this.panel1.Controls.Add(this.btnSelectOutputDir);
            this.panel1.Controls.Add(this.btnSelectFile);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(800, 239);
            this.panel1.TabIndex = 0;
            // 
            // rtbDownloadingInfo
            // 
            this.rtbDownloadingInfo.BackColor = System.Drawing.SystemColors.WindowText;
            this.rtbDownloadingInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbDownloadingInfo.ForeColor = System.Drawing.SystemColors.Window;
            this.rtbDownloadingInfo.Location = new System.Drawing.Point(103, 72);
            this.rtbDownloadingInfo.Name = "rtbDownloadingInfo";
            this.rtbDownloadingInfo.ReadOnly = true;
            this.rtbDownloadingInfo.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbDownloadingInfo.Size = new System.Drawing.Size(685, 96);
            this.rtbDownloadingInfo.TabIndex = 4;
            this.rtbDownloadingInfo.Text = "1、阻塞UI线程，请勿在下载文件过程中点击界面！！！\n2、点击【下载文件】后，会弹出“选择文件夹”对话框，请直接点【选择文件夹】！！！\n";
            // 
            // tbOutputDirSelected
            // 
            this.tbOutputDirSelected.Location = new System.Drawing.Point(103, 41);
            this.tbOutputDirSelected.Name = "tbOutputDirSelected";
            this.tbOutputDirSelected.Size = new System.Drawing.Size(685, 21);
            this.tbOutputDirSelected.TabIndex = 3;
            // 
            // tbFileSelected
            // 
            this.tbFileSelected.Location = new System.Drawing.Point(103, 14);
            this.tbFileSelected.Name = "tbFileSelected";
            this.tbFileSelected.Size = new System.Drawing.Size(685, 21);
            this.tbFileSelected.TabIndex = 3;
            // 
            // btnDownloadFiles
            // 
            this.btnDownloadFiles.Location = new System.Drawing.Point(12, 70);
            this.btnDownloadFiles.Name = "btnDownloadFiles";
            this.btnDownloadFiles.Size = new System.Drawing.Size(85, 23);
            this.btnDownloadFiles.TabIndex = 2;
            this.btnDownloadFiles.Text = "下载文件";
            this.btnDownloadFiles.UseVisualStyleBackColor = true;
            this.btnDownloadFiles.Click += new System.EventHandler(this.btnDownloadFiles_Click);
            // 
            // btnSelectOutputDir
            // 
            this.btnSelectOutputDir.Location = new System.Drawing.Point(12, 41);
            this.btnSelectOutputDir.Name = "btnSelectOutputDir";
            this.btnSelectOutputDir.Size = new System.Drawing.Size(85, 23);
            this.btnSelectOutputDir.TabIndex = 2;
            this.btnSelectOutputDir.Text = "输出目录";
            this.btnSelectOutputDir.UseVisualStyleBackColor = true;
            this.btnSelectOutputDir.Click += new System.EventHandler(this.btnSelectOutputDir_Click);
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(85, 23);
            this.btnSelectFile.TabIndex = 2;
            this.btnSelectFile.Text = "选择文件";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Location = new System.Drawing.Point(713, 210);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "另存";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(601, 210);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "导航";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 239);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(800, 3);
            this.splitter1.TabIndex = 1;
            this.splitter1.TabStop = false;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 242);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.ScriptErrorsSuppressed = true;
            this.webBrowser1.Size = new System.Drawing.Size(800, 208);
            this.webBrowser1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.RichTextBox rtbDownloadingInfo;
        private System.Windows.Forms.TextBox tbOutputDirSelected;
        private System.Windows.Forms.TextBox tbFileSelected;
        private System.Windows.Forms.Button btnDownloadFiles;
        private System.Windows.Forms.Button btnSelectOutputDir;
        private System.Windows.Forms.Button btnSelectFile;
    }
}

