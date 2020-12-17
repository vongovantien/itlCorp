import { formatDate } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AccountingConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Currency, Customer, Partner, User } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { AppForm } from '@app';

@Component({
    selector: 'customer-payment-form-search',
    templateUrl: './form-search-customer-payment.component.html',
})
export class ARCustomerPaymentFormSearchComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<IAcctReceiptCriteria> = new EventEmitter<IAcctReceiptCriteria>();

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

    customerIDs: Observable<Customer[]>;
    creators: Observable<User[]>;
    Currencys: Observable<Currency[]>;

    displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    paymentTypes: string[] = AccountingConstants.PAYMENT_TYPE;
    dateTypes: string[] = AccountingConstants.DATE_TYPE;
    syncStatuss = AccountingConstants.SYNC_STATUSS;
    statuss = AccountingConstants.STATUS;


    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
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
            date: [{ startDate: new Date(new Date().setDate(new Date().getDate() - 29)), endDate: new Date() }],
            dateType: [this.dateTypes[0]],
            currency: ['VND'],
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
                break;
            case 'currency':
                this.currency.setValue((data as Currency).id);
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
    }

    reset() {
        this.resetKeywordSearchCombogrid();
        this.date.reset({ startDate: new Date(new Date().setDate(new Date().getDate() - 29)), endDate: new Date() });

        this.refNo.reset();
        this.paymentType.reset(this.paymentTypes[0]);
        this.customerID.reset();
        this.dateType.reset(this.dateTypes[0]);
        this.currency.setValue('VND');
        this.syncStatus.reset(this.syncStatuss[0]);
        this.status.reset(this.statuss[0]);

        this.onSearch.emit(<any>{});
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
