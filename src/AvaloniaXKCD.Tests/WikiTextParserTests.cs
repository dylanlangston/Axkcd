using XKCDCore;
using System.Threading.Tasks;
using Shouldly;
using AvaloniaXKCD.Tests.VerifyPlugins;
using VerifyTests;
using NUnit.Framework;

namespace AvaloniaXKCD.Tests;

public class WikiTextParserTests
{
    [Test]
    public async Task Parse_ShouldHandleHeadings()
    {
        // Arrange
        var wikitext = """
            =Level 1=
            ==Level 2==
            ===Level 3===
            ====Level 4====
            =====Level 5=====
            ======Level 6======
            =======Level 7======= 
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("<h1>Level 1</h1>");
                html.ShouldContain("<h2>Level 2</h2>");
                html.ShouldContain("<h3>Level 3</h3>");
                html.ShouldContain("<h4>Level 4</h4>");
                html.ShouldContain("<h5>Level 5</h5>");
                html.ShouldContain("<h6>Level 6</h6>");
                // Level 7 heading should be capped at h6
                html.ShouldContain("<h6>Level 7</h6>");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleBulletPoints()
    {
        // Arrange
        var wikitext = """
            * Level 1
            ** Level 2
            *** Level 3
            * Back to Level 1
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("<ul><li>Level 1");
                html.ShouldContain("<ul><li>Level 2");
                html.ShouldContain("<ul><li>Level 3</li></ul>");
                html.ShouldContain("<li>Back to Level 1</li>");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleLinks()
    {
        // Arrange
        var wikitext = """
            Here are various links:
            * [[Internal Link]]
            * [[Display Text|Internal Link]]
            * [[123: Comic Title]]
            * {{w|Wikipedia Article}}
            * {{W|Wikipedia|Display Text}}
            * [https://example.com External Link]
            * [https://example.com]
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                // Internal wiki links
                html.ShouldContain("<a href=\"https://www.explainxkcd.com/wiki/index.php/Internal%20Link\" title=\"Internal Link\">Internal Link</a>");
                html.ShouldContain("<a href=\"https://www.explainxkcd.com/wiki/index.php/Internal%20Link\" title=\"Internal Link\">Display Text</a>");

                // XKCD comic link
                html.ShouldContain("<a href=\"https://xkcd.com/123\">123: Comic Title</a>");

                // Wikipedia links
                html.ShouldContain("<a href=\"https://en.wikipedia.org/wiki/Wikipedia%20Article\" title=\"wikipedia:Wikipedia Article\">Wikipedia Article</a>");
                html.ShouldContain("<a href=\"https://en.wikipedia.org/wiki/Wikipedia\" title=\"wikipedia:Wikipedia\">Display Text</a>");

                // External links
                html.ShouldContain("<a rel=\"nofollow\" href=\"https://example.com\">External Link</a>");
                // Reference-style external link
                html.ShouldContain("<sup id='ref-0'><a href='#note-0'>[1]</a></sup>");
                html.ShouldContain("<div class='references'><ol><li id='note-0'>");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleTextFormatting()
    {
        // Arrange
        var wikitext = """
            Here is some '''bold text''' and ''italic text''.
            You can also have '''bold and ''italic'' text'''.
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("<b>bold text</b>");
                html.ShouldContain("<i>italic text</i>");
                html.ShouldContain("<b>bold and <i>italic</i> text</b>");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleReferences()
    {
        // Arrange
        var wikitext = """
            Here is a statement<ref>This is the reference text</ref>.
            Another statement<ref>Second reference</ref>.
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                // Reference markers in text
                html.ShouldContain("<sup id='ref-0'><a href='#note-0'>[1]</a></sup>");
                html.ShouldContain("<sup id='ref-1'><a href='#note-1'>[2]</a></sup>");

                // References section
                html.ShouldContain("<div class='references'><ol>");
                html.ShouldContain("<li id='note-0'><a href='#ref-0'>↑</a><span>This is the reference text</span></li>");
                html.ShouldContain("<li id='note-1'><a href='#ref-1'>↑</a><span>Second reference</span></li>");
                html.ShouldContain("</ol></div>");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleTables()
    {
        // Arrange
        var wikitext = """
            {| class="wikitable"
            ! Header 1 !! Header 2
            |-
            | Cell 1 || Cell 2
            |-
            | Cell 3 || Cell 4
            |}
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("<table class=\"wikitable\">");
                html.ShouldContain("<tr><th>Header 1</th><th>Header 2</th></tr>");
                html.ShouldContain("<tr><td>Cell 1</td><td>Cell 2</td></tr>");
                html.ShouldContain("<tr><td>Cell 3</td><td>Cell 4</td></tr>");
                html.ShouldContain("</table>");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleQuotes()
    {
        // Arrange
        var wikitext = """
            Regular text.
            :This is a quote
            :This is another quote
            Back to regular text.
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("<dl><dd>This is a quote</dd>");
                html.ShouldContain("<dd>This is another quote</dd></dl>");
            });
    }

    [Test]
    public async Task Parse_ShouldEscapeHtmlCharacters()
    {
        // Arrange
        var wikitext = "Text with <angle brackets> & ampersand";

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("&lt;angle brackets&gt; &amp; ampersand");
            });
    }

    [Test]
    public async Task Parse_ShouldHandleEmptyInput()
    {
        // Arrange
        var wikitext = "";

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldBeEmpty();
            });
    }

    [Test]
    public async Task Parse_ShouldHandleComplexNestedFormatting()
    {
        // Arrange
        var wikitext = """
            * '''Bold [[Link|with link]]'''
            * ''Italic with {{w|Wikipedia|link}}''
            * '''Bold with <ref>reference</ref>'''
            """;

        // Act
        var result = WikiTextParser.Instance.Parse(wikitext, 1);

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(html =>
            {
                html.ShouldContain("<b>Bold <a href=\"https://www.explainxkcd.com/wiki/index.php/with%20link\" title=\"with link\">Link</a></b>");
                html.ShouldContain("<i>Italic with <a href=\"https://en.wikipedia.org/wiki/Wikipedia\" title=\"wikipedia:Wikipedia\">link</a></i>");
                html.ShouldContain("<b>Bold with <sup id='ref-0'><a href='#note-0'>[1]</a></sup></b>");
            });
    }
}