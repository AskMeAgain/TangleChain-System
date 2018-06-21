using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto.Parameters;
using System.Numerics;
using BC = Org.BouncyCastle;
using Nethereum.Signer;

namespace TangleChainIXI {
    public static class Cryptography {
        public static string CryptoSign(string data, string privKey) {

            MessageSigner gen = new MessageSigner();

            byte[] msg = Encoding.ASCII.GetBytes(data);

            return gen.HashAndSign(msg, privKey);
        }

        public static string GetPublicKey(string s) {
            return EthECKey.GetPublicAddress(s);
        }

        public static bool VerifyMessage(string message ,string signature, string pubKey) {

            MessageSigner gen = new MessageSigner();

            var addr = gen.HashAndEcRecover(message, signature);

            return (addr.Equals(pubKey))?true:false;
        }
    }
        

}
