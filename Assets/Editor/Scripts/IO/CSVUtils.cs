using Aspose.Cells;
using System.Collections.Generic;
using System.IO;


namespace XTools
{
    public class CSVUtils
    {
        public class WorkSheetInfo
        {
            public Worksheet TableSheet;
            public Dictionary<int, int> DicId2RowIndex;
            public Dictionary<string, int> DicColumnName2Index;
            public void Init()
            {
                DicId2RowIndex = new Dictionary<int, int>();
                DicColumnName2Index = new Dictionary<string, int>();
                for (int i = 2; i < TableSheet.Cells.MaxDataRow + 1; i++)
                {
                    DicId2RowIndex.Add(TableSheet.Cells[i, 0].IntValue, i);
                }
                for (int i = 0; i < TableSheet.Cells.MaxDataColumn + 1; i++)
                {
                    DicColumnName2Index.Add(TableSheet.Cells[0, i].StringValue, i);
                }
            }
        }

        private static Dictionary<string, WorkSheetInfo> CacheOfTable = new Dictionary<string, WorkSheetInfo>();

        public static WorkSheetInfo GetCSVSheetInfo(string tableName, bool isReload = false)
        {
            if (CacheOfTable.ContainsKey(tableName) && !isReload)
            {
                return CacheOfTable[tableName];
            }

            string fileName = Path.Combine(GlobalConfig.CSV_PATH, $"{tableName}.csv");
            if (!File.Exists(fileName))
            {
                return null;
            }

            var workSheetInfo = new WorkSheetInfo();
            Workbook workbook = new Workbook(fileName, new LoadOptions(LoadFormat.Csv));
            foreach (var sheet in workbook.Worksheets)
            {
                workSheetInfo.TableSheet = sheet;
                workSheetInfo.Init();
                if (!CacheOfTable.ContainsKey(tableName))
                    CacheOfTable.Add(tableName, workSheetInfo);
                else
                    CacheOfTable[tableName] = workSheetInfo;
                return workSheetInfo;
            }

            return null;
        }

        public static string GetCellValue(string tableName, int id, string columnName, WorkSheetInfo workSheetInfo = null)
        {
            if (id == 0)
            {
                return null;
            }
            if (workSheetInfo == null)
            {
                workSheetInfo = GetCSVSheetInfo(tableName);
            }

            var cells = workSheetInfo.TableSheet.Cells;
            int rowIndex, columnIndex;
            if (!workSheetInfo.DicId2RowIndex.TryGetValue(id, out rowIndex))
            {
                return null;
            }
            if (!workSheetInfo.DicColumnName2Index.TryGetValue(columnName, out columnIndex))
            {
                return null;
            }

            return cells[rowIndex, columnIndex].StringValue;
        }
    }
}
