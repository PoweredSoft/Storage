namespace PoweredSoft.Storage.Core
{
    public interface IWriteFileOptions
    {
        bool OverrideIfExists { get; }
    }

    public class DefaultWriteOptions : IWriteFileOptions
    {
        public bool OverrideIfExists { get; set; }
    }
}
