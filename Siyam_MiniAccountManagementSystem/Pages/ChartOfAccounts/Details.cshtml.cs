using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Threading.Tasks;

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
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

            Account = await _chartOfAccountsService.GetAccountByIdAsync(id.Value);

            if (Account == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}