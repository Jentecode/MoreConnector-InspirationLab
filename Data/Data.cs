using MySqlConnector;
using System;
using System.Collections.Generic;

namespace MoreConnector.Data
{
    class Data
    {
        // TODO: move connection string to config file — never hardcode credentials in source
        private readonly string connectionString =
            "server=127.0.0.1;" +
            "port=3306;" +
            "uid=root;" +
            "pwd=root;" +
            "database=moreconnector;";

        // Uses parameterized query to prevent SQL injection.
        // Returns the new row's ID, or -1 on failure.
        private int Insert(string query, params MySqlParameter[] parameters)
        {
            using var connection = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
                command.Parameters.AddRange(parameters);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                return (int)command.LastInsertedId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Insert Error] {ex.Message}");
                return -1;
            }
        }

        // Generic SELECT helper — returns raw reader results as list of string arrays.
        // Replace with typed methods once models are connected.
        private List<string[]> Select(string query, params MySqlParameter[] parameters)
        {
            var results = new List<string[]>();

            using var connection = new MySqlConnection(connectionString);
            using var command = new MySqlCommand(query, connection);

            if (parameters != null)
                command.Parameters.AddRange(parameters);

            try
            {
                connection.Open();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var row = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                        row[i] = reader[i]?.ToString() ?? "";
                    results.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB Select Error] {ex.Message}");
            }

            return results;
        }
    }
}
