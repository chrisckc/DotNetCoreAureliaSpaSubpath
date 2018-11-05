using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace dotnetcore21_spa.Pages
{
    public class IndexModel : PageModel
    {
        public string Message { get; private set; } = "Hello!";

        public void OnGet()
        {
            Message += $" Server time is { DateTime.Now }";
        }
    }
}