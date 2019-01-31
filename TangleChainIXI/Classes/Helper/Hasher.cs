using System;
using System.Collections.Generic;
using System.Text;
using Tangle.Net.Cryptography;
using Tangle.Net.Cryptography.Curl;
using Tangle.Net.Entity;

namespace TangleChainIXI.Classes.Helper
{
    public static class Hasher
    {

        public static string Hash(int length, params object[] objects)
        {

            StringBuilder builder = new StringBuilder();

            foreach (var obj in objects)
            {
                builder.Append(obj.ToString());
            }

            Curl sponge = new Curl();
            sponge.Absorb(TryteString.FromAsciiString(builder.ToString()).ToTrits());

            var hash = new int[length * 3];
            sponge.Squeeze(hash);

            return Converter.TritsToTrytes(hash);

        }
    }
}
