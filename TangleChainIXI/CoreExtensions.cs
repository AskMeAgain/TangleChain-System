using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Tangle.Net.ProofOfWork;
using Tangle.Net.Repository;
using Tangle.Net.Utils;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleNet = Tangle.Net.Entity;


namespace TangleChainIXI
{
    public static class CoreExtensions
    {
        /// <summary>
        /// Uploads the object to the specified SendTo address inside the object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Upload<T>(this T obj) where T : IDownloadable
        {

            if (!obj.IsFinalized)
            {
                throw new ArgumentException("Object not finalized");
            }

            //prepare data
            var json = TangleNet::TryteString.FromUtf8String(Utils.ToJSON(obj));

            //send json to address
            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress), new PoWService(new CpuPearlDiver()));

            var bundle = new TangleNet::Bundle();
            bundle.AddTransfer(
                new TangleNet::Transfer
                {
                    Address = new TangleNet::Address(obj.SendTo),
                    Tag = new TangleNet::Tag("TANGLECHAIN"),
                    Message = json,
                    Timestamp = Timestamp.UnixSecondsTimestamp
                });

            bundle.Finalize();
            bundle.Sign();

            repository.SendTrytes(bundle.Transactions, 2, 14);

            return obj;
        }

        /// <summary>
        /// Uploads the object to the specified SendTo address inside the object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Task<T> UploadAsync<T>(this T obj) where T : IDownloadable
        {
            return Task.Run(() => Upload<T>(obj));
        }
    }
}
