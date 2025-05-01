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

    /// <summary>
    /// Retrieves the name of the user from the LDAP server.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="password">The password for the username.</param>
    /// <returns>The name of the user if found, otherwise null.</returns>
    string getName(string username, string password);
}

public class LdapAuthenticationService : ILdapAuthenticationService
{
    private readonly IConfiguration _configuration;

    public LdapAuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string getName(string username, string password)
    {
        try
        {
            var (ldapServer, baseDn, domain) = GetLdapConfiguration();
            using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, 636))
            {
                Credential = new NetworkCredential($"{username}{domain}", password),
                AuthType = AuthType.Negotiate,
                SessionOptions =
                {
                    SecureSocketLayer = true,
                    VerifyServerCertificate = (con, cert) => true
                }
            };
            connection.Bind();

            var searchRequest = new SearchRequest(
                baseDn,
                $"(sAMAccountName={username})",
                SearchScope.Subtree
            );
            var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
            if (searchResponse.Entries.Count > 0 && 
                searchResponse.Entries[0].Attributes["cn"]?.Count > 0)
            {
                return string.Concat(searchResponse.Entries[0].Attributes["cn"][0].ToString().Split(' '));
            }
            return null;
        }
        catch (LdapException)
        {
            // Log exception if needed
            return null;
        }
        catch (Exception ex)
        {
            // Log exception if needed
            throw new InvalidOperationException("An error occurred during LDAP authentication.", ex);
        }
    }

    public bool Authenticate(string username, string password)
    {
        try
        {
            var (ldapServer, baseDn, domain) = GetLdapConfiguration();
            using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, 636))
            {
                Credential = new NetworkCredential($"{username}{domain}", password),
                AuthType = AuthType.Negotiate,
                SessionOptions =
                {
                    SecureSocketLayer = true,
                    VerifyServerCertificate = (con, cert) => true
                }
            };
            connection.Bind();

            var searchRequest = new SearchRequest(
                baseDn,
                $"(sAMAccountName={username})",
                SearchScope.Subtree
            );
            var searchResponse = (SearchResponse)connection.SendRequest(searchRequest);
            if (searchResponse.Entries.Count > 0)
            {
                return true;
            }
            return false;
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

    private (string ldapServer, string baseDn, string domain) GetLdapConfiguration()
    {
        var ldapServer = _configuration["Ldap:Server"];
        var baseDn = _configuration["Ldap:BaseDn"];
        var domain = _configuration["Ldap:Domain"];
        return (ldapServer, baseDn, domain);
    }
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILdapAuthenticationService _ldapService;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _context;


    public AuthController(ILdapAuthenticationService ldapService, IConfiguration configuration, AppDbContext context)
    {
        _ldapService = ldapService;
        _configuration = configuration;
        _context = context;
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
            var name = _ldapService.getName(request.Username, request.Password);
            var naudotojas = _context.Naudotojas.FirstOrDefault(n => n.Id == name);
            if (naudotojas != null) {
                var token = GenerateJwtToken(name);
                return Ok(new 
                { 
                    message = "Prisijungta sėkmingai",
                    token = token 
                });
            }
            else 
            {
                return Unauthorized(new { message = "Vartotojas nerastas, duombazėje.\n Jeigu jūs tikrai priklausot vpgt ir neišeina prisijungt kontaktuokit \n dominykas.pranaitis@vpgt.lt" });
            }
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
