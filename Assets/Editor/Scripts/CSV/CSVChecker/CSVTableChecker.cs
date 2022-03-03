using System;
using MoonCommonLib;
using ToolLib.Excel.Attribute;
using ToolLib.UniLua.Util;
using ToolLib.CSV;
using System.Collections.Generic;
using Serializer;

namespace ToolLib.Excel
{
    [CheckTarget(CheckTarget.Table)]
    public abstract class CSVTableChecker : BaseChecker
    {
        protected CSVReader _csv;
        protected TableLocation _location;
        public override string LocationInfo => $"【 位于 CSV {TableInfo.MainTableName} 中 】";

        public override void SetCommonArgs(params object[] args)
        {
            TableInfo = args[0] as TableInfo;
            _location = args[1] as TableLocation;
            _csv = args[2] as CSVReader;
        }

        /// <summary>
        /// 获取表头数据
        /// </summary>
        /// <returns></returns>
        List<string> GetHeadRowDatas()
        {
            if (_csv.TableData.Count == 0)
            {
                Context.Logger.Error($"{TableInfo.MainTableName}对应表数据为空 !!");
                return null;
            }
            return _csv.TableData[0];
        }

        /// <summary>
        /// 确认字段在表数据中是否存在
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected bool CheckFieldExist(string fieldName)
        {
            List<string> headRowDatas = GetHeadRowDatas();
            if (headRowDatas == null)
                return false;

            foreach (var cellData in headRowDatas)
            {
                if (cellData == fieldName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取字段在表中的列索引
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        protected int GetFieldColIndex(string fieldName)
        {
            List<string> headRowDatas = GetHeadRowDatas();
            if (headRowDatas == null)
                return -1
                    ;
            for (int i = 0; i < headRowDatas.Count; i++)
            {
                if (headRowDatas[i] == fieldName)
                    return i;
            }
            return -1;
        }
    }

    [CheckerArgsTip(0, "字段名", "")]
    [CheckerName("CSV中字段存在性检查")]
    [CheckerDescriptionTip("检查Sheet必须存在的字段:{0}")]
    [CheckMustSingle]
    public class CSVFieldExistChecker : CSVTableChecker
    {
        private bool _usedByOther;

        public override void SetCommonArgs(params object[] args)
        {
            base.SetCommonArgs(args);
            if (!bool.TryParse(args[3].ToString(), out _usedByOther)) _usedByOther = false;
        }

        public override bool Aviliable()
        {
            return true;
        }

        public override bool IsPass()
        {
            if (Args.Length != 1)
            {
                Context.Logger.Error($"参数数量错误 : {Args.ConverToString()} 应该为 1 !!");
                return false;
            }
            string requiredFieldName = Args[0];
            if (CheckFieldExist(requiredFieldName))
                return true;

            if (!_usedByOther)
            {
                Context.Logger.Error($"字段 {requiredFieldName} 在表{TableInfo.MainTableName}中无法找到 !!");
            }
            return false;
        }

        public override string OnError()
        {
            return "表中缺失一些必须的字段";
        }
    }

    [CheckerArgsTip(0, "字段名", "")]
    [CheckerArgsTip(1, "需求的值", "")]
    [CheckerName("CSV中特定字段值存在性检查")]
    [CheckerDescriptionTip("检查Workbook中字段{0}必须的值{1}是否存在")]
    public class CSVRequiredValueChecker : CSVTableChecker
    {
        private bool _usedByOther;

        public override void SetCommonArgs(params object[] args)
        {
            base.SetCommonArgs(args);
            if (!bool.TryParse(args[3].ToString(), out _usedByOther)) _usedByOther = false;
        }

        public override bool Aviliable()
        {
            return true;
        }

        public override bool IsPass()
        {
            if (Args.Length != 2)
            {
                Context.Logger.Error($"参数数量错误 : {Args.ConverToString()} 应该为 2 !!");
                return false;
            }
            var refFieldName = Args[0];
            var requiredValueStr = Args[1];

            if (string.IsNullOrWhiteSpace(requiredValueStr))
                return true;
            var targetFieldInfo = TableInfo.Fields.FindFirst(f => f.FieldName == refFieldName);
            if (targetFieldInfo == null)
            {
                Context.Logger.Error($"{TableInfo.MainTableName}的Config中未找到必须字段{refFieldName}！");
                return false;
            }
            if (!CheckFieldExist(refFieldName))
            {
                Context.Logger.Error($"{TableInfo.MainTableName}中未找到必须字段{refFieldName}！");
                return false;
            }
            var targetColIndex = GetFieldColIndex(refFieldName);
            if (targetColIndex < 0)
            {
                Context.Logger.Error($"{TableInfo.MainTableName}中无法找到目标字段 {refFieldName}");
                return false;
            }
            var parser = ParserUtil.GetParser(targetFieldInfo.FieldTypeName);
            if (!parser.Parse(requiredValueStr, out var requiredValue))
            {
                Context.Logger.Error($" 需要的数值 {requiredValueStr} 无法解析为 {targetFieldInfo.FieldTypeName} !!");
                return false;
            }
            for (int i = CSVParser.START_LINE; i < _csv.TableData.Count; i++)
            {
                var rowData = _csv.TableData[i];
                if (rowData == null)
                {
                    break;
                }
                var refValueStr = rowData[targetColIndex];
                if (!parser.Parse(refValueStr, out var target))
                {
                    Context.Logger.Error($"引用数值 {refValueStr} 无法解析为 {targetFieldInfo.FieldTypeName} !!");
                    return false;
                }
                if (requiredValue.ToString() == target.ToString())
                {
                    return true;
                }
            }

            if (!_usedByOther)
            {
                Context.Logger.Error($"表 {TableInfo.MainTableName} 在字段 {refFieldName}中无法找到 {requiredValueStr} !!");
            }
            return false;
        }

        public override string OnError()
        {
            return "未在目标WorkBook中找到值";
        }
    }
    
}