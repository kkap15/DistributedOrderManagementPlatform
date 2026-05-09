using System;
using System.ComponentModel.DataAnnotations;

namespace UserService.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Auth0Id { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}