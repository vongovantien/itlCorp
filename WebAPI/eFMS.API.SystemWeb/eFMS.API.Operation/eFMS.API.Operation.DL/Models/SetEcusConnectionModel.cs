using eFMS.API.Common.Models;
using eFMS.API.Operation.Service.Models;

namespace eFMS.API.Operation.DL.Models
{
    public class SetEcusConnectionModel : SetEcusconnection
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string UserCreatedName { get; set; }
        public string UserModifiedName { get; set; }

        public PermissionAllowBase Permission { get; set; }
    }
}
