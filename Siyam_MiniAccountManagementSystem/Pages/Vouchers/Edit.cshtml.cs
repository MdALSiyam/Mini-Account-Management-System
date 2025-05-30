// Siyam_MiniAccountManagementSystem/Pages/Vouchers/Edit.cshtml.cs

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Collections.Generic;

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    public class EditModel : PageModel
    {
        private readonly VoucherService _voucherService;
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public EditModel(VoucherService voucherService, ChartOfAccountsService chartOfAccountsService)
        {
            _voucherService = voucherService;
            _chartOfAccountsService = chartOfAccountsService;
        }

        [BindProperty]
        public Voucher Voucher { get; set; }

        public SelectList AccountList { get; set; }
        public List<string> VoucherTypes { get; set; } = new List<string> { "Journal", "Payment", "Receipt" };

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vouchers = await _voucherService.GetVouchersAsync(id);
            Voucher = vouchers.FirstOrDefault();

            if (Voucher == null || Voucher.VoucherId == 0)
            {
                return NotFound();
            }

            Voucher.Details = await _voucherService.GetVoucherDetailsAsync(id.Value);

            var allAccounts = await _chartOfAccountsService.GetAccountsAsync();
            AccountList = new SelectList(allAccounts, "AccountId", "AccountName");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var allAccounts = await _chartOfAccountsService.GetAccountsAsync();
                AccountList = new SelectList(allAccounts, "AccountId", "AccountName");
                return Page();
            }

            // --- Server-Side Calculation of Totals (as a safeguard) ---
            // This is crucial if client-side JS fails or is bypassed
            Voucher.TotalDebit = Voucher.Details?.Sum(d => d.Debit) ?? 0;
            Voucher.TotalCredit = Voucher.Details?.Sum(d => d.Credit) ?? 0;

            // Add a basic validation: Debit should equal Credit
            if (Voucher.TotalDebit != Voucher.TotalCredit)
            {
                ModelState.AddModelError(string.Empty, "Total Debit must equal Total Credit. Please ensure your voucher balances.");
                var allAccounts = await _chartOfAccountsService.GetAccountsAsync();
                AccountList = new SelectList(allAccounts, "AccountId", "AccountName");
                return Page();
            }
            // --- End Server-Side Calculation ---


            var currentUser = User.Identity.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return RedirectToPage("/Account/Login");
            }

            await _voucherService.SaveVoucherAsync(Voucher, "Update", currentUser);

            return RedirectToPage("./Index");
        }
    }
}