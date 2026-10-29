using System.Collections.Generic;
using System.Linq;
using System.Text;

// Checks player names against a word list (Assets/Data/ProfanityWords.txt). Names have no spaces, so a listed word
// anywhere in the name blocks it; look-alike characters (sh1t) and repeated letters (fuuuck) are normalized first.
// Lines starting with + are allowed words (assassin, titan, ...), blanked out before matching.
public class ProfanityFilter
{
    // Below this length, collapsing repeats would turn words into common fragments ("ass" -> "as")
    private const int MinCollapsedLength = 4;

    private readonly List<string> blocked = new();
    private readonly List<string> blockedCollapsed = new();
    private readonly List<string> blockedSymbols = new();
    private readonly List<string> allowed = new();

    public ProfanityFilter(string wordList)
    {
        foreach (string rawLine in wordList.Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#"))
            {
                continue;
            }

            if (line.StartsWith("+"))
            {
                allowed.Add(Normalize(line.Substring(1)));
                continue;
            }

            string word = Normalize(line);
            if (word.Length == 0)
            {
                // Only symbols (e.g. an emoji); matched as written
                blockedSymbols.Add(line);
                continue;
            }

            blocked.Add(word);

            string collapsed = Collapse(word);
            if (collapsed.Length >= MinCollapsedLength)
            {
                blockedCollapsed.Add(collapsed);
            }
        }

        // Longest first, so "classic" is blanked before "class"
        allowed.Sort((a, b) => b.Length.CompareTo(a.Length));
    }

    public bool IsAllowed(string name)
    {
        if (blockedSymbols.Any(name.Contains))
        {
            return false;
        }

        string normalized = Normalize(name);
        foreach (string word in allowed)
        {
            normalized = normalized.Replace(word, " ");
        }

        string collapsed = Collapse(normalized);
        return !blocked.Any(normalized.Contains) && !blockedCollapsed.Any(collapsed.Contains);
    }

    // Lowercase, look-alikes mapped to letters, everything but letters and digits dropped
    private static string Normalize(string text)
    {
        var result = new StringBuilder(text.Length);

        foreach (char raw in text.ToLowerInvariant())
        {
            char c = raw switch
            {
                '0' => 'o',
                '1' or '!' or '|' => 'i',
                '3' => 'e',
                '4' or '@' => 'a',
                '5' or '$' => 's',
                '7' => 't',
                _ => raw
            };

            if (char.IsLetterOrDigit(c))
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }

    private static string Collapse(string text)
    {
        var result = new StringBuilder(text.Length);

        foreach (char c in text)
        {
            if (result.Length == 0 || result[result.Length - 1] != c)
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
