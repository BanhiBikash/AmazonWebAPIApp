using System;
using System.Collections.Generic;
using System.Text;

namespace AmazonWeb.Core.DTO.AccountDTO
{
    public class TokenRequestDTO
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
    }
}
