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
    [Authorize(Roles = "Admin,Accountant,Viewer")] // All roles can view vouchers
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
            // Old incorrect call: Vouchers = await _voucherService.GetVouchersAsync("Select");
            // Corrected call:
            Vouchers = await _voucherService.GetVouchersAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                // Get the current user's name
                var currentUser = User.Identity.Name;
                if (string.IsNullOrEmpty(currentUser))
                {
                    // Handle case where user is not logged in or name is not available
                    TempData["ErrorMessage"] = "User information not available. Please log in.";
                    return RedirectToPage();
                }

                // Create a dummy voucher object with only the ID set for deletion
                var voucherToDelete = new Voucher { VoucherId = id };
                // Pass the currentUser parameter
                await _voucherService.SaveVoucherAsync(voucherToDelete, "Delete", currentUser);
                TempData["SuccessMessage"] = "Voucher deleted successfully.";
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Error deleting voucher: {ex.Message}";
            }
            return RedirectToPage(); // Redirect to the current page to refresh the list
        }
    }

}
