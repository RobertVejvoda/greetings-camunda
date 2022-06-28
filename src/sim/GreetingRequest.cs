namespace sim
{
    public record struct GreetingRequest(Guid GreetingId, string Name, string Email);
}