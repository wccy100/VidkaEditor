namespace Vidka.Components
{
    partial class CommonAudioClipProperties
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
            this.txtPostOp = new System.Windows.Forms.RichTextBox();
            this.btnFadeOut5 = new System.Windows.Forms.Button();
            this.btnFadeOut10 = new System.Windows.Forms.Button();
            this.btnFadeIn5 = new System.Windows.Forms.Button();
            this.btnFadeIn10 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtPostOp
            // 
            this.txtPostOp.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPostOp.Location = new System.Drawing.Point(19, 19);
            this.txtPostOp.Name = "txtPostOp";
            this.txtPostOp.Size = new System.Drawing.Size(779, 527);
            this.txtPostOp.TabIndex = 0;
            this.txtPostOp.Text = "";
            this.txtPostOp.TextChanged += new System.EventHandler(this.txtPostOp_TextChanged);
            // 
            // btnFadeOut5
            // 
            this.btnFadeOut5.Location = new System.Drawing.Point(805, 19);
            this.btnFadeOut5.Name = "btnFadeOut5";
            this.btnFadeOut5.Size = new System.Drawing.Size(384, 58);
            this.btnFadeOut5.TabIndex = 1;
            this.btnFadeOut5.Text = "Fade out (5 frames)";
            this.btnFadeOut5.UseVisualStyleBackColor = true;
            this.btnFadeOut5.Click += new System.EventHandler(this.btnFadeOut5_Click);
            // 
            // btnFadeOut10
            // 
            this.btnFadeOut10.Location = new System.Drawing.Point(805, 84);
            this.btnFadeOut10.Name = "btnFadeOut10";
            this.btnFadeOut10.Size = new System.Drawing.Size(384, 60);
            this.btnFadeOut10.TabIndex = 2;
            this.btnFadeOut10.Text = "Fade out (10 frames)";
            this.btnFadeOut10.UseVisualStyleBackColor = true;
            this.btnFadeOut10.Click += new System.EventHandler(this.btnFadeOut10_Click);
            // 
            // btnFadeIn5
            // 
            this.btnFadeIn5.Location = new System.Drawing.Point(805, 150);
            this.btnFadeIn5.Name = "btnFadeIn5";
            this.btnFadeIn5.Size = new System.Drawing.Size(384, 60);
            this.btnFadeIn5.TabIndex = 2;
            this.btnFadeIn5.Text = "Fade in (5 frames)";
            this.btnFadeIn5.UseVisualStyleBackColor = true;
            this.btnFadeIn5.Click += new System.EventHandler(this.btnFadeIn5_Click);
            // 
            // btnFadeIn10
            // 
            this.btnFadeIn10.Location = new System.Drawing.Point(805, 216);
            this.btnFadeIn10.Name = "btnFadeIn10";
            this.btnFadeIn10.Size = new System.Drawing.Size(384, 60);
            this.btnFadeIn10.TabIndex = 2;
            this.btnFadeIn10.Text = "Fade in (10 frames)";
            this.btnFadeIn10.UseVisualStyleBackColor = true;
            this.btnFadeIn10.Click += new System.EventHandler(this.btnFadeIn10_Click);
            // 
            // CommonAudioClipProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnFadeIn10);
            this.Controls.Add(this.btnFadeIn5);
            this.Controls.Add(this.btnFadeOut10);
            this.Controls.Add(this.btnFadeOut5);
            this.Controls.Add(this.txtPostOp);
            this.Name = "CommonAudioClipProperties";
            this.Size = new System.Drawing.Size(1216, 705);
            this.Load += new System.EventHandler(this.CommonAudioClipProperties_Load);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox txtPostOp;
		private System.Windows.Forms.Button btnFadeOut5;
		private System.Windows.Forms.Button btnFadeOut10;
		private System.Windows.Forms.Button btnFadeIn5;
		private System.Windows.Forms.Button btnFadeIn10;
	}
}
