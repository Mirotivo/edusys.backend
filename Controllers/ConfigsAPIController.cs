using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace skillseek.Controllers;

[Route("api/configs")]
[ApiController]
public class ConfigsAPIController : BaseController
{
    private readonly StripeOptions _stripeOptions;
    private readonly PayPalOptions _payPalOptions;
    private readonly GoogleOptions _googleOptions;

    public ConfigsAPIController(
        IOptions<StripeOptions> stripeOptions,
        IOptions<PayPalOptions> payPalOptions,
        IOptions<GoogleOptions> googleOptions
    )
    {
        _stripeOptions = stripeOptions.Value;
        _payPalOptions = payPalOptions.Value;
        _googleOptions = googleOptions.Value;
    }


    [HttpGet]
    public IActionResult GetConfig()
    {
        var config = new
        {
            stripePublishableKey = _stripeOptions.PublishableKey,
            payPalClientId = _payPalOptions.ClientId,
            googleMapsApiKey = _googleOptions.ApiKey
        };

        return Ok(config);
    }
}

