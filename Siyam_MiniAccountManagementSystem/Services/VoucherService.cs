using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using System.Data;

namespace Siyam_MiniAccountManagementSystem.Services
{
    public class VoucherService
    {
        private readonly string _connectionString;

        // Static memory list for Vouchers with human names for testing on Render
        private static readonly List<Voucher> _memoryVouchers = new List<Voucher>
        {
            new Voucher
            {
                VoucherId = 1,
                VoucherDate = DateTime.Now.AddDays(-2),
                ReferenceNo = "REF-001",
                VoucherType = "Receipt",
                Narration = "Received cash from Rahim",
                TotalDebit = 5000.00m,
                TotalCredit = 5000.00m,
                CreatedBy = "siyam@gmail.com",
                CreatedDate = DateTime.Now.AddDays(-2),
                Details = new List<VoucherDetail>
                {
                    new VoucherDetail { VoucherDetailId = 1, VoucherId = 1, AccountId = 1, Debit = 5000.00m, Credit = 0m },
                    new VoucherDetail { VoucherDetailId = 2, VoucherId = 1, AccountId = 2, Debit = 0m, Credit = 5000.00m }
                }
            },
            new Voucher
            {
                VoucherId = 2,
                VoucherDate = DateTime.Now.AddDays(-1),
                ReferenceNo = "REF-002",
                VoucherType = "Payment",
                Narration = "Paid bill to Karim",
                TotalDebit = 2000.00m,
                TotalCredit = 2000.00m,
                CreatedBy = "siyam@gmail.com",
                CreatedDate = DateTime.Now.AddDays(-1),
                Details = new List<VoucherDetail>
                {
                    new VoucherDetail { VoucherDetailId = 3, VoucherId = 2, AccountId = 3, Debit = 2000.00m, Credit = 0m },
                    new VoucherDetail { VoucherDetailId = 4, VoucherId = 2, AccountId = 1, Debit = 0m, Credit = 2000.00m }
                }
            }
        };

        public VoucherService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";
        }

        public async Task<int> SaveVoucherAsync(Voucher voucher, string action, string? currentUser)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    if (action == "Insert")
                    {
                        int newId = _memoryVouchers.Count > 0 ? _memoryVouchers.Max(x => x.VoucherId) + 1 : 1;
                        voucher.VoucherId = newId;
                        voucher.CreatedDate = DateTime.Now;
                        voucher.CreatedBy = currentUser ?? "siyam@gmail.com";
                        
                        if (voucher.Details != null)
                        {
                            int detailId = _memoryVouchers.SelectMany(v => v.Details ?? new List<VoucherDetail>()).Count() + 1;
                            foreach (var detail in voucher.Details)
                            {
                                detail.VoucherDetailId = detailId++;
                                detail.VoucherId = newId;
                            }
                        }
                        
                        _memoryVouchers.Add(voucher);
                        return newId;
                    }
                    else if (action == "Update")
                    {
                        var existing = _memoryVouchers.FirstOrDefault(x => x.VoucherId == voucher.VoucherId);
                        if (existing != null)
                        {
                            existing.VoucherDate = voucher.VoucherDate;
                            existing.ReferenceNo = voucher.ReferenceNo;
                            existing.VoucherType = voucher.VoucherType;
                            existing.Narration = voucher.Narration;
                            existing.TotalDebit = voucher.TotalDebit;
                            existing.TotalCredit = voucher.TotalCredit;
                            existing.UpdatedBy = currentUser;
                            existing.UpdatedDate = DateTime.Now;
                            existing.Details = voucher.Details;
                            return existing.VoucherId;
                        }
                    }
                    return voucher.VoucherId > 0 ? voucher.VoucherId : 1;
                }

                /* =========================================================================
                 * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
                 * =========================================================================
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
                ========================================================================= */
            }
            catch (Exception)
            {
                return 0;
            }
            return 0;
        }

        public async Task<List<Voucher>> GetVouchersAsync()
        {
            var vouchers = new List<Voucher>(_memoryVouchers);
            try
            {
                if (string.IsNullOrEmpty(_connectionString)) return vouchers;

                /* =========================================================================
                 * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
                 * =========================================================================
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("sp_GetAllVouchers", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            vouchers.Clear();
                            while (reader.Read())
                            {
                                vouchers.Add(MapVoucherFromReader(reader));
                            }
                        }
                    }
                }
                ========================================================================= */
            }
            catch (Exception) { }
            return vouchers;
        }

        public async Task<Voucher?> GetVoucherByIdAsync(int voucherId)
        {
            var voucher = _memoryVouchers.FirstOrDefault(x => x.VoucherId == voucherId);
            try
            {
                if (string.IsNullOrEmpty(_connectionString)) return voucher;

                /* =========================================================================
                 * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
                 * =========================================================================
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
                    }
                    if (voucher != null)
                    {
                        voucher.Details = await GetVoucherDetailsAsync(voucherId);
                    }
                }
                ========================================================================= */
            }
            catch (Exception) { }
            return voucher;
        }

        public async Task<List<VoucherDetail>> GetVoucherDetailsAsync(int voucherId)
        {
            var voucher = _memoryVouchers.FirstOrDefault(x => x.VoucherId == voucherId);
            var details = voucher?.Details ?? new List<VoucherDetail>();
            try
            {
                if (string.IsNullOrEmpty(_connectionString)) return details;

                /* =========================================================================
                 * ORIGINAL SQL SERVER STORED PROCEDURE IMPLEMENTATION (FOR FUTURE USE)
                 * =========================================================================
                using (var conn = new SqlConnection(_connectionString))
                {
                    using (var cmd = new SqlCommand("sp_GetVoucherDetailsByVoucherId", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@VoucherId", voucherId);
                        await conn.OpenAsync();
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            details.Clear();
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
                ========================================================================= */
            }
            catch (Exception) { }
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
