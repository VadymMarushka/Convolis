using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Convolis.Shared.DTOs.Auth
{
    public class RefreshTokenRequestDTO
    {
        public Guid Id { get; set; }
        public required string RefreshToken { get; set; }
    }
}
