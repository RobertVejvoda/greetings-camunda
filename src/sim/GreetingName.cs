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
