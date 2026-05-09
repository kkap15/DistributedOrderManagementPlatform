using System;

namespace UserService.Models;

public record UserClaims
{
    public string Auth0IdClaim;
    public string NameClaim;
    public string EmailClaim;
}