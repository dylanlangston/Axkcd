public class AppErrorEventArgs : EventArgs
{
    public Exception Exception;
    public bool Fatal;

    public AppErrorEventArgs(Exception exception, bool fatal)
        : base()
    {
        Exception = exception;
        Fatal = fatal;
    }

    public static implicit operator AppErrorEventArgs((Exception err, bool fatal) args) => new(args.err, args.fatal);

    public static implicit operator (Exception err, bool fatal)(AppErrorEventArgs args) => (args.Exception, args.Fatal);
}
