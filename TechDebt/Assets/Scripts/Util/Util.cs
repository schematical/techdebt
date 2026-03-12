
public class Util
{
    public static string GetDisplayable(string input)
    {
        string[] parts = input.Split('_');
        input = parts[parts.Length - 1];
        // Convert this from camel case to spaced so "LoadPerPacket" becomes "Load Per Packet" 
        input = System.Text.RegularExpressions.Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");
        return input;
    }
}
