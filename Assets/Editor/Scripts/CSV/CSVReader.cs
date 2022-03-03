using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ToolLib.EditorTableRead;
using ToolLib.Localization;

namespace ToolLib.CSV
{
    /// <summary>
    /// CSV读取类
    /// </summary>
    public class CSVReader
    {
        public List<List<string>> TableData => _readCurrentAreaOnly ? _partialTableData : _completeTableData;
        public List<List<string>> CompleteTableData => _completeTableData;
        private List<List<string>> _completeTableData;
        private List<List<string>> _partialTableData;

        /// <summary>
        /// true若此Reader仅读取当前地区表格配置，仅允许在构造时赋值
        /// </summary>
        private bool _readCurrentAreaOnly;

        private string _path = string.Empty;
        public string FilePath { get => _path; }
        private string _tempPath = string.Empty;

        /// <summary>
        /// 构造函数（用于存 离线表生成用）
        /// </summary>
        /// <param name="csvPath">csv路径</param>
        /// <param name="tableData">数据</param>
        public CSVReader(string csvPath, List<List<string>> tableData)
        {
            if (Path.GetExtension(csvPath) != ".csv")
            {
                throw new Exception($"错误的文件类型 错误值：{csvPath}");
            }
            string dirPath = Path.GetDirectoryName(csvPath);
            if (Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            
            _path = csvPath;
            _completeTableData = tableData;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="csvPath">csv路径</param>
        public CSVReader(string csvPath)
        {
            if (Path.GetExtension(csvPath) != ".csv")
            {
                throw new Exception($"错误的文件类型 错误值：{csvPath}");
            }

            if (!File.Exists(csvPath))
            {
                throw new Exception($"文件不存在 错误值：{csvPath}");
            }
                

            _path = csvPath;
            CopyAndReadCSV(_path);  //通过克隆打开 就算源文件已经正在开启也不会冲突

            // simple check csv column
            if (_completeTableData.Count <= 0)
            {
                return;
            }
            var baseCount = _completeTableData[0].Count;
            for (int i = 1; i < _completeTableData.Count; i++)
            {
                if (_completeTableData[i].Count != baseCount)
                {
                    throw new Exception($"[CSVReader]请检查csv，第{i + 1}列和行首列数不一致，请重新保存一下 csv:{csvPath}");
                }
            }
        }

        /// <summary>
        /// 克隆一份并读取内容
        /// </summary>
        /// <param name="filePath">源文件路径</param>
        private void CopyAndReadCSV(string filePath)
        {
            try
            {
                _tempPath = CSVUtil.GetFileTempPath(filePath);
                File.Copy(filePath, _tempPath, true);
                ReadCSV(_tempPath, filePath);
                //读完之后数据已经在内存了 临时文件直接删掉就好了
                DeleteTempFile();
            }
            catch (Exception e)
            {
                Context.Logger.Error(e.Message);
            }
        }

        /// <summary>
        /// 检查此表是否只读取当前地区配置
        /// </summary>
        /// <param name="fileName"></param>
        private void checkReadCurrentAreaOnly(string fileName)
        {
            var needSplitBytes = Excel.ExcelExporter.NeedSplitGameAreaBytes(fileName);
            _readCurrentAreaOnly = needSplitBytes && MEditorTableMgr.ReadCurrentAreaOnly;
        }

        private void prepareCurrentAreaTableData(string fileName)
        {
            _partialTableData?.Clear();
            _partialTableData = new List<List<string>>();

            if (_completeTableData.Count == 0)
            {
                throw new Exception($"要求对{fileName}进行按地区分流，但表格内容为空");
            }

            var locIndex = _completeTableData[0].IndexOf(Excel.ExcelExporter.LocalizationAreaFieldName);
            if (locIndex == -1)
            {
                throw new Exception($"要求对{fileName}进行按地区分流，但找不到相应字段" 
                                    + $"{Excel.ExcelExporter.LocalizationAreaFieldName}");
            }

            for (var rowIdx = 0; rowIdx < _completeTableData.Count; rowIdx++)
            {
                var rowData = _completeTableData[rowIdx];
                if (rowIdx >= 2)
                {
                    var locValue = rowData[locIndex];
                    if (!Excel.ExcelExporter.IsLocalizationAreaMatch(locValue))
                    {
                        // 取消注释启用测试用详细日志
                        //Context.Logger.Warning($"[CSVReader] {fileName,20} 跳过第{rowIdx,5}行配置, " +
                        //                   $"{Excel.ExcelExporter.LocalizationAreaFieldName}字段配置为{Excel.ExcelExporter.LocalizationAreaToBinary(locValue, 5)}, " +
                        //                   $"当前地区为{Excel.ExcelExporter.GameAreaToBinary(LocalizationBridge.GameArea, 5)}");
                        continue;
                    }
                }
                _partialTableData.Add(rowData);
            }
        }

        /// <summary>
        /// 从CSV文件中读取数据
        /// </summary>
        /// <param name="tempCsvPath">临时csv路径</param>
        /// <param name="originalCsvPath">原csv路径</param>
        private void ReadCSV(string tempCsvPath, string originalCsvPath)
        {
            var fileName = Path.GetFileNameWithoutExtension(originalCsvPath);
            checkReadCurrentAreaOnly(fileName);
            //编码检测
            if (!CSVUtil.CheckCSVFileEncodeType(tempCsvPath))
            {
                return;
            }

            _completeTableData?.Clear();
            _completeTableData = new List<List<string>>();
            FileStream fs = null;
            StreamReader sr = null;

            try
            {
                using (fs = new FileStream(tempCsvPath, FileMode.Open, FileAccess.Read))
                {
                    sr = new StreamReader(fs, Encoding.UTF8);

                    StringBuilder valueBuilder = new StringBuilder();
                    int quotationMarksCount = 0;  // 引号计数

                    //逐行读取CSV数据并解析
                    string lineStr = CSVReadLine(sr);

                    while (lineStr != null)
                    {
                        lineStr += ",";   //在行数据的最后加一个, 来确定最后一个字段的结束
                        List<string> lineValues = new List<string>();
                        //解析行数据
                        var lineChars = lineStr.ToCharArray();
                        for (int i = 0; i < lineChars.Length; i++)
                        {
                            char c = lineChars[i];
                            //如果读到英文逗号 且英文引号的计数为偶数 则认为一个字段读取结束
                            if (c == ',' && quotationMarksCount % 2 == 0)
                            {
                                //去除记录格式 获取真实值
                                string value = PraseValue(valueBuilder.ToString());

                                //加入值列表
                                lineValues.Add(value);
                                //清理构造器和计数器
                                valueBuilder.Clear();
                                quotationMarksCount = 0;
                                continue;
                            }

                            // "的引用计数增加
                            if (c == '\"')
                                quotationMarksCount++;

                            valueBuilder.Append(c);
                        }

                        //解析后的行数据列表 加入表数据列表
                        if (CheckLineDataValid(lineValues))
                        {
                            _completeTableData.Add(lineValues);
                        }

                        //读取下一行
                        lineStr = CSVReadLine(sr);
                    }

                    if (_readCurrentAreaOnly)
                    {
                        prepareCurrentAreaTableData(fileName);
                        if (_completeTableData.Count != _partialTableData.Count)
                        {
                            Context.Logger.Log($"[CSVReader] {fileName,30} 仅读取{LocalizationBridge.GameArea}地区表格配置，" +
                                                   $"分流结果：{_completeTableData.Count-2}条 -> {_partialTableData.Count-2}条");
                        }
                    }

                    sr.Close();
                }
            }
            catch (Exception exception)
            {
                sr?.Close();
                fs?.Close();
                _path = string.Empty;
                Context.Logger.Error(exception.Message);
                throw;
            }
        }

        /// <summary>
        /// 从流中读取一个符合CSV行格式的字符串
        /// </summary>
        /// <param name="sr">流读取器</param>
        /// <returns>CSV的一行</returns>
        string CSVReadLine(StreamReader sr)
        {
            StringBuilder lineStrBuilder = new StringBuilder();
            int quotationMarksCount = 0;  // 引号计数
            int content = sr.Read();
            while (content != -1)
            {
                char c = Convert.ToChar(content);
                if(c == '\r')// '\r' 全部忽略
                {
                    content = sr.Read();
                    continue;
                }

                //遇到换行符 切引号数两位偶数 则认为是正式的换行符
                if (c == '\n' && quotationMarksCount % 2 == 0)
                {
                    return lineStrBuilder.ToString();
                }
                //读到引号计数
                if (c == '"')
                    quotationMarksCount++;

                lineStrBuilder.Append(c);
                content = sr.Read();
            }
            //如果字符串构造器中有值 则返回对应字符串
            if (lineStrBuilder.Length > 0)
                return lineStrBuilder.ToString();
            //构造器中无内容 则认为已经读完
            return null;
        }

        /// <summary>
        /// Value值 去格式
        /// </summary>
        /// <param name="oriValueStr">初始值字符串</param>
        /// <returns>真实值</returns>
        string PraseValue(string oriValueStr)
        {
            string value = oriValueStr;

            //如果包含,或"或\n 则头尾有"包起来 所以必定有" 则只要判断是否有" 就够了
            if (value.Contains('\"'))
            {
                value = value.Substring(0, value.Length - 1).Substring(1);
            }
            // "格式的处理
            value = value.Replace("\"\"", "\"");
            //删除结尾的空格
            value = value.TrimEnd();

            return value;
        }

        /// <summary>
        /// 确认行数据是否有效 全为空则认为无效
        /// </summary>
        /// <param name="lineData"></param>
        /// <returns></returns>
        bool CheckLineDataValid(List<string> rowData)
        {
            foreach (string data in rowData)
            {
                if (!string.IsNullOrEmpty(data))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 添加一列数据
        /// </summary>
        /// <param name="index">插入的列号</param>
        /// <param name="colName">列的字段名</param>
        /// <param name="colDes">列字段的描述</param>
        /// <param name="defaultValue">默认值</param>
        public void AddNewCol(int index, string colName, string colDes, string defaultValue)
        {
            //如果已经有同名的字段 则直接返回
            if (_completeTableData[0].Contains(colName))
                return;

            for (int i = 0, count = _completeTableData.Count; i < count; i++)
            {
                List<string> rowData = _completeTableData[i];
                if (i == 0)       //表头行
                    rowData.Insert(index, colName);
                else if (i == 1)  //说明行
                    rowData.Insert(index, colDes);
                else              //数据行
                    rowData.Insert(index, defaultValue);
            }
        }

        /// <summary>
        /// 删除一列数据
        /// </summary>
        /// <param name="colName">列的字段名</param>
        public void RemoveColByName(string colName)
        {
            //找到对应列的列索引
            int targetIndex = 0;
            List<string> headRow = _completeTableData[0];
            for (int i = 0, count = headRow.Count; i < count; i++)
            {
                if (headRow[i] == colName)
                {
                    targetIndex = i;
                    break;
                }
            }
            //第0列禁止删除 或 没有找到指定列 则返回
            if (targetIndex == 0)
                return;
            //逐行删除
            for (int i = 0, count = _completeTableData.Count; i < count; i++)
            {
                List<string> rowData = _completeTableData[i];
                rowData.RemoveAt(targetIndex);
            }
        }

        /// <summary>
        /// 删除一行
        /// </summary>
        public bool DeleteRowByID(int ID)
        {
            foreach (var row in _completeTableData)
            {
                if (int.TryParse(row[0], out var id) && id == ID)
                {
                    _completeTableData.Remove(row);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 替换一行
        /// </summary>
        public bool ReplaceRowByID(int ID, List<string> line)
        {
            int index = _completeTableData.FindIndex(s => s[0] == ID.ToString());
            if (index != -1)
            {
                _completeTableData[index] = line;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 数据写回
        /// </summary>
        public void Save()
        {
            if (_completeTableData == null)
                return;

            string csvPath = _path;
            FileStream fs = null;
            StreamWriter sw = null;
            try
            {
                using (fs = new FileStream(csvPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    //设置文件写入从头开始
                    fs.Seek(0, SeekOrigin.Begin);
                    //清空原本文件流内容
                    fs.SetLength(0);

                    //总行数获取
                    int rowCount = _completeTableData.Count;
                    if (rowCount > 0)
                    {
                        //创建流写入类 并写入测试数据 CSV必须为UTF-8
                        sw = new StreamWriter(fs, Encoding.UTF8);
                        //由sheet第一行获取字段数量（列数）
                        List<string> firstRow = _completeTableData[0];
                        int columnCount = firstRow.Count;
                        //CSV每行的字符串构造类
                        StringBuilder rowStrBulider = new StringBuilder();
                        //从指定起始行开始读数据
                        List<string> rowData;
                        for (int i = 0; i < rowCount; i++)
                        {
                            //初始化字符串构造器
                            rowStrBulider.Clear();
                            //获取行数据 为空直接下一行
                            rowData = _completeTableData[i];
                            //遍历单元格
                            for (int j = 0; j < columnCount; j++)
                            {
                                //单元格数据获取
                                string value = rowData[j];

                                //CSV行规则添加  "改为""  包含"或,或\n的值用""包住 
                                //读取时从开始位置开始读到,的时候判断 "是否是偶数个 是的话则认为读完一个字段
                                if (value.Contains("\""))
                                    value = value.Replace("\"", "\"\"");
                                if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                                    value = $"\"{value}\"";
                                //删除结尾的空格
                                value.TrimEnd();
                                //加入字符串构造器 并添加分隔符
                                rowStrBulider.Append(value);
                                if (j != columnCount - 1)
                                    rowStrBulider.Append(",");
                            }
                            sw.WriteLine(rowStrBulider);
                        }
                        sw.Flush();
                        sw.Close();
                    }
                }
            }
            catch (Exception e)
            {
                if (sw != null)
                    sw.Close();
                if (fs != null)
                    fs.Close();
                Context.Logger.Error(Path.GetFileName(csvPath) + "需要写入操作，请先确保文件关闭！");
                Context.Logger.Error(e.Message);
                throw (e);
            }
        }

        /// <summary>
        /// 销毁(删除临时克隆的文件)
        /// </summary>
        public void Dispose()
        {
            DeleteTempFile();
        }

        /// <summary>
        /// 删除克隆的临时文件
        /// </summary>
        void DeleteTempFile()
        {
            if (!string.IsNullOrEmpty(_tempPath))
            {
                File.Delete(_tempPath);
                _tempPath = string.Empty;
            }
        }

    }
}
