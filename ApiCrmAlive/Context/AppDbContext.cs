using ApiCrmAlive.Models;
using ApiCrmAlive.Utils;
using Microsoft.EntityFrameworkCore;

namespace ApiCrmAlive.Context
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FipeBrand>(b =>
            {
                b.HasKey(x => x.BrandCode);
                b.Property(x => x.Name).HasMaxLength(120).IsRequired();
                b.HasIndex(x => x.Name);
            });

            modelBuilder.Entity<FipeModel>(m =>
            {
                m.HasKey(x => new { x.BrandCode, x.ModelCode });
                m.Property(x => x.Name).HasMaxLength(200).IsRequired();
                m.HasIndex(x => new { x.BrandCode, x.Name });

                m.HasOne(x => x.Brand)
                    .WithMany(b => b.Models)
                    .HasForeignKey(x => x.BrandCode)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Vehicle>()
                .Property(v => v.Status)
                .HasDefaultValue(VehicleStatusEnum.Rascunho);

            modelBuilder.Entity<Company>(c =>
            {
                c.HasIndex(x => x.Cnpj).IsUnique();
                c.HasIndex(x => x.Name);
                c.Property(x => x.HasSdr).HasDefaultValue(false);
            });

            modelBuilder.Entity<User>(u =>
            {
                u.HasIndex(x => x.CompanyId);
                u.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Sale>(s =>
            {
                // Sale pode existir sem Lead vinculado; se o Lead for removido, mantemos a venda e zeramos o FK.
                s.HasOne(x => x.Lead)
                    .WithMany()
                    .HasForeignKey(x => x.LeadId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Lead>(l =>
            {
                l.HasIndex(x => x.Phone).IsUnique();
                l.HasIndex(x => x.CompanyId);

                l.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);

                l.HasMany(x => x.LossReasonLinks)
                    .WithOne(x => x.Lead)
                    .HasForeignKey(x => x.LeadId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LeadLossReason>(r =>
            {
                r.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
                r.HasIndex(x => x.CompanyId);
                r.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LeadLossReasonLink>(l =>
            {
                l.HasIndex(x => new { x.LeadId, x.LossReasonId }).IsUnique();
                l.HasOne(x => x.LossReason)
                    .WithMany(x => x.LeadLinks)
                    .HasForeignKey(x => x.LossReasonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<FipeBrand> FipeBrands { get; set; }
        public DbSet<FipeModel> FipeModels { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<LeadLossReason> LeadLossReasons { get; set; }
        public DbSet<LeadLossReasonLink> LeadLossReasonLinks { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<LeadInteraction> LeadInteractions { get; set; }
        public DbSet<Marketplace> Marketplaces { get; set; }
        public DbSet<MarketplaceConfiguration> MarketplaceConfigurations { get; set; }
        public DbSet<MarketplaceSyncLog> MarketplaceSyncLogs { get; set; }
        public DbSet<SellerQueueState> SellerQueueStates { get; set; }

    }
}
