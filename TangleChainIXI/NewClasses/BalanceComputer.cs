using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;
using TangleChainIXI.Classes;
using TangleChainIXI.Interfaces;

namespace TangleChainIXI.NewClasses
{
    public class BalanceComputer : IBalance
    {
        public IDataAccessor _dataAccessor { get; set; }

        public BalanceComputer(IDataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        public long GetBalance(string user)
        {

            //count all incoming money
            //get all block rewards & transfees
            var blockReward = GetBlockReward(user);

            //get all outputs who point towards you
            var OutputSum = GetIncomingOutputs(user);

            //count now all removing money
            //remove all outputs which are outgoing
            var OutgoingOutputs = GetOutgoingOutputs(user);

            //remove all transaction fees (transactions)
            var OutgoingTransfees = GetOutcomingTransFees(user);

            //remove all smartcontract fees
            var OutgoingSmartfees = GetOutcomingSmartFees(user);

            return blockReward + OutputSum + OutgoingOutputs + OutgoingTransfees + OutgoingSmartfees;
        }

        //private long GetOutcomingSmartFees(string user)
        //{
        //    string transFees = $"SELECT IFNULL(SUM(Fee),0) From Smartcontracts WHERE _FROM='{user}'";

        //    using (SQLiteDataReader reader = QuerySQL(transFees))
        //    {
        //        reader.Read();
        //        return (long)reader[0] * -1;
        //    }
        //}

        //private long GetOutcomingTransFees(string user)
        //{
        //    string transFees = $"SELECT IFNULL(SUM(Data),0) FROM Transactions JOIN Data ON Transactions.ID = Data.TransID WHERE _From='{user}' AND _ArrayIndex = 0";

        //    using (SQLiteDataReader reader = QuerySQL(transFees))
        //    {
        //        reader.Read();
        //        return (long)reader[0] * -1;
        //    }
        //}

        //private long GetOutgoingOutputs(string user)
        //{
        //    string sql_Outgoing = $"SELECT IFNULL(SUM(_Values),0) FROM Transactions JOIN Output ON Transactions.ID = Output.TransID WHERE _From='{user}' AND NOT Mode = -1;";

        //    using (SQLiteDataReader reader = QuerySQL(sql_Outgoing))
        //    {
        //        reader.Read();
        //        return (long)reader[0] * -1;
        //    }
        //}

        //private long GetIncomingOutputs(string user)
        //{
        //    string sql_Outputs = $"SELECT IFNULL(SUM(_Values),0) FROM Output WHERE Receiver='{user}';";

        //    using (SQLiteDataReader reader = QuerySQL(sql_Outputs))
        //    {
        //        reader.Read();
        //        return (long)reader[0];
        //    }
        //}

        //private long GetBlockReward(string user)
        //{

        //    string sql_blockRewards = $"SELECT IFNULL(COUNT(PublicKey),0) FROM Block WHERE PublicKey='{user}'";

        //    string sql_TransFees = "SELECT IFNULL(SUM(Data),0) FROM Block AS b JOIN Transactions AS t ON " +
        //        "b.Height = t.BlockID JOIN Data as d ON t.ID = d.TransID " +
        //        $"WHERE PublicKey = '{user}' AND _ArrayIndex = 0;";


        //    using (SQLiteDataReader reader = QuerySQL(sql_blockRewards), reader2 = QuerySQL(sql_TransFees))
        //    {

        //        reader.Read();
        //        reader2.Read();

        //        long blockReward = (long)reader[0] * ChainSettings.BlockReward;

        //        long TransFees = (long)reader2[0];

        //        return blockReward + TransFees;
        //    }
        //}
    }
}
