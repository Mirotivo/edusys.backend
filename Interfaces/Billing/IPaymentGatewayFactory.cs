namespace Backend.Interfaces.Billing
{
    public interface IPaymentGatewayFactory
    {
        IPaymentGateway GetPaymentGateway(string gatewayName);
    }
}

