using System.DirectoryServices.Protocols;
using System.Net;

public interface ILdapAuthenticationService
{
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
        var ldapServer = _configuration["Ldap:Server"];
        var ldapPort = int.Parse(_configuration["Ldap:Port"]);
        var domain = _configuration["Ldap:Domain"];
        var baseDn = _configuration["Ldap:BaseDn"]; // Retrieve BaseDn from configuration

        try
        {
            using var connection = new LdapConnection(new LdapDirectoryIdentifier(ldapServer, ldapPort));
            connection.Credential = new NetworkCredential($"{domain}\\{username}", password);
            connection.AuthType = AuthType.Negotiate;
            connection.Bind();

            // Optionally, use baseDn for further LDAP operations if needed
            return true;
        }
        catch (LdapException)
        {
            return false;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
