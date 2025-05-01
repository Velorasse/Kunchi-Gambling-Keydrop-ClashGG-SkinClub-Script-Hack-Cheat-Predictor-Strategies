using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;
using Unichain.Exceptions;

namespace Unichain.Events
{
    public class NFTMint : ITransaction
    {
        #region default properties
        
        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.nft.mint";
        public string? Signature { get; set; }

        #endregion

        #region custom properties

        /// <summary>
        /// The unique Id for this Token
        /// </summary>
        public Guid NFTId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The custom metadata for this NFT
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        #endregion

        #region constructor

        public NFTMint(User actor,
            double fee,
            Dictionary<string, object> metadata) {
            Actor = actor;
            Fee = fee;
            Metadata = metadata;
        }

        #endregion

        #region Methods

        public string CalculateHash() {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{NFTId}-{Timestamp}-{JsonConvert.SerializeObject(Metadata, Formatting.None)}");
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public bool IsValid(Blockchain blockchain) {
            bool exists = blockchain.IsNFTMinted(NFTId);
            double balance = blockchain.GetBalance(Actor.Address);
            if (exists)
                return false;
            if (balance < Fee)
                return false;

            if (Signature is null)
                return false;
            var hash = CalculateHash();
            return Actor.VerifySignature(hash, Signature);
        }

        public void SignTransaction(PrivateKey? key = null) {
            string hash = CalculateHash();
            if (key is null)
                Signature = Actor.SignString(hash);
            else
                Signature = key.Sign(hash);
        }

        #endregion
    }
}
