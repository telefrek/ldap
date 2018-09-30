using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Models
{
    public class IndexModel : PageModel
    {
        [Authorize]
        public Task OnGetAsync() => Task.CompletedTask;
    }
}