using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
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

            Voucher = await _voucherService.GetVoucherByIdAsync(id.Value);

            if (Voucher == null)
            {
                return NotFound();
            }

            var allAccounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");
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
            Voucher.TotalDebit = Voucher.Details?.Sum(d => d.Debit) ?? 0;
            Voucher.TotalCredit = Voucher.Details?.Sum(d => d.Credit) ?? 0;

            if (Voucher.TotalDebit != Voucher.TotalCredit)
            {
                ModelState.AddModelError(string.Empty, "Total Debit must equal Total Credit. Please ensure your voucher balances.");
                var allAccounts = await _chartOfAccountsService.GetAccountsAsync();
                AccountList = new SelectList(allAccounts, "AccountId", "AccountName");
                return Page();
            }

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