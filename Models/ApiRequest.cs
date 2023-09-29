using Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities.Enums;

namespace Models
{
    public class ApiRequest
    {
        public RequestType RequestType { get; set; } = RequestType.GET;
        public string Url { get; set; }
        public object Data { get; set; }
        public string Token { get; set; }

        public ContentType ContentType { get; set; } = ContentType.Json;
    }
}
