namespace XTools
{
    public enum ELanguage
    {
        None = 0,
        CPlusPlus = 1 << 0,
        CSharp = 1 << 1,
        Lua = 1 << 2
    }
    public class TableLocation
    {
        public string ExcelPath;
        public string[] SheetName;
    }

    public class CheckerInfo
    {
        public bool Enable;
        public string CheckerType;
        public string[] CheckArgs;
    }

    public class TableFieldInfo
    {
        public enum EIndexType
        {
            None,
            Bin,
            Search
        }
        public string FieldName;
        public string FieldTypeName;
        public string DefaultValue;
        public bool ForClient;
        public bool ForServer;
        public bool ForEditor;
        public int ClientPosID = -1;
        public int ServerPosID = -1;
        public int EditorPosID = -1;
        public long ClientPosTimeStamp;
        public long ServerPosTimeStamp;
        public long EditorPosTimeStamp;
        public EIndexType IndexType;
        public CheckerInfo[] CheckInfos;
        public bool NeedLocal = false;
    }

    public class TableInfo
    {
        public bool isNeedPre;
        public bool isNeedPost;
        public uint TableCodeTarget;
        public uint TableBytesTarget;
        public string MainTableName;
        public TableLocation[] TableLocations;
        public TableFieldInfo[] Fields;
        public TableInfo[] Children;
        public CheckerInfo[] CheckInfos;
        public ELanguage ClientCacheTableFlags;
        public ELanguage ServerCacheTableFlags;

    }
}