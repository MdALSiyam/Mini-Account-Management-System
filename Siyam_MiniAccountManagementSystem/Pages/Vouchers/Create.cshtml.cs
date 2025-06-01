using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Siyam_MiniAccountManagementSystem.Models;
using Siyam_MiniAccountManagementSystem.Services;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace Siyam_MiniAccountManagementSystem.Pages.Vouchers
{
    [Authorize(Roles = "Admin,Accountant")]
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
            if (Voucher.Details == null)
            {
                Voucher.Details = new List<VoucherDetail>();
            }

            if (Voucher.VoucherDate == DateTime.MinValue)
            {
                Voucher.VoucherDate = DateTime.Today;
            }
            await PopulateAccountList();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Voucher.Details != null && Voucher.Details.Any())
            {
                for (int i = 0; i < Voucher.Details.Count; i++)
                {
                    var detail = Voucher.Details[i];
                    if (detail.AccountId == 0 && detail.Debit == 0 && detail.Credit == 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].AccountId", "Account, Debit, or Credit must be provided for this detail row.");
                        continue;
                    }

                    if (detail.AccountId == 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].AccountId", "Account is required.");
                    }

                    if (detail.Debit > 0 && detail.Credit > 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].Debit", "A detail row cannot have both Debit and Credit values simultaneously.");
                        ModelState.AddModelError($"Voucher.Details[{i}].Credit", "A detail row cannot have both Debit and Credit values simultaneously.");
                    }

                    else if (detail.Debit == 0 && detail.Credit == 0)
                    {
                        ModelState.AddModelError($"Voucher.Details[{i}].Debit", "Either Debit or Credit amount must be entered.");
                        ModelState.AddModelError($"Voucher.Details[{i}].Credit", "Either Debit or Credit amount must be entered.");
                    }
                }

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

            decimal calculatedTotalDebit = Voucher.Details.Sum(d => d.Debit);
            decimal calculatedTotalCredit = Voucher.Details.Sum(d => d.Credit);

            if (calculatedTotalDebit != Voucher.TotalDebit || calculatedTotalCredit != Voucher.TotalCredit)
            {
                Voucher.TotalDebit = calculatedTotalDebit;
                Voucher.TotalCredit = calculatedTotalCredit;
            }


            if (calculatedTotalDebit != calculatedTotalCredit)
            {
                ModelState.AddModelError(string.Empty, $"Total Debit ({calculatedTotalDebit:N2}) must equal Total Credit ({calculatedTotalCredit:N2}).");
                await PopulateAccountList();
                return Page();
            }

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

                await _voucherService.SaveVoucherAsync(Voucher, "Insert", currentUser);
                TempData["SuccessMessage"] = "Voucher created successfully.";
                return RedirectToPage("./Index");
            }
            catch (SqlException ex)
            {
                ModelState.AddModelError(string.Empty, $"Error creating voucher: {ex.Message}");
                await PopulateAccountList();
                return Page();
            }
            catch (Exception ex)
            {
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