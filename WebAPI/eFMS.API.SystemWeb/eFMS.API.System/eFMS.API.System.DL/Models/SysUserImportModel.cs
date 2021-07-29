
namespace eFMS.API.System.DL.Models
{
    public class SysUserImportModel: SysEmployeeModel
    {
        public string Username { get; set; }
        public bool UsernameValid { get; set; }
        public string UserType { get; set; }
        public string WorkingStatus { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public bool EmployeeNameEnValid { get; set; }
        public bool EmployeeNameVnValid { get; set; }
        public bool UserTypeValid { get; set; }
        public bool WorkingStatusValid { get; set; }
        public bool StatusValid { get; set; }
        public bool StaffCodeValid { get; set; }
        public bool TitleValid { get; set; }
        public bool EmailValid { get; set; }


        public bool IsValid { get; set; }
        public string UserRole { get; set; }
        public bool UserRoleValid { get; set; }

    }
}
