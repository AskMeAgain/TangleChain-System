namespace DebugApp {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.TextBoxAddress = new System.Windows.Forms.TextBox();
            this.TextBoxHash = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.ButtonLoadChain = new System.Windows.Forms.Button();
            this.ButtonCreateGenesis = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TextBoxReceiver = new System.Windows.Forms.TextBox();
            this.TextBoxAmount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TextBoxCoinName = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TextBoxAddress
            // 
            this.TextBoxAddress.Location = new System.Drawing.Point(12, 28);
            this.TextBoxAddress.Name = "TextBoxAddress";
            this.TextBoxAddress.Size = new System.Drawing.Size(425, 20);
            this.TextBoxAddress.TabIndex = 0;
            // 
            // TextBoxHash
            // 
            this.TextBoxHash.Location = new System.Drawing.Point(9, 82);
            this.TextBoxHash.Name = "TextBoxHash";
            this.TextBoxHash.Size = new System.Drawing.Size(428, 20);
            this.TextBoxHash.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 63);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(88, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Block LabelHash";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "LabelAddress";
            // 
            // ButtonLoadChain
            // 
            this.ButtonLoadChain.Location = new System.Drawing.Point(9, 119);
            this.ButtonLoadChain.Name = "ButtonLoadChain";
            this.ButtonLoadChain.Size = new System.Drawing.Size(100, 23);
            this.ButtonLoadChain.TabIndex = 4;
            this.ButtonLoadChain.Text = "Load Chain";
            this.ButtonLoadChain.UseVisualStyleBackColor = true;
            this.ButtonLoadChain.Click += new System.EventHandler(this.ButtonLoadChain_Click);
            // 
            // ButtonCreateGenesis
            // 
            this.ButtonCreateGenesis.Location = new System.Drawing.Point(636, 177);
            this.ButtonCreateGenesis.Name = "ButtonCreateGenesis";
            this.ButtonCreateGenesis.Size = new System.Drawing.Size(152, 23);
            this.ButtonCreateGenesis.TabIndex = 5;
            this.ButtonCreateGenesis.Text = "Create Genesis Block";
            this.ButtonCreateGenesis.UseVisualStyleBackColor = true;
            this.ButtonCreateGenesis.Click += new System.EventHandler(this.ButtonCreateGenesis_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(685, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Receiver";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(685, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Amount";
            // 
            // TextBoxReceiver
            // 
            this.TextBoxReceiver.Location = new System.Drawing.Point(688, 28);
            this.TextBoxReceiver.Name = "TextBoxReceiver";
            this.TextBoxReceiver.Size = new System.Drawing.Size(100, 20);
            this.TextBoxReceiver.TabIndex = 8;
            // 
            // TextBoxAmount
            // 
            this.TextBoxAmount.Location = new System.Drawing.Point(688, 82);
            this.TextBoxAmount.Name = "TextBoxAmount";
            this.TextBoxAmount.Size = new System.Drawing.Size(100, 20);
            this.TextBoxAmount.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(685, 119);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Coin Name";
            // 
            // TextBoxCoinName
            // 
            this.TextBoxCoinName.Location = new System.Drawing.Point(688, 135);
            this.TextBoxCoinName.Name = "TextBoxCoinName";
            this.TextBoxCoinName.Size = new System.Drawing.Size(100, 20);
            this.TextBoxCoinName.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.TextBoxCoinName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.TextBoxAmount);
            this.Controls.Add(this.TextBoxReceiver);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.ButtonCreateGenesis);
            this.Controls.Add(this.ButtonLoadChain);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TextBoxHash);
            this.Controls.Add(this.TextBoxAddress);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBoxAddress;
        private System.Windows.Forms.TextBox TextBoxHash;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button ButtonLoadChain;
        private System.Windows.Forms.Button ButtonCreateGenesis;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox TextBoxReceiver;
        private System.Windows.Forms.TextBox TextBoxAmount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox TextBoxCoinName;
    }
}

