using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Tangle.Net.Repository;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleNet = Tangle.Net.Entity;

namespace SimpleCoreComponents
{
    public class SimpleTangleAccessor : ITangleAccessor
    {

        public T GetSpecificFromAddress<T>(string hash, string address, IXISettings settings) where T : IDownloadable
        {
            var objList = GetAllFromAddress<T>(address, settings);
            ;
            foreach (T obj in objList)
            {
                if (obj.Hash.Equals(hash))
                {
                    return obj;
                }
            }

            return default(T);
        }

        public List<T> GetAllFromAddress<T>(string address, IXISettings settings) where T : IDownloadable
        {
            //create object list
            var list = new List<T>();

            var repository = new RestIotaRepository(new RestClient(settings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, false);

            foreach (TangleNet::Bundle bundle in bundles)
            {

                string json = bundle.AggregateFragments().ToUtf8String();

                var newTrans = Utils.FromJSON<T>(json);

                if (newTrans.HasValue)
                    list.Add(newTrans.Value);

            }

            return list;
        }
    }
}
