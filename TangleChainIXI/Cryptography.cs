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
using Nethereum.Hex.HexConvertors;

namespace TangleChainIXI {
    public static class Cryptography {
        public static string Sign(string data, string privKey) {

            EthereumMessageSigner gen = new EthereumMessageSigner();

            HexUTF8StringConvertor conv = new HexUTF8StringConvertor();
            string hexPrivKey = conv.ConvertToHex(privKey);

            return gen.EncodeUTF8AndSign(data, new EthECKey(hexPrivKey));
        }

        public static string GetPublicKey(string s) {

            HexUTF8StringConvertor conv = new HexUTF8StringConvertor();
            string hexPrivKey = conv.ConvertToHex(s);

            return EthECKey.GetPublicAddress(hexPrivKey);
        }

        public static bool VerifyMessage(string message, string signature, string pubKey) {

            EthereumMessageSigner gen = new EthereumMessageSigner();

            var addr = gen.EncodeUTF8AndEcRecover(message, signature);

            return (addr.ToLower().Equals(pubKey.ToLower())) ? true : false;
        }
    }


}
