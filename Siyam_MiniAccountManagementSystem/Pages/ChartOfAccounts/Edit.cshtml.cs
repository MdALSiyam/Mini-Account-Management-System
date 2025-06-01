using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
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

            Account = await _chartOfAccountsService.GetAccountByIdAsync(id.Value);

            if (Account == null)
            {
                return NotFound();
            }
            await PopulateParentAccountsDropdown(Account.AccountId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await PopulateParentAccountsDropdown(Account.AccountId);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                TempData["ErrorMessage"] = "User not authenticated. Please log in.";
                return RedirectToPage("/Account/Login");
            }

            try
            {
                await _chartOfAccountsService.SaveAccountAsync(Account, "Update", currentUser);
                TempData["SuccessMessage"] = "Account updated successfully.";
                return RedirectToPage("./Index");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Database error updating account: {ex.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                return Page();
            }
        }

        private async Task PopulateParentAccountsDropdown(int currentAccountId)
        {
            var accounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");
            var filteredAccounts = accounts.Where(a => a.AccountId != currentAccountId).ToList();
            ParentAccounts = new SelectList(filteredAccounts, "AccountId", "AccountName");
        }
    }
}