import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { forkJoin } from 'rxjs';
import _uniqBy from 'lodash/uniqBy';
import { Partner } from 'src/app/shared/models';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
@Component({
    selector: 'form-search-tariff',
    templateUrl: './form-search-tariff.component.html',
})
export class TariffFormSearchComponent extends AppForm {

    tariffTypes: CommonInterface.IValueDisplay[] | any;
    selectedTariffType: CommonInterface.IValueDisplay = null;

    status: CommonInterface.IValueDisplay[] | any;
    selectedStatus: CommonInterface.IValueDisplay = null;

    shipmentModes: CommonInterface.IValueDisplay[] | any[];
    selectedShipmentMode: CommonInterface.IValueDisplay = null;

    dateTypes: CommonInterface.IValueDisplay[] | any[];
    selectedDateType: CommonInterface.IValueDisplay = null;

    configCustomer: CommonInterface.IComboGirdConfig | any = {};
    selectedCustomer: Partial<CommonInterface.IComboGridData | any> = {};
    selectedSupplier: Partial<CommonInterface.IComboGridData | any> = {};

    configOffice: CommonInterface.IComboGirdConfig | any = {};
    selectedOffice: Partial<CommonInterface.IComboGridData | any> = {};

    suppliers: Partner[] = [];

    formSearchTariff: FormGroup;

    tariffName: AbstractControl;
    tariffType: AbstractControl;
    tariffShipmentMode: AbstractControl;
    tariffDate: AbstractControl;
    tariffDateType: AbstractControl;
    tariffStatus: AbstractControl;

    constructor(
        protected _dataService: DataService,
        protected _catalogueRepo: CatalogueRepo,
        protected _systemRepo: SystemRepo,
        protected _fb: FormBuilder
    ) {
        super();
        this.requestSearch = this.submitSearch;
        this.requestReset = this.resetForm;

    }

    ngOnInit(): void {
        this.configCustomer = Object.assign(this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'PartnerID' },
                { field: 'shortName', label: 'Abbr Name' },
                { field: 'partnerNameEn', label: 'Name EN' },
            ]
        }, { selectedDisplayFields: ['shortName'], });

        this.configOffice = Object.assign(this.configOffice, {
            displayFields: [
                { field: 'code', label: 'Office Code' },
                { field: 'shortName', label: 'Office' },
                { field: 'companyName', label: 'Company' },
            ],
        }, { selectedDisplayFields: ['shortName'], });

        this.initFormSearch();

        this.getBasicData();
        this.getCustomer();
        this.getSuppliers();
        this.getOffice();
    }

    initFormSearch() {
        this.formSearchTariff = this._fb.group({
            tariffName: [],
            tariffType: [],
            tariffShipmentMode: [],
            tariffDate: [new Date()],
            tariffDateType: [],
            tariffStatus: [],
        });

        this.tariffName = this.formSearchTariff.controls['tariffName'];
        this.tariffType = this.formSearchTariff.controls['tariffType'];
        this.tariffShipmentMode = this.formSearchTariff.controls['tariffShipmentMode'];
        this.tariffDate = this.formSearchTariff.controls['tariffDate'];
        this.tariffDateType = this.formSearchTariff.controls['tariffDateType'];
        this.tariffStatus = this.formSearchTariff.controls['tariffStatus'];
    }

    getBasicData() {
        this.status = [
            { displayName: 'All', value: null },
            { displayName: 'Active', value: true },
            { displayName: 'Inactive', value: false },
        ];

        this.tariffTypes = [
            { displayName: 'General', value: null },
            { displayName: 'Customer', value: null },
            { displayName: 'Supplier', value: null },
            { displayName: 'Agent', value: null },
            { displayName: 'All', value: null },
        ];

        this.dateTypes = [
            { displayName: 'All', value: 'All' },
            { displayName: 'Create Date', value: 'CreateDate' },
            { displayName: 'Effective Date', value: 'EffectiveDate' },
            { displayName: 'Modified Date', value: 'ModifiedDate' },
        ];

        this.shipmentModes = [
            { displayName: 'All', value: null },
            { displayName: 'Export', value: 'Export' },
            { displayName: 'Import', value: 'Import' },
        ];

        this.tariffType.setValue(this.tariffTypes[4]);
        this.tariffShipmentMode.setValue(this.shipmentModes[0]);
        this.tariffDateType.setValue(this.dateTypes[0]);
        this.tariffStatus.setValue(this.status[0]);
    }

    getCustomer() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER)) {
            this.configCustomer.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER) || [];
        } else {
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CUSTOMER)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataPartner: any) => {
                        this.configCustomer.dataSource = dataPartner || [];
                        this._dataService.setDataService(SystemConstants.CSTORAGE.CUSTOMER, dataPartner || []);
                    },
                );
        }
    }

    getSuppliers() {
        forkJoin([
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.CARRIER),
            this._catalogueRepo.getPartnersByType(PartnerGroupEnum.SHIPPER),
        ]).pipe(catchError(this.catchError))
            .subscribe(
                ([carries, shippers]: any[]) => {
                    this.suppliers = [...new Set(carries), ...shippers];
                    this.suppliers = _uniqBy(this.suppliers, 'id');
                }
            );
    }

    getOffice() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.OFFICE)) {
            this.configOffice.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.OFFICE) || [];
        } else {
            this._systemRepo.getAllOffice()
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        this.configOffice.dataSource = res || [];
                        this._dataService.setDataService(SystemConstants.CSTORAGE.OFFICE, res || []);
                    }
                );
        }
    }

    onSelectDataFormInfo(data: any, key: string | any) {
        switch (key) {
            case 'customer':
                break;
            case 'supllier':
                break;
            case 'office':
                break;
            default:
                break;
        }
    }

    submitSearch() {
        const bodySearch: Partial<ITariffSearch> = {
        };
        console.log(bodySearch);
    }

    resetForm() {

    }


}

interface ITariffSearch {
    name: string;
    serviceMode: string;
    customerID: string;
    supplierID: string;
    tariffType: string;
    dateType: string;
    fromDate: string;
    toDate: string;
    status: boolean;
    officeId: string;
}
