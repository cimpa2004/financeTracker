using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class Household
{
    public Guid HouseholdId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<HouseholdMember> HouseholdMembers { get; set; } = new List<HouseholdMember>();
}
