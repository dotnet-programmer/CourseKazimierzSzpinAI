namespace IntelligentApp.Components.Pages.Basics;

public partial class HelloUser
{
	private readonly string _name = "Michał";

	private string GetGreeting()
		=> $"Hello, {_name}!";
}