namespace Vidka.Components
{
    partial class CommonVideoClipCustomAudio
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.chkHasCustomAudio = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panelDragFile = new System.Windows.Forms.Panel();
            this.lblFilename = new System.Windows.Forms.Label();
            this.btnDown1 = new System.Windows.Forms.Button();
            this.btnDown01 = new System.Windows.Forms.Button();
            this.btnUp01 = new System.Windows.Forms.Button();
            this.btnUp1 = new System.Windows.Forms.Button();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnPreview = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.waveImageBox = new System.Windows.Forms.PictureBox();
            this.lblAudioProperties = new System.Windows.Forms.Label();
            this.panelDragFile.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waveImageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // chkHasCustomAudio
            // 
            this.chkHasCustomAudio.AutoSize = true;
            this.chkHasCustomAudio.Location = new System.Drawing.Point(19, 27);
            this.chkHasCustomAudio.Name = "chkHasCustomAudio";
            this.chkHasCustomAudio.Size = new System.Drawing.Size(412, 36);
            this.chkHasCustomAudio.TabIndex = 0;
            this.chkHasCustomAudio.Text = "Custom Custom Audio Track";
            this.chkHasCustomAudio.UseVisualStyleBackColor = true;
            this.chkHasCustomAudio.CheckedChanged += new System.EventHandler(this.chkHasCustomAudio_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(235, 32);
            this.label1.TabIndex = 1;
            this.label1.Text = "Custom audio file";
            // 
            // panelDragFile
            // 
            this.panelDragFile.AllowDrop = true;
            this.panelDragFile.BackColor = System.Drawing.Color.LightGray;
            this.panelDragFile.Controls.Add(this.lblFilename);
            this.panelDragFile.Location = new System.Drawing.Point(278, 78);
            this.panelDragFile.Name = "panelDragFile";
            this.panelDragFile.Size = new System.Drawing.Size(692, 82);
            this.panelDragFile.TabIndex = 2;
            this.panelDragFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.panelDragFile_DragDrop);
            this.panelDragFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.panelDragFile_DragEnter);
            this.panelDragFile.DragLeave += new System.EventHandler(this.panelDragFile_DragLeave);
            // 
            // lblFilename
            // 
            this.lblFilename.AutoSize = true;
            this.lblFilename.Location = new System.Drawing.Point(29, 23);
            this.lblFilename.Name = "lblFilename";
            this.lblFilename.Size = new System.Drawing.Size(333, 32);
            this.lblFilename.TabIndex = 1;
            this.lblFilename.Text = "Please drag audio here...";
            // 
            // btnDown1
            // 
            this.btnDown1.Location = new System.Drawing.Point(198, 22);
            this.btnDown1.Name = "btnDown1";
            this.btnDown1.Size = new System.Drawing.Size(79, 56);
            this.btnDown1.TabIndex = 6;
            this.btnDown1.Text = "<<";
            this.btnDown1.UseVisualStyleBackColor = true;
            this.btnDown1.Click += new System.EventHandler(this.btnDown1_Click);
            // 
            // btnDown01
            // 
            this.btnDown01.Location = new System.Drawing.Point(290, 22);
            this.btnDown01.Name = "btnDown01";
            this.btnDown01.Size = new System.Drawing.Size(79, 56);
            this.btnDown01.TabIndex = 6;
            this.btnDown01.Text = "<";
            this.btnDown01.UseVisualStyleBackColor = true;
            this.btnDown01.Click += new System.EventHandler(this.btnDown01_Click);
            // 
            // btnUp01
            // 
            this.btnUp01.Location = new System.Drawing.Point(690, 22);
            this.btnUp01.Name = "btnUp01";
            this.btnUp01.Size = new System.Drawing.Size(79, 56);
            this.btnUp01.TabIndex = 6;
            this.btnUp01.Text = ">";
            this.btnUp01.UseVisualStyleBackColor = true;
            this.btnUp01.Click += new System.EventHandler(this.btnUp01_Click);
            // 
            // btnUp1
            // 
            this.btnUp1.Location = new System.Drawing.Point(788, 22);
            this.btnUp1.Name = "btnUp1";
            this.btnUp1.Size = new System.Drawing.Size(79, 56);
            this.btnUp1.TabIndex = 6;
            this.btnUp1.Text = ">>";
            this.btnUp1.UseVisualStyleBackColor = true;
            this.btnUp1.Click += new System.EventHandler(this.btnUp1_Click);
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(398, 32);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(262, 38);
            this.txtOffset.TabIndex = 7;
            this.txtOffset.TextChanged += new System.EventHandler(this.txtOffset_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(160, 32);
            this.label2.TabIndex = 8;
            this.label2.Text = "Offset (sec)";
            // 
            // btnPreview
            // 
            this.btnPreview.Location = new System.Drawing.Point(873, 22);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(187, 56);
            this.btnPreview.TabIndex = 9;
            this.btnPreview.Text = "Preview...";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnPreview);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnUp1);
            this.panel1.Controls.Add(this.txtOffset);
            this.panel1.Controls.Add(this.btnDown1);
            this.panel1.Controls.Add(this.btnDown01);
            this.panel1.Controls.Add(this.btnUp01);
            this.panel1.Location = new System.Drawing.Point(0, 401);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1112, 100);
            this.panel1.TabIndex = 11;
            // 
            // waveImageBox
            // 
            this.waveImageBox.BackColor = System.Drawing.Color.Black;
            this.waveImageBox.Location = new System.Drawing.Point(19, 277);
            this.waveImageBox.Name = "waveImageBox";
            this.waveImageBox.Size = new System.Drawing.Size(1060, 98);
            this.waveImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.waveImageBox.TabIndex = 12;
            this.waveImageBox.TabStop = false;
            // 
            // lblAudioProperties
            // 
            this.lblAudioProperties.AutoSize = true;
            this.lblAudioProperties.Location = new System.Drawing.Point(13, 209);
            this.lblAudioProperties.Name = "lblAudioProperties";
            this.lblAudioProperties.Size = new System.Drawing.Size(185, 32);
            this.lblAudioProperties.TabIndex = 13;
            this.lblAudioProperties.Text = "Audio info: ---";
            // 
            // CommonVideoClipCustomAudio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblAudioProperties);
            this.Controls.Add(this.waveImageBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelDragFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkHasCustomAudio);
            this.Name = "CommonVideoClipCustomAudio";
            this.Size = new System.Drawing.Size(1115, 762);
            this.Load += new System.EventHandler(this.CommonVideoClipCustomAudio_Load);
            this.panelDragFile.ResumeLayout(false);
            this.panelDragFile.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.waveImageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox chkHasCustomAudio;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panelDragFile;
        private System.Windows.Forms.Label lblFilename;
        private System.Windows.Forms.Button btnDown1;
        private System.Windows.Forms.Button btnDown01;
        private System.Windows.Forms.Button btnUp01;
        private System.Windows.Forms.Button btnUp1;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox waveImageBox;
        private System.Windows.Forms.Label lblAudioProperties;
    }
}
