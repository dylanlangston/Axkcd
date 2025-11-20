import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { localized, msg, updateWhenLocaleChanges } from '@lit/localize';

@localized()
@customElement('loading-indicator')
export class LoadingIndicator extends LitElement {
  private getLoadingWords = (): string[] => [
    msg('Reticulating splines', { id: 'LoadingMsg_1' }),
    msg('Generating witty dialog', { id: 'LoadingMsg_2' }),
    msg('Swapping time and space', { id: 'LoadingMsg_3' }),
    msg('Spinning violently around the y-axis', { id: 'LoadingMsg_4' }),
    msg('Tokenizing real life', { id: 'LoadingMsg_5' }),
    msg('Bending the spoon', { id: 'LoadingMsg_6' }),
    msg('Filtering moral fiber', { id: 'LoadingMsg_7' }),
    msg("Don't think of a blue elephant", { id: 'LoadingMsg_8' }),
    msg('Buffering Please wait.', { id: 'LoadingMsg_9' }),
    msg('Calculating the meaning of life', { id: 'LoadingMsg_10' }),
    msg('Compressing humor', { id: 'LoadingMsg_11' }),
    msg('Debugging the universe', { id: 'LoadingMsg_12' }),
    msg('Synthesizing sarcasm', { id: 'LoadingMsg_13' }),
    msg('Calibrating flux capacitors', { id: 'LoadingMsg_14' }),
    msg('Loading the loading message', { id: 'LoadingMsg_15' }),
    msg('Cogitating', { id: 'LoadingMsg_16' }),
    msg('Deliberating', { id: 'LoadingMsg_17' }),
    msg('Pondering', { id: 'LoadingMsg_18' }),
    msg('Contemplating', { id: 'LoadingMsg_19' }),
    msg('Assembling bits', { id: 'LoadingMsg_20' }),
    msg('Brewing coffee', { id: 'LoadingMsg_21' }),
    msg('Fetching pixels', { id: 'LoadingMsg_22' }),
    msg('Warming up the hamsters', { id: 'LoadingMsg_23' }),
    msg('Charging the lasers', { id: 'LoadingMsg_24' }),
    msg('Aligning the planets', { id: 'LoadingMsg_25' }),
    msg('Counting to infinity', { id: 'LoadingMsg_26' }),
    msg('Twiddling thumbs', { id: 'LoadingMsg_27' }),
    msg('Herding cats', { id: 'LoadingMsg_28' }),
    msg('Shoveling coal into the server', { id: 'LoadingMsg_29' }),
    msg('Teaching the AI to love', { id: 'LoadingMsg_30' }),
  ];

  static styles = [
    css`
      :host {
        display: contents;
      }

      .splash {
        margin: auto;
        justify-content: center;
        align-items: center;
        display: flex;
        pointer-events: none;
      }

      .splash h2 {
        color: var(--text-color);
        font-weight: 400;
        font-size: 2.5rem;
        background: white;
        padding: 20px 40px;
        border: 1px solid black;
        border-radius: 7px;
      }

      .center {
        text-align: center;
      }

      .progress {
        width: 100%;
        max-width: 400px;
        margin: 0 auto;
      }

      .progress-bar {
        width: 100%;
        height: 12px;
        border: 1px solid black;
        border-radius: 3px;
        background: white;
        overflow: hidden;
        position: relative;
      }

      progress::-webkit-progress-bar {
        background: white;
        border-radius: 3px;
      }

      progress::-webkit-progress-value {
        background: var(--nav-background-color);
        border-radius: 2px;
      }

      progress::-moz-progress-bar {
        background: var(--nav-background-color);
        border-radius: 2px;
      }

      .progress-bar[indeterminate] {
        background-color: #e0e0e0;
      }

      .progress-bar[indeterminate]::before {
        content: '';
        position: absolute;
        top: 0;
        height: 100%;
        width: 30%;
        background-color: var(--nav-background-color, #333);
        animation: indeterminate-slide 2s infinite ease-out;
      }

      @keyframes indeterminate-slide {
        0% {
          left: -30%;
        }
        50% {
          left: 100%;
        }
        100% {
          left: 100%;
        }
      }
    `,
  ];

  constructor() {
    super();
    updateWhenLocaleChanges(this);
  }

  @property({ type: Number })
  progress = 0;

  @property({ type: Boolean })
  indeterminate = false;

  private readonly loadingIndex = Math.floor(Math.random() * 30);

  @state()
  private currentPercentage = 0;

  private targetPercentage = 0;
  private readonly animationDuration = 400;
  private animationStartTime: number | null = null;
  private animationStartPercentage = 0;
  private animationFrameHandle: number | null = null;

  protected updated(changedProperties: Map<string | number | symbol, unknown>): void {
    if (
      !this.indeterminate &&
      changedProperties.has('progress') &&
      this.progress > this.targetPercentage
    ) {
      if (this.animationFrameHandle === null) {
        this.animationStartPercentage = this.currentPercentage;
        this.targetPercentage = this.progress;
        this.animationFrameHandle = requestAnimationFrame(this._animate.bind(this));
      } else {
        this.targetPercentage = this.progress;
      }
    }
  }

  render() {
    return html`
      <div class="splash">
        <div class="center">
          <div class="progress">
            ${this.indeterminate
              ? html`<div class="progress-bar" role="progressbar" indeterminate></div>`
              : html`<progress
                  id="progressBar"
                  class="progress-bar"
                  role="progressbar"
                  .value="${this.currentPercentage}"
                  max="100"
                ></progress>`}
          </div>
          <h2>
            <span id="loadingText">${this.getLoadingWords()[this.loadingIndex]}</span>
            <span id="progressPercentage">${Math.round(this.currentPercentage)}%</span>
          </h2>
        </div>
      </div>
    `;
  }

  private _easeInOut(t: number): number {
    return t < 0.5 ? 2 * t * t : -1 + (4 - 2 * t) * t;
  }

  private _animate(timestamp: number): void {
    if (this.indeterminate) return;

    if (this.animationStartTime === null) {
      this.animationStartTime = timestamp;
    }

    const elapsed = timestamp - this.animationStartTime;

    const rawProgress = Math.min(elapsed / this.animationDuration, 1);

    const easedProgress = this._easeInOut(rawProgress);

    const range = this.targetPercentage - this.animationStartPercentage;
    this.currentPercentage = this.animationStartPercentage + range * easedProgress;

    if (rawProgress < 1) {
      this.animationFrameHandle = requestAnimationFrame(this._animate.bind(this));
    } else {
      this.currentPercentage = this.targetPercentage;
      this.animationStartTime = null;
      this.animationFrameHandle = null;

      if (this.progress > this.targetPercentage) {
        this.updated(new Map([['progress', this.progress]]));
      }
    }
  }
}
