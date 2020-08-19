import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { formatDate } from '@angular/common';
import { Store } from '@ngrx/store';

import { User } from '@models';
import { DocumentationRepo, SystemRepo, CatalogueRepo } from '@repositories';
import { DataService } from '@services';
import { SystemConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';

import * as fromOpsStore from './../../../store';
import { catchError, takeUntil } from 'rxjs/operators';

@Component({
    selector: 'job-management-form-search',
    templateUrl: './form-search-job.component.html'
})

export class JobManagementFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchDataShipment> = new EventEmitter<ISearchDataShipment>();
    @Output() onReset: EventEmitter<ISearchDataShipment> = new EventEmitter<ISearchDataShipment>();

    filterTypes: CommonInterface.ICommonTitleValue[];

    productServices: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.PRODUCTSERVICE;
    serviceModes: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.SERVICEMODES;
    shipmentModes: CommonInterface.INg2Select[] = JobConstants.COMMON_DATA.SHIPMENTMODES;

    formSearch: FormGroup;
    searchText: AbstractControl;
    filterType: AbstractControl;
    productService: AbstractControl;
    serviceMode: AbstractControl;
    shipmentMode: AbstractControl;
    user: AbstractControl;
    serviceDate: AbstractControl;

    configPartner: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    users: User[] = [];
    selectedPartner: any = {};

    dataSearch: ISearchDataShipment;

    constructor(
        private _documentRepo: DocumentationRepo,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _sysRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<fromOpsStore.IOperationState>
    ) {
        super();

        this.requestReset = this.resetSearch;
    }

    ngOnInit() {
        this.initFormSearch();
        this.getPartner();
        this.getUser();

        this.filterTypes = [
            { title: 'Job Id', value: 'jobNo' },
            { title: 'HBL', value: 'hwbno' },
            { title: 'Custom No', value: 'clearanceNo' },
            { title: 'MBL', value: 'mblno' },
            { title: 'Credit\/Debit\/Invoice\ No', value: 'creditDebitInvoice' },
        ];
        this.filterType.setValue(this.filterTypes[0]);

        this._store.select(fromOpsStore.getOperationTransationDataSearch)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (criteria: ISearchDataShipment) => {
                    if (!!Object.keys(criteria).length) {
                        this.dataSearch = criteria;

                        ['jobNo', 'hwbno', 'clearanceNo', 'mblno', 'creditDebitInvoice'].some((i: string) => {
                            if (!!this.dataSearch[i]) {
                                this.filterType.setValue(this.filterTypes.find(d => d.value === i));
                                this.searchText.setValue(this.dataSearch[i]);
                                return true;
                            }
                        });
                    }
                }
            )
    }

    getPartner() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER)) {
            this.getPartnerData(this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER));

        } else {
            this._catalogueRepo.getListPartner(null, null, { partnerGroup: CommonEnum.PartnerGroupEnum.ALL, active: true })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataPartner: any) => {
                        this.getPartnerData(dataPartner);
                    },
                );
        }
    }

    getUser() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SYSTEM_USER)) {
            this.users = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SYSTEM_USER);

        } else {
            this._sysRepo.getListSystemUser()
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataUser: any) => {
                        this.users = dataUser;
                    },
                );
        }
    }

    getPartnerData(data: any) {
        this.configPartner.dataSource = data;
        this.configPartner.displayFields = [
            { field: 'taxCode', label: 'Taxcode' },
            { field: 'shortName', label: 'Name' },
            { field: 'partnerNameVn', label: 'Customer Name' },
        ];
        this.configPartner.selectedDisplayFields = ['shortName'];
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            'searchText': [],
            'filterType': [],
            'productService': [],
            'serviceMode': [],
            'shipmentMode': [],
            'user': [],
            'serviceDate': [],

        });

        this.searchText = this.formSearch.controls['searchText'];
        this.filterType = this.formSearch.controls['filterType'];
        this.productService = this.formSearch.controls['productService'];
        this.serviceMode = this.formSearch.controls['serviceMode'];
        this.shipmentMode = this.formSearch.controls['shipmentMode'];
        this.user = this.formSearch.controls['user'];
        this.serviceDate = this.formSearch.controls['serviceDate'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.selectedPartner = { field: 'id', value: data.id };
                break;
            default:
                break;
        }
    }

    searchShipment() {
        const body: ISearchDataShipment = {
            all: null,
            jobNo: this.filterType.value.value === 'jobNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            hwbno: this.filterType.value.value === 'hwbno' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            mblno: this.filterType.value.value === 'mblno' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            clearanceNo: this.filterType.value.value === 'clearanceNo' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,
            creditDebitInvoice: this.filterType.value.value === 'creditDebitInvoice' ? (this.searchText.value ? this.searchText.value.trim() : '') : null,

            productService: !!this.productService.value ? this.productService.value.id : null,
            serviceMode: !!this.serviceMode.value ? this.serviceMode.value.id : null,
            shipmentMode: !!this.shipmentMode.value ? this.shipmentMode.value.id : null,
            serviceDateFrom: (!!this.serviceDate.value && !!this.serviceDate.value.startDate) ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDateTo: (!!this.serviceDate.value && !!this.serviceDate.value.endDate) ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            customerId: !!this.selectedPartner.value ? this.selectedPartner.value : null,
            fieldOps: !!this.user.value ? this.user.value.id : null,
        };
        this.onSearch.emit(body);

        this._store.dispatch(new fromOpsStore.OPSTransactionSearchListAction(body));
    }

    resetSearch() {
        this.formSearch.reset();
        this.selectedPartner = {};
        this.filterType.setValue(this.filterTypes[0]);

        this.onReset.emit(<any>{});
    }

    collapsed() {
        this.resetFormControl(this.productService);
        this.resetFormControl(this.serviceMode);
        this.resetFormControl(this.shipmentMode);
        this.resetFormControl(this.user);
        this.resetFormControl(this.serviceDate);
        this.selectedPartner = { field: 'id', value: null };
    }

    expanded() {
        if (!!this.dataSearch) {
            const advanceSearchForm = {
                productService: this.productServices.find(p => p.id === this.dataSearch.productService) || null,
                serviceMode: this.serviceModes.find(s => s.id === this.dataSearch.serviceMode) || null,
                shipmentMode: this.shipmentModes.find(s => s.id === this.dataSearch.shipmentMode) || null,
                user: this.users.find(u => u.id === this.dataSearch.fieldOps) || null
            };
            console.log(advanceSearchForm);
            this.selectedPartner = { field: 'id', value: this.dataSearch.customerId };

            this.formSearch.patchValue(advanceSearchForm);
        }
    }
}



interface ISearchDataShipment {
    all: string;
    jobNo: string;
    hwbno: string;
    productService: string;
    serviceMode: string;
    customerId: string;
    fieldOps: string;
    shipmentMode: string;
    serviceDateFrom: string;
    serviceDateTo: string;
    mblno: string;
    clearanceNo: string;
    creditDebitInvoice: string;
}

