export class OPEXConstants {
    public static readonly S_AN_CODE = '10';
    public static readonly S_DO_CODE = '12';
    public static readonly S_AL_CODE = '13';
    public static readonly S_POD_CODE = '14';
    public static readonly S_PA_CODE = '16';
    public static readonly U_ATA_CODE = '20';
    public static readonly U_ATD_CODE = '21';
    public static readonly U_ICT_CODE = '25';
    public static readonly S_HB_CODE = '34';
    public static readonly S_HAWB_CODE = '35';
    public static readonly U_POD_CODE = '36';
    public static readonly S_INV_CODE = '37';


    public static readonly S_AN_DES = 'S_AN';
    public static readonly S_DO_DES = 'S_DO';
    public static readonly S_AL_DES = 'S_AL';
    public static readonly S_POD_DES = 'S_POD';
    public static readonly S_PA_DES = 'S_PA';
    public static readonly U_ATA_DES = 'U_ATA';
    public static readonly U_ATD_DES = 'U_ATD';
    public static readonly U_ICT_DES = 'U_ICT';
    public static readonly S_HB_DES = 'S_HB';
    public static readonly S_HAWB_DES = 'S_HAWB';
    public static readonly U_POD_DES = 'U_POD';
    public static readonly S_INV_DES = 'S_INV';


    public static readonly ServiceTypeMapping: CommonInterface.INg2Select[] = <CommonInterface.INg2Select[]>[
        { id: 'All', text: 'All' },
        { text: OPEXConstants.S_AN_DES , id: OPEXConstants.S_AN_CODE} ,
        { text: OPEXConstants.S_DO_DES , id: OPEXConstants.S_DO_CODE} ,
        { text: OPEXConstants.S_AL_DES , id: OPEXConstants.S_AL_CODE} ,
        { text: OPEXConstants.S_POD_DES, id: OPEXConstants.S_POD_CODE},
        { text: OPEXConstants.S_PA_DES , id: OPEXConstants.S_PA_CODE},
        { text: OPEXConstants.U_ATA_DES, id: OPEXConstants.U_ATA_CODE},
        { text: OPEXConstants.U_ATD_DES, id: OPEXConstants.U_ATD_CODE},
        { text: OPEXConstants.U_ICT_DES, id: OPEXConstants.U_ICT_CODE},
        { text: OPEXConstants.S_HB_DES , id: OPEXConstants.S_HB_CODE},
        { text: OPEXConstants.S_HAWB_DES, id: OPEXConstants.S_HAWB_CODE},
        { text: OPEXConstants.U_POD_DES, id: OPEXConstants.U_POD_CODE},
        { text: OPEXConstants.S_INV_DES, id: OPEXConstants.S_INV_CODE},
    ];
}