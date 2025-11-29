using System.Diagnostics;
using System.Text.Json.Serialization;

namespace XKCDCore;

public partial interface IXKCDComic
{
    Uri BaseUri { get; set; }

    string Month { get; init; }
    int Num { get; init; }
    string Link { get; init; }
    string Year { get; init; }
    string News { get; init; }
    string SafeTitle { get; init; }
    string Transcript { get; init; }
    string Alt { get; init; }
    string Img { get; init; }
    string Title { get; init; }
    string Day { get; init; }

    public Uri GetURI(Uri? baseUri = null)
    {
        baseUri ??= BaseUri;
        return new Uri(
            $"{baseUri.Scheme}://{baseUri.Host}:{baseUri.Port}{(string.IsNullOrEmpty(baseUri.AbsolutePath) ? string.Empty : $"{baseUri.AbsolutePath}".TrimEnd('/'))}/{Num}/"
        );
    }

    public string Img2x => GetImg2x();

    private string GetImg2x() => GetImg2x(Num, Img);

    public static string GetImg2x(int num, string img)
    {
        if (string.IsNullOrEmpty(img))
            return img;
        if (num < 1084)
            return img;

        var extension = Path.GetExtension(img);
        return img[0..(img.Length - extension.Length)] + "_2x" + extension;
    }

    public bool TryOpen(Uri? baseUri = null)
    {
        var uri = GetURI(baseUri).AbsoluteUri;
        return TryOpenUri(uri);
    }

    public bool Explain()
    {
        var uri = $"https://www.explainxkcd.com/wiki/index.php/{Num}";
        return TryOpenUri(uri);
    }

    private static bool TryOpenUri(string uri)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
                return true;
            }
            else if (OperatingSystem.IsMacOS())
            {
                Process.Start("open", uri);
                return true;
            }
            else if (OperatingSystem.IsLinux())
            {
                var psi = new ProcessStartInfo("xdg-open", uri)
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                };
                using var proc = Process.Start(psi);
                if (proc?.WaitForExit(2000) == true && proc.ExitCode == 0)
                    return true;
            }
            else if (OperatingSystem.IsBrowser())
            {
                OpenInBrowser(uri);
                return true;
            }
            else
            {
                // Fallback for unknown platforms
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
                return true;
            }
        }
        catch (Exception er)
        {
            Trace.WriteLine(er);
        }
        return false;
    }

    [System.Runtime.InteropServices.JavaScript.JSImport("globalThis.open")]
    internal static partial void OpenInBrowser(string url);
}

public sealed record XKCDComic(
    string Month,
    int Num,
    string Link,
    string Year,
    string News,
    string SafeTitle,
    string Transcript,
    string Alt,
    string Img,
    string Title,
    string Day
) : IXKCDComic
{
    public Uri BaseUri { get; set; } = new("https://xkcd.com/");
}
