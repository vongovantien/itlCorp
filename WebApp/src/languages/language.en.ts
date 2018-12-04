


export const language = {
  Menu :[  
    {
      parent_name: "Catalogue",
      icon: "icon-books",
      route_parent: "/home/catalogue/",
      display_child: false,
      childs: [

        { name: "Warehouse", "route_child": "ware-house" },
        { name: "Port Index", "route_child": "port-index" },
        { name: "Partner Data", "route_child": "partner-data" },
        { name: "Commodity", "route_child": "commodity" },
        { name: "Stage Management", "route_child": "stage-management" },
        { name: "Unit", "route_child": "unit" },
        { name: "Location", "route_child": "location" },
        { name: "Charge", "route_child": "charge" },
        { name: "Currency", "route_child": "currency" }
      ]
    },
   
    {
      parent_name: "Operation",
      icon: "icon-cogs",
      route_parent: "/home/operation/",
      display_child: false,
      childs: [
        { name: "Job Management", "route_child": "job-management" },
        { name: "Assignment", "route_child": "assigment" },
        { name: "Trucking Assigment", "route_child": "trucking-assigment" }
      ]
    },
   
    {
      parent_name: "Documentation",
      icon: "icon-file-text2",
      route_parent: "/home/documentation/",
      display_child: false,
      childs: [
        { name: "Inland Trucking", "route_child": "inland-trucking" },
        { name: "Air Export", "route_child": "air-export" },
        { name: "Air Import", "route_child": "air-import" },
        { name: "Sea Consol Export", "route_child": "sea-consol-export" },
        { name: "Sea Consol Import", "route_child": "sea-consol-import" },
        { name: "Sea FCL Export", "route_child": "sea-fcl-export" },
        { name: "Sea FCL Import", "route_child": "sea-fcl-import" },
        { name: "Sea LCL Export", "route_child": "sea-lcl-export" },
        { name: "Sea LCL Import", "route_child": "sea-lcl-import" }
      ]
    },
   
    {
      parent_name: "Accounting",
      icon: "icon-calculator",
      route_parent: "/home/accounting/",
      display_child: false,
      childs: [
        { name: "Account Receivable Payable", "route_child": "account-receivable-payable" },
        { name: "Advance Payment", "route_child": "advance-payment" },
        { name: "Settlement Payment", "route_child": "settlement-payment" },
        { name: "Statement of Account", "route_child": "statement-of-account" }
      ]
    },
    
    {
      parent_name: "System",
      icon: "icon-database",
      route_parent: "/home/system/",
      display_child: false,
      childs: [
        { name: "User Management", "route_child": "user-management" },
        { name: "Group", "route_child": "group" },
        { name: "Role", "route_child": "role" },
        { name: "Permission", "route_child": "permission" },
        { name: "Department", "route_child": "department" },
        { name: "Company Informations", "route_child": "company-info" }
      ]
    },
   
    {
      parent_name: "Tool - Setting",
      icon: "icon-wrench",
      route_parent: "/home/tool/",
      display_child: false,
      childs: [
        { name: "ID Definition", "route_child": "id-definition" },
        { name: "Tariff", "route_child": "tariff" },
        { name: "Exchange Rate", "route_child": "exchange-rate" },
        { name: "Ecus Connection", "route_child": "ecus-connection" },
        { name: "KPI", "route_child": "kpi" },
        { name: "Supplier", "route_child": "supplier" }

      ]
    },
    
    {
      parent_name: "Report",
      icon: "icon-stats-bars",
      route_parent: "/home/report/",
      display_child: false,
      childs: [
        { name: "P/L Report", "route_child": "pl-report" },
        { name: "Performance Report", "route_child": "performance-report" },
        { name: "Shipment Overview", "route_child": "shipment-overview" }
      ]
    },
    
    {
      parent_name: "Designs Zone",
      icon: "icon-format_paint",
      route_parent: "/home/designs-zone/",
      display_child: false,
      childs: [
        { name: "Form", "route_child": "form" },
        { name: "Table", "route_child": "table" }
      ]
    }
  ]
}