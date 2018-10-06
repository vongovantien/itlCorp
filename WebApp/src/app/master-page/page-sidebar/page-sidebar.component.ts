import { Component, OnInit, Output, EventEmitter, AfterViewInit, AfterContentChecked, AfterViewChecked } from '@angular/core';
import * as $ from 'jquery';
import * as lodash from 'lodash';
import { Router } from '@angular/router';
@Component({
  selector: 'app-page-sidebar',
  templateUrl: './page-sidebar.component.html',
  // styleUrls: ['./page-sidebar.component.css']
})
export class PageSidebarComponent implements OnInit, AfterViewInit {



  index_parrent_menu = 0;
  index_sub_menu = 0;
  previous_menu_id = null;
  previous_menu_index = null;
  previous_parent: HTMLElement = null;
  previous_children: HTMLElement = null;
  //action_component="";
  @Output() Page_Information = new EventEmitter<any>();
  Page_Component = "";
  Page_Info = {
    parent: "",
    children: ""
  }


  ngAfterViewInit(): void {
    var router = this.router.url.split('/');
    var current_child_route = router[router.length - 1];
    var parentInd = null;
    var childInd = null;
    var child_name = null; 

    for (var i = 0; i < this.Menu.length; i++) {
      for (var j = 0; j < this.Menu[i].childs.length; j++) {

        if (this.Menu[i].childs[j].route_child == current_child_route) {
          this.Page_Info.parent = this.Menu[i].parent_name;
          this.Page_Info.children = this.Menu[i].childs[j].name;         
          this.Page_Information.emit(this.Page_Info);         
          this.open_sub_menu(i);
          this.Menu[i].display_child = true;
          parentInd = i; childInd = j; child_name = this.Menu[i].childs[j].name
        }
      }
    }
    if (parentInd != null && childInd != null) {
      setTimeout(() => {
        this.sub_menu_click(child_name, parentInd, childInd);
      }, 500);
    }

  }


  ngOnInit() {

  }


  /**
   * MENU COMPONENTS DEFINITION
   */

  Menu = [

    // Catalogue Module
    {
      parent_name: "Catalogue",
      icon: "icon-books",
      route_parent: "/home/catalogue/",
      display_child: false,
      childs: [

        { name: "Warehouse", route_child: "ware-house" },
        { name: "Port Index", route_child: "port-index" },
        { name: "Partner Data", route_child: "partner-data" },
        { name: "Commodity", route_child: "commodity" },
        { name: "Stage Management", route_child: "stage-management" },
        { name: "Unit", route_child: "unit" },
        { name: "Location", route_child: "location" },
        { name: "Charge", route_child: "charge" },
      ]
    },
    //Operation Module 
    {
      parent_name: "Operation",
      icon: "icon-cogs",
      route_parent: "/home/operation/",
      display_child: false,
      childs: [
        { name: "Job Management", route_child: "job-management" },
        { name: "Assignment", route_child: "assigment" },
        { name: "Trucking Assigment", route_child: "trucking-assigment" },
      ]
    },
    // Documentation Module
    {
      parent_name: "Documentation",
      icon: "icon-file-text2",
      route_parent: "/home/documentation/",
      display_child: false,
      childs: [
        { name: "Inland Trucking", route_child: "inland-trucking" },
        { name: "Air Export", route_child: "air-export" },
        { name: "Air Import", route_child: "air-import" },
        { name: "Sea Consol Export", route_child: "sea-consol-export" },
        { name: "Sea Consol Import", route_child: "sea-consol-import" },
        { name: "Sea FCL Export", route_child: "sea-fcl-export" },
        { name: "Sea FCL Import", route_child: "sea-fcl-import" },
        { name: "Sea LCL Export", route_child: "sea-lcl-export" },
        { name: "Sea LCL Import", route_child: "sea-lcl-import" },
      ]
    },
    //Accouting Module
    {
      parent_name: "Accounting",
      icon: "icon-calculator",
      route_parent: "/home/accounting/",
      display_child: false,
      childs: [
        { name: "Account Receivable Payable", route_child: "account-receivable-payable" },
        { name: "Advance Payment", route_child: "advance-payment" },
        { name: "Settlement Payment", route_child: "settlement-payment" },
        { name: "Statement of Account", route_child: "statement-of-account" }
      ]
    },
    //System Module
    {
      parent_name: "System",
      icon: "icon-database",
      route_parent: "/home/system/",
      display_child: false,
      childs: [
        { name: "User Management", route_child: "user-management" },
        { name: "Group", route_child: "group" },
        { name: "Role", route_child: "role" },
        { name: "Permission", route_child: "permission" },
        { name: "Department", route_child: "department" },
        { name: "Company Informations", route_child: "company-info" }
      ]
    },
    //Tool-setting Module
    {
      parent_name: "Tool - Setting",
      icon: "icon-wrench",
      route_parent: "/home/tool/",
      display_child: false,
      childs: [
        { name: "ID Definition", route_child: "id-definition" },
        { name: "Tariff", route_child: "tariff" },
        { name: "Ecus Connection", route_child: "ecus-connection" },
        { name: "KPI", route_child: "kpi" },
        { name: "Supplier", route_child: "supplier" },

      ]
    },
    //Report Module
    {
      parent_name: "Report",
      icon: "icon-stats-bars",
      route_parent: "/home/report/",
      display_child: false,
      childs: [
        { name: "P/L Report", route_child: "pl-report" },
        { name: "Performance Report", route_child: "performance-report" },
        { name: "Shipment Overview", route_child: "shipment-overview" },
      ]
    },
    // Designs ZONE , included html template for components
    {
      parent_name: 'Designs Zone',
      icon: 'icon-wrench',
      route_parent: "/home/designs-zone/",
      display_child: false,
      childs: [
        { name: "Form", route_child: "form" },
        { name: "Table", route_child: "table" }
      ]
    }
  ]


  constructor(private router: Router) { }


  open_sub_menu(index) {
    if (this.previous_menu_index != null) {
      this.Menu[this.previous_menu_index].display_child = false;
      var previous_menu = document.getElementById(this.previous_menu_index.toString());     
      if (index != this.previous_menu_index) {
        previous_menu.classList.remove('m-menu__item--open');
        var check_class = previous_menu.classList.contains('m-menu__item--open');       
      }

    }
    this.previous_menu_index = index;


    this.index_parrent_menu = index;
    var parentMenu = document.getElementById(index.toString());   
    if (parentMenu.classList.contains('m-menu__item--open')) {
      parentMenu.classList.remove('m-menu__item--open');
    } else {
      parentMenu.classList.add('m-menu__item--open');
    }

    this.Menu[index].display_child = !this.Menu[index].display_child;
    // this.Page_Info.parent= this.Menu[index].parent_name;    
  }

  sub_menu_click(sub_menu_name, parrent_index, children_index) { 
    var current_parent = document.getElementById(parrent_index.toString());
    var current_children = document.getElementById(parrent_index.toString() + '-' + children_index.toString());

    if (this.previous_children != null) {
      this.previous_children.classList.remove('m-menu__item--active');
      this.previous_parent.classList.remove('m-menu__item--open');
    }

    this.previous_children = current_children;
    this.previous_parent = current_parent;


    current_parent.classList.add('m-menu__item--open');
    current_children.classList.add('m-menu__item--active');


    for (var i = 0; i < this.Menu.length; i++) {
      for (var j = 0; j < this.Menu[i].childs.length; j++) {
        if (this.Menu[i].childs[j].name == sub_menu_name) {
          this.Page_Info.parent = this.Menu[i].parent_name;
          this.Page_Info.children = this.Menu[i].childs[j].name;
          this.Page_Information.emit(this.Page_Info);        
          break;
        }
      }
    }
  }

}
