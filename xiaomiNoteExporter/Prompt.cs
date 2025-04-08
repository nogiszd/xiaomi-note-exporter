using System.Drawing;

using Pastel;

namespace xiaomiNoteExporter;

public class Prompt(string message, string? defaultValue = "")
{
    private readonly string _message = message;
    private readonly string _defaultValue = defaultValue!;

    private static string InsertAfterSpace(string str, string insertion, Color? color = null)
    {
        var spaceIndex = str.IndexOf(' ');

        if (spaceIndex > 0)
        {
            return str.Insert(spaceIndex, $" {insertion.Pastel(color ?? Color.Red)}");
        }
        else
        {
            return str;
        }
    }

    /// <summary>
    /// Prompt the user for input.
    /// </summary>
    /// <param name="isToggle">If <c>true</c> then do not return received value, only wait for key to be pressed.</param>
    /// <returns>Received value from keyboard.</returns>
    public string Ask(bool isToggle = false)
    {
        Console.WriteLine(_message);

        if (isToggle)
        {
            Console.ReadKey();
            return _defaultValue;
        } 
        else
        {
            while (true)
            {
                var result = Console.ReadLine();

                if (string.IsNullOrEmpty(result))
                {
                    if (!string.IsNullOrEmpty(_defaultValue))
                    {
                        Console.WriteLine(_defaultValue.Pastel(Color.DimGray));
                        return _defaultValue;
                    } 
                    else
                    {
                        Console.Clear();
                        Console.WriteLine($"{InsertAfterSpace(_message, "valid")}");
                    }
                } 
                else
                {
                    return result;
                }
            }
        }
    }
}
