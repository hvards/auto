using System.Configuration;
using System.Data.SqlClient;

namespace Auto.helpers
{
    internal static class SqlHelper
    {
        private const string ConnectionStringBase =
            "Data Source={:dataSource};Initial Catalog={:catalog};User ID={:username};Password={:password};persist security info=False;packet size=4096";

        public static string RunSqlQuery(string dataSource, string catalog, string query, string columns)
        {
            var result = string.Empty;
            var connectionString = ConnectionStringBase.Replace("{:dataSource}", dataSource)
                .Replace("{:username}",ConfigurationManager.AppSettings["sqlUsername"])
                .Replace("{:password}",ConfigurationManager.AppSettings["sqlPassword"]);
            
            foreach (var cat in catalog.Split(","))
            {
                connectionString = connectionString.Replace("{:catalog}", cat);
                result += $"\n{cat}:\n{GetSqlQueryResult(query, connectionString, columns.Split(","))}";
            }

            return result;
        }

        public static string GetSqlQueryResult(string query, string connectionString, string[] columnNames)
        {
            var connection = new SqlConnection(connectionString);
            connection.Open();

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            var result = string.Empty;
            while (reader.Read())
            {
                foreach (var columnName in columnNames)
                {
                    var index = reader.GetOrdinal(columnName);
                    var fieldType = reader.GetFieldType(index);
                    if (!reader.IsDBNull(index))
                        if (fieldType?.Name == "Int32")
                            result += $"{reader.GetInt32(index)} ";
                        else
                            result += $"{reader.GetString(index)} ";
                    else
                        result += "null ";
                }
                result += "\n";
            }

            return result;
        }
    }
}
