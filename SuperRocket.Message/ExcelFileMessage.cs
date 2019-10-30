using System;
using System.Collections.Generic;

namespace SuperRocket.Message
{
    public class ExcelFileMessage
    {
        public string FileName { get; set; }

        public string ProcessedSql { get; set; }

        public string ReturnProcessedSql { get; set; }

        public List<Dictionary<string, string>> HeaderList { get; set; }

        public List<string> SheetNameList { get; set; }
    }
}
