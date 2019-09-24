using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Tangle.Net.Repository;
using Tangle.Net.Repository.DataTransfer;
using TangleChainIXI.Classes;
using TangleChainIXI.Classes.Helper;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleNet = Tangle.Net.Entity;

namespace IXIComponents.Simple
{
    public class SimpleTangleAccessor : ITangleAccessor
    {

        private readonly RestIotaRepository _repository;
        private readonly IXISettings _settings;

        public SimpleTangleAccessor(IXISettings settings)
        {
            _repository = new RestIotaRepository(new RestClient(settings.NodeAddress));
            _settings = settings;
        }

        public Maybe<T> GetSpecificFromAddress<T>(string hash, string address) where T : IDownloadable
        {
            var objList = GetAllFromAddress<T>(address);

            foreach (T obj in objList)
            {
                if (obj.Hash.Equals(hash))
                {
                    return Maybe<T>.Some(obj);
                }
            }

            return Maybe<T>.None;
        }

        private readonly Dictionary<string, IDownloadable> _simpleCache = new Dictionary<string, IDownloadable>();

        public List<T> GetAllFromAddress<T>(string address) where T : IDownloadable
        {
            //create object list
            var list = new List<T>();

            var addressList = new List<TangleNet::Address>
            {
                new TangleNet::Address(address)
            };

            var bundleHashList = _repository.FindTransactionsByAddresses(addressList);

            for (var i = 0; i < bundleHashList.Hashes.Count; i++)
            {
                var hash = bundleHashList.Hashes[i].Value;

                if (_simpleCache.ContainsKey(hash))
                {
                    var obj = _simpleCache[hash];

                    if (obj is T)
                    {
                        list.Add((T) _simpleCache[hash]);
                        bundleHashList.Hashes.RemoveAt(i);
                        i--;
                        Console.WriteLine("We loaded something from cache");
                    }
                }
            }

            var bundles = _repository.GetBundles(bundleHashList.Hashes, false);

            foreach (TangleNet::Bundle bundle in bundles)
            {
                string json = bundle.AggregateFragments().ToUtf8String();

                var newTrans = Utils.FromJSON<T>(json);

                if (newTrans.HasValue)
                {
                    list.Add(newTrans.Value);
                    bundle.Transactions.ForEach(x => _simpleCache[x.Hash.Value] = newTrans.Value);
                }
            }

            return list;
        }
    }
}