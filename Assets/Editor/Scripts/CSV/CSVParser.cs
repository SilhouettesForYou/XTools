using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LitJson;
using MoonCommonLib;
using Serializer;
using ToolLib.Excel;
using ToolLib.Localization;

namespace ToolLib.CSV
{
    public class CSVParser
    {
        private TableInfo _tableInfo;
        private EExportType _tableType;
        private TableFieldInfo _indexFieldInfo = null;
        private TableFieldInfo _searchFieldInfo = null;
        private List<TableFieldInfo> _fieldFieldsToExport;
        // private Dictionary<TableFieldInfo, string[]> _tableData;
        private bool _enableCheckExcel;
        private bool _rebuildStringSet = true;
        Dictionary<string, uint> _dictStringToHash = new Dictionary<string, uint>();

        public static readonly int START_LINE = 2;

        private Dictionary<string, int> _name2Index = new Dictionary<string, int>();

        #region 临时变量

        private uint _lineNumber;//行数
        private readonly List<List<string>> _sortFields = new List<List<string>>();   // 排序用
        private readonly List<List<string>> _sortOverrideFields = new List<List<string>>();   // 增量
        private int _binIndex = -1;//主键在lineArr中的index
        #endregion

        private TableDumpData _oldData;
        private HashSet<string> _originalKeySet;

        private void initExcelParser(TableInfo info, EExportType tableType, bool enableCheckExcel,
            bool rebuildStringSet)
        {
            _tableInfo = info;
            _tableType = tableType;
            _lineNumber = 0;
            _enableCheckExcel = enableCheckExcel;
            _rebuildStringSet = rebuildStringSet;
            _oldData = null;
        }

        public CSVParser(string configPath, EExportType tableType, bool enableCheckExcel, bool rebuildStringSet)
        {
            try
            {
                var json = FileEx.ReadText(configPath);
                _tableInfo = JsonMapper.ToObject<TableInfo>(json);
                initExcelParser(_tableInfo, tableType, enableCheckExcel, rebuildStringSet);
            }
            catch (Exception e)
            {
                Context.Logger.Error($"Config出错 名称为：{configPath} {e}");
                throw;
            }
        }

        public CSVParser(TableInfo info, EExportType tableType, bool enableCheckExcel, bool rebuildStringSet)
        {
            initExcelParser(info, tableType, enableCheckExcel, rebuildStringSet);
        }

        public void ResetData()
        {
            _lineNumber = 0;
            _sortFields.Clear();
            _sortOverrideFields.Clear();
            _binIndex = -1;
        }

        #region WriteFile

        private bool ReadOldStringToHash(string filePath, ref ulong preHashMD5, ref ulong preHashTime)
        {
            _dictStringToHash.Clear();
            if (File.Exists(filePath))
            {
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    using (var reader = new BinaryReader(fs))
                    {
                        var totalSize = reader.ReadInt64();
                        preHashMD5 = reader.ReadUInt64();
                        preHashTime = reader.ReadUInt64();

                        if (!_rebuildStringSet)
                        {
                            TableUtil.ReadStringInfo(reader, out uint totalStringNum, out string[] dictHashToString);
                            for (int i = 0; i < totalStringNum; i++)
                            {
                                var strValue = dictHashToString[i];
                                ParserUtil.AddStr(true, ref _dictStringToHash, null, strValue);
                            }
                        }
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 加载之前的bytes
        /// </summary>
        /// <param name="path"></param>
        private void dumpOriginalBytes(string path)
        {
            if (!File.Exists(path)) return;

            // switch
            if (!Excel.ExcelExporter.OpenDeleteKeyCheck) return;

            // filter
            if (Excel.ExcelExporter.IsCheckFilter(_tableInfo.MainTableName)) return;

            try
            {
                _oldData = TableDataDumper.DumpTable(path);
            }
            catch (Exception e)
            {
                Context.Logger.Error($"[CSVParser] {_tableInfo.MainTableName} dumpOriginalBytes exception:{e}");
            }

            if (_oldData == null) return;

            var headBin = _oldData.HeadBinID;
            if (headBin < 0 || headBin >= _oldData.FieldInfos.Length)
            {
                return;
            }

            var binHash = _oldData.FieldInfos[headBin].Hash;

            _originalKeySet = new HashSet<string>();
            for (int i = 0; i < _oldData.RowNumber; i++)
            {
                _originalKeySet.Add(_oldData.Body[i, binHash].ToString());
            }
        }

        /// <summary>
        /// 检查主键是否被删除了
        /// </summary>
        /// <returns></returns>
        private bool checkKeyCantDelete()
        {
            if (_oldData == null || _originalKeySet == null) return true;

            var binData = getBinColData();
            foreach (var s in binData)
            {
                _originalKeySet.Remove(s);
            }

            if (_originalKeySet.Count > 0)
            {
                Context.Logger.Error($"{_tableInfo.MainTableName} 不允许删除主键，如果明确此表不受检查，请于CSVExporterConfig.json中配置");
                foreach (var k in _originalKeySet)
                {
                    Context.Logger.Error($" [{_tableInfo.MainTableName}] 尝试删除主键:{k}");
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 导出配置中所有的表，包括子表
        /// </summary>
        /// <param name="outputPath"></param>
        public bool Export(string outputPath, bool reCompileAll, string parentTableJsonPath = null)
        {
            _oldData = null;
            _originalKeySet = null;

            string jsonPath = string.IsNullOrEmpty(parentTableJsonPath) ? Path.Combine(CSVUtil.ConfigPath, $"{_tableInfo.MainTableName}.json") : parentTableJsonPath;

            var preCsvHashMD5 = 0UL;
            var preJsonHashMD5 = 0UL;
            var filePath = PathEx.MakePathStandard(Path.Combine(outputPath, $"{_tableInfo.MainTableName}.bytes"));
            DirectoryEx.MakeDirectoryExist(outputPath);
            ReadOldStringToHash(filePath, ref preCsvHashMD5, ref preJsonHashMD5);
            var tempPath = filePath + ".temp";
            bool succ = true;
            try
            {
                var csvHashMD5 = GetCSVHashValue();
                var jsonHashMD5 = GetJsonHashValue(jsonPath);
                if (csvHashMD5 != preCsvHashMD5 || jsonHashMD5 != preJsonHashMD5 || reCompileAll)
                {
                    // 加载之前存的bytes
                    dumpOriginalBytes(filePath);

                    using (FileStream stream = new FileStream(tempPath, FileMode.Create))
                    {
                        using (BinaryWriter writer = new BinaryWriter(stream))
                        {

                            if (!WriteFile(writer, csvHashMD5, jsonHashMD5))
                            {
                                succ = false;
                                Context.Logger.Error($"{_tableInfo.MainTableName} export failed!!!! exportType:{_tableType} Area:{Excel.ExcelExporter.CurChannel}");
                            }
                        }
                    }
                }
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
                throw;
            }

            if (!succ)
            {
                File.Delete(tempPath);
                return false;
            }


            if (File.Exists(tempPath))
            {
                File.Delete(filePath);
                File.Move(tempPath, filePath);
            }
            if (_tableInfo.Children != null)
            {
                foreach (var child in _tableInfo.Children)
                {
                    child.Fields = _tableInfo.Fields;
                    child.CheckInfos = _tableInfo.CheckInfos;
                    child.TableBytesTarget = _tableInfo.TableBytesTarget;
                    child.TableCodeTarget = _tableInfo.TableCodeTarget;

                    var childParser = new CSVParser(child, _tableType, _enableCheckExcel, _rebuildStringSet);
                    if (!childParser.Export(outputPath, reCompileAll, jsonPath))
                    {
                        Context.Logger.Error($"导出{_tableInfo.MainTableName}的子表失败:【{child.MainTableName}】");
                        return false;
                    }
                }
            }
            return true;
        }

        private string GetPlatformName()
        {
            switch (_tableType)
            {
                case EExportType.Client:
                    {
                        return "客户端";
                    }
                case EExportType.Server:
                    {
                        return "服务器";
                    }
                case EExportType.Editor:
                    {
                        return "编辑器";
                    }
                default:
                    {
                        Context.Logger.Error($"{_tableInfo.MainTableName} {_tableType} 类型未处理");
                        break;
                    }
            }
            return "未处理";
        }

        /// <summary>
        /// 写文件 不包括子表
        /// </summary>
        /// <param name="writer"></param>
        public bool WriteFile(BinaryWriter writer, ulong csvHashMD5, ulong jsonHashMD5)
        {
            if (!WriteHead(true, ref _dictStringToHash, null))
            {
                return false;
            }
            if (!WriteBody(true, ref _dictStringToHash, null))
            {
                return false;
            }

            long size = 0;
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(size);             //size(int64) 预留位置
            writer.Write(csvHashMD5);       //size(int64) CSV的哈希值 用于判断CSV是否有修改
            writer.Write(jsonHashMD5);      //size(int64) Json的哈希值 用于判断JSON是否有修改

            ResetData();
            if (!WriteString(writer, ref _dictStringToHash))
            {
                return false;
            }
            if (!WriteHead(false, ref _dictStringToHash, writer))
            {
                return false;
            }
            if (!WriteBody(false, ref _dictStringToHash, writer))
            {
                return false;
            }

            size = writer.BaseStream.Position;
            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(size);

            writer.Close();
            return true;
        }

        private ulong GetHashByCsvTmpPath(string csvPath, string tmpPath, ulong hash)
        {
            File.Copy(csvPath, tmpPath, true);
            var csvMD5 = Util.CalculateMD5(tmpPath);
            return Util.GetLongHash(csvMD5, hash);
        }

        /// <summary>
        /// 获取CSV文件的Hash值
        /// </summary>
        private ulong GetCSVHashValue()
        {
            ulong hashValue = 0;
            if (_tableInfo == null || _tableInfo.TableLocations == null) return hashValue;
            foreach (var tableLocation in _tableInfo.TableLocations)
            {
                var excelFullPath = GetAbsolutePath(tableLocation.ExcelPath, LocalizationBridge.GameArea, out var overseaCSVPath);
                var excelFullTempPath = ExcelUtil.GetFileTempPath(excelFullPath);
                hashValue = GetHashByCsvTmpPath(excelFullPath, excelFullTempPath, hashValue);

                if (!string.IsNullOrEmpty(overseaCSVPath))
                {
                    hashValue = GetHashByCsvTmpPath(overseaCSVPath, excelFullTempPath, hashValue);
                }

                File.Delete(excelFullTempPath);
            }
            return hashValue;
        }

        /// <summary>
        /// 获取Json文件的Hash值
        /// </summary>
        private ulong GetJsonHashValue(string jsonPath)
        {
            ulong hashValue = 0;
            if (_tableInfo == null || _tableInfo.TableLocations == null) return hashValue;

            if (File.Exists(jsonPath))
            {
                var jsonTempPath = ExcelUtil.GetFileTempPath(jsonPath);
                File.Copy(jsonPath, jsonTempPath, true);
                var jsonMD5 = Util.CalculateMD5(jsonTempPath);
                hashValue = Util.GetLongHash(jsonMD5, hashValue);
                File.Delete(jsonTempPath);
            }

            return hashValue;
        }

        private bool ReadFromCSVAndReadString()
        {
            var tableLocationsLen = _tableInfo.TableLocations.Length;
            for (int i = 0; i < tableLocationsLen; i++)
            {
                var location = _tableInfo.TableLocations[i];
                var csvPath = GetAbsolutePath(location.ExcelPath, LocalizationBridge.GameArea, out var overseaCSVPath);
                if (!ReadCSV(csvPath))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(overseaCSVPath))
                {
                    if (!ReadCSV(overseaCSVPath, true))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool ReadCSV(string csvPath, bool markOverride = false)
        {
            //获取CSV中的数据
            CSVReader csvReader = new CSVReader(csvPath);
            //确认是否是海外表的数据
            List<List<string>> rowDatas = markOverride ? _sortOverrideFields : _sortFields;

            var fieldCount = _fieldFieldsToExport.Count;
            int[] fieldIndexs = new int[fieldCount];  // 字段列索引列表
            int binFieldIndex = -1;    // 主键在数据表中的列索引

            //从表头行 获取字段的列索引列表和主键索引
            List<string> HeadData = csvReader.TableData[0];
            for (var i = 0; i < fieldCount; i++)
            {
                int fieldIndex = -1;
                for (int j = 0; j < HeadData.Count; j++)
                {
                    if (_fieldFieldsToExport[i].FieldName == HeadData[j])
                    {
                        fieldIndex = j;
                        if (_fieldFieldsToExport[i].IndexType == TableFieldInfo.EIndexType.Bin)
                            binFieldIndex = j;
                        break;
                    }
                }
                //CSV中缺少对应字段报错
                if (fieldIndex == -1)
                    Context.Logger.Error($"表{_tableInfo.MainTableName}缺少字段{_fieldFieldsToExport[i].FieldName}，严重错误，请及时处理！");
                //记录字段索引
                fieldIndexs[i] = fieldIndex;

                if (!_name2Index.ContainsKey(_fieldFieldsToExport[i].FieldName))
                {
                    _name2Index.Add(_fieldFieldsToExport[i].FieldName, i);
                }
            }

            var locIndex = HeadData.IndexOf(Excel.ExcelExporter.LocalizationAreaFieldName);
            var fileName = Path.GetFileNameWithoutExtension(csvPath);
            bool needSplitBytes = Excel.ExcelExporter.NeedSplitGameAreaBytes(fileName);

            //获取实际有效数据列表
            for (int i = START_LINE; i < csvReader.TableData.Count; i++)
            {
                List<string> csvRowData = csvReader.TableData[i];
                //如果存在主键 防止检索时出错 主键为空的数据认为是无效数据
                if (!CheckRowValid(csvRowData, binFieldIndex))
                    continue; ;

                if (needSplitBytes && !markOverride && locIndex >= 0 && binFieldIndex >= 0)
                {
                    if (locIndex < csvRowData.Count && !Excel.ExcelExporter.IsLocalizationAreaMatch(csvRowData[locIndex]))
                    {
                        Context.Logger.Log($"[CSVParser]{fileName} Skip {HeadData[binFieldIndex]}:{csvRowData[binFieldIndex]}");
                        continue;
                    }
                }

                //实际有效的行数据获取
                List<string> rowData = new List<string>();
                for (var j = 0; j < fieldIndexs.Length; j++)
                {
                    var field = _fieldFieldsToExport[j];
                    if (fieldIndexs[j] == -1)
                    {
                        rowData.Add(field.DefaultValue ?? string.Empty);
                    }
                    else
                    {
                        int cellIndex = fieldIndexs[j];
                        if (cellIndex >= csvRowData.Count)
                            Context.Logger.Error($"{_tableInfo.MainTableName}中存在数据异常请确认保存的工具后 重新保存！");
                        string cellValue = cellIndex < csvRowData.Count ? csvRowData[cellIndex] : string.Empty;
                        if (string.IsNullOrEmpty(cellValue) && !string.IsNullOrEmpty(field.DefaultValue))
                            cellValue = field.DefaultValue;
                        rowData.Add(cellValue);
                    }
                }
                rowDatas.Add(rowData);
                if (!markOverride)
                    _lineNumber++;
            }

            csvReader.Dispose();
            return true;
        }

        /// <summary>
        /// 确认行数据有效性
        /// </summary>
        /// <param name="rowData">行数据</param>
        /// <param name="binIndex">主键索引 无主键则为-1</param>
        /// <returns></returns>
        public static bool CheckRowValid(List<string> rowData, int binIndex)
        {
            if (binIndex != -1)  //存在主键时 判断主键是否超出长度 或者 对应字段是否为空
                return binIndex < rowData.Count && !string.IsNullOrEmpty(rowData[binIndex]);
            else
            {
                //无主键时 不为全空行 则认为是有效数据
                foreach (var cellData in rowData)
                {
                    if (!string.IsNullOrEmpty(cellData))
                        return true;
                }
                return false;
            }
        }

        private bool WriteString(BinaryWriter writer, ref Dictionary<string, uint> dictStringToHash)
        {
            var len = (UInt32)(dictStringToHash.Count);
            string[] strDatas = new string[len];
            foreach (var val in dictStringToHash)
            {
                strDatas[val.Value] = val.Key;
            }

            writer.Write(len);
            for (int i = 0; i < strDatas.Length; i++)
            {
                var curStr = strDatas[i];
                if (curStr != null)
                {
                    writer.Write(curStr);
                }
                else
                {
                    Context.Logger.Error($"【{_tableInfo.MainTableName}】导出失败 字符串hash【{i}】不存在");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Head结构：
        /// 字段数量(int) +（字段名Hash(uint)+字段类型(int)+seq长度(byte)+默认值(根据类型)）* n
        /// </summary>
        /// <param name="writer"></param>
        private bool WriteHead(bool isColloctString, ref Dictionary<string, uint> dictStringToHash, BinaryWriter writer)
        {
            _fieldFieldsToExport = _tableInfo.GetPlatformFields(_tableType);

            //字段数量
            if (!isColloctString)
            {
                writer.Write((UInt32)_fieldFieldsToExport.Count);
            }

            for (int i = 0, sz = _fieldFieldsToExport.Count; i < sz; i++)
            {
                var fieldInfo = _fieldFieldsToExport[i];
                if (fieldInfo.IndexType == TableFieldInfo.EIndexType.Bin)
                {
                    if (_binIndex == -1)
                    {
                        _binIndex = i;
                        _indexFieldInfo = fieldInfo;
                    }
                    else
                    {
                        Context.Logger.Error($"{_tableInfo.MainTableName} 不能有多个bin:{_fieldFieldsToExport[_binIndex].FieldName},{_fieldFieldsToExport[i].FieldName}");
                    }
                }
                else if (fieldInfo.IndexType == TableFieldInfo.EIndexType.Search)
                {
                    if (_searchFieldInfo == null)
                    {
                        _searchFieldInfo = fieldInfo;
                    }
                }
            }
            //字段数量
            if (!isColloctString)
            {
                writer.Write((Int16)_binIndex);
            }

            //_fieldFieldsToExport.Sort((x, y)=> Util.GetHash(x.FieldName).CompareTo(Util.GetHash(y.FieldName)));

            for (int i = 0, sz = _fieldFieldsToExport.Count; i < sz; i++)
            {
                var fieldInfo = _fieldFieldsToExport[i];
                sbyte fieldNameHash = (sbyte)(GetPosID(fieldInfo, _tableType));
                if (!isColloctString) writer.Write(fieldInfo.NeedLocal);
                if (!isColloctString) writer.Write(fieldNameHash);//字段名
                ParserUtil.GetTypeEnum(fieldInfo.FieldTypeName, out var tableTypeEnum, fieldInfo.NeedLocal);
                if (!isColloctString) writer.Write((Int16)tableTypeEnum);//字段类型
                sbyte seqLength = ParserUtil.GetSeqLength(fieldInfo.FieldTypeName);
                if (!isColloctString) writer.Write(seqLength);//seq长度
                var parser = ParserUtil.GetParser(fieldInfo.FieldTypeName, true);
                if (!parser.WriteHashFromString(isColloctString, ref dictStringToHash, writer, fieldInfo.DefaultValue, fieldInfo.FieldTypeName))//默认值
                {
                    Context.Logger.Error($"【{_tableInfo.MainTableName}】Write False!!!写入头失败，头字段名为：【{fieldInfo.FieldName}】");
                    return false;
                }
            }
            return true;
        }

        private bool getBodyFieldsData(List<List<string>> list, bool isColloctString)
        {
            var binColData = getBinColData();
            var sortFieldsCnt = binColData.Length;

            if (!overrideFieldsIfNeeded())
            {
                return false;
            }

            var allTableData = new Dictionary<TableFieldInfo, string[]>();
            for (int iCol = 0; iCol < _fieldFieldsToExport.Count; iCol++)
            {
                string[] colData = new string[sortFieldsCnt];
                for (var iRow = 0; iRow < colData.Length; iRow++)
                {
                    colData[iRow] = list[iRow][iCol];
                }
                var array = colData.ToArray();
                if (!isColloctString && _enableCheckExcel && !ExcelChecker.CheckField(_tableInfo, _fieldFieldsToExport[iCol], array, binColData))
                {
                    return false;
                }
                allTableData.Add(_fieldFieldsToExport[iCol], array);
            }
            if (!isColloctString && _enableCheckExcel && !ExcelChecker.CheckTable(_tableInfo, allTableData))
            {
                return false;
            }

            //每一行
            if (_lineNumber != sortFieldsCnt)
            {
                Context.Logger.Error($"表名:{_tableInfo.MainTableName} 总行数不相等_lineNumber：{_lineNumber} != sortFieldsCnt：{sortFieldsCnt}");
            }

            return true;
        }

        private string[] getBinColData()
        {
            var sortFieldsCnt = _sortFields.Count;
            string[] binColData = new string[sortFieldsCnt];
            //对行进行排序
            if (_binIndex != -1)
            {
                _sortFields.Sort((x, y) =>
                {
                    var parser = ParserUtil.GetParser(_indexFieldInfo.FieldTypeName, true);
                    return parser.CompareFromString(x[_binIndex], y[_binIndex]);
                });
                for (var iRow = 0; iRow < binColData.Length; iRow++)
                {
                    binColData[iRow] = _sortFields[iRow][_binIndex];
                }
            }
            else
            {
                for (var iRow = 0; iRow < binColData.Length; iRow++)
                {
                    binColData[iRow] = string.Empty;
                }
            }

            return binColData;
        }

        private bool overrideFieldsByFieldInfo(TableFieldInfo tableFieldInfo)
        {
            if (!_name2Index.TryGetValue(tableFieldInfo.FieldName, out var index))
            {
                Context.Logger.Error($"表{tableFieldInfo.FieldName}异常，找不到在表里的位置");
                return false;
            }

            for (var index1 = 0; index1 < _sortOverrideFields.Count; index1++)
            {
                var newRow = _sortOverrideFields[index1];
                var finded = false;
                var id = newRow[index];
                for (int i = 0; i < _sortFields.Count; i++)
                {
                    var oldRow = _sortFields[i];
                    if (oldRow[index].Equals(id))
                    {
                        _sortFields[i] = newRow;
                        finded = true;
                        break;
                    }
                }

                if (!finded)
                {
                    Context.Logger.Error($"{_tableInfo.MainTableName} 海外增量bytes生成失败, 禁止添加新的索引 id:{id}");
                    return false;
                }
            }

            return true;
        }

        private bool overrideFieldsIfNeeded()
        {
            if (_sortOverrideFields.Count <= 0) return true;

            if (_binIndex != -1 && _indexFieldInfo != null)
            {
                if (!overrideFieldsByFieldInfo(_indexFieldInfo)) return false;
            }
            else if (_searchFieldInfo != null)
            {
                if (!overrideFieldsByFieldInfo(_searchFieldInfo)) return false;
            }
            else
            {
                Context.Logger.Error($"{_tableInfo.MainTableName} 海外增量bytes生成失败，表格没有配置索引，无法定位数据");
                return false;
            }

            return true;
        }

        private bool WriteBody(bool isColloctString, ref Dictionary<string, uint> dictStringToHash, BinaryWriter writer)
        {
            var bodyStartPos = writer?.BaseStream.Position;
            if (!isColloctString)
            {
                writer.Write(_lineNumber);
            }

            ReadFromCSVAndReadString();

            if (!getBodyFieldsData(_sortFields, isColloctString))
            {
                return false;
            }

            // 删主键检查
            if (Excel.ExcelExporter.OpenDeleteKeyCheck && _oldData != null)
            {
                if (!checkKeyCantDelete()) return false;
            }

            //检查Id是否重复
            Dictionary<int, HashSet<string>> indexCheckDictionary = new Dictionary<int, HashSet<string>>();

            var parserArray = new Parser[_fieldFieldsToExport.Count];
            for (int i = 0; i < _fieldFieldsToExport.Count; i++)
            {
                parserArray[i] = ParserUtil.GetParser(_fieldFieldsToExport[i].FieldTypeName, true);
            }
            //包含中文但是未进行本地化的字段
            HashSet<string> chineseWarningField = new HashSet<string>();
            //本地化字段为0
            HashSet<string> localizationStrZero = new HashSet<string>();
            for (int iRow = 0, sz = _sortFields.Count; iRow < sz; iRow++)
            {
                var fieldStrs = _sortFields[iRow];
                for (int iCol = 0, len = fieldStrs.Count; iCol < len; iCol++)
                {
                    var curField = _fieldFieldsToExport[iCol];
                    var valueStr = fieldStrs[iCol];
                    //空白部分使用占位符
                    if (string.IsNullOrEmpty(valueStr))
                    {
                        valueStr = curField.DefaultValue;
                    }
                    if (curField.IndexType != TableFieldInfo.EIndexType.None)
                    {
                        if (!indexCheckDictionary.ContainsKey(iCol))
                        {
                            indexCheckDictionary.Add(iCol, new HashSet<string>());
                        }
                        if (indexCheckDictionary[iCol].Contains(valueStr))
                        {
                            Context.Logger.Error($"键值重复！表名为{_tableInfo.MainTableName} 字段名为{curField.FieldName} 键值为{valueStr}");
                            return false;
                        }
                        indexCheckDictionary[iCol].Add(valueStr);
                    }

                    if (_enableCheckExcel)
                    {
                        if (!curField.NeedLocal && !Excel.ExcelExporter.IsIgnoreLocalization(_tableInfo.MainTableName, curField.FieldName))
                        {
                            if (!string.IsNullOrEmpty(valueStr) && valueStr.HasChinese())
                            {
                                chineseWarningField.Add($"{curField.FieldName}");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(valueStr) && valueStr.Equals("0"))
                            {
                                localizationStrZero.Add($"{curField.FieldName}");
                            }
                        }
                    }

                    //找到parser并且写入
                    var fieldTypeName = curField.FieldTypeName;
                    var parser = parserArray[iCol];
                    bool succ = true;
                    if (isColloctString)
                    {
                        succ = parser.WriteHashFromString(isColloctString, ref dictStringToHash, writer, valueStr, fieldTypeName);
                    }
                    else
                    {
                        if (curField.NeedLocal)
                        {
                            succ = parser.WriteLocalFromString(writer, valueStr);
                        }
                        else
                        {
                            succ = parser.WriteHashFromString(isColloctString, ref dictStringToHash, writer, valueStr, fieldTypeName);
                        }
                    }
                    if (!succ)
                    {
                        Context.Logger.Error($"{_tableInfo.MainTableName} write error:{valueStr} parsed by {fieldTypeName} failed!! row:{iRow + 2}|col【{iCol}】|FieldName:{curField.FieldName}");
                        return false;
                    }
                }
            }

            if (chineseWarningField.Count > 0)
            {
                Context.Logger.Warning($"{_tableInfo.MainTableName}  字段{chineseWarningField.ToList().ConverToString()}中包含中文，但是其字段为非本地化字段！！请检查！！！");
            }

            if (localizationStrZero.Count > 0)
            {
                Context.Logger.Warning($"EExportType:{_tableType} MainTableName:{_tableInfo.MainTableName}  本地化字段{localizationStrZero.ToList().ConverToString()}为0！请检查！！！");
            }

            if (!isColloctString)
            {
                var curPos = writer.BaseStream.Position;
                writer.Seek((int)bodyStartPos, SeekOrigin.Begin);
                writer.Write(_lineNumber);

                writer.Seek((int)curPos, SeekOrigin.Begin);
            }

            return true;
        }

        public static uint GetPosID(TableFieldInfo tableFieldInfo, EExportType exportTableType)
        {
            if (tableFieldInfo == null)
            {
                Context.Logger.Error("tableFieldInfo is null");
                return 0x3F3F3F3F;
            }
            switch (exportTableType)
            {
                case EExportType.Client:
                    {
                        if (!tableFieldInfo.ForClient || tableFieldInfo.ClientPosID < 0)
                        {
                            Context.Logger.Error(
                                $"error ClientPosID[{tableFieldInfo.ClientPosID}] is field:{tableFieldInfo.FieldName}"
                                );
                        }
                        return (uint)(tableFieldInfo.ClientPosID);
                    }
                case EExportType.Server:
                    {
                        if (!tableFieldInfo.ForServer || tableFieldInfo.ServerPosID < 0)
                        {
                            Context.Logger.Error(
                                $"error ServerPosID[{tableFieldInfo.ServerPosID}] is field:{tableFieldInfo.FieldName}"
                                );
                        }
                        return (uint)(tableFieldInfo.ServerPosID);
                    }
                case EExportType.Editor:
                    {
                        if (!tableFieldInfo.ForEditor || tableFieldInfo.EditorPosID < 0)
                        {
                            Context.Logger.Error(
                                $"error EditorPosID[{tableFieldInfo.EditorPosID}] is field:{tableFieldInfo.FieldName}"
                                );
                        }
                        return (uint)(tableFieldInfo.EditorPosID);
                    }
                default:
                    {
                        Context.Logger.Error($"error EExportType is{exportTableType}");
                        break;
                    }
            }
            return 0x3F3F3F3F;
        }

        #endregion WriteFile


        public static string GetAbsolutePath(string csvLocation, MGameArea gameArea, out string overseaCsvPath)
        {
            //临时处理 json统一修改后优化
            string excelExtension = Path.GetExtension(csvLocation);
            string csvName = csvLocation.Replace(excelExtension, ".csv");

            var csvPath = PathEx.MakePathStandard(Path.Combine(PathEx.MakePathStandard(CSVUtil.CSVPath), csvName));
            if (gameArea != MGameArea.China)
            {
                overseaCsvPath = Util.GetChannelConfigPathEx(gameArea, $"Table/CSV/{csvLocation}");
                if (!File.Exists(overseaCsvPath))
                {
                    overseaCsvPath = string.Empty;
                }
            }
            else
            {
                overseaCsvPath = string.Empty;
            }

            if (!string.IsNullOrEmpty(overseaCsvPath))
            {
                if (Excel.ExcelExporter.IsSpecialOverrideCSV(Path.GetFileNameWithoutExtension(overseaCsvPath)))
                {
                    csvPath = overseaCsvPath;
                    overseaCsvPath = string.Empty;
                }
            }

            return csvPath;
        }

        public static string GetAreaPath(string csvLocation, MGameArea gameArea)
        {
            string excelExtension = Path.GetExtension(csvLocation);
            string csvName = csvLocation.Replace(excelExtension, ".csv");
            if(gameArea == MGameArea.China)
            {
                return PathEx.MakePathStandard(Path.Combine(PathEx.MakePathStandard(CSVUtil.CSVPath), csvName));
            }
            else
            {
                var overseaCsvPath = Util.GetChannelConfigPathEx(gameArea, $"Table/CSV/{csvLocation}");
                if(File.Exists(overseaCsvPath))
                {
                    return overseaCsvPath;
                }
                return null;
            }
        }
    }
}