using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant, Viewer")]
    public class EditModel : PageModel
    {
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public EditModel(ChartOfAccountsService chartOfAccountsService)
        {
            _chartOfAccountsService = chartOfAccountsService;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; }
        public SelectList ParentAccounts { get; set; }
        public List<string> AccountTypes { get; set; } = new List<string> { "Asset", "Liability", "Equity", "Revenue", "Expense" };

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Fetch the specific account to edit
            var accounts = await _chartOfAccountsService.GetAccountsAsync("Select", id.Value);
            Account = accounts.FirstOrDefault();

            if (Account == null)
            {
                return NotFound();
            }

            await PopulateParentAccountsDropdown(Account.AccountId); // Pass current account ID to exclude itself from parent options
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentAccountsDropdown(Account.AccountId);
                return Page();
            }

            try
            {
                await _chartOfAccountsService.SaveAccountAsync(Account, "Update");
                TempData["SuccessMessage"] = "Account updated successfully.";
                return RedirectToPage("./Index");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error updating account: {ex.Message}");
                await PopulateParentAccountsDropdown(Account.AccountId);
                return Page();
            }
        }

        private async Task PopulateParentAccountsDropdown(int currentAccountId)
        {
            // Get all accounts, excluding the current account itself and its descendants from being a parent
            var accounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");
            var filteredAccounts = accounts.Where(a => a.AccountId != currentAccountId).ToList(); // Simple exclusion, proper exclusion would involve checking for descendants
            ParentAccounts = new SelectList(filteredAccounts, "AccountId", "AccountName");
        }
    }
}
