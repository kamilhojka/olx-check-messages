using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace OLXChecker
{
    internal class WaproAPI
    {
        private static readonly string connectionString = "Server=192.168.16.6;Database=regaly;User Id=koladmin;Password=koladmin;Connection Timeout=1;";

        public static async Task<Account> GetTokensForAccount(Account account)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT ACCESS, REFRESH FROM [regaly].[dbo].[OLX_CREDENTIALS] WHERE [ID] = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", account.Id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            account.Access = reader["ACCESS"].ToString();
                            account.Refresh = reader["REFRESH"].ToString();
                        }
                    }
                }
            }

            return account;
        }

        public static async Task UpdateTokensForAccount(Account account)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("UPDATE [regaly].[dbo].[OLX_CREDENTIALS] SET ACCESS = @access, REFRESH = @refresh WHERE [ID] = @id", connection))
                {
                    command.Parameters.AddWithValue("@access", account.Access);
                    command.Parameters.AddWithValue("@refresh", account.Refresh);
                    command.Parameters.AddWithValue("@id", account.Id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected <= 0)
                    {
                        throw new Exception("Błąd podczas aktualizacji tokenów");
                    }
                }
            }
        }

    }
}
