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
        ]
    };

}
