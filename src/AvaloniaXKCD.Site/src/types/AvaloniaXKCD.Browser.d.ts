declare function AddOnUriChangeCallback(cb: (string) => void): string
declare function RemoveOnUriChangeCallback(subscription: string): void
declare function InvokeOnUriChangeCallback(uri: string): void

type BrowserSystemActions = {
    AddOnUriChangeCallback: AddOnUriChangeCallback,
    RemoveOnUriChangeCallback: RemoveOnUriChangeCallback,
    InvokeOnUriChangeCallback: InvokeOnUriChangeCallback
}

type Browser = {
    BrowserSystemActions: BrowserSystemActions
}

type AvaloniaXKCD = {
    Browser: Browser
};

export type AvaloniaXKCDBrowser = {
    AvaloniaXKCD: AvaloniaXKCD
};