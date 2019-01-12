using System;
using System.Collections.Generic;
using System.Text;
using RestSharp;
using Tangle.Net.Repository;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;
using TangleChainIXI.Smartcontracts;
using TangleNet = Tangle.Net.Entity;

namespace TangleChainIXI.NewClasses
{
    public class SimpleTangleAccessor : ITangleAccessor
    {

        public Transaction GetTransaction(string hash, string address)
        {
            return GetSpecificFromAddress<Transaction>(address, hash);
        }

        public Smartcontract GetSmartcontract(string hash, string address)
        {
            return GetSpecificFromAddress<Smartcontract>(address, hash);
        }

        public Block GetBlock(string hash, string address)
        {
            return GetSpecificFromAddress<Block>(address, hash);
        }

        public List<Block> GetBlocks(string address)
        {
            return GetAllFromAddress<Block>(address);
        }

        private T GetSpecificFromAddress<T>(string address, string hash) where T : IDownloadable
        {
            var objList = GetAllFromAddress<T>(address);

            foreach (T obj in objList)
            {
                if (obj.Hash.Equals(hash))
                {
                    return obj;
                }
            }

            return default(T);
        }

        private List<T> GetAllFromAddress<T>(string address) where T : IDownloadable
        {
            //create object list
            var list = new List<T>();

            var repository = new RestIotaRepository(new RestClient(IXISettings.NodeAddress));
            var addressList = new List<TangleNet::Address>() {
                new TangleNet::Address(address)
            };

            var bundleList = repository.FindTransactionsByAddresses(addressList);
            var bundles = repository.GetBundles(bundleList.Hashes, false);

            foreach (TangleNet::Bundle bundle in bundles)
            {

                string json = bundle.AggregateFragments().ToUtf8String();

                T newTrans = Utils.FromJSON<T>(json);

                if (newTrans != null)
                    list.Add(newTrans);

            }

            return list;
        }
    }
}
