#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;
using System.IO;

#endregion

namespace AibuSet {

    internal class HcReader {

        #region Fields

        private string code;
        private string version;
        private char[] sepK;
        private char[] sepD;
        private string fileName;
        private byte[] M;
        private byte[] D;
        private byte[] E;

        #endregion

        #region Constructor

        internal HcReader() {
            sepK = new char[] { ' ' };
            sepD = new char[] { '-' };

            code = "AILogSys";
            version = "1.0";
            fileName = "AILicKey.txt";

            M = new byte[] { 161,85,57,179,120,246,157,30,35,96,191,227,35,97,31,237,163,85,112,226,115,94,221,186,158,130,132,249,164,97,94,136,88,148,22,218,116,151,94,225,121,168,129,18,214,195,255,75,55,236,13,93,238,163,146,163,92,18,118,2,167,244,80,109,236,115,65,3,0,192,81,128,115,115,19,64,96,16,180,188,84,9,193,226,254,171,222,252,45,181,125,17,141,55,217,12,246,202,181,11,90,208,155,152,232,128,97,110,7,5,203,51,105,150,55,235,202,63,168,199,16,237,143,49,190,152,189,3 };
            D = new byte[] { 96,13,168,212,5,1,247,91,153,62,162,32,110,209,5,188,107,145,148,43,14,251,125,81,253,203,193,182,12,59,120,151,254,252,244,122,78,14,77,140,58,237,171,35,119,64,172,63,177,100,214,52,133,191,87,175,241,66,128,134,102,78,220,143,19,106,18,245,23,223,127,255,66,229,145,39,115,134,188,51,21,58,193,71,85,93,159,171,0,153,32,209,209,28,24,105,161,63,235,90,116,158,87,202,218,202,74,249,192,225,29,197,185,111,222,122,92,144,60,194,1,63,169,71,180,103,39,121 };
            E = new byte[] { 1, 0, 1 };
        }

        //for testing
        internal HcReader(string code, string version, string fileName, string pkFileName) {
            sepK = new char[] { ' ' };
            sepD = new char[] { '-' };

            this.code = code;
            this.version = version;
            this.fileName = fileName;
            GetMDEFromPkFile(pkFileName);
        }

        #endregion

        #region internal Methods

        internal bool Process(string fileName) {
            byte[] encData = GetEncData(fileName);  //foreach (byte b in encData) { Console.Write(b + " "); }
            RSAParameters key = new RSAParameters(); key.Modulus = M; key.D = D; key.Exponent = E;
            string txtKey = Decode(encData, key); //Console.WriteLine("\n\n " + txtKey);
            return ProcessKey(txtKey);
        }

        #endregion

        #region Private Methods

        private void SetRSAParams(RSAParameters rsaParams) {
            M = rsaParams.Modulus;
            D = rsaParams.D;
            E = rsaParams.Exponent;
        }

        private byte[] GetEncData(string fileName) {
            List<byte> bytes = new List<byte>();
            try {
                StreamReader sr = new StreamReader(fileName);
                string line;
                while ((line = sr.ReadLine()) != null) { bytes.AddRange(ToByteArray(line)); }
                return bytes.ToArray();
            }
            catch(Exception ex) {
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        private string Decode(byte[] encData, RSAParameters privKey) {
            byte[] decBytes = DecryptPrivate(encData, privKey);
            byte[] decData = decBytes.SkipWhile(x => x != 0).Skip(1).ToArray();
            return ToString(decData);
        }

        private byte[] DecryptPrivate(byte[] data, RSAParameters key) {
            Params pars = GetParams(key);
            return Calculate(data, pars.privExponent, pars.modulus);
        }

        private Params GetParams(RSAParameters key) {
            Params rsap = new Params();
            rsap.modulus = new BigInteger(key.Modulus.Reverse().Concat(new byte[] { 0 }).ToArray());
            rsap.privExponent = new BigInteger(key.D.Reverse().Concat(new byte[] { 0 }).ToArray());
            rsap.pubExponent = new BigInteger(key.Exponent.Reverse().Concat(new byte[] { 0 }).ToArray());
            return rsap;
        }

        private static byte[] Calculate(byte[] data, BigInteger exp, BigInteger mod) {
            BigInteger bigData = new BigInteger(data.Reverse().Concat(new byte[] { 0 }).ToArray());
            return BigInteger.ModPow(bigData, exp, mod).ToByteArray().Reverse().ToArray();
        }

        internal struct Params {
            internal BigInteger modulus;
            internal BigInteger privExponent;
            internal BigInteger pubExponent;
        }

        private string ToString(byte[] bytes) {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) { sb.Append(Convert.ToChar(b)); }
            return sb.ToString();
        }

        private byte[] ToByteArray(string str) {
            string[] tokens = str.Split(sepK);
            byte[] res = new byte[tokens.Length - 1];
            for (int i = 0; i < tokens.Length - 1; i++) { res[i] = Convert.ToByte(tokens[i]); }
            return res;
        }

        private bool ProcessKey(string key) {
            string[] kTokens = key.Split(sepK);
            if (kTokens.Length != 3) { return false; }
            if (kTokens[0] != code) { return false; }
            if (kTokens[1] != version) { return false; }
            string[] dTokens = kTokens[2].Split(sepD);
            if (dTokens.Length != 3) { return false; }
            DateTime end = DateTime.MaxValue;
            try { end = new DateTime(Convert.ToInt32(dTokens[0]), Convert.ToInt32(dTokens[1]), Convert.ToInt32(dTokens[2])); }
            catch { }
            if (end.Date < DateTime.Now.Date) { return false; }
            return true;
        }

        private void GetMDEFromPkFile(string pkFileName) {
            char[] sep = new char[] { ' ' };
            string[] tokens;
            try {
                StreamReader sr = new StreamReader(pkFileName);
                string line;
                line = sr.ReadLine();  //PRIVATE KEY
                line = sr.ReadLine();  //
                line = sr.ReadLine();  //M:
                line = sr.ReadLine();  //M array
                tokens = line.Split(sep);
                M = new byte[tokens.Length - 1];
                for (int i = 0; i < tokens.Length - 1; i++) { M[i] = Convert.ToByte(tokens[i]); }
                line = sr.ReadLine();  //D:
                line = sr.ReadLine();  //D array
                tokens = line.Split(sep);
                D = new byte[tokens.Length - 1];
                for (int i = 0; i < tokens.Length - 1; i++) { D[i] = Convert.ToByte(tokens[i]); }
                line = sr.ReadLine();  //E:
                line = sr.ReadLine();  //E array
                tokens = line.Split(sep);
                E = new byte[tokens.Length - 1];
                for (int i = 0; i < tokens.Length - 1; i++) { E[i] = Convert.ToByte(tokens[i]); }
            }
            catch {
                Console.WriteLine("Error");
            }
        }

        #endregion

    }
}
