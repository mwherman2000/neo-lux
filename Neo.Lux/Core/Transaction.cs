using Neo.Lux.Cryptography;
using Neo.Lux.Cryptography.ECC;
using Neo.Lux.Utils;
using System;
using System.IO;
using System.Linq;

namespace Neo.Lux.Core
{
    public enum ContractPropertyState : byte
    {
        NoProperty = 0,

        HasStorage = 1 << 0,
        HasDynamicInvoke = 1 << 1,
        Payable = 1 << 2
    }

    public enum AssetType : byte
    {
        CreditFlag = 0x40,
        DutyFlag = 0x80,

        GoverningToken = 0x00,
        UtilityToken = 0x01,
        Currency = 0x08,
        Share = DutyFlag | 0x10,
        Invoice = DutyFlag | 0x18,
        Token = CreditFlag | 0x20,
    }

    public enum TransactionAttributeUsage
    {
        ContractHash = 0x00,
        ECDH02 = 0x02,
        ECDH03 = 0x03,
        Script = 0x20,
        Vote = 0x30,
        DescriptionUrl = 0x81,
        Description = 0x90,

        Hash1 = 0xa1,
        Hash2 = 0xa2,
        Hash3 = 0xa3,
        Hash4 = 0xa4,
        Hash5 = 0xa5,
        Hash6 = 0xa6,
        Hash7 = 0xa7,
        Hash8 = 0xa8,
        Hash9 = 0xa9,
        Hash10 = 0xaa,
        Hash11 = 0xab,
        Hash12 = 0xac,
        Hash13 = 0xad,
        Hash14 = 0xae,
        Hash15 = 0xaf,

        Remark = 0xf0,
        Remark1 = 0xf1,
        Remark2 = 0xf2,
        Remark3 = 0xf3,
        Remark4 = 0xf4,
        Remark5 = 0xf5,
        Remark6 = 0xf6,
        Remark7 = 0xf7,
        Remark8 = 0xf8,
        Remark9 = 0xf9,
        Remark10 = 0xfa,
        Remark11 = 0xfb,
        Remark12 = 0xfc,
        Remark13 = 0xfd,
        Remark14 = 0xfe,
        Remark15 = 0xff
    }

    public struct TransactionAttribute
    {
        public TransactionAttributeUsage Usage;
        public byte[] Data;

        public static TransactionAttribute Unserialize(BinaryReader reader)
        {
            var Usage = (TransactionAttributeUsage) reader.ReadByte();

            byte[] Data;

            if (Usage == TransactionAttributeUsage.ContractHash || Usage == TransactionAttributeUsage.Vote || ( Usage >= TransactionAttributeUsage.Hash1 && Usage <= TransactionAttributeUsage.Hash15))
                Data = reader.ReadBytes(32);
            else if (Usage == TransactionAttributeUsage.ECDH02 || Usage == TransactionAttributeUsage.ECDH03)
                Data = new[] { (byte)Usage }.Concat(reader.ReadBytes(32)).ToArray();
            else if (Usage == TransactionAttributeUsage.Script)
                Data = reader.ReadBytes(20);
            else if (Usage == TransactionAttributeUsage.DescriptionUrl)
                Data = reader.ReadBytes(reader.ReadByte());
            else if (Usage == TransactionAttributeUsage.Description || Usage >= TransactionAttributeUsage.Remark)
                Data = reader.ReadVarBytes(ushort.MaxValue);
            else
                throw new NotImplementedException();

            return new TransactionAttribute() { Usage = Usage, Data = Data};
        }
    }

    public struct Witness
    {
        public byte[] invocationScript;
        public byte[] verificationScript;

        public void Serialize(BinaryWriter writer)
        {
            writer.WriteVarBytes(this.invocationScript);
            writer.WriteVarBytes(this.verificationScript);
        }

        public static Witness Unserialize(BinaryReader reader)
        {
            var invocationScript = reader.ReadVarBytes(65536);
            var verificationScript = reader.ReadVarBytes(65536);
            return new Witness() { invocationScript = invocationScript, verificationScript = verificationScript };
        }
    }

    public enum TransactionType : byte
    {
        MinerTransaction = 0x00,
        IssueTransaction = 0x01,
        ClaimTransaction = 0x02,
        EnrollmentTransaction = 0x20,
        RegisterTransaction = 0x40,
        ContractTransaction = 0x80,
        StateTransaction = 0x90,
        PublishTransaction = 0xd0,
        InvocationTransaction = 0xd1
    }

    public class Transaction
    {
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

        public TransactionType type;
        public byte version;
        public byte[] script;
        public decimal gas;

        public Input[] inputs;
        public Output[] outputs;
        public Witness[] witnesses;

        public Input[] references;
        public TransactionAttribute[] attributes;

        #region HELPERS
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
                        case TransactionType.InvocationTransaction:
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
                            witness.Serialize(writer);
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
            this.witnesses = new Witness[] { new Witness() { invocationScript = invocationScript, verificationScript = verificationScript } };
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

        public static Transaction Unserialize(BinaryReader reader)
        {
            var tx = new Transaction();

            tx.type = (TransactionType) reader.ReadByte();
            tx.version = reader.ReadByte();

            switch (tx.type)
            {
                case TransactionType.InvocationTransaction:
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

                case TransactionType.MinerTransaction:
                    {
                        var Nonce = reader.ReadUInt32();
                        break;
                    }

                case TransactionType.ClaimTransaction:
                    {
                        var len = (int)reader.ReadVarInt((ulong)0x10000000);
                        tx.references = new Input[len];
                        for (int i = 0; i < len; i++)
                        {
                            tx.references[i] = Transaction.UnserializeTransactionInput(reader);
                        }

                        break;
                    }

                case TransactionType.ContractTransaction:
                    {
                        break;
                    }

                case TransactionType.PublishTransaction:
                    {
                        var Script = reader.ReadVarBytes();
                        var ParameterList = reader.ReadVarBytes();
                        var ReturnType = reader.ReadByte();
                        bool NeedStorage;
                        if (tx.version >= 1)
                            NeedStorage = reader.ReadBoolean();
                        else
                            NeedStorage = false;
                        var Name = reader.ReadVarString();
                        var CodeVersion = reader.ReadVarString();
                        var Author = reader.ReadVarString();
                        var Email = reader.ReadVarString();
                        var Description = reader.ReadVarString();
                        break;
                    }

                case TransactionType.EnrollmentTransaction:
                    {
                        var PublicKey = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
                        break;
                    }

                case TransactionType.RegisterTransaction:
                    {
                        var AssetType = (AssetType)reader.ReadByte();
                        var Name = reader.ReadVarString();
                        var Amount = reader.ReadFixed();
                        var Precision = reader.ReadByte();
                        var Owner = ECPoint.DeserializeFrom(reader, ECCurve.Secp256r1);
                        if (Owner.IsInfinity && AssetType != AssetType.GoverningToken && AssetType != AssetType.UtilityToken)
                            throw new FormatException();
                        var Admin = reader.ReadBytes(20);
                        break;
                    }

                case TransactionType.IssueTransaction:
                    {
                        break;
                    }

                default:
                    {
                        throw new NotImplementedException();
                    }
            }

            var attrCount = (int)reader.ReadVarInt(16);
            if (attrCount != 0)
            {
                tx.attributes = new TransactionAttribute[attrCount];
                for (int i = 0; i < attrCount; i++)
                {
                    tx.attributes[i] = TransactionAttribute.Unserialize(reader);
                }
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
            for (int i = 0; i < witnessCount; i++)
            {
                tx.witnesses[i] = Witness.Unserialize(reader);
            }

            return tx;
        }

        public static Transaction Unserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return Unserialize(reader);
                }
            }
        }
    }

}
