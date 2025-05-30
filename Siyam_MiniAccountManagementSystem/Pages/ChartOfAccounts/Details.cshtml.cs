using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant,Viewer")] // Viewers can also see details
    public class DetailsModel : PageModel
    {
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public DetailsModel(ChartOfAccountsService chartOfAccountsService)
        {
            _chartOfAccountsService = chartOfAccountsService;
        }

        public ChartOfAccount Account { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the specific account details
            var accounts = await _chartOfAccountsService.GetAccountsAsync("Select", id.Value);
            Account = accounts.FirstOrDefault();

            if (Account == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
