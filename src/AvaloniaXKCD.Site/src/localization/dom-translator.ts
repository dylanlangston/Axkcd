import { addLocaleChangeListener } from './locale';
import './localized-string';

/**
 * Translates static HTML content by finding elements with data-i18n attributes
 * and replacing their content with localized strings.
 */
export function translateStaticContent(): void {
  // Wait for the custom element to be defined
  customElements.whenDefined('localized-string').then(() => {
    // Get the LocalizedString constructor from the custom elements registry
    const LocalizedStringClass = customElements.get('localized-string') as any;
    
    if (!LocalizedStringClass?.getString) {
      console.warn('LocalizedString.getString method not available');
      return;
    }
    
    // Translate elements with data-i18n attribute for text content
    const elementsToTranslate = document.querySelectorAll('[data-i18n]');
    elementsToTranslate.forEach((element) => {
      const key = element.getAttribute('data-i18n');
      if (key) {
        const translatedText = LocalizedStringClass.getString(key);
        
        // Handle different content types
        const contentType = element.getAttribute('data-i18n-attr');
        if (contentType === 'title') {
          element.setAttribute('title', translatedText);
        } else if (contentType === 'aria-label') {
          element.setAttribute('aria-label', translatedText);
        } else {
          // Default to text content
          element.textContent = translatedText;
        }
      }
    });
    
    // Handle elements with data-i18n-title attribute for title attributes
    const elementsWithTitle = document.querySelectorAll('[data-i18n-title]');
    elementsWithTitle.forEach((element) => {
      const key = element.getAttribute('data-i18n-title');
      if (key) {
        const translatedText = LocalizedStringClass.getString(key);
        element.setAttribute('title', translatedText);
      }
    });
    
    // Also update the page title
    const titleElement = document.querySelector('title');
    if (titleElement) {
      titleElement.textContent = LocalizedStringClass.getString('Page_Title');
    }
  });
}

/**
 * Initialize DOM translation on page load and whenever locale changes
 */
export function initializeDOMTranslation(): void {
  requestAnimationFrame(translateStaticContent);
  addLocaleChangeListener(translateStaticContent);
}