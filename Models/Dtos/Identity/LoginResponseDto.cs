using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos.Identity
{
    public class LoginResponseDto
    {
        public UserDto User { get; set; }
        public TokenDto Token { get; set; }
        public string Role { get; set; }

        public LoginResponseDto()
        {
            User = new UserDto();
            Token = new TokenDto();
        }
    }
}
