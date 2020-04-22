using eFMS.API.Documentation.Service.Models;

namespace eFMS.API.Documentation.DL.Models
{
    public class CsManifestModel: CsManifest
    {
        public string PolName { get; set; }
        public string PodName { get; set; }
    }
}
