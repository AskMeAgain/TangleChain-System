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
            this.ButtonMineNextBlock = new System.Windows.Forms.Button();
            this.LabelHeight = new System.Windows.Forms.Label();
            this.ButtonLoadFromDB = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // TextBoxAddress
            // 
            this.TextBoxAddress.Location = new System.Drawing.Point(12, 101);
            this.TextBoxAddress.Name = "TextBoxAddress";
            this.TextBoxAddress.Size = new System.Drawing.Size(425, 20);
            this.TextBoxAddress.TabIndex = 0;
            this.TextBoxAddress.Text = "BUYWYIPDXRVHEAYLYXCMBHTPJRX9AOFBBAAKLMCOYSONRKBCYYPKT9RVBJMGMQDJLUFD9NALUPPJTFZ9Q" +
    "";
            // 
            // TextBoxHash
            // 
            this.TextBoxHash.Location = new System.Drawing.Point(9, 155);
            this.TextBoxHash.Name = "TextBoxHash";
            this.TextBoxHash.Size = new System.Drawing.Size(428, 20);
            this.TextBoxHash.TabIndex = 1;
            this.TextBoxHash.Text = "YOA99AZOMDVSZBWPBTNVHHVIYOFNDQXSYEGLNEZILDRVSAQORBMEUWYMIQFBGWOLLGBVXIMXWYAGDRHAT" +
    "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 136);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Genesis Hash";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 82);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Genesis Address";
            // 
            // ButtonLoadChain
            // 
            this.ButtonLoadChain.Location = new System.Drawing.Point(443, 391);
            this.ButtonLoadChain.Name = "ButtonLoadChain";
            this.ButtonLoadChain.Size = new System.Drawing.Size(216, 37);
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
            this.TextBoxCoinName.Text = "asd";
            // 
            // ButtonMineNextBlock
            // 
            this.ButtonMineNextBlock.Location = new System.Drawing.Point(12, 391);
            this.ButtonMineNextBlock.Name = "ButtonMineNextBlock";
            this.ButtonMineNextBlock.Size = new System.Drawing.Size(189, 37);
            this.ButtonMineNextBlock.TabIndex = 12;
            this.ButtonMineNextBlock.Text = "Mine Next Block";
            this.ButtonMineNextBlock.UseVisualStyleBackColor = true;
            this.ButtonMineNextBlock.Click += new System.EventHandler(this.ButtonMineNextBlock_Click);
            // 
            // LabelHeight
            // 
            this.LabelHeight.AutoSize = true;
            this.LabelHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelHeight.Location = new System.Drawing.Point(12, 9);
            this.LabelHeight.Name = "LabelHeight";
            this.LabelHeight.Size = new System.Drawing.Size(168, 46);
            this.LabelHeight.TabIndex = 13;
            this.LabelHeight.Text = "Height 0";
            // 
            // ButtonLoadFromDB
            // 
            this.ButtonLoadFromDB.Location = new System.Drawing.Point(225, 391);
            this.ButtonLoadFromDB.Name = "ButtonLoadFromDB";
            this.ButtonLoadFromDB.Size = new System.Drawing.Size(212, 37);
            this.ButtonLoadFromDB.TabIndex = 14;
            this.ButtonLoadFromDB.Text = "Load From DB";
            this.ButtonLoadFromDB.UseVisualStyleBackColor = true;
            this.ButtonLoadFromDB.Click += new System.EventHandler(this.ButtonLoadFromDB_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ButtonLoadFromDB);
            this.Controls.Add(this.LabelHeight);
            this.Controls.Add(this.ButtonMineNextBlock);
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
        private System.Windows.Forms.Button ButtonMineNextBlock;
        private System.Windows.Forms.Label LabelHeight;
        private System.Windows.Forms.Button ButtonLoadFromDB;
    }
}

