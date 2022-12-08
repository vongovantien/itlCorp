namespace eFMS.API.Report.FormatExcel
{
    public class CellAddress
    {
        /// <summary> Address of cell </summary>
        public string Address { get; set; }
        /// <summary> Row address of cell </summary>
        public int Row { get; set; }
        /// <summary> Column address of cell </summary>
        public int Column { get; set; }
        /// <summary> Column Letter address of cell </summary>
        public string ColumnLetter { get; set; }
    }
}