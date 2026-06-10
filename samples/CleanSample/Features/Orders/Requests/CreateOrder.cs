namespace CleanSample.Features.Orders.Requests
{
    // Co-located by the Request/Response suffix group with a shared "CreateOrder" stem.
    internal sealed class CreateOrderRequest
    {
        public string? Product { get; set; }
    }

    internal sealed class CreateOrderResponse
    {
        public string? OrderId { get; set; }
    }
}
