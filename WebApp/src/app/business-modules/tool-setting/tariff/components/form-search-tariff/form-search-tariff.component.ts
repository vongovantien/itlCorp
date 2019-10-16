import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';

@Component({
    selector: 'form-search-tariff',
    templateUrl: './form-search-tariff.component.html',
})
export class TariffFormSearchComponent extends AppForm {

    tariffTypes: CommonInterface.IValueDisplay[] | any = [
        { displayName: 'General', value: null },
        { displayName: 'Customer', value: null },
        { displayName: 'Supplier', value: null },
        { displayName: 'Agent', value: null },
        { displayName: 'All', value: null },
    ];
    selectedTariffType: CommonInterface.IValueDisplay = null;

    status: CommonInterface.IValueDisplay[] | any = [
        { displayName: 'All', value: null },
        { displayName: 'Active', value: true },
        { displayName: 'Inactive', value: false },
    ];
    selectedStatus: CommonInterface.IValueDisplay = null;

    shipmentModes: CommonInterface.IValueDisplay[] | any[] = [
        { displayName: 'All', value: null },
        { displayName: 'Export', value: 'Export' },
        { displayName: 'Import', value: 'Import' },
    ];
    selectedShipmentMode: CommonInterface.IValueDisplay = null;

    dateTypes: CommonInterface.IValueDisplay[] | any[] = [
        { displayName: 'All', value: 'All' },
        { displayName: 'Create Date', value: 'CreateDate' },
        { displayName: 'Effective Date', value: 'EffectiveDate' },
        { displayName: 'Modified Date', value: 'ModifiedDate' },
    ];
    selectedDateType: CommonInterface.IValueDisplay = null;

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'taxCode', label: 'Taxcode' },
            { field: 'shortName', label: 'Name' },
            { field: 'partnerNameVn', label: 'Customer Name' },
        ],
        dataSource: [],
        selectedDisplayFields: ['shortName'],
    };

    configCustomer: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'id', label: 'PartnerID' },
            { field: 'shortName', label: 'Abbr Name' },
            { field: 'partnerNameEn', label: 'Name EN' },
        ],
        dataSource: [],
        selectedDisplayFields: ['shortName'],
    };

    configOffice: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [
            { field: 'id', label: 'Office Code' },
            { field: 'shortName', label: 'Office' },
            { field: 'partnerNameEn', label: 'Company' },
        ],
        dataSource: [],
        selectedDisplayFields: ['shortName'],
    };
    selectedCustomer: CommonInterface.IComboGridData | any = {};

    constructor(
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this.selectedTariffType = this.tariffTypes[4];
        this.selectedStatus = this.status[0];
        this.selectedShipmentMode = this.shipmentModes[0];
        this.selectedDateType = this.dateTypes[0];

        this.getCustomer();
        this.getSuppliers();
        this.getOffice();
    }


    getCustomer() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER)) {
            this.configCustomer = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER) || [];
        } else {
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CUSTOMER)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataPartner: any) => {
                        this.configCustomer.dataSource = dataPartner || [];
                    },
                );
        }
    }

    getSuppliers() {
        this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER)
            .subscribe((res: any) => {
            });
    }

    getOffice() {
        this._systemRepo.getAllOffice()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.configOffice.dataSource = res || [];
                }
            );
    }

    onSelectDataFormInfo(data: any, key: string | any) {
        switch (key) {
            case 'customer':

                break;

            default:
                break;
        }
    }


}
