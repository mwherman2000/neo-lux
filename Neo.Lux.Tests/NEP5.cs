using Neo.Lux.Core;
using NUnit.Framework;

namespace Neo.Lux.Tests
{
    [TestFixture]
    public class NEP5Tests
    {
        private NeoAPI api;
        private NEP5 token;

        [OneTimeSetUp]
        public void Init()
        {
            api = NeoDB.ForMainNet();
            token = api.GetToken("RPX");
        }

        [Test]
        public void GetSymbol()
        {
            var symbol = token.Symbol;
            Assert.IsTrue(symbol == "RPX");
        }

        [Test]
        public void GetName()
        {
            var name = token.Name;
            Assert.IsTrue(name == "Red Pulse Token");
        }

        [Test]
        public void GetDecimals()
        {
            var decimals = token.Decimals;
            Assert.IsTrue(decimals == 8);
        }

    }
}
