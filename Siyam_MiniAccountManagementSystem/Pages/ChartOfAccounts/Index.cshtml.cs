using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant")]
    public class IndexModel : PageModel
    {
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public IndexModel(ChartOfAccountsService chartOfAccountsService)
        {
            _chartOfAccountsService = chartOfAccountsService;
        }

        public IList<ChartOfAccount> Accounts { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                Accounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Database error: {ex.Message}";
                Accounts = new List<ChartOfAccount>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                Accounts = new List<ChartOfAccount>();
            }
        }
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                int result = await _chartOfAccountsService.SaveAccountAsync(new ChartOfAccount { AccountId = id }, "Delete");

                if (result == -1) 
                {
                
                }
                else if (result > 0)
                {
                    TempData["SuccessMessage"] = "Account deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Account could not be deleted for an unknown reason.";
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547 || ex.Message.Contains("child accounts"))
                {
                    TempData["ErrorMessage"] = "Error deleting account: Cannot delete this account as it has associated child accounts.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Error deleting account: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
            }
            return RedirectToPage("./Index");
        }
    }
}