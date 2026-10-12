using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using System.Data;

public class ChartOfAccountsService
{
    private readonly string _connectionString;

    public ChartOfAccountsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<IList<ChartOfAccount>> GetAccountsAsync(string? operationType = null)
    {
        // Render-এ ফ্রি হোস্টিং এবং InMemory ডাটাবেজ ব্যবহার করার কারণে UI-এর Parent Account 
        // ড্রপডাউন সচল রাখতে এবং ক্র্যাশ এড়াতে প্রাথমিক ডামি লিস্ট রিটার্ন করা হলো।
        var accounts = new List<ChartOfAccount>
        {
            new ChartOfAccount 
            { 
                AccountId = 1, 
                AccountCode = "1000", 
                AccountName = "Assets", 
                AccountType = "Asset", 
                IsActive = true, 
                Level = 1, 
                DisplayName = "1000 - Assets" 
            },
            new ChartOfAccount 
            { 
                AccountId = 2, 
                AccountCode = "2000", 
                AccountName = "Liabilities", 
                AccountType = "Liability", 
                IsActive = true, 
                Level = 1, 
                DisplayName = "2000 - Liabilities" 
            },
            new ChartOfAccount 
            { 
                AccountId = 3, 
                AccountCode = "3000", 
                AccountName = "Equity", 
                AccountType = "Equity", 
                IsActive = true, 
                Level = 1, 
                DisplayName = "3000 - Equity" 
            }
        };

        try
        {
            if (string.IsNullOrEmpty(_connectionString)) return accounts;

            /* =========================================================================
             * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
             * =========================================================================
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (string.IsNullOrEmpty(operationType))
                    {
                        operationType = "SelectFlat";
                    }
                    command.Parameters.AddWithValue("@OperationType", operationType);
                    command.Parameters.AddWithValue("@AccountId", DBNull.Value);
                    command.Parameters.AddWithValue("@AccountCode", DBNull.Value);
                    command.Parameters.AddWithValue("@AccountName", DBNull.Value);
                    command.Parameters.AddWithValue("@AccountType", DBNull.Value);
                    command.Parameters.AddWithValue("@ParentAccountId", DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedBy", DBNull.Value);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        accounts.Clear(); // যদি ডাটাবেজ কানেক্টেড থাকে তবে ডামি লিস্ট রিমুভ করে আসল ডাটা লোড হবে
                        while (await reader.ReadAsync())
                        {
                            accounts.Add(new ChartOfAccount
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                                ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentAccountId")),
                                ParentAccountName = reader.IsDBNull(reader.GetOrdinal("ParentAccountName")) ? null : reader.GetString(reader.GetOrdinal("ParentAccountName")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                                UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                                Level = reader.GetInt32(reader.GetOrdinal("Level")),
                                DisplayName = reader.IsDBNull(reader.GetOrdinal("DisplayName")) ? null : reader.GetString(reader.GetOrdinal("DisplayName"))
                            });
                        }
                    }
                }
            }
            ========================================================================= */
        }
        catch (Exception)
        {
            // Render environment safe fallback - will use dummy list on error
        }

        return accounts;
    }

    public async Task<ChartOfAccount?> GetAccountByIdAsync(int accountId)
    {
        ChartOfAccount? account = null;
        try
        {
            if (string.IsNullOrEmpty(_connectionString)) return null;

            /* =========================================================================
             * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
             * =========================================================================
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OperationType", "SelectOne");
                    command.Parameters.AddWithValue("@AccountId", accountId);
                    command.Parameters.AddWithValue("@AccountCode", DBNull.Value);
                    command.Parameters.AddWithValue("@AccountName", DBNull.Value);
                    command.Parameters.AddWithValue("@AccountType", DBNull.Value);
                    command.Parameters.AddWithValue("@ParentAccountId", DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedBy", DBNull.Value);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            account = new ChartOfAccount
                            {
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                AccountType = reader.GetString(reader.GetOrdinal("AccountType")),
                                ParentAccountId = reader.IsDBNull(reader.GetOrdinal("ParentAccountId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ParentAccountId")),
                                ParentAccountName = reader.IsDBNull(reader.GetOrdinal("ParentAccountName")) ? null : reader.GetString(reader.GetOrdinal("ParentAccountName")),
                                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                                UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate")),
                                Level = reader.GetInt32(reader.GetOrdinal("Level")),
                                DisplayName = reader.IsDBNull(reader.GetOrdinal("DisplayName")) ? null : reader.GetString(reader.GetOrdinal("DisplayName"))
                            };
                        }
                    }
                }
            }
            ========================================================================= */
        }
        catch (Exception) { }

        return account;
    }

    public async Task<int> SaveAccountAsync(ChartOfAccount account, string operationType, string? currentUser = null)
    {
        try
        {
            if (string.IsNullOrEmpty(_connectionString)) return 1; // Render-এ সফল রেসপন্স দেখানোর জন্য 1 রিটার্ন করা হলো

            /* =========================================================================
             * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
             * =========================================================================
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("sp_ManageChartOfAccounts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@OperationType", operationType);
                    command.Parameters.AddWithValue("@AccountId", account.AccountId == 0 ? DBNull.Value : (object)account.AccountId);
                    command.Parameters.AddWithValue("@AccountCode", string.IsNullOrEmpty(account.AccountCode) ? DBNull.Value : (object)account.AccountCode);
                    command.Parameters.AddWithValue("@AccountName", string.IsNullOrEmpty(account.AccountName) ? DBNull.Value : (object)account.AccountName);
                    command.Parameters.AddWithValue("@AccountType", string.IsNullOrEmpty(account.AccountType) ? DBNull.Value : (object)account.AccountType);
                    command.Parameters.AddWithValue("@ParentAccountId", account.ParentAccountId.HasValue ? (object)account.ParentAccountId.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", account.IsActive);
                    command.Parameters.AddWithValue("@CreatedBy", string.IsNullOrEmpty(currentUser) ? DBNull.Value : (object)currentUser);
                    command.Parameters.AddWithValue("@UpdatedBy", string.IsNullOrEmpty(currentUser) ? DBNull.Value : (object)currentUser);
                    var result = await command.ExecuteScalarAsync();
                    return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
            ========================================================================= */
        }
        catch (Exception)
        {
            return 0;
        }

        return 1; // Successful dummy execution ID for Render
    }
}
