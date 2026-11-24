using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using FinancialSuite.Domain.Interfaces.Utilities;

namespace FinancialSuite.Infrastructure.Utilities.Word
{
    public class WordDocumentService : IWordDocumentService
    {
        public byte[] FillTemplate(Stream template, IReadOnlyDictionary<string, string> placeholders, IEnumerable<IReadOnlyList<string>> tableRows)
        {
            using var output = new MemoryStream();
            template.CopyTo(output);

            using (var document = WordprocessingDocument.Open(output, true))
            {
                var mainPart = document.MainDocumentPart ??
                               throw new InvalidDataException("The Word template has no document body.");
                var body = mainPart.Document.Body ??
                           throw new InvalidDataException("The Word template has no document body.");

                FillTable(body, tableRows);

                ReplacePlaceholders(body, placeholders);
                foreach (var header in mainPart.HeaderParts) if (header.Header != null) ReplacePlaceholders(header.Header, placeholders);
                foreach (var footer in mainPart.FooterParts) if (footer.Footer != null) ReplacePlaceholders(footer.Footer, placeholders);

                mainPart.Document.Save();
            }

            return output.ToArray();
        }

        private static void FillTable(Body body, IEnumerable<IReadOnlyList<string>> tableRows)
        {
            var table = body.Elements<Table>().FirstOrDefault() ??
                        throw new InvalidDataException("No table found in the Word template.");

            var rows = table.Elements<TableRow>().ToList();
            if (rows.Count < 2) throw new InvalidDataException("The template table must contain a header row and a template row.");

            // Row 0 is the header; row 1 is the formatted row cloned for every item.
            var templateRow = rows[1];
            var lastRow = templateRow;

            foreach (var values in tableRows)
            {
                var newRow = (TableRow)templateRow.CloneNode(true);
                var cells = newRow.Elements<TableCell>().ToList();

                if (cells.Count < values.Count) throw new InvalidDataException("The template table does not contain enough columns.");

                for (var i = 0; i < values.Count; i++) SetCellText(cells[i], values[i]);

                lastRow = lastRow.InsertAfterSelf(newRow);
            }

            templateRow.Remove();
        }

        // Keeps the cell's own formatting: the new run takes the first run's properties, or the paragraph mark's when the cell is empty.
        private static void SetCellText(TableCell cell, string value)
        {
            var paragraph = cell.Elements<Paragraph>().FirstOrDefault() ?? cell.AppendChild(new Paragraph());

            var runProperties = paragraph.Elements<Run>().FirstOrDefault()?.RunProperties?.CloneNode(true) as RunProperties;
            if (runProperties == null && paragraph.ParagraphProperties?.ParagraphMarkRunProperties is { } markProperties)
                runProperties = new RunProperties(markProperties.ChildElements.Select(x => x.CloneNode(true)));

            paragraph.RemoveAllChildren<Run>();

            var run = new Run();
            if (runProperties != null) run.Append(runProperties);
            run.Append(new Text(value) { Space = SpaceProcessingModeValues.Preserve });

            paragraph.Append(run);
        }

        // Word may split a placeholder across several runs, so matching is done on each paragraph's combined text.
        // The replacement takes the formatting of the run where the placeholder starts.
        private static void ReplacePlaceholders(OpenXmlElement root, IReadOnlyDictionary<string, string> placeholders)
        {
            foreach (var paragraph in root.Descendants<Paragraph>())
            {
                foreach (var (placeholder, value) in placeholders)
                {
                    if (placeholder.Length == 0) continue;

                    var searchFrom = 0;

                    while (true)
                    {
                        var texts = paragraph.Descendants<Text>().ToList();
                        var combined = string.Concat(texts.Select(x => x.Text));
                        var start = combined.IndexOf(placeholder, searchFrom, StringComparison.Ordinal);
                        if (start < 0) break;

                        ReplaceRange(texts, start, placeholder.Length, value);
                        searchFrom = start + value.Length;
                    }
                }
            }
        }

        private static void ReplaceRange(List<Text> texts, int start, int length, string value)
        {
            var end = start + length;
            var offset = 0;
            var replaced = false;

            foreach (var text in texts)
            {
                var textStart = offset;
                var textEnd = offset + text.Text.Length;
                offset = textEnd;

                if (textEnd <= start || textStart >= end) continue;

                var before = start > textStart ? text.Text[..(start - textStart)] : string.Empty;
                var after = end < textEnd ? text.Text[(end - textStart)..] : string.Empty;

                text.Text = replaced ? after : before + value + after;
                text.Space = SpaceProcessingModeValues.Preserve;
                replaced = true;
            }
        }
    }
}
