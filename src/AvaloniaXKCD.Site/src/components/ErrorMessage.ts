import { LitElement, html, css } from 'lit';
import { customElement, property } from 'lit/decorators.js';

@customElement('error-message')
export class ErrorMessage extends LitElement {
  static styles = css`
    :host {
      display: contents;
    }

    .error-container {
      font-family: sans-serif;
      margin: auto;
      text-align: center;
      padding: 40px;
      border: 1px solid var(--border-color, #d9534f);
      border-radius: 7px;
      background-color: var(--error-background, #f2dede);
      color: var(--error-color, #a94442);
    }

    .error-container h2 {
      margin-top: 0;
    }

    .reload-button {
      background-color: var(--button-background, #d9534f);
      color: white;
      border: none;
      padding: 10px 20px;
      border-radius: 5px;
      cursor: pointer;
      font-size: 1rem;
      margin-top: 20px;
    }

    .reload-button:hover {
      background-color: var(--button-hover-background, #c9302c);
    }
  `;

  @property({ type: String })
  message = 'An unexpected error has occurred.';

  render() {
    return html`
      <div class="error-container">
        <h2><span aria-hidden="true">( ༎ຶ ۝ ༎ຶ ) - </span>Error</h2>
        <hr />
        <p>${this.message}</p>
        <button class="reload-button" @click="${this._reloadPage}">Reload</button>
      </div>
    `;
  }

  private _reloadPage(): void {
    window.location.reload();
  }
}
