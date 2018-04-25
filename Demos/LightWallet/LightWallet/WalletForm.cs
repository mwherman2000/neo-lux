using Neo.Lux.Core;
using Neo.Lux.Utils;
using Neo.Lux.Cryptography;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace neo_lux_light_wallet
{
    public partial class WalletForm : Form
    {
        private KeyPair keyPair;
        private NeoAPI api;


        public WalletForm()
        {
            InitializeComponent();

            dataGridView1.Columns.Add("Property", "Property");
            dataGridView1.Columns.Add("Value", "Value");

            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[0].FillWeight = 3;

            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[1].FillWeight = 4;

            assetComboBox.Items.Clear();
            foreach (var symbol in NeoAPI.AssetSymbols)
            {
                assetComboBox.Items.Add(symbol);
            }
            foreach (var symbol in NeoAPI.TokenSymbols)
            {
                assetComboBox.Items.Add(symbol);
            }
            assetComboBox.SelectedIndex = 0;

            withdrawAssetComboBox.Items.Clear();
            foreach (var symbol in NeoAPI.AssetSymbols)
            {
                withdrawAssetComboBox.Items.Add(symbol);
            }
            foreach (var symbol in NeoAPI.TokenSymbols)
            {
                withdrawAssetComboBox.Items.Add(symbol);
            }
            withdrawAssetComboBox.SelectedIndex = 0;

            fromAddressBox.ReadOnly = true;

            netComboBox.SelectedIndex = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            tabs.TabPages.Remove(balancePage);
            tabs.TabPages.Remove(withdrawPage);
            tabs.TabPages.Remove(transferPage);
            tabs.TabPages.Remove(loadingPage);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (privateKeyInput.Text.Length == 52)
            {
                keyPair = KeyPair.FromWIF(privateKeyInput.Text);
            }
            else
            if (privateKeyInput.Text.Length == 64)
            {
                var keyBytes = privateKeyInput.Text.HexToBytes();
                keyPair = new KeyPair(keyBytes);
            }
            else
            {
                MessageBox.Show("Invalid key input, must be 104 or 64 hexdecimal characters.");
                return;
            }

            var net = netComboBox.SelectedItem.ToString();
            switch (net)
            {
                case "Test": api = NeoDB.ForTestNet(); break;
                case "Main": api = NeoDB.ForMainNet(); break;
                default:
                    {
                        MessageBox.Show("Invalid net.");
                        return;
                    }
            }


            tabs.TabPages.Remove(loginPage);
            tabs.TabPages.Add(loadingPage);

            timer1.Enabled = true;

            var task = new Task(() => OpenWallet());
            task.Start();
        }

        private bool isWalletOpen;
        private Dictionary<string, decimal> balances;

        private void OpenWallet()
        {
            isWalletOpen = false;

            dataGridView1.Rows.Clear();
            dataGridView1.Rows.Add("Address", keyPair.address);

            this.balances = api.GetBalancesOf(keyPair);

            isWalletOpen = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toAddressBox.Text))
            {
                MessageBox.Show("Please insert destination address");
                return;
            }

            var symbol = assetComboBox.SelectedItem.ToString();

            int amount = int.Parse(amountBox.Text);
            if (amount<=0)
            {
                MessageBox.Show("Please insert a valid amount of "+symbol);
                return;
            }

            if (!balances.ContainsKey(symbol) || balances[symbol] < amount) {
                MessageBox.Show("You dont have enough " + symbol);
                return;
            }

            if (api.IsAsset(symbol))
            {
                api.SendAsset(keyPair, toAddressBox.Text, symbol, amount);
            }
            else
            {
                var token = api.GetToken(symbol);
                token.Transfer(keyPair, toAddressBox.Text, amount);
            }
        }

        private void withdrawButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(withdrawFromAddress.Text))
            {
                MessageBox.Show("Please insert source address");
                return;
            }

            var symbol = withdrawAssetComboBox.SelectedItem.ToString();

            int amount = int.Parse(withdrawAmount.Text);
            if (amount <= 0)
            {
                MessageBox.Show("Please insert a valid amount of " + symbol);
                return;
            }

            api.WithdrawAsset(keyPair, withdrawFromAddress.Text, symbol, amount);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isWalletOpen)
            {
                foreach (var entry in balances)
                {
                    var symbol = entry.Key;
                    var amount = entry.Value;
                    if (amount > 0)
                    {
                        dataGridView1.Rows.Add(symbol, amount);
                    }
                }

                fromAddressBox.Text = keyPair.address;
                withdrawToAddress.Text = keyPair.address;

                tabs.TabPages.Remove(loadingPage);
                tabs.TabPages.Add(balancePage);
                tabs.TabPages.Add(transferPage);
                tabs.TabPages.Add(withdrawPage);

                timer1.Enabled = false;

                return;
            }

            loadingBar.Value += 2;
            if (loadingBar.Value >= 100)
            {
                loadingBar.Value = 0;
            }
        }
    }
}
