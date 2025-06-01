using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;

namespace Siyam_MiniAccountManagementSystem.Services
{
    public class VoucherService
    {
        private readonly string _connectionString;

        public VoucherService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> SaveVoucherAsync(Voucher voucher, string action, string? currentUser)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_SaveVoucher", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (action == "Insert")
                    {
                        cmd.Parameters.AddWithValue("@VoucherId", DBNull.Value);
                    }
                    else
                    {
                        if (voucher.VoucherId <= 0)
                        {
                            throw new ArgumentException("VoucherId must be a valid, non-zero ID for Update or Delete operations.", nameof(voucher.VoucherId));
                        }
                        cmd.Parameters.AddWithValue("@VoucherId", voucher.VoucherId);
                    }
                    cmd.Parameters.AddWithValue("@VoucherDate", voucher.VoucherDate);
                    cmd.Parameters.AddWithValue("@ReferenceNo", (object)voucher.ReferenceNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@VoucherType", (object)voucher.VoucherType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Narration", (object)voucher.Narration ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@TotalDebit", voucher.TotalDebit);
                    cmd.Parameters.AddWithValue("@TotalCredit", voucher.TotalCredit);
                    cmd.Parameters.AddWithValue("@OperationType", action);
                    if (action == "Insert")
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", (object)currentUser ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", DBNull.Value);
                    }
                    else if (action == "Update")
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", (object)voucher.CreatedBy ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", (object)currentUser ?? DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", DBNull.Value);
                        cmd.Parameters.AddWithValue("@UpdatedBy", DBNull.Value); 
                    }
                    DataTable voucherDetailsTable = new DataTable("VoucherDetailType");
                    voucherDetailsTable.Columns.Add("AccountId", typeof(int));
                    voucherDetailsTable.Columns.Add("Debit", typeof(decimal));
                    voucherDetailsTable.Columns.Add("Credit", typeof(decimal));
                    if (voucher.Details != null)
                    {
                        foreach (var detail in voucher.Details)
                        {
                            voucherDetailsTable.Rows.Add(detail.AccountId, detail.Debit, detail.Credit);
                        }
                    }
                    SqlParameter tvpParam = cmd.Parameters.AddWithValue("@VoucherDetails", voucherDetailsTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "VoucherDetailType";
                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    if (result == null || result == DBNull.Value)
                    {
                        return 0;
                    }
                    return Convert.ToInt32(result);
                }
            }
        }
        public async Task<List<Voucher>> GetVouchersAsync()
        {
            List<Voucher> vouchers = new List<Voucher>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_GetAllVouchers", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
                            vouchers.Add(MapVoucherFromReader(reader));
                        }
                    }
                }
            }
            return vouchers;
        }
        public async Task<Voucher?> GetVoucherByIdAsync(int voucherId)
        {
            Voucher? voucher = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_GetVoucherById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            voucher = MapVoucherFromReader(reader);
                        }
                    }
                    if (voucher != null)
                    {
                        voucher.Details = await GetVoucherDetailsAsync(voucherId);
                    }
                }
            }
            return voucher;
        }
        public async Task<List<VoucherDetail>> GetVoucherDetailsAsync(int voucherId)
        {
            List<VoucherDetail> details = new List<VoucherDetail>();
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand("sp_GetVoucherDetailsByVoucherId", conn))
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
                                VoucherDetailId = reader.GetInt32("VoucherDetailId"),
                                VoucherId = reader.GetInt32("VoucherId"),
                                AccountId = reader.GetInt32("AccountId"),
                                Debit = reader.GetDecimal("Debit"),
                                Credit = reader.GetDecimal("Credit")
                            });
                        }
                    }
                }
            }
            return details;
        }
        private Voucher MapVoucherFromReader(SqlDataReader reader)
        {
            return new Voucher
            {
                VoucherId = reader.GetInt32("VoucherId"),
                VoucherDate = reader.GetDateTime("VoucherDate"),
                ReferenceNo = reader.GetString("ReferenceNo"),
                VoucherType = reader.GetString("VoucherType"),
                Narration = reader.IsDBNull("Narration") ? null : reader.GetString("Narration"),
                TotalDebit = reader.GetDecimal("TotalDebit"),
                TotalCredit = reader.GetDecimal("TotalCredit"),
                CreatedBy = reader.IsDBNull("CreatedBy") ? null : reader.GetString("CreatedBy"),
                CreatedDate = reader.GetDateTime("CreatedDate"),
                UpdatedBy = reader.IsDBNull("UpdatedBy") ? null : reader.GetString("UpdatedBy"),
                UpdatedDate = reader.IsDBNull("UpdatedDate") ? (DateTime?)null : reader.GetDateTime("UpdatedDate")
            };
        }
    }
}