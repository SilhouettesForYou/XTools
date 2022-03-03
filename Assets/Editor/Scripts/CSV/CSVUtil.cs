using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToolLib.Excel;
using ToolLib.Localization;

namespace ToolLib.CSV
{
    public static class CSVUtil
    {
        public static string ConfigPath => Path.Combine(Util.MoonClientConfigPath, "Table/Configs");
        public static string CSVPath => Path.Combine(Util.MoonClientConfigPath, "Table/CSV");
        public static string LuaPath => Path.Combine(Util.MoonClientConfigPath, "Table/Lua");
        public static string ToolsPath => Path.Combine(Util.MoonClientConfigPath, "Table/Tools");

        private static string TableClientBytesPath => Path.Combine(Util.MoonClientConfigPath, @"Assets\Resources\Table");
        private static string TableServerBytesPath => Path.Combine(Util.MoonClientConfigPath, "Table/ServerBytes");
        private static string TableEditorBytesPath => Path.Combine(Util.MoonClientConfigPath, @"Assets\Editor\Table");

        /// <summary>
        /// 获取克隆备份的临时文件路径
        /// </summary>
        /// <param name="filePath">原文件路径</param>
        /// <returns></returns>
        internal static string GetFileTempPath(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var ex = Path.GetExtension(filePath);
            var rootPath = Path.GetDirectoryName(filePath);
            var tempDirPath = Path.Combine(rootPath, "_Temp");
            if (!Directory.Exists(tempDirPath))
            {
                Directory.CreateDirectory(tempDirPath);
            }
            var tempPath = Path.Combine(tempDirPath, fileName + "_temp" + ex);
            return tempPath;
        }

        /// <summary>
        /// 检查CSV的编码格式是不是UTF-8  简易判断
        /// </summary>
        /// <param name="csvPath">csv路径</param>
        public static bool CheckCSVFileEncodeType(string csvPath)
        {
            try
            {
                using (FileStream fs = new FileStream(csvPath, FileMode.Open, FileAccess.Read))
                {
                    string fileName = csvPath.Replace("_temp", string.Empty); //有时候是克隆打开 报错的时候就去掉_temp
                    BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
                    byte[] ss = r.ReadBytes(3);
                    r.Close();
                    //编码类型 Coding=编码类型.ASCII;   
                    if (ss[0] >= 0xEF)
                    {
                        if (ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF)
                        {
                            return true;
                        }
                        else if (ss[0] == 0xFE && ss[1] == 0xFF)
                        {
                            Context.Logger.Error($"{fileName}编码错误 当前为BigEndianUnicode");
                            return false;
                        }
                        else if (ss[0] == 0xFF && ss[1] == 0xFE)
                        {
                            Context.Logger.Error($"{fileName}编码错误 当前为Unicode");
                            return false;
                        }
                        else
                        {
                            Context.Logger.Error($"{fileName}编码格式无法识别");
                            return false;
                        }
                    }
                    else
                    {
                        Context.Logger.Error($"{fileName}编码格式无法识别");
                        return false;
                    }
                }
            }
            catch(Exception e)
            {
                Context.Logger.Error(e.Message);
                Context.Logger.Error("CSV文件读取异常");
                return false;
            }
        }
        
        /// <summary>
        /// CSV数据转化为Lua用查错数据格式
        /// </summary>
        /// <param name="csv">CSV</param>
        /// <param name="startRow">开始行索引</param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> GetLuaCheckTableData(this CSVReader csv, int startRow = 2)
        {
            var res = new List<Dictionary<string, string>>();
            var headRowDatas = csv.TableData[0];
            for (int i = startRow; i < csv.TableData.Count; i++)
            {
                var rowDataDic = new Dictionary<string, string>();
                var rowDatas = csv.TableData[i];
                for (int j = 0; j < headRowDatas.Count; j++)
                {
                    if (rowDatas.Count < j)
                    {
                        Context.Logger.Error($"第{i + 1}行 第{j + 1}列 不存在");
                    }
                    if (!string.IsNullOrEmpty(headRowDatas[j]) && !rowDataDic.ContainsKey(headRowDatas[j]))
                    {
                        rowDataDic.Add(headRowDatas[j], rowDatas[j]);
                    }
                    else
                    {
                        rowDataDic.Add($"value{j}", rowDatas[j]);
                    }
                }

                res.Add(rowDataDic);
            }
            return res;
        }

        internal static bool CheckCsvFileColumnsMatched(List<string> ret1, List<string> ret2)
        {
            if (ret1.Count != ret2.Count)
            {
                Context.Logger.Error($"[CSVUtil] checkCsvFileColumnsMatched fail, 两张表列数不匹配");
                return false;
            }

            for (int i = 0; i < ret1.Count; i++)
            {
                var id1 = ret1[i];
                var id2 = ret2[i];
                if (id1 != id2)
                {
                    Context.Logger.Error($"[CSVUtil] checkCsvFileColumnsMatched 两张表列不匹配");
                    return false;
                }
            }

            return true;
        }

        internal static bool ReadCSV(string csvPath, out List<List<string>> result)
        {
            //获取CSV中的数据
            CSVReader csvReader = new CSVReader(csvPath);

            result = new List<List<string>>();

            if (csvReader.TableData.Count < 2) return true;

            // todo@om 这段逻辑应该在CSVReader里就处理好。
            // 剔除末尾无用的列
            var index = csvReader.TableData[0].Count;
            for (int i = csvReader.TableData[0].Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrEmpty(csvReader.TableData[0][i]))
                {
                    index--;
                    continue;
                }

                break;
            }

            foreach (var list in csvReader.TableData)
            {
                if (index > list.Count)
                {
                    Context.Logger.Error($"[CSVUtil]ReadCSV fail，检查下表({csvPath})，列数对不上");
                }
                result.Add(new List<string>(list.GetRange(0, index)));
            }

            csvReader.Dispose();

            return true;
        }

        internal static bool FindMainKey(TableInfo tableInfo, out string key)
        {
            var binKey = string.Empty;
            var searchKey = string.Empty;

            foreach (var field in tableInfo.Fields)
            {
                if (field.IndexType == TableFieldInfo.EIndexType.Bin)
                {
                    binKey = field.FieldName;
                    break;
                }
                else if (field.IndexType == TableFieldInfo.EIndexType.Search && string.IsNullOrEmpty(searchKey))
                {
                    searchKey = field.FieldName;
                }
            }

            if (!string.IsNullOrEmpty(binKey))
            {
                key = binKey;
                return true;
            }

            if (!string.IsNullOrEmpty(searchKey))
            {
                key = searchKey;
                return true;
            }

            key = string.Empty;
            Context.Logger.Error($"[CSVUtil] findMainKey fail, 找不到主键  MainTableName:{tableInfo.MainTableName} ");
            return false;
        }

        internal static bool FindReplaceColumnIndex(TableInfo tableInfo, List<List<string>> originalRows, out int index)
        {
            index = -1;
            if (!FindMainKey(tableInfo, out var key))
            {
                Context.Logger.Error($"[CSVUtil] findReplaceRowIndex fail, 找不到主键");
                return false;
            }

            // 找index
            var columnIndex = -1;
            for (int i = 0; i < originalRows[0].Count; i++)
            {
                if (originalRows[0][i] == key)
                {
                    columnIndex = i;
                    break;
                }
            }

            if (columnIndex < 0)
            {
                Context.Logger.Error($"[CSVUtil] findReplaceRowIndex fail 找不到对应的列");
                return false;
            }

            index = columnIndex;

            return true;
        }

        internal static bool MergeData(List<List<string>> result, List<List<string>> addition)
        {
            if (result.Count > 0)
            {
                if (addition.Count <= 2) return true;
                result.AddRange(addition.GetRange(2, addition.Count - 2));
            }
            else
            {
                result.AddRange(addition);
            }
            
            return true;
        }

        public static Dictionary<string, int> GetFieldIndex(List<List<string>> data, bool forLua)
        {
            var ret = new Dictionary<string, int>();
            for (var i = 0; i < data[0].Count; i++)
            {
                var s = data[0][i];

                if (ret.ContainsKey(s)) ret.Remove(s);

                var index = forLua ? (i + 1) : i;
                ret.Add(s, index);
            }

            return ret;
        }

        public static bool MergeCSVData(TableInfo tableInfo, out List<List<string>> result)
        {
            result = new List<List<string>>();
            var area = LocalizationBridge.GameArea;
            foreach (var location in tableInfo.TableLocations)
            {
                List<List<string>> ret;
                var csvPath = CSVParser.GetAbsolutePath(location.ExcelPath, area, out var overseaCsvPath);
                if (string.IsNullOrEmpty(overseaCsvPath))
                {
                    if (!ReadCSV(csvPath, out ret))
                    {
                        Context.Logger.Error($"[CSVUtil]读表失败 {csvPath}");
                        return false;
                    }

                    MergeData(result, ret);
                    continue;
                }

                // 原表必须满足基本表格格式，因此判定count大于等于2
                if (!ReadCSV(csvPath, out ret) || ret.Count <= 2)
                {
                    Context.Logger.Error($"[CSVUtil] 读取增量表失败，原始表行数小于3{csvPath}");
                    return false;
                }

                if (!ReadCSV(overseaCsvPath, out var retOverride))
                {
                    Context.Logger.Error($"[CSVUtil] 读取海外表失败:{overseaCsvPath}");
                    return false;
                }

                // 增量表不要求必须填前两行，这种情况被认为是空表即可
                if (retOverride.Count <= 2)
                {
                    MergeData(result, ret);
                    continue;
                }

                // 检查列匹配
                if (!CheckCsvFileColumnsMatched(ret[0], retOverride[0]))
                {
                    Context.Logger.Error($"[CSVUtil] 读取增量表失败，和原始表列不匹配 csvPath:{csvPath} overseaCsvPath:{overseaCsvPath}");
                    return false;
                }

                if (!FindReplaceColumnIndex(tableInfo, ret, out var columnCount) || columnCount < 0 || columnCount >= ret.First().Count)
                {
                    Context.Logger.Error($"[CSVUtil] 读取增量表失败，查询主键失败");
                    return false;
                }

                // 执行表数据行覆盖操作
                for (int i = 2; i < retOverride.Count; i++)
                {
                    bool overrideSuccess = false;
                    for (int j = 2; j < ret.Count; j++)
                    {
                        if (ret[j][columnCount] != retOverride[i][columnCount]) continue;
                        
                        ret[j] = retOverride[i];
                        overrideSuccess = true;
                        break;
                    }

                    if (overrideSuccess) continue;

                    Context.Logger.Error($"[CSVUtil]读取增量表失败,新增了主键 {retOverride[i][columnCount]} overseaCsvPath:{overseaCsvPath}");
                    return false;
                }

                MergeData(result, ret);
            }

            return true;
        }
    }
}
