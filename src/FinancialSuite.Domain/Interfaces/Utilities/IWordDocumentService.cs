namespace FinancialSuite.Domain.Interfaces.Utilities
{
    public interface IWordDocumentService
    {
        // Replaces every placeholder in the template's text, and fills its first table by cloning the row after the header once per item in tableRows.
        byte[] FillTemplate(Stream template, IReadOnlyDictionary<string, string> placeholders, IEnumerable<IReadOnlyList<string>> tableRows);
    }
}
