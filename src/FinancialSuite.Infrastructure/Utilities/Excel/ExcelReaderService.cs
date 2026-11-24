using FinancialSuite.Domain.Interfaces.Utilities;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace FinancialSuite.Infrastructure.Utilities.Excel
{
    public class ExcelReaderService : IExcelReaderService
    {
        public IReadOnlyList<IReadOnlyDictionary<string, string>> ReadRows(Stream stream)
        {
            using var buffer = new MemoryStream();
            stream.CopyTo(buffer);
            var bytes = buffer.ToArray();

            return ToRecords(ReadCells(bytes));
        }

        // The format is detected from the content rather than the extension, because legacy exports often save other formats as .xls.
        private static List<List<string>> ReadCells(byte[] bytes)
        {
            if (bytes.Length >= 4 && bytes[0] == 0xD0 && bytes[1] == 0xCF && bytes[2] == 0x11 && bytes[3] == 0xE0)
                return ReadWorkbook(new HSSFWorkbook(new MemoryStream(bytes)));

            if (bytes.Length >= 2 && bytes[0] == 0x50 && bytes[1] == 0x4B)
                return ReadWorkbook(new XSSFWorkbook(new MemoryStream(bytes)));

            if (LegacyExcelReader.IsLegacyWorksheet(bytes))
                return LegacyExcelReader.ReadCells(bytes);

            throw new InvalidDataException("File is not a valid Excel (.xls or .xlsx) document.");
        }

        private static List<List<string>> ReadWorkbook(IWorkbook workbook)
        {
            using (workbook)
            {
                var sheet = workbook.GetSheetAt(0);
                var rows = new List<List<string>>();

                for (var rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    var values = new List<string>();

                    if (row != null)
                    {
                        for (var col = 0; col < row.LastCellNum; col++)
                            values.Add(row.GetCell(col)?.ToString()?.Trim() ?? string.Empty);
                    }

                    rows.Add(values);
                }

                return rows;
            }
        }

        private static List<IReadOnlyDictionary<string, string>> ToRecords(List<List<string>> rows)
        {
            if (rows.Count == 0) throw new InvalidDataException("Header row not found.");

            var headers = UniqueHeaders(rows[0]);
            var records = new List<IReadOnlyDictionary<string, string>>();

            foreach (var row in rows.Skip(1))
            {
                if (row.All(string.IsNullOrWhiteSpace)) continue;

                var record = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var col = 0; col < headers.Count; col++)
                    record[headers[col]] = col < row.Count ? row[col] : string.Empty;

                records.Add(record);
            }

            return records;
        }

        // Repeated headers get a numeric suffix (erdate, erdate2, ...) so no column is lost.
        private static List<string> UniqueHeaders(List<string> row)
        {
            var headers = new List<string>();
            var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var col = 0; col < row.Count; col++)
            {
                var header = string.IsNullOrWhiteSpace(row[col]) ? $"Column{col}" : row[col];
                seen[header] = seen.GetValueOrDefault(header) + 1;
                headers.Add(seen[header] == 1 ? header : $"{header}{seen[header]}");
            }

            return headers;
        }
    }
}
