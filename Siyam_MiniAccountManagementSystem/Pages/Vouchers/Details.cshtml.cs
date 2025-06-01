using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class DetailsModel : PageModel
    {
        private readonly VoucherService _voucherService;
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public DetailsModel(VoucherService voucherService, ChartOfAccountsService chartOfAccountsService)
        {
            _voucherService = voucherService;
            _chartOfAccountsService = chartOfAccountsService;
        }
        public Voucher Voucher { get; set; }
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