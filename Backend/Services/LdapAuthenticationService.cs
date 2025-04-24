using System.DirectoryServices.Protocols;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public interface ILdapAuthenticationService
{
    /// <summary>
    /// Authenticates a user against the LDAP server.
    /// </summary>
    /// <param name="username">The username to authenticate.</param>
    /// <param name="password">The password for the username.</param>
    /// <returns>True if authentication is successful, otherwise false.</returns>
    bool Authenticate(string username, string password);
}

public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly IConfiguration _configuration;

    public LdapAuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool Authenticate(string username, string password)
    {
        try
        {
            var (ldapServer, ldapPort, domain) = GetLdapConfiguration();
            using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, ldapPort))
            {
                Credential = new NetworkCredential($"{domain}\\{username}", password),
                AuthType = AuthType.Negotiate
            };
            connection.Bind();
            return true;
        }
        catch (LdapException)
        {
            // Log exception if needed
            return false;
        }
        catch (Exception ex)
        {
            // Log exception if needed
            throw new InvalidOperationException("An error occurred during LDAP authentication.", ex);
        }
    }

    private (string ldapServer, int ldapPort, string domain) GetLdapConfiguration()
    {
        var ldapServer = _configuration["Ldap:Server"];
        var ldapPort = int.Parse(_configuration["Ldap:Port"]);
        var domain = _configuration["Ldap:Domain"];
        return (ldapServer, ldapPort, domain);
    }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILdapAuthenticationService _ldapService;
    private readonly IConfiguration _configuration;

    public AuthController(ILdapAuthenticationService ldapService, IConfiguration configuration)
    {
        _ldapService = ldapService;
        _configuration = configuration;
    }

    /// <summary>
    /// Authenticates a user using LDAP.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>HTTP 200 if successful, HTTP 401 otherwise.</returns>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (_ldapService.Authenticate(request.Username, request.Password))
        {
            var token = GenerateJwtToken(request.Username);
            return Ok(new 
            { 
                message = "Prisijungta sėkmingai",
                token = token 
            });
        }
        return Unauthorized(new { message = "Neteisingas slaptažodis arba prisijungimo vardas" });
    }
    /// <summary>
    /// Generuoja JWT tokeną.
    /// </summary>
    /// <param name="username">Vartotojo id</param>
    /// <returns>Sugeneruota JWT tokena</returns>
    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    /// <summary>
    /// The username for authentication.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// The password for authentication.
    /// </summary>
    public string Password { get; set; }
}
