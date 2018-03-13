// Neo Unity Wallet

/* 	PlayerPrefs Structure
 * ====================================================
 * 	Default Account:
 * 		"neoAccount": "literalAccountName"
 * 	Encrypted WIF Location:
 * 		"literalAccountName": "literalEncryptedWif"
*/

using System;
using System.Text;
using System.Linq;

using System.Numerics;

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

	public static byte AddressVersion = 23;

	public GameObject loginPanel;
	public GameObject startPanel;
	public GameObject newAccountPanel;
	public GameObject accountPanel;

	public Button startBtn;
	public Button loginButton;

	public PlayerPrefs playerPrefs;

	public string encryptedWif = "";
	public string decryptedWif = "";
	public KeyPair keys;

	public Text loginStatus;

	public Text addressLabel;
	public Text balanceLabel;
	public Text wifLabel;

	// Login Panel Input Fields
	public InputField accountNameInput; 
	public InputField encryptedWifInput;
	public InputField passwordInput;

	// Create New Account Panel Input FIelds
	public InputField createAccount_nameInput; 
	public InputField createAccount_passwordInput; 

	private WalletState state = WalletState.Init;

	private bool hasAccount = false;
	private bool loggedIn = false;
	private decimal balance;

	private string accountName;
	private static string password = "test";

	private const string assetSymbol = "GAS";


	void Start () {


		// make sure everything that should be invisible is invisible
		loginPanel.gameObject.SetActive(false);
		accountPanel.gameObject.SetActive(false);
		newAccountPanel.gameObject.SetActive(false);

		PlayerPrefs.DeleteAll ();
	}


	IEnumerator SyncBalance()
	{
		if (loggedIn) {
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


	public void loginPanelShow () {
		if (!loginPanel.activeSelf) {
			if (PlayerPrefs.HasKey ("neoAccount")) {
				accountName = PlayerPrefs.GetString ("neoAccount");
				accountNameInput.text = accountName;

				Debug.Log ("PlayerPrefs: found account name: " + accountName);

				// Load Encrypted WIF from PlayerPrefs
				if (accountName.Length > 0 && PlayerPrefs.HasKey (accountName)) {
					encryptedWif = PlayerPrefs.GetString (accountName);
					Debug.Log ("PlayerPrefs: found encrypted wif: " + encryptedWif);

					encryptedWifInput.text = encryptedWif;
				}
				isLoginFormValid ();

				loginPanel.gameObject.SetActive (true);
			} else {
				newAccountPanel.gameObject.SetActive (true);
			}

		}
		if (startPanel.activeSelf) {
			startPanel.gameObject.SetActive (false);
		}
	}


	public void cancelButton () {
		if (loginPanel.activeSelf) {
			loginPanel.gameObject.SetActive (false);
		}

		if (!startPanel.activeSelf) {
			startPanel.gameObject.SetActive (true);
		}
	}


	public void login () {
		loginStatus.text = "Attempting to login";

		// If "neoAccount" exists, then user has already setup an account. Otherwise, let's make a new one.
		// Each account exists as a key with a unique account name.

		if (!PlayerPrefs.HasKey ("neoAccount") || !PlayerPrefs.HasKey (accountName)) {
			Debug.Log ("\nno neo account found, creating new account");


			hasAccount = loggedIn = true;
		} else { // an account exists, lets look it up and load it
			Debug.Log("found a neo account");

			if (!isLoginFormValid ()) return;

			// Decrypt WIF 

			password = passwordInput.text;
			Debug.Log ("login password: " + passwordInput.text);

			GetPrivateKeyFromNEP2 (encryptedWif, password);

			hasAccount = loggedIn = true;
		}
	}


	public bool isLoginFormValid () {
		if (accountNameInput.text == "" || accountNameInput.text == null) {
			Debug.Log ("user supplied no account name");
			loginStatus.text = "Please supply an account name";
			return false;
		} else {
			PlayerPrefs.SetString ("neoAccount", accountNameInput.text);
		}

		if (encryptedWifInput.text == null || encryptedWifInput.text == "") {
			Debug.Log ("user supplied no encrypted wif");
			loginStatus.text = "Please supply encrypted wif";
			return false;
		} else {
			Debug.Log ("encrypted wif input: " + encryptedWifInput.text);
			PlayerPrefs.SetString (accountName, encryptedWifInput.text);
		}

		if (passwordInput.text == "") {
			Debug.Log ("user supplied no password");
			loginStatus.text = "Please supply a password";
			return false;
		} else {
			Debug.Log ("password input: " + passwordInput.text);
			password = passwordInput.text;
		}

		return true;
	}


	public void createNewAccount() {
		
		if (createAccount_nameInput.text != "" && createAccount_nameInput.text != null) {
			accountName = accountNameInput.text = createAccount_nameInput.text;
			PlayerPrefs.SetString ("neoAccount", createAccount_nameInput.text);
			Debug.Log ("account: '" + accountName + "' created and stored in PlayerPrefs");
		} else {
			loginStatus.text = "Please supply an account name";
			return;
		}

		// TODO: Add check for password

		if (createAccount_passwordInput.text != "" && createAccount_passwordInput.text != null) {
			password = createAccount_passwordInput.text;
			Debug.Log("password: "+password);
		} else {
			loginStatus.text = "Please supply a password";
			return;
		}

		// TODO: write a function to do all of the following key generation and wif encryption
		byte[] privateKey = new byte[32];

		// generate a new private key
		RandomNumberGenerator rng = RandomNumberGenerator.Create ();
		rng.GetBytes (privateKey);


		// generate a key pair
		keys = new KeyPair (privateKey);

		// for loading specific private key strings, do it this way
		//keys = new KeyPair("a9e2b5436cab6ff74be2d5c91b8a67053494ab5b454ac2851f872fb0fd30ba5e".HexToBytes());

		addressLabel.text = keys.address;
		balanceLabel.text = "Please WAIT, syncing balance: ...";
		wifLabel.text = keys.WIF; 

		// WIF ENCRYPTION
		byte[] addresshash = Encoding.ASCII.GetBytes (keys.address).Sha256 ().Sha256 ().Take (4).ToArray ();

		// these are the hardcoded scrypt defaults per neo-gui / neon-js: int N = 16384, int r = 8, int p = 8
		byte[] derivedkey = SCrypt.DeriveKey (Encoding.UTF8.GetBytes (password), addresshash, 16384, 8, 8, 64);
		Debug.Log ("derived key: " + derivedkey.ByteToHex());
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

		Debug.Log ("WIF: " + keys.WIF);
		Debug.Log ("Encrypted WIF: " + encryptedWif);
		Debug.Log ("Encrypted Private Key: " + encryptedkey.ByteToHex());
		Debug.Log ("Private Key: " + keys.PrivateKey.ByteToHex());
		Debug.Log ("Public Key: " + keys.PublicKey.ByteToHex());
		Debug.Log ("Address: " + keys.address);

		encryptedWifInput.text = encryptedWif;
		PlayerPrefs.SetString (accountName, encryptedWif);

		newAccountPanel.gameObject.SetActive (false);
		loginPanel.gameObject.SetActive (true);
	}

	public  byte[] GetPrivateKeyFromNEP2(string nep2, string passphrase)
	{
//		if (nep2 == null)
//			throw new ArgumentNullException (nameof (nep2));
//		if (passphrase == null)
//			throw new ArgumentNullException (nameof (passphrase));

		byte[] data = nep2.Base58CheckDecode ();

		if (data.Length != 39 || data [0] != 0x01 || data [1] != 0x42 || data [2] != 0xe0)
			throw new FormatException ();

		Debug.Log ("nep2 " + nep2);

		byte[] addresshash = new byte[4];

		Buffer.BlockCopy (data, 3, addresshash, 0, 4);

		Debug.Log ("in nep2: " + passphrase);

		byte[] derivedkey = SCrypt.DeriveKey (Encoding.UTF8.GetBytes (passphrase), addresshash, 16384, 8, 8, 64);
		byte[] derivedhalf1 = derivedkey.Take (32).ToArray ();
		byte[] derivedhalf2 = derivedkey.Skip (32).ToArray ();
		byte[] encryptedkey = new byte[32];
		Buffer.BlockCopy (data, 7, encryptedkey, 0, 32);
		byte[] prikey = XOR (encryptedkey.AES256Decrypt (derivedhalf2), derivedhalf1);
		Neo.Cryptography.ECC.ECPoint pubkey = Neo.Cryptography.ECC.ECCurve.Secp256r1.G * prikey;

		var bytes = pubkey.EncodePoint(true).ToArray();
//		byte[] CompressedPublicKey = bytes;

		// byte[] PublicKeyHash = Crypto.Default.ToScriptHash (bytes);

		string signatureScript = KeyPair.CreateSignatureScript(bytes);
		UInt160 signatureHash = Crypto.Default.ToScriptHash(signatureScript.HexToBytes());

		byte[] publickey = pubkey.EncodePoint(false).Skip(1).ToArray();

		string address = Crypto.Default.ToAddress(signatureHash);

		Debug.Log ("decrypted private key: " + prikey.ByteToHex());
		Debug.Log ("decrypted public key: " + publickey.ByteToHex());
		Debug.Log ("decrypted address: " + address);

		return prikey;
	}
		

	private static byte[] XOR(byte[] x, byte[] y)
	{
		if (x.Length != y.Length) throw new ArgumentException();
		return x.Zip(y, (a, b) => (byte)(a ^ b)).ToArray();
	}


	public static string ToAddress(UInt160 scriptHash)
	{
		byte[] data = new byte[21];
		data[0] = AddressVersion;
		Buffer.BlockCopy(scriptHash.ToArray(), 0, data, 1, 20);
		return data.Base58CheckEncode();
	}
		

	public static UInt160 ToScriptHash(string address)
	{
		byte[] data = address.Base58CheckDecode();
		if (data.Length != 21)
			Debug.Log("toscripthash error: bad length");
		if (data[0] != AddressVersion)
			Debug.Log("toscripthash error: 0 != addressversion");
		return new UInt160(data.Skip(1).ToArray());
	}
		

	// TESTS

	public static void testsComparePrivateKeys(byte[] generated, byte[] decrypted) {
	
		Debug.Log("generated private key: " +  generated.ByteToHex());
		Debug.Log("decrypted private key: " + decrypted.ByteToHex());

		if(generated.SequenceEqual(decrypted)) Debug.Log("[+++]PRIVATE KEY HIT");
		else Debug.Log("[---] PRIVATE KEY MISS");
	}
}
