namespace Client
{
    partial class Client
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
            this.outputTextBlock = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.discoverServerButton = new System.Windows.Forms.Button();
            this.getDataButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // outputTextBlock
            // 
            this.outputTextBlock.Location = new System.Drawing.Point(76, 241);
            this.outputTextBlock.Multiline = true;
            this.outputTextBlock.Name = "outputTextBlock";
            this.outputTextBlock.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBlock.Size = new System.Drawing.Size(712, 197);
            this.outputTextBlock.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 244);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Output:";
            // 
            // discoverServerButton
            // 
            this.discoverServerButton.Location = new System.Drawing.Point(177, 106);
            this.discoverServerButton.Name = "discoverServerButton";
            this.discoverServerButton.Size = new System.Drawing.Size(96, 23);
            this.discoverServerButton.TabIndex = 2;
            this.discoverServerButton.Text = "Discover Server";
            this.discoverServerButton.UseVisualStyleBackColor = true;
            this.discoverServerButton.Click += new System.EventHandler(this.discoverServerButton_Click);
            // 
            // getDataButton
            // 
            this.getDataButton.Enabled = false;
            this.getDataButton.Location = new System.Drawing.Point(279, 106);
            this.getDataButton.Name = "getDataButton";
            this.getDataButton.Size = new System.Drawing.Size(96, 23);
            this.getDataButton.TabIndex = 3;
            this.getDataButton.Text = "Get Data";
            this.getDataButton.UseVisualStyleBackColor = true;
            this.getDataButton.Click += new System.EventHandler(this.getDataButton_Click);
            // 
            // Client
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.getDataButton);
            this.Controls.Add(this.discoverServerButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.outputTextBlock);
            this.Name = "Client";
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox outputTextBlock;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button discoverServerButton;
        private System.Windows.Forms.Button getDataButton;
    }
}

