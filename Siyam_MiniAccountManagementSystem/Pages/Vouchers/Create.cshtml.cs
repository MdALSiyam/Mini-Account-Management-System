using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq; // Ensure this is present for .Sum()

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant,Viewer")]
    public class CreateModel : PageModel
    {
        private readonly VoucherService _voucherService;
        private readonly ChartOfAccountsService _chartOfAccountsService;

        public CreateModel(VoucherService voucherService, ChartOfAccountsService chartOfAccountsService)
        {
            _voucherService = voucherService;
            _chartOfAccountsService = chartOfAccountsService;
        }

        [BindProperty]
        public Voucher Voucher { get; set; } = new Voucher();

        public SelectList AccountList { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>());

        public List<string> VoucherTypes { get; set; } = new List<string> { "Journal", "Payment", "Receipt" };

        public async Task<IActionResult> OnGetAsync(string type = "Journal")
        {
            Voucher.VoucherType = type;
            // Ensure details list is initialized to avoid NullReferenceException on subsequent POSTs
            if (Voucher.Details == null)
            {
                Voucher.Details = new List<VoucherDetail>();
            }
            // Set default date to today for convenience on new forms
            if (Voucher.VoucherDate == DateTime.MinValue) // Check if it's already set (e.g., from a previous invalid submission)
            {
                Voucher.VoucherDate = DateTime.Today; // <--- Add this line
            }
            await PopulateAccountList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // IMPORTANT: Model binding populates Voucher.Details from the form.
            // It also populates Voucher.TotalDebit and Voucher.TotalCredit from the hidden inputs.

            // Manual validation for Voucher.Details, AccountId, Debit/Credit combination
            // Check for empty rows or rows with only one amount
            if (Voucher.Details != null && Voucher.Details.Any())
            {
                for (int i = 0; i < Voucher.Details.Count; i++)
                {
                    var detail = Voucher.Details[i];

                    // Remove empty rows if JavaScript added them but they weren't filled
                    if (detail.AccountId == 0 && detail.Debit == 0 && detail.Credit == 0)
                    {
                        // Remove this detail, but be careful with modifying a list while iterating.
                        // A common pattern is to filter it before processing.
                        // For now, let's just add a model error if required.
                        ModelState.AddModelError($"Voucher.Details[{i}].AccountId", "Account, Debit, or Credit must be provided for this detail row.");
                        continue; // Skip further validation for this empty row
                    }

                    if (detail.AccountId == 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].AccountId", "Account is required.");
                    }

                    // Check if both Debit and Credit have values greater than 0
                    if (detail.Debit > 0 && detail.Credit > 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].Debit", "A detail row cannot have both Debit and Credit values simultaneously.");
                        ModelState.AddModelError($"Voucher.Details[{i}].Credit", "A detail row cannot have both Debit and Credit values simultaneously.");
                    }
                    // Check if neither Debit nor Credit has a value
                    else if (detail.Debit == 0 && detail.Credit == 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].Debit", "Either Debit or Credit amount must be entered.");
                        ModelState.AddModelError($"Voucher.Details[{i}].Credit", "Either Debit or Credit amount must be entered.");
                    }
                }

                // Filter out effectively empty rows after validation if desired, or let validation handle it
                Voucher.Details = Voucher.Details.Where(d => d.AccountId > 0 || d.Debit > 0 || d.Credit > 0).ToList();
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Voucher must contain at least one detail entry.");
            }


            if (!ModelState.IsValid)
            {
                await PopulateAccountList();
                return Page();
            }

            // Server-side recalculation and check for balance (redundant if client-side is perfect, but good for security/integrity)
            // This also ensures that if client-side JS fails or is bypassed, totals are still correct.
            decimal calculatedTotalDebit = Voucher.Details.Sum(d => d.Debit);
            decimal calculatedTotalCredit = Voucher.Details.Sum(d => d.Credit);

            // Double-check if the submitted totals match the calculated totals (optional, but good for debugging/security)
            // If they don't match, you might have a client-side issue or tampering.
            if (calculatedTotalDebit != Voucher.TotalDebit || calculatedTotalCredit != Voucher.TotalCredit)
            {
                // Optionally, re-assign to ensure consistency, or log a warning
                Voucher.TotalDebit = calculatedTotalDebit;
                Voucher.TotalCredit = calculatedTotalCredit;
                // You could also add a ModelState error if strict validation is needed here:
                // ModelState.AddModelError(string.Empty, "Submitted totals do not match calculated totals.");
                // await PopulateAccountList();
                // return Page();
            }


            if (calculatedTotalDebit != calculatedTotalCredit)
            {
                ModelState.AddModelError(string.Empty, $"Total Debit ({calculatedTotalDebit:N2}) must equal Total Credit ({calculatedTotalCredit:N2}).");
                await PopulateAccountList();
                return Page();
            }

            // Ensure the voucher details are not empty after filtering
            if (!Voucher.Details.Any())
            {
                ModelState.AddModelError(string.Empty, "Please add at least one valid voucher detail entry with an account and amount.");
                await PopulateAccountList();
                return Page();
            }

            try
            {
                var currentUser = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(currentUser))
                {
                    ModelState.AddModelError(string.Empty, "User information not available. Please log in again.");
                    await PopulateAccountList();
                    return Page();
                }

                // The Voucher object now has its TotalDebit and TotalCredit populated
                // by model binding from the hidden inputs, or recalculated on the server.
                // The SaveVoucherAsync will now receive these correct totals.
                await _voucherService.SaveVoucherAsync(Voucher, "Insert", currentUser);
                TempData["SuccessMessage"] = "Voucher created successfully.";
                return RedirectToPage("./Index");
            }
            catch (SqlException ex)
            {
                // Log the full exception details
                // _logger.LogError(ex, "Error creating voucher.");
                ModelState.AddModelError(string.Empty, $"Error creating voucher: {ex.Message}");
                await PopulateAccountList();
                return Page();
            }
            catch (Exception ex)
            {
                // Log the full exception details
                // _logger.LogError(ex, "An unexpected error occurred during voucher creation.");
                ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
                await PopulateAccountList();
                return Page();
            }
        }

        private async Task PopulateAccountList()
        {
            var accounts = await _chartOfAccountsService.GetAccountsAsync("SelectFlat");
            AccountList = new SelectList(accounts, "AccountId", "AccountName");
        }
    }
}