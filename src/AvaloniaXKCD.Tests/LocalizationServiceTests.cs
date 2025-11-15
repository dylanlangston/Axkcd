using System.Globalization;
using AvaloniaXKCD.Exports;
using AvaloniaXKCD.Tests.Exports;

namespace AvaloniaXKCD.Tests;

/// <summary>
/// Tests for LocalizationService to ensure localization works correctly and doesn't regress
/// </summary>
public class LocalizationServiceTests
{
    [Test]
    public void ShouldInitializeWithDefaultCulture()
    {
        // Arrange & Act
        var service = new TestLocalizationService();

        // Assert
        service.CurrentCulture.ShouldNotBeNull();
        // Should be either English or the system's culture if it's supported
        var supportedCultures = new[] { "en", "es" };
        var isSupported = supportedCultures.Contains(service.CurrentCulture.TwoLetterISOLanguageName);
        isSupported.ShouldBeTrue($"Culture should be one of the supported cultures, but was {service.CurrentCulture.Name}");
    }

    [Test]
    public void ShouldSetCultureByLanguageCode()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act
        service.SetCulture("es");

        // Assert
        service.CurrentCulture.TwoLetterISOLanguageName.ShouldBe("es");
    }

    [Test]
    public void ShouldSetCultureByCultureInfo()
    {
        // Arrange
        var service = new TestLocalizationService();
        var spanishCulture = CultureInfo.GetCultureInfo("es");

        // Act
        service.SetCulture(spanishCulture);

        // Assert
        service.CurrentCulture.TwoLetterISOLanguageName.ShouldBe("es");
    }

    [Test]
    public void ShouldFallbackToEnglishForUnsupportedCulture()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act - Try to set an unsupported culture
        service.SetCulture("zh"); // Chinese is not supported

        // Assert - Should fallback to English
        service.CurrentCulture.TwoLetterISOLanguageName.ShouldBe("en");
    }

    [Test]
    public void ShouldRaiseCultureChangedEvent()
    {
        // Arrange
        var service = new TestLocalizationService();
        CultureInfo? capturedCulture = null;
        service.CultureChanged += (sender, culture) => capturedCulture = culture;

        // Act
        service.SetCulture("es");

        // Assert
        capturedCulture.ShouldNotBeNull();
        capturedCulture.TwoLetterISOLanguageName.ShouldBe("es");
    }

    [Test]
    public void ShouldNotRaiseCultureChangedEventWhenSettingSameCulture()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");
        var eventRaised = false;
        service.CultureChanged += (sender, culture) => eventRaised = true;

        // Act
        service.SetCulture("en");

        // Assert
        eventRaised.ShouldBeFalse();
    }

    [Test]
    public void ShouldGetEnglishStringForNavigationButton()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var randomButton = service.GetString("NavButton_Random");
        var explainButton = service.GetString("NavButton_Explain");
        var firstButton = service.GetString("NavButton_First");
        var lastButton = service.GetString("NavButton_Last");

        // Assert
        randomButton.ShouldBe("Random");
        explainButton.ShouldBe("Explain");
        firstButton.ShouldBe("|<");
        lastButton.ShouldBe(">|");
    }

    [Test]
    public void ShouldGetSpanishStringForNavigationButton()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var randomButton = service.GetString("NavButton_Random");
        var explainButton = service.GetString("NavButton_Explain");
        var previousButton = service.GetString("NavButton_Previous");
        var nextButton = service.GetString("NavButton_Next");

        // Assert
        randomButton.ShouldBe("Aleatorio");
        explainButton.ShouldBe("Explicar");
        previousButton.ShouldBe("< Anterior");
        nextButton.ShouldBe("Siguiente >");
    }

    [Test]
    public void ShouldGetEnglishStringForDialogLabels()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var error = service.GetString("Dialog_Error");
        var quit = service.GetString("Dialog_Quit");
        var continueStr = service.GetString("Dialog_Continue");
        var cancel = service.GetString("Dialog_Cancel");

        // Assert
        error.ShouldBe("Error");
        quit.ShouldBe("Quit");
        continueStr.ShouldBe("Continue");
        cancel.ShouldBe("Cancel");
    }

    [Test]
    public void ShouldGetSpanishStringForDialogLabels()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var error = service.GetString("Dialog_Error");
        var quit = service.GetString("Dialog_Quit");
        var continueStr = service.GetString("Dialog_Continue");
        var cancel = service.GetString("Dialog_Cancel");

        // Assert
        error.ShouldBe("Error");
        quit.ShouldBe("Salir");
        continueStr.ShouldBe("Continuar");
        cancel.ShouldBe("Cancelar");
    }

    [Test]
    public void ShouldGetEnglishStringForTooltips()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var errorTitle = service.GetString("Dialog_ErrorTitle");
        var errorMessage = service.GetString("Dialog_ErrorMessage");

        // Assert
        errorTitle.ShouldBe("Title");
        errorMessage.ShouldBe("Error");
    }

    [Test]
    public void ShouldGetSpanishStringForTooltips()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var errorTitle = service.GetString("Dialog_ErrorTitle");
        var errorMessage = service.GetString("Dialog_ErrorMessage");

        // Assert
        errorTitle.ShouldBe("Título");
        errorMessage.ShouldBe("Error");
    }

    [Test]
    public void ShouldGetEnglishStringForContextMenu()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var copyUrl = service.GetString("ContextMenu_CopyURL");

        // Assert
        copyUrl.ShouldBe("Copy URL");
    }

    [Test]
    public void ShouldGetSpanishStringForContextMenu()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("es");

        // Act
        var copyUrl = service.GetString("ContextMenu_CopyURL");

        // Assert
        copyUrl.ShouldBe("Copiar URL");
    }

    [Test]
    public void ShouldGetWindowTitle()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var title = service.GetString("Window_Title");
        var titleFormat = service.GetString("Window_TitleFormat");

        // Assert
        title.ShouldBe("Axkcd");
        titleFormat.ShouldBe("AXKCD: {0}");
    }

    [Test]
    public void ShouldFormatStringWithArguments()
    {
        // Arrange
        var service = new TestLocalizationService();
        service.SetCulture("en");

        // Act
        var formattedTitle = service.GetString("Window_TitleFormat", "Test Comic");

        // Assert
        formattedTitle.ShouldBe("AXKCD: Test Comic");
    }

    [Test]
    public void ShouldReturnKeyForMissingResource()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act
        var missing = service.GetString("NonExistent_Key");

        // Assert
        missing.ShouldBe("NonExistent_Key");
    }

    [Test]
    public void ShouldHandleMultipleConsecutiveCultureChanges()
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
        cultures.Count.ShouldBeGreaterThanOrEqualTo(2); // At least some changes occurred
        var randomInEnglish = service.GetString("NavButton_Random");
        randomInEnglish.ShouldBe("Aleatorio"); // Should be Spanish at the end
    }

    [Test]
    public void AllNavigationButtonsShouldBeLocalized()
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

        // Act & Assert for English
        service.SetCulture("en");
        foreach (var key in buttonKeys)
        {
            var value = service.GetString(key);
            value.ShouldNotBe(key, $"Key {key} should have an English translation");
            value.ShouldNotBeEmpty($"Key {key} should not be empty in English");
        }

        // Act & Assert for Spanish
        service.SetCulture("es");
        foreach (var key in buttonKeys)
        {
            var value = service.GetString(key);
            value.ShouldNotBe(key, $"Key {key} should have a Spanish translation");
            value.ShouldNotBeEmpty($"Key {key} should not be empty in Spanish");
        }
    }

    [Test]
    public void AllDialogLabelsShouldBeLocalized()
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

        // Act & Assert for English
        service.SetCulture("en");
        foreach (var key in dialogKeys)
        {
            var value = service.GetString(key);
            value.ShouldNotBe(key, $"Key {key} should have an English translation");
            value.ShouldNotBeEmpty($"Key {key} should not be empty in English");
        }

        // Act & Assert for Spanish
        service.SetCulture("es");
        foreach (var key in dialogKeys)
        {
            var value = service.GetString(key);
            value.ShouldNotBe(key, $"Key {key} should have a Spanish translation");
            value.ShouldNotBeEmpty($"Key {key} should not be empty in Spanish");
        }
    }

    [Test]
    public void ShouldUpdateCurrentUICultureWhenCultureChanges()
    {
        // Arrange
        var service = new TestLocalizationService();

        // Act
        service.SetCulture("es");

        // Assert
        CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ShouldBe("es");
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ShouldBe("es");
    }

    [Test]
    public void ShouldSupportMultipleInstancesViaGetAll()
    {
        // Arrange & Act
        var services = ExportContainer.GetAll<ILocalizationService>();

        // Assert
        // We expect to get the TestLocalizationService from the tests
        services.ShouldNotBeNull();
        services.Length.ShouldBeGreaterThanOrEqualTo(1);
    }
}
