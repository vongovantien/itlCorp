using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class UserInfoViewModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public short UserGroupId { get; set; }
        public string UserGroupName { get; set; }
        public int? UserDeparmentId { get; set; }
        public string UserDeparmentName { get; set; }
        public Guid? UserOfficeId { get; set; }
        public string UserOfficeName { get; set; }
        public Guid? UserCompanyId { get; set; }
        public short UserCompanyName { get; set; }
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
    }
}
