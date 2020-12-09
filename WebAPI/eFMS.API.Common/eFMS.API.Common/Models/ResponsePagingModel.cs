using System.Linq;

namespace eFMS.API.Common.Models
{
    public class ResponsePagingModel<T>
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
        public IQueryable<T> Data { get; set; }
    }
}
