import { Component, OnInit,Output,EventEmitter } from '@angular/core';
import * as $ from 'jquery';
import * as lodash from 'lodash';
@Component({
  selector: 'app-page-sidebar',
  templateUrl: './page-sidebar.component.html',
  // styleUrls: ['./page-sidebar.component.css']
})
export class PageSidebarComponent implements OnInit {


  index_parrent_menu=0;
  index_sub_menu=0;
  @Output() Page_Information = new EventEmitter<any>();
  Page_Info={
    parent:"Master",
    children:""
  }

  /**
   * MENU COMPONENTS DEFINITION
   */

   Menu = [
     
     // Catalogue Module
     {
      parent_name:"Catalogue",
      icon:"icon-books",
      route_parent:"/home/catalogue/",
      display_child:false,
      childs:[
       
        {name:"Warehouse",route_child:"ware-house"},
        {name:"Port Index",route_child:"port-index"},
        {name:"Partner Data",route_child:"partner-data"},       
        {name:"Commodity",route_child:"commodity"},              
        {name:"Stage Management",route_child:"stage-management"},
        {name:"Unit",route_child:"unit"},
        {name:"Location",route_child:"location"},
        {name:"Charge",route_child:"charge"},
      ]
    },
     //Operation Module 
     {
      parent_name:"Operation",
      icon:"icon-cogs",
      route_parent:"/home/operation/",
      display_child:false,
      childs:[
        {name:"Job Management",route_child:"job-management"},
        {name:"Assignment",route_child:"assigment"},
        {name:"Trucking Assigment",route_child:"trucking-assigment"},        
      ]
    },
    // Documentation Module
    {
      parent_name:"Documentation",
      icon:"icon-file-text2",
      route_parent:"/home/documentation/",
      display_child:false,
      childs:[
        {name:"Inland Trucking",route_child:"inland-trucking"},
        {name:"Air Export",route_child:"air-export"},
        {name:"Air Import",route_child:"air-import"},
        {name:"Sea Consol Export",route_child:"sea-consol-export"},
        {name:"Sea Consol Import",route_child:"sea-consol-import"},
        {name:"Sea FCL Export",route_child:"sea-fcl-export"},
        {name:"Sea FCL Import",route_child:"sea-fcl-import"},
        {name:"Sea LCL Export",route_child:"sea-lcl-export"},
        {name:"Sea LCL Import",route_child:"sea-lcl-import"},        
      ]
    },
    //Accouting Module
    {
      parent_name:"Accounting",
      icon:"icon-calculator",
      route_parent:"/home/accounting/",
      display_child:false,
      childs:[
        {name:"Account Receivable Payable",route_child:"account-receivable-payable"},
        {name:"Advance Payment",route_child:"advance-payment"},
        {name:"Settlement Payment",route_child:"settlement-payment"},
        {name:"Statement of Account",route_child:"statement-of-account"}       
      ]
    },
    //System Module
    {
      parent_name:"System",
      icon:"icon-database",
      route_parent:"/home/system/",
      display_child:false,
      childs:[
        {name:"User Management",route_child:"user-management"},
        {name:"Group",route_child:"group"},
        {name:"Role",route_child:"role"},
        {name:"Permission",route_child:"permission"},
        {name:"Department",route_child:"department"},
        {name:"Company Informations",route_child:"company-info"}                      
      ]
    },
    //Tool-setting Module
    {
      parent_name:"Tool - Setting",
      icon:"icon-wrench",
      route_parent:"/home/tool/",
      display_child:false,
      childs:[       
        {name:"ID Definition",route_child:"id-definition"},
        {name:"Tariff",route_child:"tariff"},
        {name:"Ecus Connection",route_child:"ecus-connection"},
        {name:"KPI",route_child:"kpi"},
        {name:"Supplier",route_child:"supplier"},
       
      ]
    },
    //Report Module
    {
      parent_name:"Report",
      icon:"icon-stats-bars",
      route_parent:"/home/report/",
      display_child:false,
      childs:[
        {name:"P/L Report",route_child:"pl-report"},
        {name:"Performance Report",route_child:"performance-report"},
        {name:"Shipment Overview",route_child:"shipment-overview"},
      ]
    }    
   ]
  

  constructor() { }

  ngOnInit() {
  }

  open_sub_menu(index,parent){  
    this.index_parrent_menu=index;
    var parrent = $(parent);
    if(parrent.hasClass('m-menu__item--open')){
      parrent.removeClass('m-menu__item--open');
    }else{
      parrent.addClass('m-menu__item--open')
    }
     this.Menu[index].display_child = !this.Menu[index].display_child;
    // this.Page_Info.parent= this.Menu[index].parent_name;    
  }

  sub_menu_click(sub_menu_name){    
    for(var i=0;i<this.Menu.length;i++){
      for(var j=0;j<this.Menu[i].childs.length;j++){
        if(this.Menu[i].childs[j].name==sub_menu_name){
          this.Page_Info.parent = this.Menu[i].parent_name;
          this.Page_Info.children = this.Menu[i].childs[j].name;
          this.Page_Information.emit(this.Page_Info);
          console.log(this.Page_Info)
          break;
        }
      }
    }
  }

}
