using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using System.Data;
using System.Linq; // Add this using directive for .Any() and .GetColumnSchema()

namespace Siyam_MiniAccountManagementSystem.Services
{
    public class ChartOfAccountsService
    {
        private readonly string _connectionString;

        public ChartOfAccountsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<ChartOfAccount>> GetAccountsAsync(string action = "Select", int? accountId = null, string? accountCode = null, string? accountName = null)
        {
            var accounts = new List<ChartOfAccount>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", action);
                    cmd.Parameters.AddWithValue("@AccountId", (object)accountId ?? DBNull.Value); // Ensure DBNull for null int?
                    cmd.Parameters.AddWithValue("@AccountCode", (object)accountCode ?? DBNull.Value); // Ensure DBNull for null string
                    cmd.Parameters.AddWithValue("@AccountName", (object)accountName ?? DBNull.Value); // Ensure DBNull for null string

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        // Get column schema once to check for column existence
                        var columnSchema = reader.GetColumnSchema();
                        bool hasLevelColumn = columnSchema.Any(col => col.ColumnName == "Level");
                        bool hasDisplayNameColumn = columnSchema.Any(col => col.ColumnName == "DisplayName");

                        while (reader.Read())
                        {
                            var account = new ChartOfAccount
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                                ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentAccountId")),
                                ParentAccountName = reader.IsDBNull(reader.GetOrdinal("ParentAccountName")) ? null : reader.GetString(reader.GetOrdinal("ParentAccountName")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy"))
                            };

                            // Conditionally assign Level and DisplayName if the columns exist
                            if (hasLevelColumn && !reader.IsDBNull(reader.GetOrdinal("Level")))
                            {
                                account.Level = reader.GetInt32(reader.GetOrdinal("Level"));
                            }
                            else
                            {
                                account.Level = 0; // Default or handle as appropriate for "Select" action
                            }

                            if (hasDisplayNameColumn && !reader.IsDBNull(reader.GetOrdinal("DisplayName")))
                            {
                                account.DisplayName = reader.GetString(reader.GetOrdinal("DisplayName"));
                            }
                            else
                            {
                                // If DisplayName is not available, use AccountName as default
                                account.DisplayName = account.AccountName;
                            }

                            accounts.Add(account);
                        }
                    }
                }
            }
            return accounts;
        }

        public async Task<int> SaveAccountAsync(ChartOfAccount account, string action)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_ManageChartOfAccounts", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", action);

                    // AccountId is needed for Update, Delete, and sometimes for Select (if by ID)
                    if (action == "Update" || action == "Delete" || action == "Select")
                    {
                        cmd.Parameters.AddWithValue("@AccountId", (object)account.AccountId ?? DBNull.Value);
                    }

                    // Parameters for Insert or Update
                    if (action == "Insert" || action == "Update")
                    {
                        cmd.Parameters.AddWithValue("@AccountCode", (object)account.AccountCode ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AccountName", (object)account.AccountName ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@AccountType", (object)account.AccountType ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@ParentAccountId", (object)account.ParentAccountId ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", account.IsActive);
                        cmd.Parameters.AddWithValue("@CreatedBy", (object)account.CreatedBy ?? DBNull.Value); // Pass CreatedBy
                        cmd.Parameters.AddWithValue("@UpdatedBy", (object)account.UpdatedBy ?? DBNull.Value); // Pass UpdatedBy
                    }

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync(); // For Insert, Update, Delete, returns ID

                    // Handle DBNull result from ExecuteScalar in case SP doesn't return anything or returns NULL
                    if (result == null || result == DBNull.Value)
                    {
                        return 0; // Return 0 or throw an exception if no ID is returned
                    }
                    return Convert.ToInt32(result);
                }
            }
        }
    }
}