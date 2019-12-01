using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShaderPlayground.Core.Util
{
    internal sealed class JsonTable
    {
        public JsonTableRow Header { get; set; }

        public List<JsonTableRow> Rows { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    internal sealed class JsonTableRow
    {
        public string[] Data { get; set; }
    }
}
