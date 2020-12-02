import { Component, EventEmitter, Output } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { formatDate } from '@angular/common';
import { Store } from '@ngrx/store';

import { Customer, User } from '@models';
import { SystemRepo, CatalogueRepo } from '@repositories';
import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';

import * as fromOpsStore from './../../../store';
import { takeUntil } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Component({
    selector: 'job-management-form-search',
    templateUrl: './form-search-job.component.html'
})

export class JobManagementFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchDataShipment> = new EventEmitter<ISearchDataShipment>();
    @Output() onReset: EventEmitter<ISearchDataShipment> = new EventEmitter<ISearchDataShipment>();

    filterTypes: CommonInterface.ICommonTitleValue[];

    productServices: string[] = JobConstants.COMMON_DATA.PRODUCTSERVICE;
    serviceModes: string[] = JobConstants.COMMON_DATA.SERVICEMODES;
    shipmentModes: string[] = JobConstants.COMMON_DATA.SHIPMENTMODES;

    formSearch: FormGroup;
    searchText: AbstractControl;
    filterType: AbstractControl;
    productService: AbstractControl;
    serviceMode: AbstractControl;
    shipmentMode: AbstractControl;
    serviceDate: AbstractControl;
    customerId: AbstractControl;
    fieldOps: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    dataSearch: ISearchDataShipment;

    customers: Observable<Customer[]>;
    users: Observable<User[]>;

    constructor(
        private _fb: FormBuilder,
        private _sysRepo: SystemRepo,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<fromOpsStore.IOperationState>
    ) {
        super();

        this.requestReset = this.resetSearch;
    }

    ngOnInit() {

        this.customers = this._catalogueRepo.getListPartner(null, null, { partnerGroup: CommonEnum.PartnerGroupEnum.ALL, active: true });
        this.users = this._sysRepo.getListSystemUser();

        this.initFormSearch();

        this.filterTypes = [
            { title: 'Custom No', value: 'clearanceNo' },
            { title: 'Job Id', value: 'jobNo' },
            { title: 'HBL', value: 'hwbno' },
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
            );
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            'searchText': [],
            'filterType': [],
            'productService': [],
            'serviceMode': [],
            'shipmentMode': [],
            'serviceDate': [],
            'customerId': [],
            'fieldOps': []

        });

        this.searchText = this.formSearch.controls['searchText'];
        this.filterType = this.formSearch.controls['filterType'];
        this.productService = this.formSearch.controls['productService'];
        this.serviceMode = this.formSearch.controls['serviceMode'];
        this.shipmentMode = this.formSearch.controls['shipmentMode'];
        this.serviceDate = this.formSearch.controls['serviceDate'];
        this.customerId = this.formSearch.controls['customerId'];
        this.fieldOps = this.formSearch.controls['fieldOps'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.customerId.setValue(data.id);
                break;
            case 'user':
                this.fieldOps.setValue(data.id);
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
            customerId: this.customerId.value,
            fieldOps: this.fieldOps.value,
        };
        this.onSearch.emit(body);

        this._store.dispatch(new fromOpsStore.OPSTransactionSearchListAction(body));
    }

    resetSearch() {
        this.formSearch.reset();
        this.filterType.setValue(this.filterTypes[0]);

        this.onReset.emit(<any>{});

        this._store.dispatch(new fromOpsStore.OPSTransactionSearchListAction({}));

    }

    collapsed() {
        this.resetFormControl(this.productService);
        this.resetFormControl(this.serviceMode);
        this.resetFormControl(this.shipmentMode);
        this.resetFormControl(this.serviceDate);
        this.resetFormControl(this.customerId);
        this.resetFormControl(this.fieldOps);

    }

    expanded() {
        if (!!this.dataSearch) {
            const advanceSearchForm = {
                productService: this.productServices.find(p => p === this.dataSearch.productService) || null,
                serviceMode: this.serviceModes.find(s => s === this.dataSearch.serviceMode) || null,
                shipmentMode: this.shipmentModes.find(s => s === this.dataSearch.shipmentMode) || null,
                serviceDate: !!this.dataSearch.serviceDateFrom && !!this.dataSearch.serviceDateTo ? {
                    startDate: new Date(this.dataSearch.serviceDateFrom),
                    endDate: new Date(this.dataSearch.serviceDateTo)
                } : null,
                customerId: this.dataSearch.customerId,
                fieldOps: this.dataSearch.fieldOps
            };

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

