using System.ComponentModel.DataAnnotations;

namespace Siyam_MiniAccountManagementSystem.Models
{
    public class Voucher
    {
        public int VoucherId { get; set; }
        [Required] public DateTime VoucherDate { get; set; }
        [Required, StringLength(100)] public string? ReferenceNo { get; set; }
        [Required, StringLength(50)] public string? VoucherType { get; set; }
        public string? Narration { get; set; }
        [Display(Name = "Total Debit")] public decimal TotalDebit { get; set; }
        [Display(Name = "Total Credit")] public decimal TotalCredit { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public List<VoucherDetail> Details { get; set; } = new List<VoucherDetail>();
    }
    public class VoucherDetail
    {
        public int VoucherDetailId { get; set; }
        public int VoucherId { get; set; }
        [Required] public int AccountId { get; set; }
        public string? AccountCode { get; set; }
        public string? AccountName { get; set; }
        [Required, DataType(DataType.Currency), Range(0, 9999999999999999.99)] public decimal Debit { get; set; }
        [Required, DataType(DataType.Currency), Range(0, 9999999999999999.99)] public decimal Credit { get; set; }
    }
}
