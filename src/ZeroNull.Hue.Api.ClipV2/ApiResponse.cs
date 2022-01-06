using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeroNull.Hue.Api.ClipV2
{
    public class ApiResponse<T>
    {
        [JsonProperty("errors")]
        public IEnumerable<Error> Errors { get; set; }

        [JsonProperty("data")]
        public T[] Data { get; set; }

        public ApiResponse()
        {
            Errors = Enumerable.Empty<Error>();
        }
    }
}
