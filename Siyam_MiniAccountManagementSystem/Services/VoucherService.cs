using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace Siyam_MiniAccountManagementSystem.Services
{
    public class VoucherService
    {
        private readonly string _connectionString;

        public VoucherService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        /// <summary>
        /// Saves or Updates a voucher and its details.
        /// </summary>
        /// <param name="voucher">The Voucher object to save.</param>
        /// <param name="action">Action to perform: "Insert" or "Update".</param>
        /// <param name="currentUser">The user performing the action.</param>
        /// <returns>The VoucherId of the affected voucher.</returns>
        public async Task<int> SaveVoucherAsync(Voucher voucher, string action, string currentUser)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_SaveVoucher", conn)) // Now only handles Insert/Update
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Action", action);

                    // For 'Insert', VoucherId is output. For 'Update', it's input.
                    cmd.Parameters.AddWithValue("@VoucherId", voucher.VoucherId > 0 ? voucher.VoucherId : (object)DBNull.Value);

                    // Parameters relevant for Insert or Update actions
                    cmd.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                    cmd.Parameters.AddWithValue("@ReferenceNo", voucher.ReferenceNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VoucherType", voucher.VoucherType ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Narration", (object)voucher.Narration ?? DBNull.Value);

                    // TotalDebit and TotalCredit should ideally be calculated or validated based on details on client side
                    // or recalculated in the SP before saving the header.
                    cmd.Parameters.AddWithValue("@TotalDebit", voucher.TotalDebit);
                    cmd.Parameters.AddWithValue("@TotalCredit", voucher.TotalCredit);

                    if (action == "Insert")
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", currentUser);
                        cmd.Parameters.AddWithValue("@UpdatedBy", DBNull.Value); // Null for Insert
                    }
                    else if (action == "Update")
                    {
                        // Assuming CreatedBy is read from the existing voucher for updates
                        cmd.Parameters.AddWithValue("@CreatedBy", voucher.CreatedBy ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", currentUser);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid action for SaveVoucherAsync. Must be 'Insert' or 'Update'.");
                    }

                    // Populate and pass the Table-Valued Parameter (TVP)
                    DataTable dtVoucherDetails = new DataTable();
                    dtVoucherDetails.Columns.Add("VoucherDetailId", typeof(int)); // Must match TVP column name and type
                    dtVoucherDetails.Columns.Add("AccountId", typeof(int));
                    dtVoucherDetails.Columns.Add("Debit", typeof(decimal));
                    dtVoucherDetails.Columns.Add("Credit", typeof(decimal));

                    if (voucher.Details != null)
                    {
                        foreach (var detail in voucher.Details)
                        {
                            dtVoucherDetails.Rows.Add(
                                detail.VoucherDetailId,
                                detail.AccountId,
                                detail.Debit,
                                detail.Credit
                            );
                        }
                    }

                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@VoucherDetail_TVP", dtVoucherDetails);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.VoucherDetailType"; // Crucial: This must match your TVP name in SQL Server

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Retrieves a list of vouchers based on optional filters.
        /// </summary>
        /// <param name="voucherId">Optional: Filter by VoucherId.</param>
        /// <param name="voucherType">Optional: Filter by VoucherType.</param>
        /// <returns>A list of Voucher objects.</returns>
        public async Task<List<Voucher>> GetVouchersAsync(int? voucherId = null, string voucherType = null)
        {
            var vouchers = new List<Voucher>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_GetVouchers", conn)) // Calling new sp_GetVouchers
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId.HasValue ? voucherId.Value : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@VoucherType", string.IsNullOrEmpty(voucherType) ? (object)DBNull.Value : voucherType);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            vouchers.Add(new Voucher
                            {
                                VoucherId = reader.GetInt32(reader.GetOrdinal("VoucherId")),
                                VoucherDate = reader.GetDateTime(reader.GetOrdinal("VoucherDate")),
                                ReferenceNo = reader.GetString(reader.GetOrdinal("ReferenceNo")),
                                VoucherType = reader.GetString(reader.GetOrdinal("VoucherType")),
                                Narration = reader.IsDBNull(reader.GetOrdinal("Narration")) ? null : reader.GetString(reader.GetOrdinal("Narration")),
                                TotalDebit = reader.GetDecimal(reader.GetOrdinal("TotalDebit")),
                                TotalCredit = reader.GetDecimal(reader.GetOrdinal("TotalCredit")),
                                CreatedBy = reader.GetString(reader.GetOrdinal("CreatedBy")),
                                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                                UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetString(reader.GetOrdinal("UpdatedBy")),
                                UpdatedDate = reader.IsDBNull(reader.GetOrdinal("UpdatedDate")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("UpdatedDate"))
                            });
                        }
                    }
                }
            }
            return vouchers;
        }

        /// <summary>
        /// Retrieves detailed entries for a specific voucher.
        /// </summary>
        /// <param name="voucherId">The ID of the voucher to retrieve details for.</param>
        /// <returns>A list of VoucherDetail objects.</returns>
        public async Task<List<VoucherDetail>> GetVoucherDetailsAsync(int voucherId)
        {
            var details = new List<VoucherDetail>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_GetVoucherDetails", conn)) // Calling new sp_GetVoucherDetails
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            details.Add(new VoucherDetail
                            {
                                VoucherDetailId = reader.GetInt32(reader.GetOrdinal("VoucherDetailId")),
                                VoucherId = reader.GetInt32(reader.GetOrdinal("VoucherId")),
                                AccountId = reader.GetInt32(reader.GetOrdinal("AccountId")),
                                AccountCode = reader.GetString(reader.GetOrdinal("AccountCode")),
                                AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
                                Debit = reader.GetDecimal(reader.GetOrdinal("Debit")),
                                Credit = reader.GetDecimal(reader.GetOrdinal("Credit"))
                            });
                        }
                    }
                }
            }
            return details;
        }

        /// <summary>
        /// Deletes a voucher and its details.
        /// </summary>
        /// <param name="voucherId">The ID of the voucher to delete.</param>
        public async Task DeleteVoucherAsync(int voucherId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_DeleteVoucher", conn)) // Calling new sp_DeleteVoucher
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}