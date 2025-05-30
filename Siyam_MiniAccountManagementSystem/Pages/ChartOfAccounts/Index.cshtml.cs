// Pages/ChartOfAccounts/IndexModel.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using ClosedXML.Excel;
using System.Security.Claims; // For getting user details if needed in delete

namespace Siyam_MiniAccountManagementSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Admin,Accountant,Viewer")]
    public class IndexModel : PageModel
    {
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public IndexModel(ChartOfAccountsService chartOfAccountsService)
        {
            _chartOfAccountsService = chartOfAccountsService;
        }

        public List<ChartOfAccount> Accounts { get; set; } = new List<ChartOfAccount>(); // FIX: Initialize Accounts property

        public async Task OnGetAsync()
        {
            // Fetch hierarchical data for display
            Accounts = await _chartOfAccountsService.GetAccountsAsync("SelectHierarchy");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            // Example: Get current user's name for auditing delete operation (optional)
            // string currentUser = User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

            try
            {
                // For delete, you only need to pass the AccountId and action
                await _chartOfAccountsService.SaveAccountAsync(new ChartOfAccount { AccountId = id }, "Delete");
                TempData["SuccessMessage"] = "Account deleted successfully.";
            }
            catch (SqlException ex)
            {
                // Check if the error is due to foreign key constraint (e.g., has child accounts)
                if (ex.Number == 547) // Foreign key constraint violation error number
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

        public async Task<IActionResult> OnGetExportToExcelAsync()
        {
            // FIX: Use "SelectFlat" to get all accounts for export, not "Select" which is for specific ID/Code
            var accounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ChartOfAccounts");
                var currentRow = 1;

                // Headers
                worksheet.Cell(currentRow, 1).Value = "Account ID";
                worksheet.Cell(currentRow, 2).Value = "Account Code";
                worksheet.Cell(currentRow, 3).Value = "Account Name";
                worksheet.Cell(currentRow, 4).Value = "Account Type";
                worksheet.Cell(currentRow, 5).Value = "Parent Account";
                worksheet.Cell(currentRow, 6).Value = "Is Active";
                worksheet.Cell(currentRow, 7).Value = "Created Date";
                worksheet.Cell(currentRow, 8).Value = "Updated Date";
                worksheet.Cell(currentRow, 9).Value = "Created By"; // Added for Excel export
                worksheet.Cell(currentRow, 10).Value = "Updated By"; // Added for Excel export
                worksheet.Range(1, 1, 1, 10).Style.Font.Bold = true; // Adjusted range
                worksheet.Range(1, 1, 1, 10).Style.Fill.BackgroundColor = XLColor.LightGray; // Adjusted range

                // Data
                foreach (var account in accounts)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = account.AccountId;
                    worksheet.Cell(currentRow, 2).Value = account.AccountCode;
                    worksheet.Cell(currentRow, 3).Value = account.AccountName;
                    worksheet.Cell(currentRow, 4).Value = account.AccountType;
                    worksheet.Cell(currentRow, 5).Value = account.ParentAccountName; // Use ParentAccountName for display
                    worksheet.Cell(currentRow, 6).Value = account.IsActive ? "Yes" : "No";
                    worksheet.Cell(currentRow, 7).Value = account.CreatedDate;
                    worksheet.Cell(currentRow, 8).Value = account.UpdatedDate;
                    worksheet.Cell(currentRow, 9).Value = account.CreatedBy; // Added for Excel export
                    worksheet.Cell(currentRow, 10).Value = account.UpdatedBy; // Added for Excel export
                }

                worksheet.Columns().AdjustToContents(); // Auto-adjust column widths

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ChartOfAccounts.xlsx");
                }
            }
        }
    }
}