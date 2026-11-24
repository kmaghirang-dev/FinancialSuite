using System.Globalization;
using System.Text;

namespace FinancialSuite.Infrastructure.Utilities.Excel
{
    // Reads stand-alone BIFF2–BIFF4 worksheets (Excel 2.x–4.0 .xls files without an OLE2 container), which NPOI cannot open.
    // Only cell values are read; formatting is ignored.
    internal static class LegacyExcelReader
    {
        private const int Eof = 0x000A;

        private static readonly int[] BofOpcodes = { 0x0009, 0x0209, 0x0409 };

        public static bool IsLegacyWorksheet(byte[] bytes) =>
            bytes.Length >= 4 && BofOpcodes.Contains(ReadUInt16(bytes, 0));

        public static List<List<string>> ReadCells(byte[] bytes)
        {
            var cells = new Dictionary<(int Row, int Col), string>();
            var position = 0;

            while (position + 4 <= bytes.Length)
            {
                var opcode = ReadUInt16(bytes, position);
                var length = ReadUInt16(bytes, position + 2);
                var data = position + 4;
                position = data + length;

                if (opcode == Eof) break;
                if (position > bytes.Length) throw new InvalidDataException("The Excel file is truncated.");

                var value = ReadCellValue(bytes, opcode, data, length);
                if (value != null) cells[(ReadUInt16(bytes, data), ReadUInt16(bytes, data + 2))] = value.Trim();
            }

            var rowCount = cells.Count == 0 ? 0 : cells.Keys.Max(x => x.Row) + 1;
            var rows = Enumerable.Range(0, rowCount).Select(_ => new List<string>()).ToList();

            foreach (var ((row, col), value) in cells)
            {
                var cellsInRow = rows[row];
                while (cellsInRow.Count <= col) cellsInRow.Add(string.Empty);
                cellsInRow[col] = value;
            }

            return rows;
        }

        private static string? ReadCellValue(byte[] bytes, int opcode, int data, int length)
        {
            switch (opcode)
            {
                // BIFF2 records: row, column, 3 attribute bytes, value.
                case 0x0002 when length >= 9:
                    return ReadUInt16(bytes, data + 7).ToString(CultureInfo.InvariantCulture);
                case 0x0003 when length >= 15:
                case 0x0006 when length >= 15:
                    return FormatNumber(BitConverter.ToDouble(bytes, data + 7));
                case 0x0004 when length >= 8:
                    return Encoding.Latin1.GetString(bytes, data + 8, Math.Min(bytes[data + 7], length - 8));
                case 0x0005 when length >= 9:
                    return bytes[data + 8] == 0 ? (bytes[data + 7] != 0).ToString().ToUpperInvariant() : string.Empty;

                // BIFF3/BIFF4 records: row, column, 2-byte format index, value.
                case 0x0203 when length >= 14:
                case 0x0206 when length >= 14:
                case 0x0406 when length >= 14:
                    return FormatNumber(BitConverter.ToDouble(bytes, data + 6));
                case 0x0204 when length >= 8:
                    return Encoding.Latin1.GetString(bytes, data + 8, Math.Min(ReadUInt16(bytes, data + 6), length - 8));
                case 0x027E when length >= 10:
                    return FormatNumber(DecodeRk(BitConverter.ToUInt32(bytes, data + 6)));
                case 0x0205 when length >= 8:
                    return bytes[data + 7] == 0 ? (bytes[data + 6] != 0).ToString().ToUpperInvariant() : string.Empty;

                default:
                    return null;
            }
        }

        private static double DecodeRk(uint rk)
        {
            var value = (rk & 0x02) != 0
                ? (int)rk >> 2
                : BitConverter.Int64BitsToDouble((long)(rk & 0xFFFFFFFC) << 32);

            return (rk & 0x01) != 0 ? value / 100 : value;
        }

        // Formula results that are not numbers are stored as a NaN pattern; they are read as empty.
        private static string FormatNumber(double value) =>
            double.IsFinite(value) ? value.ToString(CultureInfo.InvariantCulture) : string.Empty;

        private static int ReadUInt16(byte[] bytes, int offset) => bytes[offset] | bytes[offset + 1] << 8;
    }
}
