namespace Serilog.Exceptions.Core
{
    public interface IDestructuringOptions
    {
        /// <summary>
        /// Name of the key dictionary to which destructured exception
        /// will be assigned. Default value is "ExceptionDetail".
        /// </summary>
        string RootName { get; }
    }
}