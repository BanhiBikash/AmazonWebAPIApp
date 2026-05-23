using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.ServiceContracts.TokenContracts
{
    public interface IJWTTokenservice
    {
        string CreateJWTToken(string email, string name, string userId, string role);
        string CreateRefreshToken();
    }
}
