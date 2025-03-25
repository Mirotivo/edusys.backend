using System;
using Backend.Interfaces.Billing;
using Microsoft.Extensions.DependencyInjection;

public class PaymentGatewayFactory : IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IPaymentGateway GetPaymentGateway(string gatewayName)
    {
        return gatewayName switch
        {
            "PayPal" => _serviceProvider.GetService<PayPalPaymentGateway>()
                        ?? throw new InvalidOperationException("PayPal payment gateway is not registered."),
            "Stripe" => _serviceProvider.GetService<StripePaymentGateway>()
                        ?? throw new InvalidOperationException("Stripe payment gateway is not registered."),
            _ => throw new ArgumentException("Invalid payment gateway")
        };
    }
}

