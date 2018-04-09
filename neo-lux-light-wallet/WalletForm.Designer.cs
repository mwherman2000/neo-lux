namespace neo_lux_light_wallet
{
    partial class WalletForm
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
            this.tabs = new System.Windows.Forms.TabControl();
            this.loginPage = new System.Windows.Forms.TabPage();
            this.warningLb = new System.Windows.Forms.Label();
            this.privateKeyInput = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.balancePage = new System.Windows.Forms.TabPage();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.transferPage = new System.Windows.Forms.TabPage();
            this.button2 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.toAddressBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.fromAddressBox = new System.Windows.Forms.TextBox();
            this.amountLb = new System.Windows.Forms.Label();
            this.amountBox = new System.Windows.Forms.TextBox();
            this.assetComboBox = new System.Windows.Forms.ComboBox();
            this.withdrawPage = new System.Windows.Forms.TabPage();
            this.withdrawButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.withdrawToAddress = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.withdrawFromAddress = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.withdrawAmount = new System.Windows.Forms.TextBox();
            this.withdrawAssetComboBox = new System.Windows.Forms.ComboBox();
            this.tabs.SuspendLayout();
            this.loginPage.SuspendLayout();
            this.balancePage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.transferPage.SuspendLayout();
            this.withdrawPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.loginPage);
            this.tabs.Controls.Add(this.balancePage);
            this.tabs.Controls.Add(this.transferPage);
            this.tabs.Controls.Add(this.withdrawPage);
            this.tabs.Location = new System.Drawing.Point(12, 12);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(427, 282);
            this.tabs.TabIndex = 0;
            // 
            // loginPage
            // 
            this.loginPage.Controls.Add(this.warningLb);
            this.loginPage.Controls.Add(this.privateKeyInput);
            this.loginPage.Controls.Add(this.label1);
            this.loginPage.Controls.Add(this.button1);
            this.loginPage.Location = new System.Drawing.Point(4, 22);
            this.loginPage.Name = "loginPage";
            this.loginPage.Padding = new System.Windows.Forms.Padding(3);
            this.loginPage.Size = new System.Drawing.Size(419, 256);
            this.loginPage.TabIndex = 0;
            this.loginPage.Text = "Login";
            this.loginPage.UseVisualStyleBackColor = true;
            // 
            // warningLb
            // 
            this.warningLb.Location = new System.Drawing.Point(17, 26);
            this.warningLb.Name = "warningLb";
            this.warningLb.Size = new System.Drawing.Size(386, 75);
            this.warningLb.TabIndex = 3;
            this.warningLb.Text = "Warning: This wallet was created just for demo purposes. Don\'t use it in the Neo " +
    "main net, the developers cannot be responsibilized for any assets lost.";
            // 
            // privateKeyInput
            // 
            this.privateKeyInput.Location = new System.Drawing.Point(20, 127);
            this.privateKeyInput.Name = "privateKeyInput";
            this.privateKeyInput.Size = new System.Drawing.Size(393, 20);
            this.privateKeyInput.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 111);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Private Key (Full or WIF)";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(151, 188);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(131, 39);
            this.button1.TabIndex = 0;
            this.button1.Text = "Open Wallet";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // balancePage
            // 
            this.balancePage.Controls.Add(this.dataGridView1);
            this.balancePage.Location = new System.Drawing.Point(4, 22);
            this.balancePage.Name = "balancePage";
            this.balancePage.Padding = new System.Windows.Forms.Padding(3);
            this.balancePage.Size = new System.Drawing.Size(419, 256);
            this.balancePage.TabIndex = 1;
            this.balancePage.Text = "Balances";
            this.balancePage.UseVisualStyleBackColor = true;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 6);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(407, 244);
            this.dataGridView1.TabIndex = 0;
            // 
            // transferPage
            // 
            this.transferPage.Controls.Add(this.button2);
            this.transferPage.Controls.Add(this.label3);
            this.transferPage.Controls.Add(this.toAddressBox);
            this.transferPage.Controls.Add(this.label2);
            this.transferPage.Controls.Add(this.fromAddressBox);
            this.transferPage.Controls.Add(this.amountLb);
            this.transferPage.Controls.Add(this.amountBox);
            this.transferPage.Controls.Add(this.assetComboBox);
            this.transferPage.Location = new System.Drawing.Point(4, 22);
            this.transferPage.Name = "transferPage";
            this.transferPage.Padding = new System.Windows.Forms.Padding(3);
            this.transferPage.Size = new System.Drawing.Size(419, 256);
            this.transferPage.TabIndex = 2;
            this.transferPage.Text = "Transfer";
            this.transferPage.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(169, 207);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(92, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "Send";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 132);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(20, 13);
            this.label3.TabIndex = 15;
            this.label3.Text = "To";
            // 
            // toAddressBox
            // 
            this.toAddressBox.Location = new System.Drawing.Point(23, 148);
            this.toAddressBox.Name = "toAddressBox";
            this.toAddressBox.Size = new System.Drawing.Size(376, 20);
            this.toAddressBox.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 83);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "From";
            // 
            // fromAddressBox
            // 
            this.fromAddressBox.Location = new System.Drawing.Point(23, 99);
            this.fromAddressBox.Name = "fromAddressBox";
            this.fromAddressBox.ReadOnly = true;
            this.fromAddressBox.Size = new System.Drawing.Size(376, 20);
            this.fromAddressBox.TabIndex = 12;
            // 
            // amountLb
            // 
            this.amountLb.AutoSize = true;
            this.amountLb.Location = new System.Drawing.Point(110, 27);
            this.amountLb.Name = "amountLb";
            this.amountLb.Size = new System.Drawing.Size(43, 13);
            this.amountLb.TabIndex = 11;
            this.amountLb.Text = "Amount";
            // 
            // amountBox
            // 
            this.amountBox.Location = new System.Drawing.Point(113, 43);
            this.amountBox.Name = "amountBox";
            this.amountBox.Size = new System.Drawing.Size(100, 20);
            this.amountBox.TabIndex = 10;
            this.amountBox.Text = "0";
            // 
            // assetComboBox
            // 
            this.assetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assetComboBox.FormattingEnabled = true;
            this.assetComboBox.Location = new System.Drawing.Point(219, 43);
            this.assetComboBox.Name = "assetComboBox";
            this.assetComboBox.Size = new System.Drawing.Size(100, 21);
            this.assetComboBox.TabIndex = 9;
            // 
            // withdrawPage
            // 
            this.withdrawPage.Controls.Add(this.withdrawButton);
            this.withdrawPage.Controls.Add(this.label4);
            this.withdrawPage.Controls.Add(this.withdrawToAddress);
            this.withdrawPage.Controls.Add(this.label5);
            this.withdrawPage.Controls.Add(this.withdrawFromAddress);
            this.withdrawPage.Controls.Add(this.label6);
            this.withdrawPage.Controls.Add(this.withdrawAmount);
            this.withdrawPage.Controls.Add(this.withdrawAssetComboBox);
            this.withdrawPage.Location = new System.Drawing.Point(4, 22);
            this.withdrawPage.Name = "withdrawPage";
            this.withdrawPage.Padding = new System.Windows.Forms.Padding(3);
            this.withdrawPage.Size = new System.Drawing.Size(419, 256);
            this.withdrawPage.TabIndex = 3;
            this.withdrawPage.Text = "Withdraw";
            this.withdrawPage.UseVisualStyleBackColor = true;
            // 
            // withdrawButton
            // 
            this.withdrawButton.Location = new System.Drawing.Point(169, 207);
            this.withdrawButton.Name = "withdrawButton";
            this.withdrawButton.Size = new System.Drawing.Size(92, 23);
            this.withdrawButton.TabIndex = 16;
            this.withdrawButton.Text = "Withdraw";
            this.withdrawButton.UseVisualStyleBackColor = true;
            this.withdrawButton.Click += new System.EventHandler(this.withdrawButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 132);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(20, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "To";
            // 
            // withdrawToAddress
            // 
            this.withdrawToAddress.Location = new System.Drawing.Point(23, 148);
            this.withdrawToAddress.Name = "withdrawToAddress";
            this.withdrawToAddress.ReadOnly = true;
            this.withdrawToAddress.Size = new System.Drawing.Size(376, 20);
            this.withdrawToAddress.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 83);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(30, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "From";
            // 
            // withdrawFromAddress
            // 
            this.withdrawFromAddress.Location = new System.Drawing.Point(23, 99);
            this.withdrawFromAddress.Name = "withdrawFromAddress";
            this.withdrawFromAddress.Size = new System.Drawing.Size(376, 20);
            this.withdrawFromAddress.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(110, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(43, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Amount";
            // 
            // withdrawAmount
            // 
            this.withdrawAmount.Location = new System.Drawing.Point(113, 43);
            this.withdrawAmount.Name = "withdrawAmount";
            this.withdrawAmount.Size = new System.Drawing.Size(100, 20);
            this.withdrawAmount.TabIndex = 10;
            this.withdrawAmount.Text = "0";
            // 
            // withdrawAssetComboBox
            // 
            this.withdrawAssetComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.withdrawAssetComboBox.FormattingEnabled = true;
            this.withdrawAssetComboBox.Location = new System.Drawing.Point(219, 43);
            this.withdrawAssetComboBox.Name = "withdrawAssetComboBox";
            this.withdrawAssetComboBox.Size = new System.Drawing.Size(100, 21);
            this.withdrawAssetComboBox.TabIndex = 9;
            // 
            // WalletForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(451, 306);
            this.Controls.Add(this.tabs);
            this.Name = "WalletForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Neo Light Wallet Example";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabs.ResumeLayout(false);
            this.loginPage.ResumeLayout(false);
            this.loginPage.PerformLayout();
            this.balancePage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.transferPage.ResumeLayout(false);
            this.transferPage.PerformLayout();
            this.withdrawPage.ResumeLayout(false);
            this.withdrawPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage loginPage;
        private System.Windows.Forms.TabPage balancePage;
        private System.Windows.Forms.TextBox privateKeyInput;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label warningLb;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TabPage transferPage;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox toAddressBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox fromAddressBox;
        private System.Windows.Forms.Label amountLb;
        private System.Windows.Forms.TextBox amountBox;
        private System.Windows.Forms.ComboBox assetComboBox;
        private System.Windows.Forms.TabPage withdrawPage;
        private System.Windows.Forms.Button withdrawButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox withdrawToAddress;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox withdrawFromAddress;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox withdrawAmount;
        private System.Windows.Forms.ComboBox withdrawAssetComboBox;
    }
}

