namespace eFMS.API.Catalogue.DL.ViewModels
{
    public class SysUserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string StaffCode { get; set; }
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string UserType { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public bool? Active { get; set; }
    }
}
