using Neo.Cryptography;
using System.Numerics;

namespace Neo.Lux
{
    public class NEP5
    {
        private readonly byte[] contractHash;
        private readonly NeoAPI api;

        public NEP5(NeoAPI api, string contractHash) :  this(api, NeoAPI.GetScriptHashFromString(contractHash))
        {

        }

        public NEP5(NeoAPI api, byte[] contractHash)
        {
            this.api = api;
            this.contractHash = contractHash;
        }

        private string _name = null;
        public string Name
        {
            get
            {
                try
                {
                    if (_name == null)
                    {
                        var response = api.TestInvokeScript(contractHash, "name", new object[] { "" });
                        _name = System.Text.Encoding.ASCII.GetString((byte[])response.result[0]);
                    }

                    return _name;
                }
                catch
                {
                    throw new NeoException("Api did not return a value.");
                }

            }
        }

        private string _symbol = null;
        public string Symbol
        {
            get
            {
                if (_symbol == null)
                {
                    var response = api.TestInvokeScript(contractHash, "symbol", new object[] { "" });
                    _symbol = System.Text.Encoding.ASCII.GetString((byte[])response.result[0]);
                }

                return _symbol;
            }
        }

        private BigInteger _decimals = -1;
        public BigInteger Decimals
        {
            get
            {
                try
                {
                    if (_decimals < 0)
                    {
                        var response = api.TestInvokeScript(contractHash, "decimals", new object[] { "" });
                        _decimals = (BigInteger)response.result[0];
                    }

                    return _decimals;
                }
                catch
                {
                    throw new NeoException("Api did not return a value.");
                }

            }
        }

        private BigInteger _totalSupply = -1;
        public BigInteger TotalSupply
        {
            get
            {
                try
                {
                    if (_totalSupply < 0)
                    {
                        var response = api.TestInvokeScript(contractHash, "totalSupply", new object[] { "" });
                        _totalSupply = new BigInteger((byte[])response.result[0]);

                        var decs = Decimals;
                        while (decs > 0)
                        {
                            _totalSupply /= 10;
                            decs--;
                        }
                    }

                    return _totalSupply;

                }
                catch
                {
                    throw new NeoException("Api did not return a value.");
                }

            }
        }

        // FIXME - I'm almost sure that this code won't return non-integer balances correctly...
        private decimal ConvertToDecimal(BigInteger value)
        {
            var decs = this.Decimals;
            while (decs > 0)
            {
                value /= 10;
                decs--;
            }
            return (decimal)value;
        }

        private BigInteger ConvertToBigInt(decimal value)
        {
            var decs = this.Decimals;
            while (decs > 0)
            {
                value *= 10;
                decs--;
            }
            return new BigInteger((ulong)value);
        }

        public decimal BalanceOf(string address)
        {
            return BalanceOf(address.GetScriptHashFromAddress());
        }

        public decimal BalanceOf(byte[] addressHash)
        {
            try
            {
                var response = api.TestInvokeScript(contractHash, "balanceOf", new object[] { addressHash });
                var balance = new BigInteger((byte[])response.result[0]);
                return ConvertToDecimal(balance);
            }
            catch
            {
                throw new NeoException("Api did not return a value.");
            }
        }

        public Transaction Transfer(KeyPair from_key, string to_address, decimal value)
        {
            return Transfer(from_key, to_address.GetScriptHashFromAddress(), value);
        }

        public Transaction Transfer(KeyPair from_key, byte[] to_address_hash, decimal value)
        {
            BigInteger amount = ConvertToBigInt(value);

            var sender_address_hash = from_key.address.GetScriptHashFromAddress();
            var response = api.CallContract(from_key, contractHash, "transfer", new object[] { sender_address_hash, to_address_hash, amount });
            return response;
        }

        // optional methods, not all NEP5 support this!

        public decimal Allowance(string from_address, string to_address)
        {
            return Allowance(from_address.GetScriptHashFromAddress(), to_address.GetScriptHashFromAddress());

        }

        public decimal Allowance(byte[] from_address_hash, byte[] to_address_hash)
        {
            var response = api.TestInvokeScript(contractHash, "allowance", new object[] { from_address_hash, to_address_hash });

            try
            {
                return ConvertToDecimal((BigInteger)response.result[0]);
            }
            catch
            {
                throw new NeoException("Api did not return a value.");
            }

        }

        public Transaction TransferFrom(byte[] originator, byte[] from, byte[] to, BigInteger amount)
        {
            throw new System.NotImplementedException();
        }

        public Transaction Approve(byte[] originator, byte[] to, BigInteger amount)
        {
            throw new System.NotImplementedException();
        }
    }
}
