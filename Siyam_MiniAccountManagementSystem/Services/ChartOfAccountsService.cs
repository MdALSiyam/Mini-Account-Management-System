using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using System.Data;

public class ChartOfAccountsService
{
    private readonly string _connectionString;

    public ChartOfAccountsService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
    public async Task<IList<ChartOfAccount>> GetAccountsAsync(string? operationType = null)
    {
        var accounts = new List<ChartOfAccount>();
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
        return accounts;
    }
    public async Task<ChartOfAccount?> GetAccountByIdAsync(int accountId)
    {
        ChartOfAccount? account = null;
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
        return account;
    }
    public async Task<int> SaveAccountAsync(ChartOfAccount account, string operationType, string? currentUser = null)
    {
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
    }
}
