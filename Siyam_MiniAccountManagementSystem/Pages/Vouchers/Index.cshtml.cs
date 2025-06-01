using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Security.Claims;

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class IndexModel : PageModel
    {
        private readonly VoucherService _voucherService;

        public IndexModel(VoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public IList<Voucher> Vouchers { get; set; }

        public async Task OnGetAsync()
        {
            Vouchers = await _voucherService.GetVouchersAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var currentUser = User.Identity.Name;
                if (string.IsNullOrEmpty(currentUser))
                {
                    TempData["ErrorMessage"] = "User information not available. Please log in.";
                    return RedirectToPage();
                }
                var voucherToDelete = new Voucher { VoucherId = id };
                await _voucherService.SaveVoucherAsync(voucherToDelete, "Delete", currentUser);
                TempData["SuccessMessage"] = "Voucher deleted successfully.";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Error deleting voucher: {ex.Message}";
            }
            return RedirectToPage();
        }
    }

}
