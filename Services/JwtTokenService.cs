using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

public class Meeting
{
    public string Token { get; set; } = string.Empty;
    public string MeetingUrl { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ServerUrl { get; set; } = string.Empty;
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JitsiOptions _jitsiOptions;
    private readonly JwtOptions _jwtOptions;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IOptions<JitsiOptions> jitsiOptions,
        IOptions<JwtOptions> jwtOptions,
        UserManager<User> userManager,
        ILogger<JwtTokenService> logger
    )
    {
        _jitsiOptions = jitsiOptions.Value;
        _jwtOptions = jwtOptions.Value;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.FullName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_jwtOptions.ExpiryDays),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(_jwtOptions.Key ?? string.Empty)), SecurityAlgorithms.HmacSha256Signature),
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return tokenString;
    }

    public Meeting GetMeeting(string userName, string roomName)
    {
        // Generate the token
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jitsiOptions.AppSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var header = new JwtHeader(credentials);
        var expirationTime = DateTime.UtcNow.AddHours(1);

        var payload = new JwtPayload
        {
            { "context", JsonConvert.SerializeObject(new { user = new { name = userName } }) },
            { "role", "moderator" },
            { "iss", _jitsiOptions.AppId },
            { "aud", "meet.jitsi.com" },
            { "sub", "meet.jitsi.com" },
            { "room", roomName },
            { "exp", new DateTimeOffset(expirationTime).ToUnixTimeSeconds() },
            { "iat", new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() }
        };

        var secToken = new JwtSecurityToken(header, payload);
        var handler = new JwtSecurityTokenHandler();
        var tokenString = handler.WriteToken(secToken);

        // Construct the meeting URL
        var meetingUrl = $"https://{_jitsiOptions.Domain}/{roomName}?token={tokenString}";

        // Return the meeting object
        return new Meeting
        {
            Token = tokenString,
            MeetingUrl = meetingUrl,
            Domain = _jitsiOptions.Domain,
            ServerUrl = $"https://{_jitsiOptions.Domain}/",
            RoomName = roomName,
            UserName = userName
        };
    }
}

