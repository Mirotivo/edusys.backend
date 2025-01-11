using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
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
    private readonly JitsiOptions _options;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(
        IOptions<JitsiOptions> options,
        ILogger<JwtTokenService> logger
    )
    {
        _options = options.Value;
        _logger = logger;
    }

    public Meeting GetMeeting(string userName, string roomName)
    {
        // Generate the token
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.AppSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var header = new JwtHeader(credentials);
        var expirationTime = DateTime.UtcNow.AddHours(1);

        var payload = new JwtPayload
        {
            { "context", JsonConvert.SerializeObject(new { user = new { name = userName } }) },
            { "role", "moderator" },
            { "iss", _options.AppId },
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
        var meetingUrl = $"https://{_options.Domain}/{roomName}?token={tokenString}";

        // Return the meeting object
        return new Meeting
        {
            Token = tokenString,
            MeetingUrl = meetingUrl,
            Domain = _options.Domain,
            ServerUrl = $"https://{_options.Domain}/",
            RoomName = roomName,
            UserName = userName
        };
    }
}
