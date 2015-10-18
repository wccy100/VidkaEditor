namespace Vidka.Components
{
	partial class VideoClipPropertiesWindow
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
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabCommon = new System.Windows.Forms.TabPage();
			this.commonVideoClipProperties = new Vidka.Components.CommonVideoClipProperties();
			this.tabCustomAudio = new System.Windows.Forms.TabPage();
			this.commonVideoClipCustomAudio = new Vidka.Components.CommonVideoClipCustomAudio();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.tabControl.SuspendLayout();
			this.tabCommon.SuspendLayout();
			this.tabCustomAudio.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabCommon);
			this.tabControl.Controls.Add(this.tabCustomAudio);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(1369, 844);
			this.tabControl.TabIndex = 0;
			// 
			// tabCommon
			// 
			this.tabCommon.Controls.Add(this.commonVideoClipProperties);
			this.tabCommon.Location = new System.Drawing.Point(4, 40);
			this.tabCommon.Name = "tabCommon";
			this.tabCommon.Padding = new System.Windows.Forms.Padding(3);
			this.tabCommon.Size = new System.Drawing.Size(1361, 800);
			this.tabCommon.TabIndex = 0;
			this.tabCommon.Text = "Common";
			this.tabCommon.UseVisualStyleBackColor = true;
			// 
			// commonVideoClipProperties
			// 
			this.commonVideoClipProperties.Cursor = System.Windows.Forms.Cursors.Default;
			this.commonVideoClipProperties.Location = new System.Drawing.Point(3, 16);
			this.commonVideoClipProperties.Name = "commonVideoClipProperties";
			this.commonVideoClipProperties.Size = new System.Drawing.Size(1355, 769);
			this.commonVideoClipProperties.TabIndex = 2;
			// 
			// tabCustomAudio
			// 
			this.tabCustomAudio.Controls.Add(this.commonVideoClipCustomAudio);
			this.tabCustomAudio.Location = new System.Drawing.Point(4, 40);
			this.tabCustomAudio.Name = "tabCustomAudio";
			this.tabCustomAudio.Size = new System.Drawing.Size(1361, 800);
			this.tabCustomAudio.TabIndex = 1;
			this.tabCustomAudio.Text = "Audio";
			this.tabCustomAudio.UseVisualStyleBackColor = true;
			// 
			// commonVideoClipCustomAudio
			// 
			this.commonVideoClipCustomAudio.AutoSize = true;
			this.commonVideoClipCustomAudio.Dock = System.Windows.Forms.DockStyle.Fill;
			this.commonVideoClipCustomAudio.Location = new System.Drawing.Point(0, 0);
			this.commonVideoClipCustomAudio.Name = "commonVideoClipCustomAudio";
			this.commonVideoClipCustomAudio.Size = new System.Drawing.Size(1361, 800);
			this.commonVideoClipCustomAudio.TabIndex = 0;
			// 
			// btnOk
			// 
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Location = new System.Drawing.Point(875, 876);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(232, 64);
			this.btnOk.TabIndex = 1;
			this.btnOk.Text = "OK";
			this.btnOk.UseVisualStyleBackColor = true;
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(1126, 876);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(232, 64);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			// 
			// VideoClipPropertiesWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1393, 952);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.tabControl);
			this.Name = "VideoClipPropertiesWindow";
			this.Text = "VideoClipPropertiesWindow";
			this.Load += new System.EventHandler(this.VideoClipPropertiesWindow_Load);
			this.tabControl.ResumeLayout(false);
			this.tabCommon.ResumeLayout(false);
			this.tabCustomAudio.ResumeLayout(false);
			this.tabCustomAudio.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabCommon;
		private Components.CommonVideoClipProperties commonVideoClipProperties;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TabPage tabCustomAudio;
		private CommonVideoClipCustomAudio commonVideoClipCustomAudio;
	}
}