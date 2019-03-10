using System;

namespace PHmiTools.Utils.Npg.ExIm {
    [Serializable]
    public class TableData {
        public string[] Columns;

        public object[][] Data;
        public string TableName;
    }
}