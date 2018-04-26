using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.IO;
using System.Text;

namespace Neo.Lux.Core
{
    public class Transaction
    {
        public struct Witness
        {
            public byte[] invocationScript;
            public byte[] verificationScript;
        }

        public struct Input
        {
            public byte[] prevHash;
            public uint prevIndex;
        }

        public struct Output
        {
            public byte[] scriptHash;
            public byte[] assetID;
            public decimal value;
        }

        public byte type;
        public byte version;
        public byte[] script;
        public decimal gas;

        public Input[] inputs;
        public Output[] outputs;
        public Witness[] witnesses;

        #region HELPERS

        protected static string num2hexstring(long num, int size = 2)
        {
            return num.ToString("X" + size);
        }

        protected static void SerializeWitness(BinaryWriter writer, Witness witness)
        {
            writer.Write((byte)witness.invocationScript.Length);
            writer.Write(witness.invocationScript);
            writer.Write((byte)witness.verificationScript.Length);
            writer.Write(witness.verificationScript);
        }

        protected static void SerializeTransactionInput(BinaryWriter writer, Input input)
        {
            writer.Write(input.prevHash);
            writer.Write((ushort)input.prevIndex);
        }

        protected static void SerializeTransactionOutput(BinaryWriter writer, Output output)
        {
            //return LuxUtils.reverseHex(output.assetID) + LuxUtils.num2fixed8(output.value)+ LuxUtils.reverseHex(output.scriptHash);
            writer.Write(output.assetID);
            writer.WriteFixed(output.value);
            writer.Write(output.scriptHash);
        }
        #endregion

        public virtual byte[] Serialize(bool signed = true)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)this.type);
                    writer.Write((byte)this.version);

                    // exclusive data
                    if (this.type == 0xd1)
                    {
                        writer.WriteVarInt(this.script.Length);
                        writer.Write(this.script);
                        if (this.version >= 1)
                        {
                            writer.WriteFixed(this.gas);
                        }
                    }

                    // Don't need any attributes
                    writer.Write((byte)0);

                    writer.WriteVarInt(this.inputs.Length);
                    foreach (var input in this.inputs)
                    {
                        SerializeTransactionInput(writer, input);
                    }

                    writer.WriteVarInt(this.outputs.Length);
                    foreach (var output in this.outputs)
                    {
                        SerializeTransactionOutput(writer, output);
                    }

                    if (signed && this.witnesses != null)
                    {
                        writer.WriteVarInt(this.witnesses.Length);
                        foreach (var witness in this.witnesses)
                        {
                            SerializeWitness(writer, witness);
                        }
                    }

                }

                return stream.ToArray();
            }
        }

        public void Sign(KeyPair key)
        {
            var txdata = this.Serialize(false);

            var privkey = key.PrivateKey;
            var pubkey = key.PublicKey;
            var signature = CryptoUtils.Sign(txdata, privkey, pubkey);

            var invocationScript = ("40" + signature.ByteToHex()).HexToBytes();
            var verificationScript = key.signatureScript.HexToBytes();
            this.witnesses = new Transaction.Witness[] { new Transaction.Witness() { invocationScript = invocationScript, verificationScript = verificationScript } };
        }

        private UInt256 _hash = null;

        public UInt256 Hash
        {
            get
            {
                if (_hash == null)
                {
                    var rawTx = this.Serialize(false);
                    _hash = new UInt256(CryptoUtils.Hash256(rawTx));
                }

                return _hash;
            }
        }

        public static Transaction Decode(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {

                }
            }


            throw new NotImplementedException();
        }
    }

}
