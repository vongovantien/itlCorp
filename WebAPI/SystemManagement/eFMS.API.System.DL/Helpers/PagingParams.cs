using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace SystemManagement.DL.Helpers.PagingPrams
{
    public class PagingParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public string Filter { get; set; } = "{\r\n \"id\": \"\"}";
    }

    public class LinkInfo
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }
    public class PagingHeader
    {
        public PagingHeader(
           int totalItems, int pageNumber, int pageSize, int totalPages)
        {
            this.TotalItems = totalItems;
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.TotalPages = totalPages;
        }

        public int TotalItems { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalPages { get; }

        public string ToJson() => JsonConvert.SerializeObject(this,
                                    new JsonSerializerSettings
                                    {
                                        ContractResolver = new
                    CamelCasePropertyNamesContractResolver()
                                    });

    }
}
