import { LitElement } from 'lit';
import { customElement, property } from 'lit/decorators.js';
import { msg, localized } from '@lit/localize';

@customElement('localized-string')
@localized()
class LocalizedString extends LitElement {
  @property({ type: String })
  key: string = '';

  @property({ type: Array })
  args: string[] = [];

  // Map of all localized strings with their IDs
  private static strings: Record<string, () => string> = {
    NavButton_First: () => msg('|<', { id: 'NavButton_First' }),
    NavButton_Previous: () => msg('< Prev', { id: 'NavButton_Previous' }),
    NavButton_Random: () => msg('Random', { id: 'NavButton_Random' }),
    NavButton_Explain: () => msg('Explain', { id: 'NavButton_Explain' }),
    NavButton_GoTo: () => msg('Go To', { id: 'NavButton_GoTo' }),
    NavButton_Next: () => msg('Next >', { id: 'NavButton_Next' }),
    NavButton_Last: () => msg('>|', { id: 'NavButton_Last' }),

    Dialog_Error: () => msg('Error', { id: 'Dialog_Error' }),
    Dialog_ErrorTitle: () => msg('Title', { id: 'Dialog_ErrorTitle' }),
    Dialog_ErrorMessage: () => msg('Error', { id: 'Dialog_ErrorMessage' }),
    Dialog_Quit: () => msg('Quit', { id: 'Dialog_Quit' }),
    Dialog_Continue: () => msg('Continue', { id: 'Dialog_Continue' }),
    Dialog_Cancel: () => msg('Cancel', { id: 'Dialog_Cancel' }),

    ContextMenu_CopyURL: () => msg('Copy URL', { id: 'ContextMenu_CopyURL' }),

    Window_Title: () => msg('Axkcd', { id: 'Window_Title' }),
    Window_TitleFormat: () => msg('AXKCD: {0}', { id: 'Window_TitleFormat' }),

    // Static HTML content
    Page_Title: () => msg('A(valonia)XKCD', { id: 'Page_Title' }),
    Page_Subtitle: () =>
      msg('Because laughing in C# is still (technically) laughing.', { id: 'Page_Subtitle' }),
    DevBanner_Text: () => msg('In Development', { id: 'DevBanner_Text' }),
    DevBanner_Title: () => msg('Click to view source', { id: 'DevBanner_Title' }),
    Footer_MadeWith: () =>
      msg(
        "Made with an amalgamation of technologies that probably shouldn't work together, but somehow does:",
        { id: 'Footer_MadeWith' }
      ),
    Footer_Avalonia_Title: () =>
      msg('Built with Avalonia UI Framework', { id: 'Footer_Avalonia_Title' }),
    Footer_Dotnet_Title: () => msg('Powered by .NET and C#', { id: 'Footer_Dotnet_Title' }),
    Footer_Lit_Title: () => msg('Enhanced with Lit Web Components', { id: 'Footer_Lit_Title' }),
    Footer_Vite_Title: () => msg('Bundled with Vite', { id: 'Footer_Vite_Title' }),
  };

  private localizedText?: string;

  // Static method to get localized strings for DOM manipulation
  static getString(key: string): string {
    return LocalizedString.strings[key]?.() || key;
  }

  render() {
    this.localizedText = LocalizedString.strings[this.key]?.() || this.key;
    return null;
  }

  getStringValue(): string {
    if (this.localizedText === undefined) {
      this.render();
    }
    return this.localizedText ?? this.key;
  }
}

function decodeHtmlEntities(text: string): string {
  const textarea = document.createElement('textarea');
  textarea.innerHTML = text;
  return textarea.value;
}

export function getLocalizedString(key: string, ...args: string[]): string {
  const localizedStrings = new LocalizedString();
  localizedStrings.key = key;
  localizedStrings.args = args;
  const result = localizedStrings.getStringValue();
  return decodeHtmlEntities(result);
}
