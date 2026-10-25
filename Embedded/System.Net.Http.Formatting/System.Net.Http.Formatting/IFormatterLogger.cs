namespace System.Net.Http.Formatting;

public interface IFormatterLogger
{
	void LogError(string errorPath, string errorMessage);

	void LogError(string errorPath, Exception exception);
}
