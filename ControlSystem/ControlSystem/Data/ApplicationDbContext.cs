using ControlSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using YourProject.Models;

namespace YourProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectStage> ProjectStages { get; set; }
        public DbSet<Defect> Defects { get; set; }
        public DbSet<DefectAttachment> DefectAttachments { get; set; }
        public DbSet<DefectComment> DefectComments { get; set; }
        public DbSet<DefectHistory> DefectHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Project>()
                .HasMany(p => p.Stages)
                .WithOne(s => s.Project)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Project>()
                .HasMany(p => p.Defects)
                .WithOne(d => d.Project)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Defect>()
                .HasMany(d => d.Attachments)
                .WithOne(a => a.Defect)
                .HasForeignKey(a => a.DefectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Defect>()
                .HasMany(d => d.Comments)
                .WithOne(c => c.Defect)
                .HasForeignKey(c => c.DefectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Defect>()
                .HasMany(d => d.History)
                .WithOne(h => h.Defect)
                .HasForeignKey(h => h.DefectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
