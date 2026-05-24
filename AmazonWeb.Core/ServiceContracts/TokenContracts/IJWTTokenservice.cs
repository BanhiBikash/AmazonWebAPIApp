using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace AmazonWeb.Core.ServiceContracts.TokenContracts
{
    public interface IJWTTokenservice
    {
        string CreateJWTToken(string email, string name, string userId, string role);
        string CreateRefreshToken();
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
