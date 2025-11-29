type BrowserSystemActions = {
  AddOnUriChangeCallback: (cb: (string) => void) => string;
  RemoveOnUriChangeCallback: (subscription: string) => void;
  InvokeOnUriChangeCallback: (uri: string) => void;
};

type Browser = {
  BrowserSystemActions: BrowserSystemActions;
};

type AvaloniaXKCD = {
  Browser: Browser;
};

export type AvaloniaXKCDBrowser = {
  AvaloniaXKCD: AvaloniaXKCD;
};
