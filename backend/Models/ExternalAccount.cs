using System;
using System.Collections.Generic;

namespace backend.Models;

public partial class ExternalAccount
{
    public Guid AccountId { get; set; }

    public Guid UserId { get; set; }

    public string Platform { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public DateTime? ExpiresAt { get; set; }

    public virtual User User { get; set; } = null!;
}
