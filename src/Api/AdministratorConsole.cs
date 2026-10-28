namespace Api;

internal static class AdministratorConsole
{
    internal static string ReadRequired(string prompt)
    {
        Console.Write(prompt);
        var value = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("A value is required.");
        }

        return value;
    }

    internal static string ReadPassword(string prompt)
    {
        Console.Write(prompt);
        var characters = new List<char>();

        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return new string(characters.ToArray());
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (characters.Count > 0)
                {
                    characters.RemoveAt(characters.Count - 1);
                }

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                characters.Add(key.KeyChar);
            }
        }
    }
}
