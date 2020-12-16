import { formatDate } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AccountingConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { DataService } from '@services';
import { getCatalogueCurrencyState, IAppState } from '@store';
import { Observable } from 'rxjs';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'customer-payment-form-search',
    templateUrl: './form-search-customer-payment.component.html',
})
export class ARCustomerPaymentFormSearchComponent extends AppForm implements OnInit {
    @Output() onSearch: EventEmitter<IAcctReceiptCriteria> = new EventEmitter<IAcctReceiptCriteria>();
    @Output() onReset: EventEmitter<IAcctReceiptCriteria> = new EventEmitter<IAcctReceiptCriteria>();

    customerIDs: Observable<Customer[]>;
    creators: Observable<User[]>;
    customerID: AbstractControl;
    creator: AbstractControl;
    refNo: AbstractControl;
    paymentTypes: string[] = AccountingConstants.PAYMENT_TYPE;
    paymentType: AbstractControl;
    displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    date: AbstractControl;
    dateTypes: string[] = AccountingConstants.DATE_TYPE;
    dateType: AbstractControl;
    Currencys: Observable<Currency[]>;
    currency: AbstractControl;
    syncStatuss = AccountingConstants.SYNC_STATUSS;
    syncStatus: AbstractControl;
    statuss = AccountingConstants.STATUS;
    status: AbstractControl;
    formSearch: FormGroup;


    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _store: Store<IAppState>,
        private _dataService: DataService,


    ) {
        super();
        this.requestReset = this.requestSearch;
    }

    ngOnInit() {
        this.initForm();
        this.customerIDs = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, null);
        this.creators = this._systemRepo.getSystemUsers();
        this.Currencys = this._catalogueRepo.getListCurrency();
    }
    initForm() {
        this.formSearch = this._fb.group({
            refNo: [],
            paymentType: [this.paymentTypes[0]],
            customerID: [],
            date: [{ startDate: new Date(), endDate: new Date() }],
            dateType: [this.dateTypes[0]],
            currency: [],
            syncStatus: [this.syncStatuss[0]],
            status: [this.statuss[0]],
        });
        this.refNo = this.formSearch.controls['refNo'];
        this.paymentType = this.formSearch.controls['paymentType'];
        this.customerID = this.formSearch.controls['customerID'];
        this.date = this.formSearch.controls['date'];
        this.dateType = this.formSearch.controls['dateType'];
        this.currency = this.formSearch.controls['currency'];
        this.syncStatus = this.formSearch.controls['syncStatus'];
        this.status = this.formSearch.controls['status'];
    }
    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.customerID.setValue((data as Partner).id);
                this._dataService.setData('customer', data);
                break;
            case 'currency':
                this.currency.setValue((data as Currency).id);
                this._dataService.setData('currency', data);
                break;
            default:
                break;

        }

    }

    search() {
        const body: IAcctReceiptCriteria = {
            refNo: this.refNo.value,
            paymentType: this.paymentType.value !== this.paymentTypes[0] ? this.paymentType.value : null,
            customerID: this.customerID.value,
            dateFrom: (!!this.date.value && !!this.date.value.startDate) ? formatDate(this.date.value.startDate, 'yyyy-MM-dd', 'en') : null,
            dateTo: (!!this.date.value && !!this.date.value.endDate) ? formatDate(this.date.value.endDate, 'yyyy-MM-dd', 'en') : null,
            dateType: this.dateType.value !== this.dateTypes[0] ? this.dateType.value : null,
            currency: this.currency.value,
            syncStatus: this.syncStatus.value !== this.syncStatuss[0] ? this.syncStatus.value : null,
            status: this.status.value !== this.statuss[0] ? this.status.value : null,
        };
        this.onSearch.emit(body);
        console.log(body);
    }
    reset() {
        this.resetKeywordSearchCombogrid();
        this.refNo.reset();
        this.paymentType.reset(this.paymentTypes[0]);
        this.customerID.reset();
        this.date.reset();
        this.dateType.reset(this.dateTypes[0]);
        this.currency.reset();
        this.syncStatus.reset(this.syncStatuss[0]);
        this.status.reset(this.statuss[0]);
        this.onReset.emit(<any>{ transactionType: null });
    }
    resetFormSearch() {

    }
}
interface IAcctReceiptCriteria {
    refNo: string;
    paymentType: string;
    customerID: string;
    dateFrom: string;
    dateTo: string;
    dateType: string;
    currency: string;
    syncStatus: string;
    status: string;
}
