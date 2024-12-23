public class PaymentGatewayFactory
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
            "PayPal" => _serviceProvider.GetService<PayPalPaymentGateway>(),
            "Stripe" => _serviceProvider.GetService<StripePaymentGateway>(),
            _ => throw new ArgumentException("Invalid payment gateway")
        };
    }
}
