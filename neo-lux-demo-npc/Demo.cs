using System;

using Neo.Cryptography;

namespace NeoLux.Demo
{
    class Demo
    {
        static void Main(string[] args)
        {
            // NOTE - You can also create an API instance for a specific private net
            var api = NeoDB.ForTestNet();

            //const string NeoDraw_ContractHash = "694ebe0840d1952b09f5435152eebbbc1f8e4b8e";
            //var response = api.TestInvokeScript(NeoDraw_ContractHash, new object[] { "getx", "user", new object[] { "100" } });
            //TEST CASE 1: object[] results = { -1 }; return results; /* WORKS */
            //{ "jsonrpc":"2.0","id":1,
            //  "result":{ "script":"0331303051c104757365720467657478678e4b8e1fbcbbee525143f5092b95d14008be4e69",
            //      "state":"HALT, BREAK","gas_consumed":"0.47",
            //      "stack":[
            //          { "type":"ByteArray", { "value":"756e6b6e6f776e206f7065726174696f6e2027"},
            //          { "type":"Array","value":[
            //              { "type":"Integer","value":"-1"}]}]}}

            //const string NeoDraw_ContractHash = "694ebe0840d1952b09f5435152eebbbc1f8e4b8e";
            //var response = api.TestInvokeScript(NeoDraw_ContractHash, new object[] { "get", "user", new object[] { "100" } });
            //TEST CASE 2: UserCredentials uc = FindUser(AppVAU, encodedUsername); results = new object[] { uc }; return results; /* DOESN'T WORK */
            //{ "jsonrpc":"2.0","id":1,
            //  "result":{ "script":"0331303051c1047573657203676574678e4b8e1fbcbbee525143f5092b95d14008be4e69",
            //      "state":"HALT, BREAK","gas_consumed":"2.082",
            //      "stack":[
            //          { "type":"Array","value":[
            //              {"type":"Array","value":[
            //                  {"type":"ByteArray","value":"313030"},{"type":"ByteArray","value":"313030"},{"type":"Integer","value":"4"},
            //                  {"type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  {"type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  {"type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  {"type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false}]}]}]}}

            const string NeoDraw_ContractHash = "694ebe0840d1952b09f5435152eebbbc1f8e4b8e";
            var response = api.TestInvokeScript(NeoDraw_ContractHash, new object[] { "getall", "point", new object[] { "100" } });
            // TEST CASE 3: UserPoint[] points = new UserPoint[(int)nPoints]; results = points; return results;
            //{ "jsonrpc":"2.0","id":1,
            //  "result":{ "script":"0331303051c105706f696e7406676574616c6c678e4b8e1fbcbbee525143f5092b95d14008be4e69",
            //      "state":"HALT, BREAK","gas_consumed":"7.724",
            //      "stack":[
            //          { "type":"Array","value":[
            //              { "type":"Array","value":[
            //                  { "type":"ByteArray","value":"3130"},{"type":"ByteArray","value":"3230"},{"type":"Integer","value":"4"},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false}]},
            //              { "type":"Array","value":[
            //                  { "type":"ByteArray","value":"3430"},{"type":"ByteArray","value":"3630"},{"type":"Integer","value":"4"},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false}]},
            //              { "type":"Array","value":[
            //                  { "type":"ByteArray","value":"35"},{"type":"ByteArray","value":"35"},{"type":"Integer","value":"4"},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false},
            //                  { "type":"Boolean","value":false},{"type":"Boolean","value":false},{"type":"Boolean","value":false}]}]}]}}                                                                     //TEST CASE 3: UserPoint[] points = new UserPoint[(int)nPoints]; results = points; return results; // DOESN'T WORK

            object[] resultsArray = (object[])response.result;
            Console.WriteLine("resultsArray.length: " + resultsArray.Length);
            int raIndex = 0;
            foreach (object resultsElement in resultsArray)
            {
                Console.WriteLine("resultsElement:" + resultsElement.GetType().Name);
                if (resultsElement.GetType().Name != "Object[]")
                {
                    Console.WriteLine("resultsElement:\t" + resultsElement.ToString());
                }
                else
                {
                    int rIndex = 0;
                    object[] results = (object[])resultsElement;
                    if (results != null)
                    {
                        Console.WriteLine("results.length: " + results.Length);
                        foreach (object result in results)
                        {
                            Console.WriteLine("result:\t" + raIndex.ToString() + "\t" + rIndex.ToString() + "\t" + result.ToString() + "\t" + result.GetType().Name);
                            if (result.GetType().Name == "Object[]")
                            {
                                int oooooIndex = 0;
                                foreach (object ooooo in (object[])result)
                                {
                                    if (ooooo != null)
                                    {
                                        Console.WriteLine("ooooo:\t" + raIndex.ToString() + "\t" + rIndex.ToString() + "\t" + oooooIndex.ToString() + "\t" + ooooo.ToString() + "\t" + ooooo.GetType().Name);
                                        if (ooooo.GetType().Name == "Object[]")
                                        {
                                            int ooooIndex = 0;
                                            foreach (object oooo in (object[])ooooo)
                                            {
                                                if (oooo != null)
                                                {
                                                    Console.WriteLine("oooo:\t" + raIndex.ToString() + "\t" + rIndex.ToString() + "\t" + oooooIndex.ToString() + "\t" + ooooIndex.ToString() + "\t" + oooo.ToString() + "\t" + oooo.GetType().Name);
                                                    ooooIndex++;
                                                }
                                            }
                                        }
                                        oooooIndex++;
                                    }
                                }
                            }
                            rIndex++;
                        }
                    }
                    raIndex++;
                }
            }

            Console.WriteLine("Press Enter to Exit...");
            Console.ReadLine();
        }
    }
}
