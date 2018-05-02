using System;
using System.Collections.Generic;

namespace TangleChain {
    [Serializable]
    public class Block {
        public int Nonce { get; set; }
        public int Height { get; set; }

        public string Hash { get; set; }
        public string NextAddress { get; set; }
        public string Owner { get; set; }
        public string SendTo { get; set; }


        public override bool Equals(object obj) {

            Block newBlock = obj as Block;

            if (this.Nonce != newBlock.Nonce && this.Height != newBlock.Height) {
                return false;
            } else if (!this.Hash.Equals(newBlock.Hash)){
                return false;
            } else if (!this.NextAddress.Equals(newBlock.NextAddress)) {
                return false;
            } else if (!this.Owner.Equals(newBlock.Owner)) {
                return false;
            } else if (!this.SendTo.Equals(newBlock.SendTo)) {
                return false;
            }

            return true;
        }

        public override int GetHashCode() {
            var hashCode = -496733639;
            hashCode = hashCode * -1521134295 + Nonce.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Hash);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(NextAddress);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Owner);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SendTo);
            return hashCode;
        }

        public Block() {
            Nonce = 123456;
            Hash = "HASH";
            Height = 123456;
            NextAddress = "NEXTADDRESS";
            Owner = "OWNER";
            SendTo = "SENDTO";
        }


    }
}
