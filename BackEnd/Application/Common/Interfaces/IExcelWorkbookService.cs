using Application.DataTransfer.Excel;

namespace Application.Common.Interfaces
{
    /// <summary>Reads/writes .xlsx files. Implemented in Infrastructure so Application stays library-agnostic.</summary>
    public interface IExcelWorkbookService
    {
        byte[] Write(ExcelWorkbookModel model);

        /// <summary>
        /// Reads the sheets matching <paramref name="schemas"/> (by sheet caption or key). Sheets missing
        /// from the file are returned empty. Throws <see cref="ExcelFormatException"/> for unreadable files.
        /// </summary>
        IReadOnlyList<ExcelSheetData> Read(Stream stream, IReadOnlyList<ExcelSheetSchema> schemas);
    }
}
