using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class HouseholdMember
{
    public Guid HouseholdId { get; set; }

    public Guid UserId { get; set; }

    public bool? IsAdmin { get; set; }

    public virtual Household Household { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
