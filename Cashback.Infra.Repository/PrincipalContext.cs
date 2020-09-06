using Cashback.Domain.Constants;
using Cashback.Domain.Entities;
using Cashback.Infra.CrossCutting.Auth;
using Microsoft.EntityFrameworkCore;
using System;

namespace Cashback.Infra.Repository
{
    public class PrincipalContext : DbContext
    {
        public PrincipalContext(DbContextOptions<PrincipalContext> options)
          : base(options)
        {
        }

        public DbSet<Reseller> Reselers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Reseller>().Property(t => t.Id).IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<Reseller>().Property(t => t.CPF).IsRequired().HasColumnType("CHAR(14)");
            modelBuilder.Entity<Reseller>().Property(t => t.Name).IsRequired().HasColumnType("VARCHAR(100)");
            modelBuilder.Entity<Reseller>().Property(t => t.Email).IsRequired().HasColumnType("VARCHAR(250)");
            modelBuilder.Entity<Reseller>().Property(t => t.Password).IsRequired().HasColumnType("VARCHAR(64)");
            modelBuilder.Entity<Reseller>().Property(t => t.Password).IsRequired();

            modelBuilder.Entity<Reseller>().HasData(new Reseller() { Id = Guid.NewGuid(), Name = "Usuário [153.509.460-56]", Email = "15350946056@teste.com.br", CPF = "153.509.460-56", Password = Hasher.CreatePasswordHash("15350946056@teste.com.br", "15350946056"), AutoApproved = true });

            modelBuilder.Entity<PurchaseOrder>().Property(t => t.Id).IsRequired().ValueGeneratedNever();
            modelBuilder.Entity<PurchaseOrder>().Property(t => t.Date).IsRequired().ValueGeneratedNever().HasDefaultValue(DateTime.Now);
            modelBuilder.Entity<PurchaseOrder>().Property(t => t.ResellerId).IsRequired();
            modelBuilder.Entity<PurchaseOrder>().Property(t => t.Status).IsRequired().HasColumnType("CHAR(1)").HasDefaultValue(Constants.PURCHASE_STATUS_WAITING_APPROVAL);
            modelBuilder.Entity<PurchaseOrder>().Property(t => t.Value).IsRequired();
        }

    }
}
