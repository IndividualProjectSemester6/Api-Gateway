namespace JwtAuthenticationManager.Models
{
    public class AuthenticationResponse
    {
        public string Username { get; set; }
        public string JwtToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
