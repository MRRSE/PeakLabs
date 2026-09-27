using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeakLabs.Models.Identity;

namespace PeakLabs.Data;

public class ApplicationDbContext : IdentityDbContext<PeakLabsUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
