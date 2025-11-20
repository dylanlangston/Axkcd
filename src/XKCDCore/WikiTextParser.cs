using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace XKCDCore;

// Inspired by https://stackoverflow.com/a/61364514
public partial class WikiTextParser
{
    public static WikiTextParser Instance = new();

    private WikiTextParser() { }

    private class ParseState
    {
        public int ComicId { get; }
        public int RefNum { get; set; } = 0;
        public List<string> Refs { get; } = new();

        public ParseState(int comicId)
        {
            ComicId = comicId;
        }
    }

    #region Regex Definitions
    [GeneratedRegex("^(=+)(.*?)\\1$")]
    private static partial Regex HeadingsRegex();

    [GeneratedRegex(@"\[\[([0-9]+): ([^\]|]+)(?:\|[^\]]+)?\]\]")]
    private static partial Regex XkcdComicLinkRegex();

    [GeneratedRegex(@"\[\[([^\]|]+)(?:\|([^\]]+))?\]\]")]
    private static partial Regex InternalWikiLinkRegex();

    [GeneratedRegex(@"\{\{[wW]\|([^|}]+)(?:\|([^}]+))?\}\}")]
    private static partial Regex WikipediaLinkRegex();

    [GeneratedRegex(@"\[(https?://[^ ]+) ([^\]]+)\]")]
    private static partial Regex ExternalLinkWithTextRegex();

    [GeneratedRegex(@"\[(https?://[^\]]+)\]")]
    private static partial Regex ExternalLinkSimpleRegex();

    [GeneratedRegex("<ref>(.+?)</ref>")]
    private static partial Regex ReferenceRegex();

    [GeneratedRegex("'''(.+?)'''")]
    private static partial Regex BoldRegex();

    [GeneratedRegex("''(.+?)''")]
    private static partial Regex ItalicsRegex();
    #endregion

    public string Parse(string wikitext, int comicId)
    {
        var state = new ParseState(comicId);
        var html = new StringBuilder();
        var bulletLevel = 0;
        var inQuote = false;
        var inTableRow = false;

        var remainingText = wikitext.AsSpan();
        while (!remainingText.IsEmpty)
        {
            var lineEnd = remainingText.IndexOfAny('\r', '\n');
            var rawLine = lineEnd == -1 ? remainingText : remainingText.Slice(0, lineEnd);

            // Advance the span
            if (lineEnd != -1)
            {
                int skip =
                    remainingText[lineEnd] == '\r'
                    && lineEnd + 1 < remainingText.Length
                    && remainingText[lineEnd + 1] == '\n'
                        ? 2
                        : 1;
                remainingText = remainingText[(lineEnd + skip)..];
            }
            else
            {
                remainingText = ReadOnlySpan<char>.Empty;
            }

            var line = rawLine.Trim();
            if (line.IsEmpty)
                continue;

            // Bullet points
            if (line.StartsWith("*".AsSpan()))
            {
                if (inQuote)
                {
                    html.Append("</dl>");
                    inQuote = false;
                }

                var currentLevel = 0;
                foreach (var c in line)
                {
                    if (c == '*')
                        currentLevel++;
                    else
                        break;
                }

                while (bulletLevel < currentLevel)
                {
                    html.Append("<ul>");
                    bulletLevel++;
                }
                while (bulletLevel > currentLevel)
                {
                    html.Append("</ul>");
                    bulletLevel--;
                }

                html.Append("<li>");
                ConvertLine(line[currentLevel..].Trim(), html, state);
                html.Append("</li>");
                continue;
            }

            // Close bullet lists if not continuing
            while (bulletLevel > 0)
            {
                html.Append("</ul>");
                bulletLevel--;
            }

            // Quotes
            if (line.StartsWith(":".AsSpan()))
            {
                if (!inQuote)
                {
                    html.Append("<dl>");
                    inQuote = true;
                }
                html.Append("<dd>");
                ConvertLine(line[1..].Trim(), html, state);
                html.Append("</dd>");
                continue;
            }
            else if (inQuote)
            {
                html.Append("</dl>");
                inQuote = false;
            }

            // Tables
            if (line.StartsWith("{|".AsSpan()))
            {
                var attrs = line[2..].Trim().ToString();
                html.Append($"<table {attrs}>");
                continue;
            }
            if (line.StartsWith("|-".AsSpan()))
            {
                if (inTableRow)
                    html.Append("</tr>");
                html.Append("<tr>");
                inTableRow = true;
                continue;
            }
            if (line.StartsWith("|}".AsSpan()))
            {
                if (inTableRow)
                {
                    html.Append("</tr>");
                    inTableRow = false;
                }
                html.Append("</table>");
                continue;
            }
            if (line.StartsWith("!".AsSpan()))
            {
                if (!inTableRow)
                    html.Append("<tr>");
                AppendTableCells(line[1..], "th", "!!".AsSpan(), html, state);
                html.Append("</tr>");
                inTableRow = false;
                continue;
            }
            if (line.StartsWith("|".AsSpan()))
            {
                if (!inTableRow)
                    html.Append("<tr>");
                AppendTableCells(line[1..], "td", "||".AsSpan(), html, state);
                html.Append("</tr>");
                inTableRow = false;
                continue;
            }

            // Paragraphs
            html.Append("<p>");
            ConvertLine(line, html, state);
            html.Append("</p>");
        }

        // Close any remaining open tags
        while (bulletLevel-- > 0)
            html.Append("</ul>");
        if (inQuote)
            html.Append("</dl>");
        if (inTableRow)
            html.Append("</tr>");

        if (state.Refs.Count > 0)
        {
            html.Append("<div class='references'><ol>");
            for (int i = 0; i < state.Refs.Count; i++)
            {
                html.Append($"<li id='note-{i}'><a href='#ref-{i}'>↑</a><span>{state.Refs[i]}</span></li>");
            }
            html.Append("</ol></div>");
        }

        return html.ToString();
    }

    private void AppendTableCells(
        ReadOnlySpan<char> line,
        string cellType,
        ReadOnlySpan<char> delimiter,
        StringBuilder html,
        ParseState state
    )
    {
        var remaining = line;
        while (true)
        {
            var index = remaining.IndexOf(delimiter);
            var cellContent = index == -1 ? remaining : remaining[..index];

            html.Append($"<{cellType}>");
            ConvertLine(cellContent.Trim(), html, state);
            html.Append($"</{cellType}>");

            if (index == -1)
                break;
            remaining = remaining[(index + delimiter.Length)..];
        }
    }

    private record MatchInfo(Match Match, Action<Match, StringBuilder, ParseState> Appender);

    private void ConvertLine(ReadOnlySpan<char> line, StringBuilder html, ParseState state)
    {
        if (line.IsEmpty)
            return;
        var lineAsString = line.ToString();

        var headingMatch = HeadingsRegex().Match(lineAsString);
        if (headingMatch.Success && headingMatch.Index == 0 && headingMatch.Length == line.Length)
        {
            var level = Math.Min(headingMatch.Groups[1].Length, 6);
            html.Append($"<h{level}>");
            ConvertLine(headingMatch.Groups[2].Value.AsSpan(), html, state);
            html.Append($"</h{level}>");
            return;
        }

        var matches = new List<MatchInfo>();

        Action<Match, StringBuilder, ParseState> Nested(string open, string close, int g) =>
            (m, sb, st) =>
            {
                sb.Append(open);
                ConvertLine(m.Groups[g].Value.AsSpan(), sb, st);
                sb.Append(close);
            };

        foreach (Match m in XkcdComicLinkRegex().Matches(lineAsString))
            matches.Add(
                new(
                    m,
                    (x, sb, st) =>
                        sb.Append(
                            $"<a href=\"https://xkcd.com/{x.Groups[1].Value}\">{x.Groups[1].Value}: {WebUtility.HtmlEncode(x.Groups[2].Value)}</a>"
                        )
                )
            );

        foreach (Match m in InternalWikiLinkRegex().Matches(lineAsString))
            matches.Add(
                new(
                    m,
                    (x, sb, st) =>
                    {
                        var display = x.Groups[1].Value;
                        var target = string.IsNullOrEmpty(x.Groups[2].Value) ? display : x.Groups[2].Value;
                        sb.Append(
                            $"<a href=\"https://www.explainxkcd.com/wiki/index.php/{Uri.EscapeDataString(target)}\" title=\"{WebUtility.HtmlEncode(target)}\">"
                        );
                        ConvertLine(display.AsSpan(), sb, st);
                        sb.Append("</a>");
                    }
                )
            );

        foreach (Match m in WikipediaLinkRegex().Matches(lineAsString))
            matches.Add(
                new(
                    m,
                    (x, sb, st) =>
                    {
                        var target = x.Groups[1].Value;
                        var display = string.IsNullOrEmpty(x.Groups[2].Value) ? target : x.Groups[2].Value;
                        sb.Append(
                            $"<a href=\"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(target)}\" title=\"wikipedia:{WebUtility.HtmlEncode(target)}\">"
                        );
                        ConvertLine(display.AsSpan(), sb, st);
                        sb.Append("</a>");
                    }
                )
            );

        foreach (Match m in ExternalLinkWithTextRegex().Matches(lineAsString))
            matches.Add(
                new(
                    m,
                    (x, sb, st) =>
                    {
                        sb.Append($"<a rel=\"nofollow\" href=\"{WebUtility.HtmlEncode(x.Groups[1].Value)}\">");
                        ConvertLine(x.Groups[2].Value.AsSpan(), sb, st);
                        sb.Append("</a>");
                    }
                )
            );

        foreach (Match m in ExternalLinkSimpleRegex().Matches(lineAsString))
            matches.Add(
                new(
                    m,
                    (x, sb, st) =>
                    {
                        int refIndex = st.RefNum++;
                        st.Refs.Add($"External link: {WebUtility.HtmlEncode(x.Groups[1].Value)}");
                        sb.Append($"<sup id='ref-{refIndex}'><a href='#note-{refIndex}'>[{refIndex + 1}]</a></sup>");
                    }
                )
            );

        foreach (Match m in ReferenceRegex().Matches(lineAsString))
            matches.Add(
                new(
                    m,
                    (x, sb, st) =>
                    {
                        var refHtml = new StringBuilder();
                        ConvertLine(x.Groups[1].Value.AsSpan(), refHtml, st);
                        st.Refs.Add(refHtml.ToString());
                        int idx = st.RefNum++;
                        sb.Append($"<sup id='ref-{idx}'><a href='#note-{idx}'>[{idx + 1}]</a></sup>");
                    }
                )
            );

        foreach (Match m in BoldRegex().Matches(lineAsString))
            matches.Add(new(m, Nested("<b>", "</b>", 1)));

        foreach (Match m in ItalicsRegex().Matches(lineAsString))
            matches.Add(new(m, Nested("<i>", "</i>", 1)));

        matches.Sort((a, b) => a.Match.Index.CompareTo(b.Match.Index));

        int lastIndex = 0;
        foreach (var info in matches)
        {
            if (info.Match.Index < lastIndex)
                continue;
            var plainSpan = line[lastIndex..info.Match.Index];
            html.Append(WebUtility.HtmlEncode(plainSpan.ToString()));
            info.Appender(info.Match, html, state);
            lastIndex = info.Match.Index + info.Match.Length;
        }

        if (lastIndex < line.Length)
            html.Append(WebUtility.HtmlEncode(line[lastIndex..].ToString()));
    }
}
