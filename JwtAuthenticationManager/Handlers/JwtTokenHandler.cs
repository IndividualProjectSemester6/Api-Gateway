using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthenticationManager.Models;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthenticationManager.Handlers
{
    public class JwtTokenHandler
    {
        public const string JWT_SECURITY_KEY = "q91NNStZ2B1aD5pZhv3mc5OcsWF582VLetYnWziA54IE1rTD3isw1iDoDt8sOBz";
        private const int JWT_TOKEN_VALIDITY_MINS = 60;
        private const int JWT_TOKEN_VALIDITY_MAXS = 60;
        private readonly List<User> _users;

        public JwtTokenHandler()
        {
            _users = new List<User>()
            {
                new User(username: "Admin", password: "Admin123!", role: "Administrator"),
                new User(username: "User01", password: "User123!", role: "User")
            };
        }

        public AuthenticationResponse? GenerateJwtToken(AuthenticationRequest authenticationRequest)
        {
            if (string.IsNullOrWhiteSpace(authenticationRequest.Username) || string.IsNullOrWhiteSpace(authenticationRequest.Password))
                return null;

            // Validate mock users:
            var userAccount = _users.Where(u => u.Username == authenticationRequest.Username && u.Password == authenticationRequest.Password).FirstOrDefault();
            if (userAccount == null)
                return null;

            // Set up all information we need for the web token:
            var tokenExpiryTimeStamp = DateTime.Now.AddMinutes(JWT_TOKEN_VALIDITY_MINS);
            var tokenKey = Encoding.ASCII.GetBytes(JWT_SECURITY_KEY);
            var claimsIdentity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Name, authenticationRequest.Username),
                new Claim(ClaimTypes.Role, userAccount.Role)
            });

            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(tokenKey),
                SecurityAlgorithms.HmacSha256Signature);

            var securityTokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = claimsIdentity,
                Expires = tokenExpiryTimeStamp,
                SigningCredentials = signingCredentials
            };

            // Generate web token:
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            var webToken = jwtSecurityTokenHandler.WriteToken(securityToken);

            return new AuthenticationResponse()
            {
                Username = userAccount.Username,
                ExpiresIn = (int)tokenExpiryTimeStamp.Subtract(DateTime.Now).TotalSeconds,
                JwtToken = webToken
            };


        }
    }
}
