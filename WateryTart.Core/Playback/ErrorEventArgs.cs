using System;

namespace WateryTart.Core.Playback;

/// <summary>
/// Event args for errors.
/// </summary>
public class ErrorEventArgs : EventArgs
{
    public Exception Exception { get; }
    public string Message { get; }

    public ErrorEventArgs(Exception exception)
    {
        Exception = exception ?? throw new ArgumentNullException(nameof(exception));
        Message = exception.Message;
    }

    public ErrorEventArgs(string message, Exception exception = null)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
    }
}
