import { Component, Output, EventEmitter } from '@angular/core';
import { formatDate } from '@angular/common';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';

import { AppForm } from 'src/app/app.form';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { User } from 'src/app/shared/models';
import { DataService } from 'src/app/shared/services';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { SystemConstants } from 'src/constants/system.const';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { Observable } from 'rxjs';

@Component({
    selector: 'sea-fcl-export-form-search',
    templateUrl: './form-search-sea-fcl-export.component.html'
})

export class SeaFCLExportFormSearchComponent extends AppForm {

    @Output() onSearch: EventEmitter<Partial<ISearchSeaFCLExport>> = new EventEmitter<Partial<ISearchSeaFCLExport>>();

    formSearch: FormGroup;
    searchText: AbstractControl;
    rangeDate: AbstractControl;
    filterType: AbstractControl;
    hblNo: AbstractControl;
    contNo: AbstractControl;
    sealNo: AbstractControl;
    saleMan: AbstractControl;
    customer: AbstractControl;
    notifyParty: AbstractControl;

    searchFilters: Array<CommonInterface.INg2Select> = [
        { id: 'jobNo', text: 'Job ID' },
        { id: 'mawb', text: 'MBL No' },
        { id: 'supplierName', text: 'Supplier' },
        { id: 'agentName', text: 'Agent' },
        { id: 'hwbNo', text: 'HBL No' },
    ];

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'id', label: 'Partner ID' },
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' }
    ];

    displayFieldsSaleMan: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' }
    ];

    customers: Observable<Customer[]>;
    notifyPartries: Observable<Customer[]>;
    salemans: Observable<User[]>;

    dataSearch: Partial<ISearchSeaFCLExport> = {};

    constructor(
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo
    ) {
        super();

        this.requestSearch = this.seachShipment;
        this.requestReset = this.resetSearchShipment;
    }

    ngOnInit() {
        this.initFormSearch();

        this.getConsignee();
        this.getCustomer();
        this.getSaleMan();
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            searchText: [],
            rangeDate: [
                {
                    startDate: this.ranges["This Month"][0],
                    endDate: this.ranges["This Month"][1],
                }
            ],
            filterType: [],
            hblNo: [],
            contNo: [],
            sealNo: [],
            saleMan: [],
            customer: [],
            notifyParty: [],
        });

        this.searchText = this.formSearch.controls['searchText'];
        this.rangeDate = this.formSearch.controls['rangeDate'];
        this.filterType = this.formSearch.controls['filterType'];
        this.hblNo = this.formSearch.controls['hblNo'];
        this.contNo = this.formSearch.controls['contNo'];
        this.sealNo = this.formSearch.controls['sealNo'];

        this.saleMan = this.formSearch.controls['saleMan'];
        this.customer = this.formSearch.controls['customer'];
        this.notifyParty = this.formSearch.controls['notifyParty'];

        // * Set default
        this.filterType.setValue([this.searchFilters[0]]);
    }

    getConsignee() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CONSIGNEE)) {
            this.notifyPartries = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CONSIGNEE);
        } else {
            this.notifyPartries = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CONSIGNEE);

            this._dataService.setDataService(SystemConstants.CSTORAGE.CONSIGNEE, this.notifyPartries);
        }
    }

    getCustomer() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER)) {
            this.customers = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER);
        } else {
            this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);

            this._dataService.setDataService(SystemConstants.CSTORAGE.CUSTOMER, this.customers);
        }
    }

    getSaleMan() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SALE)) {
            this.salemans = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SALE);
        } else {
            this.salemans = this._systemRepo.getSystemUsers({ active: true });
            this._dataService.setDataService(SystemConstants.CSTORAGE.SALE, this.salemans);
        }
    }

    seachShipment() {
        const body: ISearchSeaFCLExport = {
            transactionType: CommonEnum.TransactionTypeEnum.SeaFCLExport,
            fromDate: !!this.rangeDate.value && !!this.rangeDate.value.startDate ? formatDate(this.rangeDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!this.rangeDate.value && !!this.rangeDate.value.endDate ? formatDate(this.rangeDate.value.endDate, 'yyyy-MM-dd', 'en') : null,

            notifyPartyId: this.notifyParty.value,
            customerId: this.customer.value,
            saleManId: this.saleMan.value,

            jobNo: this.filterType.value[0].id === 'jobNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            mawb: this.filterType.value[0].id === 'mawb' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            hwbno: this.filterType.value[0].id === 'hwbno' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            supplierName: this.filterType.value[0].id === 'supplierName' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            agentName: this.filterType.value[0].id === 'agentName' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
        };

        this.onSearch.emit(body || {});
    }

    onSelectDataFormInfo(data: Customer | User | any, type: string) {
        switch (type) {
            case 'saleman':
                this.formSearch.controls['saleMan'].setValue(data.id);
                break;
            case 'party':
                this.formSearch.controls['notifyParty'].setValue(data.id);
                break;
            case 'customer':
                this.formSearch.controls['customer'].setValue(data.id);
                break;
            default:
                break;
        }
    }

    resetSearchShipment() {
        this.formSearch.reset();

        //  * Default.
        this.filterType.setValue([this.searchFilters[0]]);
        this.rangeDate.setValue(
            { startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1), endDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0), }
        );

        this.resetFormControl(this.saleMan);
        this.resetFormControl(this.customer);
        this.resetFormControl(this.notifyParty);

        this.onSearch.emit({
            transactionType: CommonEnum.TransactionTypeEnum.SeaFCLExport,
            fromDate: this.ranges['This Month'][0],
            toDate: this.ranges['This Month'][1],
        });
    }
}

interface ISearchSeaFCLExport {
    transactionType: CommonEnum.TransactionTypeEnum;
    fromDate: string;
    toDate: string;
    notifyPartyId: string;
    customerId: string;
    saleManId: string;
    jobNo: string;
    mawb?: string;
    hwbno?: string;
    supplierName?: string;
    agentName?: string;
}
