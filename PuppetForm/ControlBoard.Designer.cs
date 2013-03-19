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
            this.testButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // createClientButton
            // 
            this.createClientButton.Location = new System.Drawing.Point(185, 28);
            this.createClientButton.Name = "createClientButton";
            this.createClientButton.Size = new System.Drawing.Size(75, 23);
            this.createClientButton.TabIndex = 0;
            this.createClientButton.Text = "createClient";
            this.createClientButton.UseVisualStyleBackColor = true;
            this.createClientButton.Click += new System.EventHandler(this.createClientButton_Click);
            // 
            // clientNameTextBox
            // 
            this.clientNameTextBox.Location = new System.Drawing.Point(6, 28);
            this.clientNameTextBox.Name = "clientNameTextBox";
            this.clientNameTextBox.Size = new System.Drawing.Size(100, 20);
            this.clientNameTextBox.TabIndex = 1;
            this.clientNameTextBox.Text = "1";
            // 
            // clientPortTextBox
            // 
            this.clientPortTextBox.Location = new System.Drawing.Point(112, 28);
            this.clientPortTextBox.Name = "clientPortTextBox";
            this.clientPortTextBox.Size = new System.Drawing.Size(62, 20);
            this.clientPortTextBox.TabIndex = 2;
            this.clientPortTextBox.Text = "8085";
            // 
            // startMetadataServersButton
            // 
            this.startMetadataServersButton.Location = new System.Drawing.Point(53, 28);
            this.startMetadataServersButton.Name = "startMetadataServersButton";
            this.startMetadataServersButton.Size = new System.Drawing.Size(143, 23);
            this.startMetadataServersButton.TabIndex = 3;
            this.startMetadataServersButton.Text = "startMetadataServers";
            this.startMetadataServersButton.UseVisualStyleBackColor = true;
            this.startMetadataServersButton.Click += new System.EventHandler(this.startMetadataServersButton_Click);
            // 
            // dataServerIdTextBox
            // 
            this.dataServerIdTextBox.Location = new System.Drawing.Point(17, 41);
            this.dataServerIdTextBox.Name = "dataServerIdTextBox";
            this.dataServerIdTextBox.Size = new System.Drawing.Size(62, 20);
            this.dataServerIdTextBox.TabIndex = 4;
            this.dataServerIdTextBox.Text = "0";
            // 
            // createDataServerButton
            // 
            this.createDataServerButton.Location = new System.Drawing.Point(162, 39);
            this.createDataServerButton.Name = "createDataServerButton";
            this.createDataServerButton.Size = new System.Drawing.Size(75, 23);
            this.createDataServerButton.TabIndex = 5;
            this.createDataServerButton.Text = "createDS";
            this.createDataServerButton.UseVisualStyleBackColor = true;
            this.createDataServerButton.Click += new System.EventHandler(this.createDataServerButton_Click);
            // 
            // dataServerPortTextBox
            // 
            this.dataServerPortTextBox.Location = new System.Drawing.Point(85, 41);
            this.dataServerPortTextBox.Name = "dataServerPortTextBox";
            this.dataServerPortTextBox.Size = new System.Drawing.Size(62, 20);
            this.dataServerPortTextBox.TabIndex = 6;
            this.dataServerPortTextBox.Text = "8000";
            // 
            // testButton
            // 
            this.testButton.Location = new System.Drawing.Point(55, 28);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(164, 23);
            this.testButton.TabIndex = 7;
            this.testButton.Text = "test";
            this.testButton.UseVisualStyleBackColor = true;
            this.testButton.Click += new System.EventHandler(this.testButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.clientNameTextBox);
            this.groupBox1.Controls.Add(this.clientPortTextBox);
            this.groupBox1.Controls.Add(this.createClientButton);
            this.groupBox1.Location = new System.Drawing.Point(351, 26);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(266, 70);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "CLIENTS";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.startMetadataServersButton);
            this.groupBox2.Location = new System.Drawing.Point(23, 26);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(254, 66);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "METADATA";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataServerIdTextBox);
            this.groupBox3.Controls.Add(this.dataServerPortTextBox);
            this.groupBox3.Controls.Add(this.createDataServerButton);
            this.groupBox3.Location = new System.Drawing.Point(23, 120);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(254, 100);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "DATASERVERS";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.exitButton);
            this.groupBox4.Controls.Add(this.testButton);
            this.groupBox4.Location = new System.Drawing.Point(357, 120);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(260, 100);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "GENERAL";
            // 
            // exitButton
            // 
            this.exitButton.Location = new System.Drawing.Point(55, 70);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(164, 23);
            this.exitButton.TabIndex = 8;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // ControlBoard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(635, 281);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ControlBoard";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button createClientButton;
        private System.Windows.Forms.TextBox clientNameTextBox;
        private System.Windows.Forms.TextBox clientPortTextBox;
        private System.Windows.Forms.Button startMetadataServersButton;
        private System.Windows.Forms.TextBox dataServerIdTextBox;
        private System.Windows.Forms.Button createDataServerButton;
        private System.Windows.Forms.TextBox dataServerPortTextBox;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button exitButton;
    }
}

