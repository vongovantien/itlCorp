﻿using System.Collections.Generic;

namespace eFMS.API.System.DL.ViewModels
{
    public class MenuUserModel
    {
        public MenuUserModel()
        {
            SubMenus = new List<MenuUserModel>();
        }
        public string Id { get; set; }
        public string ParentId { get; set; }
        public string NameVn { get; set; }
        public string NameEn { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AssemplyName { get; set; }
        public string Icon { get; set; }
        public int? Sequence { get; set; }
        public string Arguments { get; set; }
        public string Route { get; set; }
        public bool? DisplayChild { get; set; }
        public bool? Display { get; set; }
        public int? OrderNumber { get; set; }
        public List<MenuUserModel> SubMenus { get; set; }
    }
}
