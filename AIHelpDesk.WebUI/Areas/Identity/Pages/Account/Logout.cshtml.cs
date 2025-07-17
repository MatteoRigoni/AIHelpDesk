using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System;

public class LogoutModel : PageModel
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public LogoutModel(SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    // GET: /Identity/Account/Logout?returnUrl=/   ← logout immediato
    public async Task<IActionResult> OnGet(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        returnUrl ??= Url.Content("~/");
        return LocalRedirect(returnUrl);
    }

    // POST (lasciato per compatibilità)
    public async Task<IActionResult> OnPost(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        returnUrl ??= Url.Content("~/");
        return LocalRedirect(returnUrl);
    }
}
