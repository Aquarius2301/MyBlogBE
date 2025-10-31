using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace BusinessObject.Models;

[Index(nameof(AccountId), nameof(FollowingId), IsUnique = true)]
public class Follow
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AccountId { get; set; }

    [Required]
    public Guid FollowingId { get; set; }

    // Navigation
    public Account Account { get; set; } = null!;
    public Account Following { get; set; } = null!;
}
