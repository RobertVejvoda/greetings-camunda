using System;
using System.ComponentModel.DataAnnotations;

namespace sim
{
  public class GreetingRequest
  {
        [Required]
        public Guid GreetingId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string Email { get; set; }
        
        public override string ToString()
			=> $"{GreetingId}, {Name}, {Email}";
	}
}