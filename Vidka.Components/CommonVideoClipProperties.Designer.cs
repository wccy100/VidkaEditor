namespace Vidka.Components
{
	partial class CommonVideoClipProperties
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
            this.btnRotate180 = new System.Windows.Forms.Button();
            this.btnFlipH = new System.Windows.Forms.Button();
            this.btnFlipV = new System.Windows.Forms.Button();
            this.btnFill = new System.Windows.Forms.Button();
            this.chkIsPixelTypeStandard = new System.Windows.Forms.CheckBox();
            this.btnFadeOut5 = new System.Windows.Forms.Button();
            this.btnFadeIn5 = new System.Windows.Forms.Button();
            this.btnFadeoutAudio5 = new System.Windows.Forms.Button();
            this.chkIsRenderBreakupPoint = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLabel = new System.Windows.Forms.TextBox();
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
            // btnRotate180
            // 
            this.btnRotate180.Location = new System.Drawing.Point(805, 19);
            this.btnRotate180.Name = "btnRotate180";
            this.btnRotate180.Size = new System.Drawing.Size(384, 58);
            this.btnRotate180.TabIndex = 1;
            this.btnRotate180.Text = "Rotate 180";
            this.btnRotate180.UseVisualStyleBackColor = true;
            this.btnRotate180.Click += new System.EventHandler(this.btnRotate180_Click);
            // 
            // btnFlipH
            // 
            this.btnFlipH.Location = new System.Drawing.Point(805, 84);
            this.btnFlipH.Name = "btnFlipH";
            this.btnFlipH.Size = new System.Drawing.Size(384, 60);
            this.btnFlipH.TabIndex = 2;
            this.btnFlipH.Text = "Flip Horizontal";
            this.btnFlipH.UseVisualStyleBackColor = true;
            this.btnFlipH.Click += new System.EventHandler(this.btnFlipH_Click);
            // 
            // btnFlipV
            // 
            this.btnFlipV.Location = new System.Drawing.Point(805, 150);
            this.btnFlipV.Name = "btnFlipV";
            this.btnFlipV.Size = new System.Drawing.Size(384, 60);
            this.btnFlipV.TabIndex = 2;
            this.btnFlipV.Text = "Flip Vertical";
            this.btnFlipV.UseVisualStyleBackColor = true;
            this.btnFlipV.Click += new System.EventHandler(this.btnFlipV_Click);
            // 
            // btnFill
            // 
            this.btnFill.Location = new System.Drawing.Point(805, 216);
            this.btnFill.Name = "btnFill";
            this.btnFill.Size = new System.Drawing.Size(384, 60);
            this.btnFill.TabIndex = 2;
            this.btnFill.Text = "Fill";
            this.btnFill.UseVisualStyleBackColor = true;
            this.btnFill.Click += new System.EventHandler(this.btnFill_Click);
            // 
            // chkIsPixelTypeStandard
            // 
            this.chkIsPixelTypeStandard.AutoSize = true;
            this.chkIsPixelTypeStandard.Location = new System.Drawing.Point(19, 572);
            this.chkIsPixelTypeStandard.Name = "chkIsPixelTypeStandard";
            this.chkIsPixelTypeStandard.Size = new System.Drawing.Size(320, 36);
            this.chkIsPixelTypeStandard.TabIndex = 3;
            this.chkIsPixelTypeStandard.Text = "is pixel type standard";
            this.chkIsPixelTypeStandard.UseVisualStyleBackColor = true;
            this.chkIsPixelTypeStandard.CheckedChanged += new System.EventHandler(this.chkIsPixelTypeStandard_CheckedChanged);
            // 
            // btnFadeOut5
            // 
            this.btnFadeOut5.Location = new System.Drawing.Point(804, 346);
            this.btnFadeOut5.Name = "btnFadeOut5";
            this.btnFadeOut5.Size = new System.Drawing.Size(384, 58);
            this.btnFadeOut5.TabIndex = 4;
            this.btnFadeOut5.Text = "Fade out (5 frames)";
            this.btnFadeOut5.UseVisualStyleBackColor = true;
            this.btnFadeOut5.Click += new System.EventHandler(this.btnFadeOut5_Click);
            // 
            // btnFadeIn5
            // 
            this.btnFadeIn5.Location = new System.Drawing.Point(805, 282);
            this.btnFadeIn5.Name = "btnFadeIn5";
            this.btnFadeIn5.Size = new System.Drawing.Size(384, 58);
            this.btnFadeIn5.TabIndex = 5;
            this.btnFadeIn5.Text = "Fade in (5 frames)";
            this.btnFadeIn5.UseVisualStyleBackColor = true;
            this.btnFadeIn5.Click += new System.EventHandler(this.btnFadeIn5_Click);
            // 
            // btnFadeoutAudio5
            // 
            this.btnFadeoutAudio5.Location = new System.Drawing.Point(805, 410);
            this.btnFadeoutAudio5.Name = "btnFadeoutAudio5";
            this.btnFadeoutAudio5.Size = new System.Drawing.Size(384, 58);
            this.btnFadeoutAudio5.TabIndex = 6;
            this.btnFadeoutAudio5.Text = "Fade out AUDIO (5 frames)";
            this.btnFadeoutAudio5.UseVisualStyleBackColor = true;
            this.btnFadeoutAudio5.Click += new System.EventHandler(this.btnFadeoutAudio5_Click);
            // 
            // chkIsRenderBreakupPoint
            // 
            this.chkIsRenderBreakupPoint.AutoSize = true;
            this.chkIsRenderBreakupPoint.Location = new System.Drawing.Point(19, 627);
            this.chkIsRenderBreakupPoint.Name = "chkIsRenderBreakupPoint";
            this.chkIsRenderBreakupPoint.Size = new System.Drawing.Size(343, 36);
            this.chkIsRenderBreakupPoint.TabIndex = 7;
            this.chkIsRenderBreakupPoint.Text = "is render breakup point";
            this.chkIsRenderBreakupPoint.UseVisualStyleBackColor = true;
            this.chkIsRenderBreakupPoint.CheckedChanged += new System.EventHandler(this.chkIsRenderBreakupPoint_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 683);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 32);
            this.label1.TabIndex = 8;
            this.label1.Text = "Label";
            // 
            // txtLabel
            // 
            this.txtLabel.Location = new System.Drawing.Point(105, 680);
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.Size = new System.Drawing.Size(693, 38);
            this.txtLabel.TabIndex = 9;
            this.txtLabel.TextChanged += new System.EventHandler(this.txtLabel_TextChanged);
            // 
            // CommonVideoClipProperties
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkIsRenderBreakupPoint);
            this.Controls.Add(this.btnFadeoutAudio5);
            this.Controls.Add(this.btnFadeIn5);
            this.Controls.Add(this.btnFadeOut5);
            this.Controls.Add(this.chkIsPixelTypeStandard);
            this.Controls.Add(this.btnFill);
            this.Controls.Add(this.btnFlipV);
            this.Controls.Add(this.btnFlipH);
            this.Controls.Add(this.btnRotate180);
            this.Controls.Add(this.txtPostOp);
            this.Name = "CommonVideoClipProperties";
            this.Size = new System.Drawing.Size(1216, 781);
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox txtPostOp;
		private System.Windows.Forms.Button btnRotate180;
		private System.Windows.Forms.Button btnFlipH;
		private System.Windows.Forms.Button btnFlipV;
		private System.Windows.Forms.Button btnFill;
        private System.Windows.Forms.CheckBox chkIsPixelTypeStandard;
        private System.Windows.Forms.Button btnFadeOut5;
        private System.Windows.Forms.Button btnFadeIn5;
        private System.Windows.Forms.Button btnFadeoutAudio5;
        private System.Windows.Forms.CheckBox chkIsRenderBreakupPoint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLabel;
	}
}
