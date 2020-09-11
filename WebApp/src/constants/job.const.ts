import { formatDate } from "@angular/common";

export class JobConstants {
    public static readonly Overdued: string = "Overdued";
    public static readonly Processing: string = "Processing";
    public static readonly InSchedule: string = "InSchedule";
    public static readonly Pending = "Pending";
    public static readonly Finish: string = "Finish";
    public static readonly Canceled: string = "Canceled";
    public static readonly Warning: string = "Warning";

    public static COMMON_DATA = {
        FREIGHTTERMS: <CommonInterface.INg2Select[]>[
            {
                id: "Collect",
                text: "Collect"
            },
            {
                id: "Prepaid",
                text: "Prepaid"
            }
        ],
        SHIPMENTTYPES: <CommonInterface.INg2Select[]>[
            {
                id: "Freehand",
                text: "Freehand"
            },
            {
                id: "Nominated",
                text: "Nominated"
            }
        ],
        BILLOFLADINGS: <CommonInterface.INg2Select[]>[
            {
                id: "Copy",
                text: "Copy"
            },
            {
                id: "Original",
                text: "Original"
            },
            {
                id: "Sea Waybill",
                text: "Sea Waybill"
            },
            {
                id: "Surrendered",
                text: "Surrendered"
            }
        ],
        SERVICETYPES: <CommonInterface.INg2Select[]>[
            {
                id: "FCL/FCL",
                text: "FCL/FCL"
            },
            {
                id: "LCL/LCL",
                text: "LCL/LCL"
            },
            {
                id: "FCL/LCL",
                text: "FCL/LCL"
            },
            {
                id: "CY/CFS",
                text: "CY/CFS"
            },
            {
                id: "CY/CY",
                text: "CY/CY"
            },
            {
                id: "CFS/CY",
                text: "CFS/CY"
            },
            {
                id: "CFS/CFS",
                text: "CFS/CFS"
            },
            {
                id: "CY/DR",
                text: "CY/DR"
            },
            {
                id: "DR/CY",
                text: "DR/CY"
            },
            {
                id: "DR/DR",
                text: "DR/DR"
            },
            {
                id: "DR/CFS",
                text: "DR/CFS"
            },
            {
                id: "CFS/DR",
                text: "CFS/DR"
            }
        ],
        TYPEOFMOVES: <CommonInterface.INg2Select[]>[
            {
                id: "FCL/FCL-CY/CY",
                text: "FCL/FCL-CY/CY"
            },
            {
                id: "LCL/LCL-CY/CY",
                text: "LCL/LCL-CY/CY"
            },
            {
                id: "LCL/LCL-CFS/CFS",
                text: "LCL/LCL-CFS/CFS"
            },
            {
                id: "LCL/FCL-CFS/CY",
                text: "LCL/FCL-CFS/CY"
            },
            {
                id: "FCL/LCL-CY/CFS",
                text: "FCL/LCL-CY/CFS"
            }
        ],
        BLNUMBERS: <CommonInterface.INg2Select[]>[
            { id: '0', text: 'Zero (0)' },
            { id: 1, text: 'One (1)' },
            { id: 2, text: 'Two (2)' },
            { id: 3, text: 'Three (3)' }
        ],
        RCLASS: <CommonInterface.INg2Select[]>[
            { id: 'M', text: 'M' },
            { id: 'N', text: 'N' },
            { id: 'Q', text: 'Q' }
        ],
        WT: <CommonInterface.INg2Select[]>[
            { id: 'PP', text: 'PP' },
            { id: 'CLL', text: 'CLL' }
        ],
        SHIPMENTMODES: <CommonInterface.INg2Select[]>[
            { id: "Internal", text: "Internal" },
            { id: "External", text: "External" }
        ],
        SERVICEMODES: <CommonInterface.INg2Select[]>[
            { id: "Export", text: "Export" },
            { id: "Import", text: "Import" }
        ],
        PRODUCTSERVICE: <CommonInterface.INg2Select[]>[
            { id: "SeaFCL", text: "Sea FCL" },
            { id: "SeaLCL", text: "Sea LCL" },
            { id: "Air", text: "Air" },
            { id: "Sea", text: "Sea" },
            { id: "Trucking", text: "Trucking" },
            { id: "Crossborder", text: "Cross border" },
            { id: "Warehouse", text: "Warehouse" },
            { id: "Railway", text: "Railway" },
            { id: "Express", text: "Express" },
            { id: "BondedWarehouse", text: "Bonded Warehouse" },
            { id: "Other", text: "Other" },
        ],
    };

    public static readonly CONFIG = {
        COMBOGRID_PARTNER: <CommonInterface.IComboGridDisplayField[]>[
            { field: 'shortName', label: 'Name ABBR' },
            { field: 'partnerNameEn', label: 'Name EN' },
            { field: 'taxCode', label: 'Tax Code' },
        ],
        COMBOGRID_PORT: <CommonInterface.IComboGridDisplayField[]>[
            { field: 'code', label: 'Port Code' },
            { field: 'nameEn', label: 'Port Name' },
            { field: 'countryNameEN', label: 'Country' },
        ],
        COMBOGRID_COUNTRY: <CommonInterface.IComboGridDisplayField[]>[
            { field: 'code', label: 'Country Code' },
            { field: 'nameEn', label: 'Name EN' },
        ],
        COMBOGRID_CITY_PROVINCE: <CommonInterface.IComboGridDisplayField[]>[
            { field: 'code', label: 'City Code' },
            { field: 'name_EN', label: 'Name En' },
        ]
    };

    // * 60 days -> current day
    public static readonly DEFAULT_RANGE_DATE_SEARCH = {
        fromDate: formatDate(new Date(new Date().getFullYear(), new Date().getMonth() - 2, new Date().getDate()), 'yyyy-MM-dd', 'en'),
        toDate: formatDate(new Date(), 'yyyy-MM-dd', 'en'),
    };

}
