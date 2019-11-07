import { Component, EventEmitter, Output } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { AbstractControl, FormGroup, FormBuilder } from '@angular/forms';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { formatDate } from '@angular/common';
import { TransactionTypeEnum } from 'src/app/shared/enums/transaction-type.enum';

@Component({
    selector: 'form-search-fcl-import',
    templateUrl: './form-search-fcl-import.component.html',
    styleUrls: ['./form-search-fcl-import.component.scss']
})
export class SeaFCLImportManagementFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchDataShipment> = new EventEmitter<ISearchDataShipment>();
    filterTypes: CommonInterface.ICommonTitleValue[];

    creators: CommonInterface.IValueDisplay[] = [];

    formSearch: FormGroup;
    searchText: AbstractControl;
    filterType: AbstractControl;
    serviceDate: AbstractControl;

    configCustomer: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    configSupplier: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    configAgent: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    configSaleman: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };
    configCreator: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    selectedCustomer: any = {};
    selectedSupplier: any = {};
    selectedAgent: any = {};
    selectedSaleman: any = {};
    selectedCreator: any = {};

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
        });

        this.searchText = this.formSearch.controls['searchText'];
        this.filterType = this.formSearch.controls['filterType'];
        this.serviceDate = this.formSearch.controls['serviceDate'];
    }

    getCustomer() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER)) {
            this.getCustomerData(this._dataService.getDataByKey(SystemConstants.CSTORAGE.CUSTOMER));
        } else {
            this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER, active: true })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataCustomer: any) => {
                        //console.log(dataCustomer)
                        this.getCustomerData(dataCustomer);

                        this._dataService.setDataService(SystemConstants.CSTORAGE.CUSTOMER, dataCustomer);

                    },
                );
        }
    }

    getCustomerData(data: any) {
        this.configCustomer.dataSource = data;
        this.configCustomer.displayFields = [
            { field: 'id', label: 'Partner ID' },
            { field: 'shortName', label: 'Name ABBR' },
            { field: 'partnerNameEn', label: 'Name En' },
            { field: 'taxCode', label: 'Taxcode' },
        ];
        this.configCustomer.selectedDisplayFields = ['shortName'];
    }

    getSupplier() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER)) {
            this.getSupplierData(this._dataService.getDataByKey(SystemConstants.CSTORAGE.CARRIER));
        } else {
            this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CARRIER, active: true })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataSupplier: any) => {
                        //console.log(dataSupplier)
                        this.getSupplierData(dataSupplier);

                        this._dataService.setDataService(SystemConstants.CSTORAGE.CARRIER, dataSupplier);

                    },
                );
        }
    }

    getSupplierData(data: any) {
        this.configSupplier.dataSource = data;
        this.configSupplier.displayFields = [
            { field: 'id', label: 'Partner ID' },
            { field: 'shortName', label: 'Name ABBR' },
            { field: 'partnerNameEn', label: 'Name En' },
            { field: 'taxCode', label: 'Taxcode' },
        ];
        this.configSupplier.selectedDisplayFields = ['shortName'];
    }

    getAgent() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT)) {
            this.getAgentData(this._dataService.getDataByKey(SystemConstants.CSTORAGE.AGENT));
        } else {
            this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.AGENT, active: true })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataAgent: any) => {
                        //console.log(dataAgent)
                        this.getAgentData(dataAgent);

                        this._dataService.setDataService(SystemConstants.CSTORAGE.AGENT, dataAgent);
                    },
                );
        }
    }

    getAgentData(data: any) {
        this.configAgent.dataSource = data;
        this.configAgent.displayFields = [
            { field: 'id', label: 'Partner ID' },
            { field: 'shortName', label: 'Name ABBR' },
            { field: 'partnerNameEn', label: 'Name En' },
            { field: 'taxCode', label: 'Taxcode' },
        ];
        this.configAgent.selectedDisplayFields = ['shortName'];
    }

    getSaleman() {
        this._systemRepo.getUser(1, 100, { active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataUser: any) => {
                    //console.log(dataUser.data)
                    this.getSalemanData(dataUser.data);
                },
            );
    }

    getSalemanData(data: any) {
        this.configSaleman.dataSource = data;
        this.configSaleman.displayFields = [
            { field: 'username', label: 'User Name' },
            { field: 'employeeNameVn', label: 'Full Name' },
            { field: 'role', label: 'Role' },
        ];
        this.configSaleman.selectedDisplayFields = ['employeeNameVn'];
    }

    getCreator() {
        this._systemRepo.getUser(1, 100, { active: true })
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataUser: any) => {
                    //console.log(dataUser.data)
                    this.getCreatorData(dataUser.data);
                },
            );
    }

    getCreatorData(data: any) {
        this.configCreator.dataSource = data;
        this.configCreator.displayFields = [
            { field: 'username', label: 'User Name' },
            { field: 'employeeNameVn', label: 'Full Name' },
            { field: 'role', label: 'Role' },
        ];
        this.configCreator.selectedDisplayFields = ['employeeNameVn'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.selectedCustomer = { field: data.partnerNameEn, value: data.id };
                break;
            case 'supplier':
                this.selectedSupplier = { field: data.partnerNameEn, value: data.id };
                break;
            case 'agent':
                this.selectedAgent = { field: data.partnerNameEn, value: data.id };
                break;
            case 'saleman':
                this.selectedSaleman = { field: data.userName, value: data.id };
                break;
            case 'creator':
                this.selectedCreator = { field: data.userName, value: data.id };
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
            customerId: !!this.selectedCustomer.value ? this.selectedCustomer.value : null,
            coloaderId: !!this.selectedSupplier.value ? this.selectedSupplier.value : null,
            agentId: !!this.selectedAgent.value ? this.selectedAgent.value : null,
            saleManId: !!this.selectedSaleman.value ? this.selectedSaleman.value : null,
            userCreated: !!this.selectedCreator.value ? this.selectedCreator.value : null,
            fromDate: (!!this.serviceDate.value && !!this.serviceDate.value.startDate) ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: (!!this.serviceDate.value && !!this.serviceDate.value.endDate) ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            transactionType: TransactionTypeEnum.SeaFCLImport
        };
        //console.log(body);
        this.onSearch.emit(body);
    }

    resetSearch() {
        this.formSearch.reset();
        this.selectedCustomer = {};
        this.selectedSupplier = {};
        this.selectedAgent = {};
        this.selectedSaleman = {};
        this.selectedCreator = {};
        this.filterType.setValue(this.filterTypes[0]);
        this.onSearch.emit(<any>{ transactionType: TransactionTypeEnum.SeaFCLImport });
    }

    collapsed() {
        this.selectedCustomer = {};
        this.selectedSupplier = {};
        this.selectedAgent = {};
        this.selectedSaleman = {};
        this.selectedCreator = {};
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
