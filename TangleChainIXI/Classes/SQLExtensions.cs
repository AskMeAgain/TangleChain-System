using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using TangleChainIXI.Smartcontracts;
using TangleChainIXI.Smartcontracts.Classes;

namespace TangleChainIXI.Classes
{
    public static class SQLExtensions
    {
        public static Block ToBlock(this SQLiteDataReader reader, string name)
        {
            return new Block()
            {
                Height = (int)reader[0],
                Nonce = (int)reader[1],
                Time = (long)reader[2],
                Hash = (string)reader[3],
                NextAddress = (string)reader[4],
                Owner = (string)reader[5],
                SendTo = (string)reader[6],
                CoinName = name,
                Difficulty = (int)reader[7],
                TransactionHashes = new List<string>(),
                SmartcontractHashes = new List<string>()
            };
        }


        public static Transaction ToTransaction(this SQLiteDataReader reader, List<int> value, List<string> receiver, List<string> data)
        {
            return new Transaction()
            {
                Hash = (string)reader[1],
                Time = (long)reader[2],
                From = (string)reader[3],
                Signature = (string)reader[4],
                Mode = (int)reader[5],
                OutputValue = value,
                OutputReceiver = receiver,
                Data = data
            };
        }


        public static Smartcontract ToSmartcontract(this SQLiteDataReader reader)
        {
            return new Smartcontract()
            {
                Name = (string)reader[1],
                Hash = (string)reader[2],
                Code = SmartcontractUtils.StringToCode((string)reader[3]),
                From = (string)reader[4],
                Signature = (string)reader[5],
                TransactionFee = (int)reader[6],
                SendTo = (string)reader[7],
                ReceivingAddress = (string)reader[8]
            };
        }
    }
}
