namespace Hyper;

public static class HyperSerializerSettings
{
	/// <summary>
	/// Set to true to write the serialize proxy code to Console.Write. Default is false;
	/// </summary>
	public static bool WriteProxyToConsoleOutput { get; set; } = false;

	/// <summary>
	/// Set to true to serialize fields. Default is true.
	/// </summary>
	public static bool SerializeFields { get; set; } = true;
}