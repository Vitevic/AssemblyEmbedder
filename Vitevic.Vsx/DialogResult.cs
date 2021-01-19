namespace Vitevic.Vsx
{
    /// <summary>
    /// Dialog result values. These match Windows buttons IDs (IDOK, IDCANCEL, etc.) In addition we define the result for failure to display the dialog. WPF dialogs return OK/Cancel for the boolean value result of ShowDialog() call, and Fail if the dialog does not have return value yet.
    /// Got from Microsoft.Internal.VisualStudio.PlatformUI.DialogResult
    /// </summary>
    public enum DialogResult
    {
        Fail = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
        Close = 8,
        Help = 9,
        TryAgain = 10,
        Continue = 11,
    }
}
