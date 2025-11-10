import { LitElement, html, css } from 'lit';
import { customElement, state } from 'lit/decorators.js';
import { dotnet } from '/_framework/dotnet.js';
import './LoadingIndicator';
import './ErrorMessage';

import * as interop from '../interop';

import fullscreenIcon from '../assets/fullscreen.svg'
import fullscreenExitIcon from '../assets/fullscreen-exit.svg'
import type { AvaloniaXKCDBrowser } from '/_framework/AvaloniaXKCD.Browser';

@customElement('avalonia-app')
export class AvaloniaApp extends LitElement {
    static styles = css`
:host {
    display: flex;
    flex: 1;
    justify-content: center;
}

:host([fullscreen]) {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    z-index: 9999;
}

.container {
    position: relative;
    display: flex;
    flex: 1;
    transition: width 0.4s ease-in-out, height 0.4s ease-in-out, margin 0.4s ease-in-out, border-radius 0.4s ease-in-out;
}

.card {
    background: var(--content-background);
    border: 1px solid var(--border-color);
    border-radius: 7px;
    margin: 14px;
    width: 90vw;
    max-width: 800px;
}

slot {
    display: flex;
    flex: 1;
    position: relative;
    overflow: hidden;
    transition: margin 0.4s ease-in-out, border-radius 0.4s ease-in-out;
}

.container.loaded {
    animation: tv-turn-on 0.6s cubic-bezier(0.25, 1, 0.5, 1);
}

@keyframes tv-turn-on {
    0% {
        transform: scale(1, 0.03);
        opacity: 0;
    }
    40% {
        transform: scale(1, 0.03);
        opacity: 1;
    }
    100% {
        transform: scale(1, 1);
        opacity: 1;
    }
}

.loaded {
    animation: fadeIn 0.5s ease-in-out;
}

@keyframes fadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

:host([fullscreen]) slot {
    margin: 0;
    border: none;
    border-radius: 0;
}

.fullscreen-button {
    background: transparent;
    position: absolute;
    top: 18px;
    right: 18px;
    z-index: 1;
    color: white;
    border: none;
    cursor: pointer;
    font-size: 14px;
}

:host([fullscreen]) .fullscreen-button {
    top: 10px;
    right: 10px;
}
    `;

    @state()
    private isLoading = true;

    @state()
    private loadingProgress = 0;

    @state()
    private fatalError: string | null = null;

    @state()
    private isFullscreen = false;

    private avaloniaApp: AvaloniaXKCDBrowser | null = null;
    private uriChangeSubscription: string | null = null;

    constructor() {
        super();
        this.handleFullscreenChange = this.handleFullscreenChange.bind(this);
        this.handlePopState = this.handlePopState.bind(this);
    }

    connectedCallback() {
        super.connectedCallback();
        this.startAvalonia();
        document.addEventListener('fullscreenchange', this.handleFullscreenChange);
        window.addEventListener('popstate', this.handlePopState);
    }

    disconnectedCallback() {
        super.disconnectedCallback();
        document.removeEventListener('fullscreenchange', this.handleFullscreenChange);
        window.removeEventListener('popstate', this.handlePopState);

        if (this.uriChangeSubscription && this.avaloniaApp) {
            this.avaloniaApp.AvaloniaXKCD.Browser.BrowserSystemActions.RemoveOnUriChangeCallback(this.uriChangeSubscription);
        }
    }

    render() {
        if (this.fatalError) {
            return html`<error-message .message="${this.fatalError}"></error-message>`;
        }

        return html`
            ${this.isLoading
                ? html`<loading-indicator .indeterminate="${this.loadingProgress < 10}" .progress="${this.loadingProgress}"></loading-indicator>`
                : html`
<div class="${this.isFullscreen ? "container" : "container card"} loaded">
<slot class="loaded"></slot>
<button
                            title="${this.isFullscreen ? 'Exit Fullscreen' : 'Enter Fullscreen'}"
                            class="fullscreen-button"
                            @click="${this.toggleFullscreen}"
                            aria-label="${this.isFullscreen ? 'Exit Fullscreen' : 'Enter Fullscreen'}">
<img src="${this.isFullscreen ? fullscreenExitIcon : fullscreenIcon}" alt="${this.isFullscreen ? 'Exit Fullscreen' : 'Enter Fullscreen'}">
</button>
</div>
                `
            }
        `;
    }

    private comicRegex = /\/(\d+)$/;

    async startAvalonia() {
        try {
            const dotnetRuntime = await dotnet
                .withDiagnosticTracing(true)
                .withConfig({
                    environmentVariables: {
                        // Mono AOT-related logs
                        "MONO_LOG_LEVEL": "debug",
                        "MONO_LOG_MASK": "aot"
                    }
                })
                .withModuleConfig({
                    onAbort: (_: any) => {
                        console.error("Fatal error in .NET runtime.");
                        this.fatalError = "A fatal error has occurred in the .NET runtime. Please reload the page.";
                    },
                    onExit: (code: number) => {
                        console.log(`.NET runtime exited with code ${code}.`);
                    },
                    onDownloadResourceProgress: (resourcesLoaded: number, totalResources: number) => {
                        // This ensures that when we're at the load end of loading resouces we don't over estimate our progress
                        if (totalResources < 50) totalResources = 50;

                        const newPercentage = (resourcesLoaded / totalResources) * 90.0;
                        this.loadingProgress = newPercentage;
                    },
                    onDotnetReady: () => {
                        this.loadingProgress = 100;
                    }
                })
                .create();

            dotnetRuntime.setModuleImports('interop', interop);

            const config = dotnetRuntime.getConfig();

            const match = window.location.href.match(this.comicRegex);
            const comicNumber = match ? match[1] : null;
            await dotnetRuntime.runMain(config.mainAssemblyName, comicNumber ? [comicNumber] : []);

            requestIdleCallback(() => {
                this.isLoading = false;
            });

            this.avaloniaApp = await dotnetRuntime.getAssemblyExports("AvaloniaXKCD.Browser");
            this.uriChangeSubscription = this.avaloniaApp?.AvaloniaXKCD.Browser.BrowserSystemActions.AddOnUriChangeCallback((newUri: string) => {
                const match = window.location.href.match(this.comicRegex);
                const currentComicNumber = match ? match[1] : null;
                if (currentComicNumber == newUri) return;
                history.pushState({ uri: newUri }, '', `${window.location.origin}/${newUri}`);
            });

        } catch (error) {
            console.error("Failed to start Avalonia application:", error);
            this.fatalError = `${error instanceof Error ? error.message : String(error)}`;
        }
    }

    async toggleFullscreen() {
        if (!document.fullscreenElement) {
            await this.requestFullscreen();
        } else {
            await document.exitFullscreen();
        }
    }

    handleFullscreenChange() {
        this.isFullscreen = !!document.fullscreenElement;
        this.toggleAttribute('fullscreen', this.isFullscreen);
    }

    private handlePopState(event: PopStateEvent) {
        if (event.state && event.state.uri && this.avaloniaApp) {
            this.avaloniaApp.AvaloniaXKCD.Browser.BrowserSystemActions.InvokeOnUriChangeCallback(event.state.uri);
        }
    }
}