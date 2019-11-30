import { Component, EventEmitter, Output } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { formatDate } from '@angular/common';
import { TransactionTypeEnum } from 'src/app/shared/enums/transaction-type.enum';
import { Observable } from 'rxjs';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { User } from 'src/app/shared/models';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

@Component({
    selector: 'form-search-fcl-import',
    templateUrl: './form-search-fcl-import.component.html',
    styleUrls: ['./form-search-fcl-import.component.scss']
})
export class SeaFCLImportManagementFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchDataShipment> = new EventEmitter<ISearchDataShipment>();
    filterTypes: CommonInterface.ICommonTitleValue[];

    formSearch: FormGroup;
    searchText: AbstractControl;
    filterType: AbstractControl;
    serviceDate: AbstractControl;
    customer: AbstractControl;
    supplier: AbstractControl;
    agent: AbstractControl;
    saleman: AbstractControl;
    creator: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'id', label: 'Partner ID' },
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameEn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' }
    ];

    displayFieldsSaleMan: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' },
        { field: 'role', label: 'Role' },
    ];

    customers: Observable<Customer[]>;
    suppliers: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    salemans: Observable<User[]>;
    creators: Observable<User[]>;

    constructor(
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo) {
        super();
    }

    ngOnInit(): void {
        this.initFormSearch();
        this.getCustomer();
        this.getSupplier();
        this.getAgent();
        this.getSaleman();
        this.getCreator();

        this.filterTypes = [
            { title: 'Job ID', value: 'jobNo' },
            { title: 'MBL No', value: 'mawb' },
            { title: 'HBL No', value: 'hwbno' },
            { title: 'Cont No', value: 'containerNo' },
            { title: 'Seal No', value: 'sealNo' },
            { title: 'Mark No', value: 'markNo' },
            { title: 'C/D No', value: 'creditDebitNo' },
            { title: 'SOA No', value: 'soaNo' },
        ];

        this.filterType.setValue(this.filterTypes[0]);
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            searchText: [],
            filterType: [],
            serviceDate: [],
            customer: [],
            supplier: [],
            agent: [],
            saleman: [],
            creator: [],
        });

        this.customer = this.formSearch.controls['customer'];
        this.supplier = this.formSearch.controls['supplier'];
        this.agent = this.formSearch.controls['agent'];
        this.saleman = this.formSearch.controls['saleman'];
        this.creator = this.formSearch.controls['creator'];

        this.searchText = this.formSearch.controls['searchText'];
        this.filterType = this.formSearch.controls['filterType'];
        this.serviceDate = this.formSearch.controls['serviceDate'];
    }

    getCustomer() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER)) {
            this.customers = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER);
        } else {
            this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
            this._dataService.setDataService(SystemConstants.CSTORAGE.CUSTOMER, this.customers);
        }
    }

    getSupplier() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER)) {
            this.suppliers = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER);
        } else {
            this.suppliers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER);
            this._dataService.setDataService(SystemConstants.CSTORAGE.CARRIER, this.suppliers);
        }
    }

    getAgent() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT)) {
            this.agents = this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT);
        } else {
            this.agents = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.AGENT);
            this._dataService.setDataService(SystemConstants.CSTORAGE.AGENT, this.agents);
        }
    }

    getSaleman() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SALE)) {
            this.salemans = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SALE);
        } else {
            this.salemans = this._systemRepo.getSystemUsers({ active: true });
            this._dataService.setDataService(SystemConstants.CSTORAGE.SALE, this.salemans);
        }
    }

    getCreator() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SALE)) {
            this.creators = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SALE);
        } else {
            this.creators = this._systemRepo.getSystemUsers({ active: true });
            this._dataService.setDataService(SystemConstants.CSTORAGE.SALE, this.creators);
        }
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.formSearch.controls['customer'].setValue(data.id);
                break;
            case 'supplier':
                this.formSearch.controls['supplier'].setValue(data.id);
                break;
            case 'agent':
                this.formSearch.controls['agent'].setValue(data.id);
                break;
            case 'saleman':
                this.formSearch.controls['saleman'].setValue(data.id);
                break;
            case 'creator':
                this.formSearch.controls['creator'].setValue(data.id);
                break;
            default:
                break;
        }
    }

    searchShipment() {
        const body: ISearchDataShipment = {
            all: null,
            jobNo: this.filterType.value.value === 'jobNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            mawb: this.filterType.value.value === 'mawb' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            hwbno: this.filterType.value.value === 'hwbno' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            containerNo: this.filterType.value.value === 'containerNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            sealNo: this.filterType.value.value === 'sealNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            markNo: this.filterType.value.value === 'markNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            creditDebitNo: this.filterType.value.value === 'creditDebitNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            soaNo: this.filterType.value.value === 'soaNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            customerId: this.customer.value,
            coloaderId: this.supplier.value,
            agentId: this.agent.value,
            saleManId: this.saleman.value,
            userCreated: this.creator.value,
            fromDate: (!!this.serviceDate.value && !!this.serviceDate.value.startDate) ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: (!!this.serviceDate.value && !!this.serviceDate.value.endDate) ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            transactionType: TransactionTypeEnum.SeaFCLImport
        };
        //console.log(body);
        this.onSearch.emit(body);
    }

    resetSearch() {
        this.formSearch.reset();
        
        this.resetFormControl(this.customer);
        this.resetFormControl(this.supplier);
        this.resetFormControl(this.agent);
        this.resetFormControl(this.saleman);
        this.resetFormControl(this.creator);
        this.filterType.setValue(this.filterTypes[0]);
        this.onSearch.emit(<any>{ transactionType: TransactionTypeEnum.SeaFCLImport });
    }

    collapsed() {
        this.resetFormControl(this.customer);
        this.resetFormControl(this.supplier);
        this.resetFormControl(this.agent);
        this.resetFormControl(this.saleman);
        this.resetFormControl(this.creator);
        this.resetFormControl(this.serviceDate);
    }

    expanded() {
        console.log('expanded')
    }
}

interface ISearchDataShipment {
    all: string;
    jobNo: string;
    mawb: string;
    hwbno: string;
    containerNo: string;
    sealNo: string;
    markNo: string;
    creditDebitNo: string;
    soaNo: string;
    customerId: string;
    coloaderId: string;
    agentId: string;
    saleManId: string;
    userCreated: string;
    fromDate: string;
    toDate: string;
    transactionType: Number;
}
