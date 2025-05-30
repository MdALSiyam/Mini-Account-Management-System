using System.Linq; // Ensure this is at the top
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    public class DetailsModel : PageModel
    {
        private readonly VoucherService _voucherService;
        private readonly ChartOfAccountsService _chartOfAccountsService; // Assuming you have this service for Chart of Accounts

        public DetailsModel(VoucherService voucherService, ChartOfAccountsService chartOfAccountsService)
        {
            _voucherService = voucherService;
            _chartOfAccountsService = chartOfAccountsService; // Initialize ChartOfAccountsService
        }

        public Voucher Voucher { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Load Voucher header details
            // OLD INCORRECT LINE: var vouchers = await _voucherService.GetVouchersAsync("Select", id);
            // CORRECTED LINE:
            var vouchers = await _voucherService.GetVouchersAsync(id); // Pass only the ID. Line 33
            Voucher = vouchers.FirstOrDefault();

            if (Voucher == null || Voucher.VoucherId == 0) // Check if voucher was found
            {
                return NotFound();
            }

            // Load Voucher Details (sub-items)
            Voucher.Details = await _voucherService.GetVoucherDetailsAsync(id.Value); // Use id.Value since it's confirmed not null

            var allAccounts = await _chartOfAccountsService.GetAccountsAsync(); // Adjust this call based on your actual GetAccountsAsync signature

            foreach (var detail in Voucher.Details)
            {
                var account = allAccounts.FirstOrDefault(a => a.AccountId == detail.AccountId);
                if (account != null)
                {
                    detail.AccountCode = account.AccountCode;
                    detail.AccountName = account.AccountName;
                }
            }

            return Page();
        }
    }
}