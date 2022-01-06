using System;
using System.Collections.Generic;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class AuthResponse
    {
        public AuthSuccessData Success { get; set; }
        public AuthErrorData Error { get; set; }
    }
}
