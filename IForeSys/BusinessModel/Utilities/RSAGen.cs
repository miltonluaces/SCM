#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Security.Cryptography;

#endregion

namespace AibuSet {

    internal class RSAGen {

        #region Fields
        
        private RSACryptoServiceProvider csp;
        private RSAParameters privateKey;
        private RSAParameters publicKey;
       
        #endregion
        
        #region Constructor

        internal RSAGen() { 
            csp = new RSACryptoServiceProvider();
        }

        #endregion

        #region Properties

        internal RSAParameters PrivateKey {
            get { return privateKey; }
        }

        internal RSAParameters PublicKey {
            get { return publicKey; }
        }

        #endregion

        #region internal Methods

        internal byte[] Code(string dataStr) {
            byte[] data = ToByteArray(dataStr);
            publicKey = csp.ExportParameters(false);
            privateKey = csp.ExportParameters(true);
            byte[] encData = csp.Encrypt(data, false);
            return encData;
        }

        internal string Decode(byte[] encData, RSAParameters privKey) {
            byte[] decBytes = DecryptPrivate(encData, privKey);
            byte[] decData = decBytes.SkipWhile(x => x != 0).Skip(1).ToArray();
            return ToString(decData);
        }

        #endregion

        #region Private Methods

        #region Main Methods

        internal byte[] EncryptPrivate(byte[] data, RSAParameters key) {
            Params pars = GetParams(key);
            return Calculate(data, pars.privExponent, pars.modulus);
        }
        internal byte[] EncryptPublic(byte[] data, RSAParameters key) {
            Params pars = GetParams(key);
            return Calculate(data, pars.pubExponent, pars.modulus);
        }
        internal byte[] DecryptPrivate(byte[] data, RSAParameters key) {
            Params pars = GetParams(key);
            return Calculate(data, pars.privExponent, pars.modulus);
        }
        internal byte[] DecryptPublic(byte[] data, RSAParameters key) {
            Params pars = GetParams(key);
            return Calculate(data, pars.pubExponent, pars.modulus);
        }

        #endregion

        #region Auxiliar Methods

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
            //data : our data block, Reverse(): as BigInteger has another order, append 0 only positive numbers, should be array
            //ModPow: the RSA operation itself, bytes from BigInteger, back to original order, return as byte array
        }

        #endregion

        #endregion

        #region Text methods

        private byte[] ToByteArray(string text) {
            char[] textChar = text.ToCharArray();
            byte[] res = new byte[textChar.Length];
            for (int i = 0; i < textChar.Length;i++) { res[i] = Convert.ToByte(textChar[i]); }
            return res;
        }

        private string ToString(byte[] bytes) {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) { sb.Append(Convert.ToChar(b)); }
            return sb.ToString();
        }
        
        #endregion

        #region Inner struct

        internal struct Params {
            internal BigInteger modulus;
            internal BigInteger privExponent;
            internal BigInteger pubExponent;
        }

        #endregion
    }
}
