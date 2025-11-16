using System.Globalization;
using AvaloniaXKCD.Exports;
using AvaloniaXKCD.Tests.Exports;
using AvaloniaXKCD.Tests.VerifyPlugins;

namespace AvaloniaXKCD.Tests;

/// <summary>
/// Tests for LocalizationService to ensure localization works correctly and doesn't regress
/// </summary>
public class LocalizationServiceTests
{
    [Test]
    public async Task ShouldInitializeWithDefaultCulture()
    {
        // Arrange & Act
        var service = new TestLocalizationService();

        // Assert
        await VerifyAssertionsPlugin.Verify(new { Culture = service.CurrentCulture.TwoLetterISOLanguageName })
            .Assert(result =>
            {
                result.Culture.ShouldNotBeNull();
                // Should be either English or the system's culture if it's supported
                var supportedCultures = new[] { "en", "es" };
                supportedCultures.ShouldContain(result.Culture);
            });
    }

    [Test]
    public async Task ShouldSetCultureByLanguageCode()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act
        service.SetCulture("es");

        // Assert
        await Verifier.Verify(service.CurrentCulture.TwoLetterISOLanguageName)
            .Assert<string>(culture => culture.ShouldBe("es"));
    }

    [Test]
    public async Task ShouldSetCultureByCultureInfo()
    {
        // Arrange
        var service = new TestLocalizationService();
        var spanishCulture = CultureInfo.GetCultureInfo("es");

        // Act
        service.SetCulture(spanishCulture);

        // Assert
        await Verifier.Verify(service.CurrentCulture.TwoLetterISOLanguageName)
            .Assert<string>(culture => culture.ShouldBe("es"));
    }

    [Test]
    public async Task ShouldFallbackToEnglishForUnsupportedCulture()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act - Try to set an unsupported culture
        service.SetCulture("zh"); // Chinese is not supported

        // Assert - Should fallback to English
        await Verifier.Verify(service.CurrentCulture.TwoLetterISOLanguageName)
            .Assert<string>(culture => culture.ShouldBe("en"));
    }

    [Test]
    public async Task ShouldRaiseCultureChangedEvent()
    {
        // Arrange
        var service = new TestLocalizationService();
        CultureInfo? capturedCulture = null;
        service.CultureChanged += (sender, culture) => capturedCulture = culture;

        // Act
        service.SetCulture("es");

        // Assert
        await VerifyAssertionsPlugin.Verify(new { CapturedCulture = capturedCulture?.TwoLetterISOLanguageName })
            .Assert(result =>
            {
                result.CapturedCulture.ShouldNotBeNull();
                result.CapturedCulture.ShouldBe("es");
            });
    }

    [Test]
    public async Task ShouldNotRaiseCultureChangedEventWhenSettingSameCulture()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");
        var eventRaised = false;
        service.CultureChanged += (sender, culture) => eventRaised = true;

        // Act
        service.SetCulture("en");

        // Assert
        await VerifyAssertionsPlugin.Verify(new { EventRaised = eventRaised })
            .Assert(result => result.EventRaised.ShouldBeFalse());
    }

    [Test]
    public async Task ShouldGetEnglishStringForNavigationButton()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var translations = new
        {
            RandomButton = service.GetString("NavButton_Random"),
            ExplainButton = service.GetString("NavButton_Explain"),
            FirstButton = service.GetString("NavButton_First"),
            LastButton = service.GetString("NavButton_Last")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(translations)
            .Assert(t =>
            {
                t.RandomButton.ShouldBe("Random");
                t.ExplainButton.ShouldBe("Explain");
                t.FirstButton.ShouldBe("|<");
                t.LastButton.ShouldBe(">|");
            });
    }

    [Test]
    public async Task ShouldGetSpanishStringForNavigationButton()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var translations = new
        {
            RandomButton = service.GetString("NavButton_Random"),
            ExplainButton = service.GetString("NavButton_Explain"),
            PreviousButton = service.GetString("NavButton_Previous"),
            NextButton = service.GetString("NavButton_Next")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(translations)
            .Assert(t =>
            {
                t.RandomButton.ShouldBe("Aleatorio");
                t.ExplainButton.ShouldBe("Explicar");
                t.PreviousButton.ShouldBe("< Anterior");
                t.NextButton.ShouldBe("Siguiente >");
            });
    }

    [Test]
    public async Task ShouldGetEnglishStringForDialogLabels()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var translations = new
        {
            Error = service.GetString("Dialog_Error"),
            Quit = service.GetString("Dialog_Quit"),
            Continue = service.GetString("Dialog_Continue"),
            Cancel = service.GetString("Dialog_Cancel")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(translations)
            .Assert(t =>
            {
                t.Error.ShouldBe("Error");
                t.Quit.ShouldBe("Quit");
                t.Continue.ShouldBe("Continue");
                t.Cancel.ShouldBe("Cancel");
            });
    }

    [Test]
    public async Task ShouldGetSpanishStringForDialogLabels()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var translations = new
        {
            Error = service.GetString("Dialog_Error"),
            Quit = service.GetString("Dialog_Quit"),
            Continue = service.GetString("Dialog_Continue"),
            Cancel = service.GetString("Dialog_Cancel")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(translations)
            .Assert(t =>
            {
                t.Error.ShouldBe("Error");
                t.Quit.ShouldBe("Salir");
                t.Continue.ShouldBe("Continuar");
                t.Cancel.ShouldBe("Cancelar");
            });
    }

    [Test]
    public async Task ShouldGetEnglishStringForTooltips()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var translations = new
        {
            ErrorTitle = service.GetString("Dialog_ErrorTitle"),
            ErrorMessage = service.GetString("Dialog_ErrorMessage")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(translations)
            .Assert(t =>
            {
                t.ErrorTitle.ShouldBe("Title");
                t.ErrorMessage.ShouldBe("Error");
            });
    }

    [Test]
    public async Task ShouldGetSpanishStringForTooltips()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var translations = new
        {
            ErrorTitle = service.GetString("Dialog_ErrorTitle"),
            ErrorMessage = service.GetString("Dialog_ErrorMessage")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(translations)
            .Assert(t =>
            {
                t.ErrorTitle.ShouldBe("Título");
                t.ErrorMessage.ShouldBe("Error");
            });
    }

    [Test]
    public async Task ShouldGetEnglishStringForContextMenu()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var copyUrl = service.GetString("ContextMenu_CopyURL");

        // Assert
        await Verifier.Verify(copyUrl)
            .Assert<string>(url => url.ShouldBe("Copy URL"));
    }

    [Test]
    public async Task ShouldGetSpanishStringForContextMenu()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var copyUrl = service.GetString("ContextMenu_CopyURL");

        // Assert
        await Verifier.Verify(copyUrl)
            .Assert<string>(url => url.ShouldBe("Copiar URL"));
    }

    [Test]
    public async Task ShouldGetWindowTitle()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var titles = new
        {
            Title = service.GetString("Window_Title"),
            TitleFormat = service.GetString("Window_TitleFormat")
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(titles)
            .Assert(t =>
            {
                t.Title.ShouldBe("Axkcd");
                t.TitleFormat.ShouldBe("AXKCD: {0}");
            });
    }

    [Test]
    public async Task ShouldFormatStringWithArguments()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var formattedTitle = service.GetString("Window_TitleFormat", "Test Comic");

        // Assert
        await Verifier.Verify(formattedTitle)
            .Assert<string>(title => title.ShouldBe("AXKCD: Test Comic"));
    }

    [Test]
    public async Task ShouldReturnKeyForMissingResource()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act
        var missing = service.GetString("NonExistent_Key");

        // Assert
        await Verifier.Verify(missing)
            .Assert<string>(key => key.ShouldBe("NonExistent_Key"));
    }

    [Test]
    public async Task ShouldHandleMultipleConsecutiveCultureChanges()
    {
        // Arrange
        var service = new TestLocalizationService();
        var cultures = new List<string>();
        service.CultureChanged += (sender, culture) => cultures.Add(culture.TwoLetterISOLanguageName);

        // Act
        service.SetCulture("en");
        service.SetCulture("es");
        service.SetCulture("en");
        service.SetCulture("es");

        // Assert
        var result = new
        {
            CultureChangeCount = cultures.Count,
            FinalTranslation = service.GetString("NavButton_Random")
        };

        await VerifyAssertionsPlugin.Verify(result)
            .Assert(r =>
            {
                r.CultureChangeCount.ShouldBeGreaterThanOrEqualTo(2);
                r.FinalTranslation.ShouldBe("Aleatorio"); // Should be Spanish at the end
            });
    }

    [Test]
    public async Task AllNavigationButtonsShouldBeLocalized()
    {
        // Arrange
        var service = new TestLocalizationService();
        var buttonKeys = new[]
        {
            "NavButton_First",
            "NavButton_Previous",
            "NavButton_Random",
            "NavButton_Explain",
            "NavButton_GoTo",
            "NavButton_Next",
            "NavButton_Last"
        };

        // Act - Get English translations
        service.SetCulture("en");
        var englishTranslations = buttonKeys.ToDictionary(
            key => key,
            key => service.GetString(key)
        );

        // Get Spanish translations
        service.SetCulture("es");
        var spanishTranslations = buttonKeys.ToDictionary(
            key => key,
            key => service.GetString(key)
        );

        var result = new
        {
            English = englishTranslations,
            Spanish = spanishTranslations
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(r =>
            {
                foreach (var key in buttonKeys)
                {
                    r.English[key].ShouldNotBe(key, $"Key {key} should have an English translation");
                    r.English[key].ShouldNotBeEmpty($"Key {key} should not be empty in English");
                    r.Spanish[key].ShouldNotBe(key, $"Key {key} should have a Spanish translation");
                    r.Spanish[key].ShouldNotBeEmpty($"Key {key} should not be empty in Spanish");
                }
            });
    }

    [Test]
    public async Task AllDialogLabelsShouldBeLocalized()
    {
        // Arrange
        var service = new TestLocalizationService();
        var dialogKeys = new[]
        {
            "Dialog_Error",
            "Dialog_ErrorTitle",
            "Dialog_ErrorMessage",
            "Dialog_Quit",
            "Dialog_Continue",
            "Dialog_Cancel"
        };

        // Act - Get English translations
        service.SetCulture("en");
        var englishTranslations = dialogKeys.ToDictionary(
            key => key,
            key => service.GetString(key)
        );

        // Get Spanish translations
        service.SetCulture("es");
        var spanishTranslations = dialogKeys.ToDictionary(
            key => key,
            key => service.GetString(key)
        );

        var result = new
        {
            English = englishTranslations,
            Spanish = spanishTranslations
        };

        // Assert
        await VerifyAssertionsPlugin.Verify(result)
            .Assert(r =>
            {
                foreach (var key in dialogKeys)
                {
                    r.English[key].ShouldNotBe(key, $"Key {key} should have an English translation");
                    r.English[key].ShouldNotBeEmpty($"Key {key} should not be empty in English");
                    r.Spanish[key].ShouldNotBe(key, $"Key {key} should have a Spanish translation");
                    r.Spanish[key].ShouldNotBeEmpty($"Key {key} should not be empty in Spanish");
                }
            });
    }

    [Test]
    public async Task ShouldUpdateCurrentUICultureWhenCultureChanges()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act
        service.SetCulture("es");

        // Assert
        var result = new
        {
            CurrentCulture = CultureInfo.CurrentCulture.TwoLetterISOLanguageName,
            CurrentUICulture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        };

        await VerifyAssertionsPlugin.Verify(result)
            .Assert(r =>
            {
                r.CurrentCulture.ShouldBe("es");
                r.CurrentUICulture.ShouldBe("es");
            });
    }

    [Test]
    public async Task ShouldSupportMultipleInstancesViaGetAll()
    {
        // Arrange & Act
        var services = ExportContainer.GetAll<ILocalizationService>();

        // Assert
        await VerifyAssertionsPlugin.Verify(new { ServiceCount = services.Length })
            .Assert(result =>
            {
                result.ServiceCount.ShouldBeGreaterThanOrEqualTo(1);
            });
    }
}
