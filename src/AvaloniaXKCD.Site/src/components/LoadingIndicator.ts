import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { msg, updateWhenLocaleChanges } from '@lit/localize';

const getLoadingWords = (): string[] => [
  msg("Reticulating splines"),
  msg("Generating witty dialog"),
  msg("Swapping time and space"),
  msg("Spinning violently around the y-axis"),
  msg("Tokenizing real life"),
  msg("Bending the spoon"),
  msg("Filtering moral fiber"),
  msg("Don't think of a blue elephant"),
  msg("Buffering Please wait."),
  msg("Calculating the meaning of life"),
  msg("Compressing humor"),
  msg("Debugging the universe"),
  msg("Synthesizing sarcasm"),
  msg("Calibrating flux capacitors"),
  msg("Loading the loading message"),
  msg("Cogitating"),
  msg("Deliberating"),
  msg("Pondering"),
  msg("Contemplating"),
  msg("Assembling bits"),
  msg("Brewing coffee"),
  msg("Fetching pixels"),
  msg("Warming up the hamsters"),
  msg("Charging the lasers"),
  msg("Aligning the planets"),
  msg("Counting to infinity"),
  msg("Twiddling thumbs"),
  msg("Herding cats"),
  msg("Shoveling coal into the server"),
  msg("Teaching the AI to love")
];

export const getFunnyLoadingMessage = (): string => {
  const loadingWords = getLoadingWords();
  const randomIndex = Math.floor(Math.random() * loadingWords.length);
  return loadingWords[randomIndex];
};

@customElement('loading-indicator')
export class LoadingIndicator extends LitElement {
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
  `];

  constructor() {
    super();
    updateWhenLocaleChanges(this);
  }

  @property({ type: Number })
  progress = 0;

  @property({ type: Boolean })
  indeterminate = false;

  @state()
  private loadingText = getFunnyLoadingMessage();

  @state()
  private currentPercentage = 0;

  private targetPercentage = 0;
  private readonly animationDuration = 400; 
  private animationStartTime: number | null = null;
  private animationStartPercentage = 0;
  private animationFrameHandle: number | null = null;


  protected updated(changedProperties: Map<string | number | symbol, unknown>): void {
    if (!this.indeterminate && changedProperties.has('progress') && this.progress > this.targetPercentage) {
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
                ></progress>`
      }
          </div>
          <h2>
            <span id="loadingText">${this.loadingText}</span>
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
    this.currentPercentage = this.animationStartPercentage + (range * easedProgress);

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