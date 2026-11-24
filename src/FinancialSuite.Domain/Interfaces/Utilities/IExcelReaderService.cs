namespace FinancialSuite.Domain.Interfaces.Utilities
{
    public interface IExcelReaderService
    {
        // Reads the first worksheet: the first row holds the headers, every following non-empty row becomes one item keyed by header.
        IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(Stream stream);
    }
}
