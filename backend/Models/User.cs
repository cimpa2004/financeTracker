using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<ExternalAccount> ExternalAccounts { get; set; } = new List<ExternalAccount>();

    public virtual ICollection<HouseholdMember> HouseholdMembers { get; set; } = new List<HouseholdMember>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
