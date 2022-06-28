using System;
using System.ComponentModel.DataAnnotations;

namespace greetings_camunda.Services
{
    public record struct GreetingRequest(Guid GreetingId, string Name, string Email);
}
