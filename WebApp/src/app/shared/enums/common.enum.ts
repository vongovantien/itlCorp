export namespace CommonEnum {
    export enum UnitType {
        'CONTAINER' = "Container",
        'PACKAGE' = "Package",
        'WEIGHT' = "Weight",
        'LENGTH' = "Length",
        'VOLUMN' = "Volumn",
    }

    export enum PartnerGroupEnum {
        ALL = 0,
        AGENT = 1,
        CONSIGNEE = 2,
        CUSTOMER = 3,
        PAYMENTOBJECT = 4,
        PETROLSTATION = 5,
        SHIPPER = 6,
        SHIPPINGLINE = 7,
        SUPPLIER = 8,
        SUPPLIERMATERIAL = 9,
        CARRIER = 10,
        AIRSHIPSUP = 11
    }

    export enum TransactionTypeEnum {
        InlandTrucking = 1,
        AirExport = 2,
        AirImport = 3,
        SeaConsolExport = 4,
        SeaConsolImport = 5,
        SeaFCLExport = 6,
        SeaFCLImport = 7,
        SeaLCLExport = 8,
        SeaLCLImport = 9
    }

    export enum ButtonType {
        add = "add",
        edit = "edit",
        delete = "delete",
        import = "import",
        export = "export",
        save = "save",
        cancel = "cancel",
        reset = "reset",
        detail = "detail",
        closeModal = "closeModal",
        search = "search",
        searchMultiple = 'searchMultiple'
    }

    export enum TypeSearch {
        intab = "intab",
        outtab = "outtab"
    }

    export enum SurchargeTypeEnum {
        BUYING_RATE = "BUY",
        SELLING_RATE = "SELL",
        OBH = "OBH",
        CHARGE = "CHARGE"
    }

    export enum TableActions {
        EDIT = 'EDIT',
        DELETE = 'DELETE'
    }

}

