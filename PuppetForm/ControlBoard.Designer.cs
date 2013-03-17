namespace PuppetForm
{
    partial class ControlBoard
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
            this.createClientButton = new System.Windows.Forms.Button();
            this.clientNameTextBox = new System.Windows.Forms.TextBox();
            this.clientPortTextBox = new System.Windows.Forms.TextBox();
            this.startMetadataServersButton = new System.Windows.Forms.Button();
            this.dataServerIdTextBox = new System.Windows.Forms.TextBox();
            this.createDataServerButton = new System.Windows.Forms.Button();
            this.dataServerPortTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // createClientButton
            // 
            this.createClientButton.Location = new System.Drawing.Point(197, 29);
            this.createClientButton.Name = "createClientButton";
            this.createClientButton.Size = new System.Drawing.Size(75, 23);
            this.createClientButton.TabIndex = 0;
            this.createClientButton.Text = "createClient";
            this.createClientButton.UseVisualStyleBackColor = true;
            this.createClientButton.Click += new System.EventHandler(this.createClientButton_Click);
            // 
            // clientNameTextBox
            // 
            this.clientNameTextBox.Location = new System.Drawing.Point(23, 28);
            this.clientNameTextBox.Name = "clientNameTextBox";
            this.clientNameTextBox.Size = new System.Drawing.Size(100, 20);
            this.clientNameTextBox.TabIndex = 1;
            this.clientNameTextBox.Text = "c1";
            // 
            // clientPortTextBox
            // 
            this.clientPortTextBox.Location = new System.Drawing.Point(129, 28);
            this.clientPortTextBox.Name = "clientPortTextBox";
            this.clientPortTextBox.Size = new System.Drawing.Size(62, 20);
            this.clientPortTextBox.TabIndex = 2;
            this.clientPortTextBox.Text = "8085";
            // 
            // startMetadataServersButton
            // 
            this.startMetadataServersButton.Location = new System.Drawing.Point(129, 70);
            this.startMetadataServersButton.Name = "startMetadataServersButton";
            this.startMetadataServersButton.Size = new System.Drawing.Size(143, 23);
            this.startMetadataServersButton.TabIndex = 3;
            this.startMetadataServersButton.Text = "startMetadataServers";
            this.startMetadataServersButton.UseVisualStyleBackColor = true;
            this.startMetadataServersButton.Click += new System.EventHandler(this.startMetadataServersButton_Click);
            // 
            // dataServerIdTextBox
            // 
            this.dataServerIdTextBox.Location = new System.Drawing.Point(52, 120);
            this.dataServerIdTextBox.Name = "dataServerIdTextBox";
            this.dataServerIdTextBox.Size = new System.Drawing.Size(62, 20);
            this.dataServerIdTextBox.TabIndex = 4;
            this.dataServerIdTextBox.Text = "0";
            // 
            // createDataServerButton
            // 
            this.createDataServerButton.Location = new System.Drawing.Point(197, 117);
            this.createDataServerButton.Name = "createDataServerButton";
            this.createDataServerButton.Size = new System.Drawing.Size(75, 23);
            this.createDataServerButton.TabIndex = 5;
            this.createDataServerButton.Text = "createDS";
            this.createDataServerButton.UseVisualStyleBackColor = true;
            this.createDataServerButton.Click += new System.EventHandler(this.createDataServerButton_Click);
            // 
            // dataServerPortTextBox
            // 
            this.dataServerPortTextBox.Location = new System.Drawing.Point(129, 120);
            this.dataServerPortTextBox.Name = "dataServerPortTextBox";
            this.dataServerPortTextBox.Size = new System.Drawing.Size(62, 20);
            this.dataServerPortTextBox.TabIndex = 6;
            this.dataServerPortTextBox.Text = "8000";
            // 
            // ControlBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 386);
            this.Controls.Add(this.dataServerPortTextBox);
            this.Controls.Add(this.createDataServerButton);
            this.Controls.Add(this.dataServerIdTextBox);
            this.Controls.Add(this.startMetadataServersButton);
            this.Controls.Add(this.clientPortTextBox);
            this.Controls.Add(this.clientNameTextBox);
            this.Controls.Add(this.createClientButton);
            this.Name = "ControlBoard";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button createClientButton;
        private System.Windows.Forms.TextBox clientNameTextBox;
        private System.Windows.Forms.TextBox clientPortTextBox;
        private System.Windows.Forms.Button startMetadataServersButton;
        private System.Windows.Forms.TextBox dataServerIdTextBox;
        private System.Windows.Forms.Button createDataServerButton;
        private System.Windows.Forms.TextBox dataServerPortTextBox;
    }
}

