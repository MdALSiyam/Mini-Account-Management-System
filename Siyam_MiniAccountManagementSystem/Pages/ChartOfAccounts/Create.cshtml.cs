// Pages/ChartOfAccounts/CreateModel.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Security.Claims; // For getting user details

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant,Viewer")] // Restrict creation to Admin and Accountant roles
    public class CreateModel : PageModel
    {
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public CreateModel(ChartOfAccountsService chartOfAccountsService)
        {
            _chartOfAccountsService = chartOfAccountsService;
        }

        [BindProperty]
        public ChartOfAccount Account { get; set; } = new ChartOfAccount(); // FIX: Initialize Account property

        public SelectList ParentAccounts { get; set; }
        public List<string> AccountTypes { get; set; } = new List<string> { "Asset", "Liability", "Equity", "Revenue", "Expense" };

        public async Task<IActionResult> OnGetAsync()
        {
            await PopulateParentAccountsDropdown();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateParentAccountsDropdown();
                return Page();
            }

            try
            {
                // Set CreatedBy and UpdatedBy before saving
                // This assumes your application has a way to get the current user's name/ID
                Account.CreatedBy = User.FindFirstValue(ClaimTypes.Name) ?? "System"; // Example: Get current user's name
                Account.UpdatedBy = Account.CreatedBy; // On creation, UpdatedBy is same as CreatedBy

                int newAccountId = await _chartOfAccountsService.SaveAccountAsync(Account, "Insert");

                if (newAccountId > 0)
                {
                    TempData["SuccessMessage"] = "Account created successfully.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create account. The database operation did not return a valid ID or an error occurred.");
                    await PopulateParentAccountsDropdown();
                    return Page();
                }
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Database error: {ex.Message}");
                await PopulateParentAccountsDropdown();
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                await PopulateParentAccountsDropdown();
                return Page();
            }
        }

        private async Task PopulateParentAccountsDropdown()
        {
            // Fetch flat list for parent accounts dropdown (no hierarchy needed here)
            var accounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");
            ParentAccounts = new SelectList(accounts, "AccountId", "AccountName");
        }
    }
}