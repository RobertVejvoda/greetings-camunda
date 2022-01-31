using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sim
{
	public class GreetingName
	{
		public string FullName => $"{FirstName} {LastName}";

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
	}
}
