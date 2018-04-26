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
            writer.Write(output.assetID);
            writer.WriteFixed(output.value);
            writer.Write(output.scriptHash);
        }

        protected static Witness UnserializeWitness(BinaryReader reader)
        {
            var invocationScriptLength = reader.ReadByte();
            var invocationScript = reader.ReadBytes(invocationScriptLength);
            var verificationScriptLength = reader.ReadByte();
            var verificationScript = reader.ReadBytes(verificationScriptLength);
            return new Witness() { invocationScript = invocationScript, verificationScript = verificationScript };
        }

        protected static Input UnserializeTransactionInput(BinaryReader reader)
        {
            var prevHash = reader.ReadBytes(32);
            var prevIndex = reader.ReadUInt16();
            return new Input() { prevHash = prevHash, prevIndex = prevIndex };
        }

        protected static Output UnserializeTransactionOutput(BinaryReader reader)
        {
            var assetID = reader.ReadBytes(32);
            var value = reader.ReadFixed();
            var scriptHash = reader.ReadBytes(20);
            return new Output() { assetID = assetID, value = value, scriptHash = scriptHash };
        }
        #endregion

        public byte[] Serialize(bool signed = true)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write((byte)this.type);
                    writer.Write((byte)this.version);

                    // exclusive data
                    switch (this.type)
                    {
                        case 0xd1:
                            {
                                writer.WriteVarInt(this.script.Length);
                                writer.Write(this.script);
                                if (this.version >= 1)
                                {
                                    writer.WriteFixed(this.gas);
                                }
                                break;
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
            var tx = new Transaction();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    tx.type = reader.ReadByte();
                    tx.version = reader.ReadByte();

                    switch (tx.type)
                    {
                        case 0xd1:
                            {
                                var scriptLength = reader.ReadVarInt();
                                tx.script = reader.ReadBytes((int)scriptLength);

                                if (tx.version >= 1)
                                {
                                    tx.gas = reader.ReadFixed();
                                }
                                else
                                {
                                    tx.gas = 0;
                                }

                                break;
                            }
                    }

                    var attrCount = reader.ReadByte();
                    if (attrCount != 0)
                    {
                        throw new NotImplementedException();
                    }

                    var inputCount = (int)reader.ReadVarInt();
                    tx.inputs = new Input[inputCount];
                    for (int i = 0; i < inputCount; i++)
                    {
                        tx.inputs[i] = UnserializeTransactionInput(reader);
                    }

                    var outputCount = (int)reader.ReadVarInt();
                    tx.outputs = new Output[outputCount];
                    for (int i = 0; i < outputCount; i++)
                    {
                        tx.outputs[i] = UnserializeTransactionOutput(reader);
                    }

                    var witnessCount = (int)reader.ReadVarInt();
                    tx.witnesses = new Witness[witnessCount];
                    for (int i=0; i< witnessCount; i++)
                    {
                        tx.witnesses[i] = UnserializeWitness(reader);
                    }
                }
            }

            return tx;
        }
    }

}
