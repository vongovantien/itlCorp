import { formatDate } from '@angular/common';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AccountingConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, Partner, User } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { AppForm } from '@app';
import { Store } from '@ngrx/store';
import { SearchListCustomerPayment } from '../../store/actions';
import { customerPaymentReceipSearchState, ICustomerPaymentState } from '../../store/reducers';
import { take, takeUntil } from 'rxjs/operators';
import { isNull } from '@angular/compiler/src/output/output_ast';
import { DataService } from '@services';

@Component({
    selector: 'customer-payment-form-search',
    templateUrl: './form-search-customer-payment.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush

})
export class ARCustomerPaymentFormSearchComponent extends AppForm implements OnInit {

    formSearch: FormGroup;
    customerID: AbstractControl;
    creator: AbstractControl;
    refNo: AbstractControl;
    paymentType: AbstractControl;
    date: AbstractControl;
    dateType: AbstractControl;
    currency: AbstractControl;
    syncStatus: AbstractControl;
    status: AbstractControl;
    typeReceipt: AbstractControl;
    class: AbstractControl;
    paymentMethod: AbstractControl;

    customerIDs: Observable<Customer[]>;
    creators: Observable<User[]>;
    Currencys: Observable<Currency[]>;

    displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    paymentTypes: string[] = AccountingConstants.PAYMENT_TYPE;
    dateTypes: string[] = AccountingConstants.DATE_TYPE;
    syncStatuss = AccountingConstants.SYNC_STATUSS;
    statuss = AccountingConstants.STATUS;
    typesReceipt: string[] = ['Customer', 'Agent'];
    statusRecepit: string[] = ['Draft', 'Cancel', 'Done'];
    classReceipts: string[] = [
        AccountingConstants.RECEIPT_CLASS.CLEAR_DEBIT,
        AccountingConstants.RECEIPT_CLASS.ADVANCE,
        AccountingConstants.RECEIPT_CLASS.COLLECT_OBH,
        AccountingConstants.RECEIPT_CLASS.COLLECT_OBH_OTHER,
        AccountingConstants.RECEIPT_CLASS.PAY_OBH,
        AccountingConstants.RECEIPT_CLASS.NET_OFF];

    paymentMethods: string[] = [
        AccountingConstants.RECEIPT_PAYMENT_METHOD.CASH,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.BANK,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_ADVANCE,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_ADVANCE_BANK,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.CLEAR_ADVANCE_CASH,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.INTERNAL,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.COLL_INTERNAL,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.OBH_INTERNAL,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.MANAGEMENT_FEE,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.OTHER_FEE,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.EXTRA,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.OTHER,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.COLLECT_OBH_AGENCY,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.PAY_OBH_AGENCY,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.COLLECTED_AMOUNT,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.ADVANCE_AGENCY,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.BANK_FEE_AGENCY,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.RECEIVE_FROM_PAY_OBH,
        AccountingConstants.RECEIPT_PAYMENT_METHOD.RECEIVE_FROM_COLLECT_OBH
    ];
    
    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _store: Store<ICustomerPaymentState>
    ) {
        super();
        this.requestReset = this.requestSearch;
    }

    ngOnInit() {
        this.initForm();

        this.customerIDs = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, null);
        this.creators = this._systemRepo.getSystemUsers();
        this.Currencys = this._catalogueRepo.getListCurrency();
        this.subscriptionSearchParamState();
    }

    initForm() {
        this.formSearch = this._fb.group({
            refNo: [],
            paymentType: [],
            customerID: [],
            date: [{ startDate: new Date(new Date().setDate(new Date().getDate() - 29)), endDate: new Date() }],
            dateType: [],
            currency: [],
            status: [],
            syncStatus: [],
            typeReceipt: [],
            class: [],
            paymentMethod: []
        });
        this.refNo = this.formSearch.controls['refNo'];
        this.paymentType = this.formSearch.controls['paymentType'];
        this.customerID = this.formSearch.controls['customerID'];
        this.date = this.formSearch.controls['date'];
        this.dateType = this.formSearch.controls['dateType'];
        this.currency = this.formSearch.controls['currency'];
        this.syncStatus = this.formSearch.controls['syncStatus'];
        this.typeReceipt = this.formSearch.controls['typeReceipt'];
        this.status = this.formSearch.controls['status'];
        this.class = this.formSearch.controls['class'];
        this.paymentMethod = this.formSearch.controls['paymentMethod'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.customerID.setValue((data as Partner).id);
                break;
            case 'currency':
                this.currency.setValue((data as Currency).id);
                break;
            default:
                break;
        }
    }

    search() {
        let body: any = {
            refNo: this.refNo.value,
            paymentType: this.paymentType.value,
            customerID: this.customerID.value,
            dateFrom: (!!this.date.value && !!this.date.value.startDate) ? formatDate(this.date.value.startDate, 'yyyy-MM-dd', 'en') : null,
            dateTo: (!!this.date.value && !!this.date.value.endDate) ? formatDate(this.date.value.endDate, 'yyyy-MM-dd', 'en') : null,
            dateType: this.dateType.value,
            currency: this.currency.value,
            syncStatus: this.syncStatus.value,
            status: this.status.value,
            class: this.class.value,
            paymentMethod: this.paymentMethod.value
        };
        //this._listReceipt.onSearchCPs(body);
        this._store.select(customerPaymentReceipSearchState)
        .pipe(take(1))
        .subscribe(
            (data: any) => {
                body.typeReceipt = data.typeReceipt;
                this._store.dispatch(SearchListCustomerPayment(body));
            }
        );
    }

    reset() {
        this.resetKeywordSearchCombogrid();
        this.date.reset({ startDate: new Date(new Date().setDate(new Date().getDate() - 29)), endDate: new Date() });

        this.refNo.reset();
        this.paymentType.reset();
        this.customerID.reset();
        this.dateType.reset();
        this.currency.setValue(null);
        this.syncStatus.reset();
        this.status.reset();
        this.class.reset();
        this.paymentMethod.reset();
        this._store.select(customerPaymentReceipSearchState)
            .pipe(take(1))
            .subscribe(
                (data: any) => {
                    this._store.dispatch(SearchListCustomerPayment({ typeReceipt: data.typeReceipt }));
                }
            );
    }

    subscriptionSearchParamState() {
        this._store.select(customerPaymentReceipSearchState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (data) {
                        let formData: any = {
                            refNo: data?.refNo?.toString().replace(/[,]/g, "\n") || null,
                            paymentType: data.paymentType ? data.paymentType : null,
                            customerID: data?.customerID,
                            date: (!!data?.dateFrom && !!data?.dateTo) ? { startDate: new Date(data?.dateFrom), endDate: new Date(data?.dateTo) } : null,
                            dateType: data.dateType ? data.dateType : null,
                            currency: data.currency ? data.currency : null,
                            status: data.status ? data.status : null,
                            syncStatus: data.syncStatus ? data.syncStatus : null,
                            typeReceipt: data.typeReceipt ? data.typeReceipt : null,
                            class: data.class ? data.class : null,
                            paymentMethod: data.paymentMethod ? data.paymentMethod : null
                        };

                        this.formSearch.patchValue(formData);
                        console.log(data);
                    }
                }
            );
    }
}

export interface IAcctReceiptCriteria {
    refNo: string;
    paymentType: string;
    customerID: string;
    dateFrom: string;
    dateTo: string;
    dateType: string;
    currency: string;
    syncStatus: string;
    status: string;
    typeReceipt: string;
    class: string;
    paymentMethod: string;
}
