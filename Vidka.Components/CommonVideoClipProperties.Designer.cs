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
			// CommonVideoClipProperties
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.btnFill);
			this.Controls.Add(this.btnFlipV);
			this.Controls.Add(this.btnFlipH);
			this.Controls.Add(this.btnRotate180);
			this.Controls.Add(this.txtPostOp);
			this.Name = "CommonVideoClipProperties";
			this.Size = new System.Drawing.Size(1216, 705);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox txtPostOp;
		private System.Windows.Forms.Button btnRotate180;
		private System.Windows.Forms.Button btnFlipH;
		private System.Windows.Forms.Button btnFlipV;
		private System.Windows.Forms.Button btnFill;
	}
}
