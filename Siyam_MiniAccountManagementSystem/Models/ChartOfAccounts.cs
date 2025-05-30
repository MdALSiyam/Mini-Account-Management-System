// Models/ChartOfAccount.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace Siyam_MiniAccountManagementSystem.Models
{
    public class ChartOfAccount
    {
        public int AccountId { get; set; }
        [Required, StringLength(50)] public string? AccountCode { get; set; }
        [Required, StringLength(255)] public string? AccountName { get; set; }
        [Required, StringLength(50)] public string? AccountType { get; set; }
        public int? ParentAccountId { get; set; }
        public string? ParentAccountName { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int Level { get; set; }
        public string? DisplayName { get; set; } 
    }
}