namespace Vidka.Components
{
	partial class SimpleTextSettings
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
			this.txt = new System.Windows.Forms.RichTextBox();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.btnBackground = new System.Windows.Forms.Button();
			this.pnlColor = new System.Windows.Forms.Panel();
			this.btnFontColor = new System.Windows.Forms.Button();
			this.pnlFontColor = new System.Windows.Forms.Panel();
			this.colorDialog2 = new System.Windows.Forms.ColorDialog();
			this.label1 = new System.Windows.Forms.Label();
			this.ddlFontSize = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// txt
			// 
			this.txt.Location = new System.Drawing.Point(16, 20);
			this.txt.Name = "txt";
			this.txt.Size = new System.Drawing.Size(850, 505);
			this.txt.TabIndex = 0;
			this.txt.Text = "";
			this.txt.TextChanged += new System.EventHandler(this.txt_TextChanged);
			// 
			// btnBackground
			// 
			this.btnBackground.Location = new System.Drawing.Point(16, 544);
			this.btnBackground.Name = "btnBackground";
			this.btnBackground.Size = new System.Drawing.Size(294, 65);
			this.btnBackground.TabIndex = 1;
			this.btnBackground.Text = "Background Color";
			this.btnBackground.UseVisualStyleBackColor = true;
			this.btnBackground.Click += new System.EventHandler(this.btnBackground_Click);
			// 
			// pnlColor
			// 
			this.pnlColor.BackColor = System.Drawing.Color.Black;
			this.pnlColor.Location = new System.Drawing.Point(326, 544);
			this.pnlColor.Name = "pnlColor";
			this.pnlColor.Size = new System.Drawing.Size(165, 65);
			this.pnlColor.TabIndex = 2;
			// 
			// btnFontColor
			// 
			this.btnFontColor.Location = new System.Drawing.Point(16, 615);
			this.btnFontColor.Name = "btnFontColor";
			this.btnFontColor.Size = new System.Drawing.Size(294, 65);
			this.btnFontColor.TabIndex = 3;
			this.btnFontColor.Text = "Font Color";
			this.btnFontColor.UseVisualStyleBackColor = true;
			this.btnFontColor.Click += new System.EventHandler(this.btnFontColor_Click);
			// 
			// pnlFontColor
			// 
			this.pnlFontColor.BackColor = System.Drawing.Color.White;
			this.pnlFontColor.Location = new System.Drawing.Point(326, 615);
			this.pnlFontColor.Name = "pnlFontColor";
			this.pnlFontColor.Size = new System.Drawing.Size(165, 65);
			this.pnlFontColor.TabIndex = 2;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(511, 632);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(135, 32);
			this.label1.TabIndex = 4;
			this.label1.Text = "Font Size";
			// 
			// ddlFontSize
			// 
			this.ddlFontSize.FormattingEnabled = true;
			this.ddlFontSize.Items.AddRange(new object[] {
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "22",
            "24",
            "26",
            "28",
            "30",
            "32",
            "34",
            "36",
            "38",
            "40",
            "45",
            "50",
            "55",
            "60",
            "65",
            "70"});
			this.ddlFontSize.Location = new System.Drawing.Point(652, 629);
			this.ddlFontSize.Name = "ddlFontSize";
			this.ddlFontSize.Size = new System.Drawing.Size(154, 39);
			this.ddlFontSize.TabIndex = 5;
			this.ddlFontSize.SelectedIndexChanged += new System.EventHandler(this.ddlFontSize_SelectedIndexChanged);
			// 
			// SimpleTextSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 31F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.ddlFontSize);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.btnFontColor);
			this.Controls.Add(this.pnlFontColor);
			this.Controls.Add(this.pnlColor);
			this.Controls.Add(this.btnBackground);
			this.Controls.Add(this.txt);
			this.Name = "SimpleTextSettings";
			this.Size = new System.Drawing.Size(889, 718);
			this.Load += new System.EventHandler(this.SimpleTextSettings_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox txt;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.Button btnBackground;
		private System.Windows.Forms.Panel pnlColor;
		private System.Windows.Forms.Button btnFontColor;
		private System.Windows.Forms.Panel pnlFontColor;
		private System.Windows.Forms.ColorDialog colorDialog2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox ddlFontSize;
	}
}
