using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public LoginViewModel()
    {
        Email = string.Empty;
        Password = string.Empty;
    }

}
public class SocialLoginRequest
{
    //[Required]
    public string Provider { get; set; } = string.Empty;

    //[Required]
    public string Token { get; set; } = string.Empty;
}
public class SocialUser
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
public class GoogleUserPayload
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
}
public class FacebookUserPayload
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public FacebookPicture Picture { get; set; }
}

public class FacebookPicture
{
    public FacebookPictureData Data { get; set; }
}

public class FacebookPictureData
{
    public string Url { get; set; } = string.Empty;
}

public class SocialLoginResult
{
    public string Token { get; set; } = string.Empty;
    public List<string> Roles { get; set; }
    public bool isRegistered { get; set; }
}

