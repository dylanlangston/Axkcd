namespace AvaloniaXKCD.Models;

public partial class XKCDComicModel : ObservableObject
{
    private readonly IXKCDComic _comic;

    public int Num => _comic.Num;
    public string Title => _comic.Title;
    public string Alt => _comic.Alt;
    public string Month => _comic.Month;
    public string Day => _comic.Day;
    public string Year => _comic.Year;
    public string Img2x => _comic.Img2x;

    public string ComicTitle => $"{Title}";
    public string ComicInfo => $"#{Num} - {Month}/{Day}/{Year}";

    public XKCDComicModel(IXKCDComic comic)
    {
        _comic = comic;
    }

    public Uri GetUri(Uri baseUri) => _comic.GetURI(baseUri);

    public bool TryOpen(Uri baseUri) => _comic.TryOpen(baseUri);

    public bool Explain() => _comic.Explain();
}
