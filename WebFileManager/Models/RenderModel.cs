namespace WebFileManager.Models
{
    public class RenderModel
    {
        public int FileId { get; set; }
        public List<SheetModel> Sheets { get; set; }
        //public int CurrSheetId { get; set; }
        public List<ClassModel> Classes { get; set; }
        //public int CurrClassId { get; set; }
        public List<RowModel> Rows { get; set; }
    }
}
