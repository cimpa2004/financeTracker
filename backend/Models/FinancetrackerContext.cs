using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace backend.Models;

public partial class FinancetrackerContext : DbContext
{
    public FinancetrackerContext()
    {
    }

    public FinancetrackerContext(DbContextOptions<FinancetrackerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Budget> Budgets { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ExternalAccount> ExternalAccounts { get; set; }

    public virtual DbSet<Household> Households { get; set; }

    public virtual DbSet<HouseholdMember> HouseholdMembers { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    // added subscriptions table
    public virtual DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=financetracker;Trusted_Connection=True;MultipleActiveResultSets=true;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.BudgetId).HasName("PK__budget__3A655C14965D44A5");

            entity.ToTable("budget");

            entity.Property(e => e.BudgetId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("budget_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.LastReset)
                .HasColumnType("datetime")
                .HasColumnName("last_reset");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__budget__category__37A5467C");

            entity.HasOne(d => d.User).WithMany(p => p.Budgets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__budget__user_id__36B12243");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__category__D54EE9B43AE1C9B4");

            entity.ToTable("category");

            entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("category_id");
            entity.Property(e => e.Color)
                .HasMaxLength(7)
                .HasColumnName("color");
            entity.Property(e => e.Icon)
                .HasMaxLength(255)
                .HasColumnName("icon");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Categories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK__category__user_i__2D27B809");
        });

        modelBuilder.Entity<ExternalAccount>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__external__46A222CD25BBEC9D");

            entity.ToTable("external_account");

            entity.Property(e => e.AccountId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("account_id");
            entity.Property(e => e.AccessToken).HasColumnName("access_token");
            entity.Property(e => e.ExpiresAt)
                .HasColumnType("datetime")
                .HasColumnName("expires_at");
            entity.Property(e => e.Platform)
                .HasMaxLength(50)
                .HasColumnName("platform");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ExternalAccounts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__external___user___3C69FB99");
        });

        modelBuilder.Entity<Household>(entity =>
        {
            entity.HasKey(e => e.HouseholdId).HasName("PK__househol__95A4F1B0C15510D0");

            entity.ToTable("household");

            entity.HasIndex(e => e.Name, "UQ__househol__72E12F1BA53EA0C2").IsUnique();

            entity.Property(e => e.HouseholdId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("household_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<HouseholdMember>(entity =>
        {
            entity.HasKey(e => new { e.HouseholdId, e.UserId }).HasName("PK__househol__7E3F12C04BE2B202");

            entity.ToTable("household_member");

            entity.Property(e => e.HouseholdId).HasColumnName("household_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValue(false)
                .HasColumnName("is_admin");

            entity.HasOne(d => d.Household).WithMany(p => p.HouseholdMembers)
                .HasForeignKey(d => d.HouseholdId)
                .HasConstraintName("FK__household__house__440B1D61");

            entity.HasOne(d => d.User).WithMany(p => p.HouseholdMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__household__user___44FF419A");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__transact__85C600AFBB62D269");

            entity.ToTable("transaction");

            entity.Property(e => e.TransactionId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Date)
                .HasColumnType("datetime")
                .HasColumnName("date");
            entity.Property(e => e.Description).HasColumnName("description");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            // new optional FK column linking a transaction to a subscription
            entity.Property(e => e.SubscriptionId)
                .HasColumnName("subscription_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__transacti__categ__32E0915F");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__transacti__user___31EC6D26");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK__transaction__subscr__...");
        });

        // new mapping for subscription
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__subscription__subscription_id");

            entity.ToTable("subscription");

            entity.Property(e => e.SubscriptionId)
                .HasDefaultValueSql("(newsequentialid())")
                .HasColumnName("subscription_id");

            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.Property(e => e.CategoryId).HasColumnName("category_id");

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");

            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.Property(e => e.Interval)
                .HasMaxLength(50)
                .HasColumnName("interval");

            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("payment_date");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasColumnName("created_at")
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__subscription__user__...");

            entity.HasOne(d => d.Category).WithMany()
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK__subscription__category__...");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
