export const language = {
  Menu : [  
    {
      parent_name: "Danh Mục",
      icon: "icon-books",
      route_parent: "/home/catalogue/",
      display_child: false,
      childs: [

        { name: "Kho", route_child: "ware-house" },
        { name: "Cửa Khẩu", route_child: "port-index" },
        { name: "Port Index Import", route_child: "port-index-import" },
        { name: "Dữ Liệu Đối Tác", route_child: "partner-data" },
        { name: "Hàng Hóa", route_child: "commodity" },
        { name: "Quản Lí Stage", route_child: "stage-management" },
        { name: "Đơn Vị", route_child: "unit" },
        { name: "Địa Điểm", route_child: "location" },
        { name: "Phí", route_child: "charge" },
        { name: "Tiền Tệ", route_child: "currency" }
      ]
    },
   
    {
      parent_name: "Hoạt Động",
      icon: "icon-cogs",
      route_parent: "/home/operation/",
      display_child: false,
      childs: [
        { name: "Quản Lý Job", route_child: "job-management" },
        { name: "Điều Phối", route_child: "assigment" },
        { name: "Điều Phối Xe", route_child: "trucking-assigment" }
      ]
    },
   
    {
      parent_name: "Chứng Từ",
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
        { name: "Sea LCL Import", route_child: "sea-lcl-import" }
      ]
    },
   
    {
      parent_name: "Kế Toán",
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
    
    {
      parent_name: "Hệ Thống",
      icon: "icon-database",
      route_parent: "/home/system/",
      display_child: false,
      childs: [
        { name: "Quản Lý Người Dùng", route_child: "user-management" },
        { name: "Nhóm Người Dùng", route_child: "group" },
        { name: "Vai Trò", route_child: "role" },
        { name: "Quyền", route_child: "permission" },
        { name: "Bộ Phận", route_child: "department" },
        { name: "Thông Tin Công Ty", route_child: "company-info" }
      ]
    },
   
    {
      parent_name: "Công Cụ - Cài Đặt",
      icon: "icon-wrench",
      route_parent: "/home/tool/",
      display_child: false,
      childs: [
        { name: "Định Nghĩa ID", route_child: "id-definition" },
        { name: "Thuế Quan", route_child: "tariff" },
        { name: "Tỉ Giá", route_child: "exchange-rate" },
        { name: "Ecus Connection", route_child: "ecus-connection" },
        { name: "KPI", route_child: "kpi" },
        { name: "Nhà Cung Cấp", route_child: "supplier" },
        { name: "Truy Xuất Lịch Sử", route_child: "log-viewer" }

      ]
    },
    
    {
      parent_name: "Báo Cáo",
      icon: "icon-stats-bars",
      route_parent: "/home/report/",
      display_child: false,
      childs: [
        { name: "Báo Cáo P/L", route_child: "pl-report" },
        { name: "Báo Cáo Hiệu Suất", route_child: "performance-report" },
        { name: "Tổng Quan Lô Hàng", route_child: "shipment-overview" }
      ]
    },
    
    {
      parent_name: "Designs Zone",
      icon: "icon-format_paint",
      route_parent: "/home/designs-zone/",
      display_child: false,
      childs: [
        { name: "Form", route_child: "form" },
        { name: "Table", route_child: "table" }
      ]
    }
  ]
}
