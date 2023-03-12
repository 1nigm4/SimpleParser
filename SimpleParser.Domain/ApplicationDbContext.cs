using SimpleParser.Domain.Entities;
using SimpleParser.Domain.Enums;
using System;
using System.Data.SqlClient;
using System.Text;

namespace SimpleParser.Domain
{
    public class ApplicationDbContext
    {
        private string _connectionString;
        private StringBuilder _expressionBuilder;

        public ApplicationDbContext(string connectionString)
        {
            _connectionString = connectionString;
            _expressionBuilder = new StringBuilder();
        }

        public void AddWoodDeal(WoodDeal deal)
        {
            _expressionBuilder.AppendLine("INSERT INTO WoodDeals(DealNumber,SellerName,SellerInn,BuyerName,BuyerInn,DealDate,WoodVolumeSeller,WoodVolumeBuyer) " +
                $"VALUES('{deal.DealNumber}','{deal.SellerName}','{deal.SellerInn}','{deal.BuyerName}','{deal.BuyerInn}','{deal.DealDate:yyyy.MM.dd}',{deal.WoodVolumeSeller},{deal.WoodVolumeBuyer});");
        }

        public void UpdateWoodDeal(WoodDeal deal)
        {
            _expressionBuilder.AppendLine("UPDATE WoodDeals " +
                $"SET SellerName='{deal.SellerName}',SellerInn='{deal.SellerInn}',BuyerName='{deal.BuyerName}',BuyerInn='{deal.BuyerInn}',DealDate='{deal.DealDate:yyyy.MM.dd}',WoodVolumeSeller={deal.WoodVolumeSeller},WoodVolumeBuyer={deal.WoodVolumeBuyer} " +
                $"WHERE DealNumber = '{deal.DealNumber}';");
        }

        public EntityState GetEntityState(WoodDeal deal)
        {
            WoodDeal existsDeal = ExecuteReader($"SELECT * FROM WoodDeals WHERE DealNumber = '{deal.DealNumber}'");
            if (existsDeal == null) return EntityState.NotFounded;
            if (!deal.Equals(existsDeal)) return EntityState.Modified;
            return EntityState.Exists;
        }

        public void SaveChanges()
        {
            string expression = _expressionBuilder.ToString();
            _expressionBuilder.Clear();
            if (!string.IsNullOrWhiteSpace(expression))
                ExecuteScalar(expression);
        }

        private WoodDeal ExecuteReader(string sqlExpression)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand(sqlExpression, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) return null;

                        object[] row = new object[reader.FieldCount];
                        reader.GetValues(row);

                        return new WoodDeal
                        {
                            DealNumber = (string)row[1],
                            SellerName = (string)row[2],
                            SellerInn = (string)row[3],
                            BuyerName = (string)row[4],
                            BuyerInn = (string)row[5],
                            DealDate = (DateTime)row[6],
                            WoodVolumeSeller = (double)row[7],
                            WoodVolumeBuyer = (double)row[8]
                        };
                    }
                }
            }
        }

        private object ExecuteScalar(string sqlExpression)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                return command.ExecuteScalar();
            }
        }
    }
}
