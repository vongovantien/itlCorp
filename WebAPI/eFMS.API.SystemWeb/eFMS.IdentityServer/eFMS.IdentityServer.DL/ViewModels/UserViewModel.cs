using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.System.DL.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        //public short UserGroupId { get; set; }
        public string EmployeeId { get; set; }
        public Guid? WorkPlaceId { get; set; }
        public bool? RefuseEmail { get; set; }
        public Guid? LdapObjectGuid { get; set; }
        public string DepartmentId { get; set; }
        public string EmployeeNameVn { get; set; }
        public string EmployeeNameEn { get; set; }
        public string Position { get; set; }
        public DateTime? Birthday { get; set; }
        public string ExtNo { get; set; }
        public string Tel { get; set; }
        public string HomePhone { get; set; }
        public string HomeAddress { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public string EmpPhotoSize { get; set; }
        public bool Active { get; set; }
    }
}
