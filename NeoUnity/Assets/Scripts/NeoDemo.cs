using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;

using NeoLux;
using Neo;
using Neo.Cryptography;
using Neo.VM;


public class NeoDemo : MonoBehaviour {

	private enum WalletState
	{
		Init,
		Sync,
		Update,
		Ready
	}
	private bool hasAccount = false;
	private bool accountLoaded = false;
	public PlayerPrefs playerPrefs;

	public string encryptedWif = "";
	public KeyPair keys;

	public Text addressLabel;
	public Text balanceLabel;
	public Text wifLabel;
	public string passphrase = "test";

	public Button startBtn;

	private WalletState state = WalletState.Init;
	private decimal balance;

	private const string assetSymbol = "GAS";

	void Start () {

		// If "neoAccount" exists, then user has already setup an account. Otherwise, let's make a new one.
		// Each account exists as a key with a unique account name.

		if (!PlayerPrefs.HasKey ("neoAccount")) {
			Debug.Log ("no neo account found, prompting for generation");

			// Get an account name from the user

			// Get a password from the user

			byte[] privateKey = new byte[32];

			// generate a new private key
			using (RandomNumberGenerator rng = RandomNumberGenerator.Create ()) {
				rng.GetBytes (privateKey);
			}

			// generate a key pair
			keys = new KeyPair (privateKey);

			// for loading specific private key strings, do it this way
			//keys = new KeyPair("a9e2b5436cab6ff74be2d5c91b8a67053494ab5b454ac2851f872fb0fd30ba5e".HexToBytes());

			addressLabel.text = keys.address;
			balanceLabel.text = "Please WAIT, syncing balance: ...";
			wifLabel.text = keys.WIF; 

			Debug.Log ("address: " + keys.address);
			Debug.Log ("raw wif: " + keys.WIF);

			// let's encrypt the wif
			byte[] addresshash = Encoding.ASCII.GetBytes (keys.address).Sha256 ().Sha256 ().Take (4).ToArray ();

			// these are the hardcoded scrypt defaults per neo-gui / neon-js: int N = 16384, int r = 8, int p = 8
			byte[] derivedkey = SCrypt.DeriveKey (Encoding.UTF8.GetBytes (passphrase), addresshash, 16384, 8, 8, 64);
			byte[] derivedhalf1 = derivedkey.Take (32).ToArray ();
			byte[] derivedhalf2 = derivedkey.Skip (32).ToArray ();
			byte[] encryptedkey = XOR (keys.PrivateKey, derivedhalf1).AES256Encrypt (derivedhalf2);
			byte[] buffer = new byte[39];
			buffer [0] = 0x01;
			buffer [1] = 0x42;
			buffer [2] = 0xe0;
			Buffer.BlockCopy (addresshash, 0, buffer, 3, addresshash.Length);
			Buffer.BlockCopy (encryptedkey, 0, buffer, 7, encryptedkey.Length);
			encryptedWif = buffer.Base58CheckEncode ();

			Debug.Log ("encrypted WIF: " + encryptedWif);
			hasAccount = accountLoaded = true;
		} else { // an account exists, lets look it up and load it

			// Prompt the user for the account name or use the default


			// Prompt the user for the password

			hasAccount = accountLoaded = true;
		}
	}

	IEnumerator SyncBalance()
	{
		if (accountLoaded) {
			yield return new WaitForSeconds (2);

			Debug.Log ("getting balance for address: " + keys.address);

			var balances = NeoAPI.GetBalance (NeoAPI.Net.Test, keys.address);

			balance = balances.ContainsKey (assetSymbol) ? balances [assetSymbol] : 0;
			state = WalletState.Update;
		}
	}

	void Update () {

		switch (state)
		{
		case WalletState.Init:
			{
				state = WalletState.Sync;
				StartCoroutine(SyncBalance());
				break;
			}

		case WalletState.Update:
			{
				state = WalletState.Ready;
				balanceLabel.text = balance.ToString() + " "+ assetSymbol;
				Debug.Log ("balance: " + balanceLabel.text);

				wifLabel.text = keys.WIF;

				startBtn.interactable = true;
				break;
			}
		}		
	}

	private static byte[] XOR(byte[] x, byte[] y)
	{
		if (x.Length != y.Length) throw new ArgumentException();
		return x.Zip(y, (a, b) => (byte)(a ^ b)).ToArray();
	}
}
