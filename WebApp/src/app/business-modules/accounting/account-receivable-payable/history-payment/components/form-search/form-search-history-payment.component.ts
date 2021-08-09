import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { formatDate } from '@angular/common';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';

import { Observable } from 'rxjs';
import { SearchListHistoryPayment } from '../../store/actions';
import { getDataSearchHistoryPaymentState, IHistoryPaymentState } from '../../store/reducers';
import { Store } from '@ngrx/store';
import { takeUntil } from 'rxjs/operators';

enum OverDueDays {
    All,
    Between1_15,
    Between16_30,
    Between31_60,
    Between61_90
}

@Component({
    selector: 'form-search-history-payment',
    templateUrl: './form-search-history-payment.component.html'
})
export class ARHistoryPaymentFormSearchComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<Partial<ISearchAccPayment>> = new EventEmitter<Partial<ISearchAccPayment>>();

    formSearch: FormGroup;

    partnerId: AbstractControl;
    referenceNo: AbstractControl;
    issuedDate: AbstractControl;
    paidDate: AbstractControl;
    dueDate: AbstractControl;
    overdueDate: AbstractControl;
    paymentStatus: AbstractControl;
    searchType: AbstractControl;

    partners: Observable<Partner[]>;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = [
        { field: 'accountNo', label: 'ID' },
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name Local' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    payments: string[] = ['All', 'Unpaid', 'Paid A Part', 'Paid'];

    overDueDays: CommonInterface.INg2Select[] = [
        { id: '0', text: 'All' },
        { id: 1, text: '01-15 days' },
        { id: 2, text: '16-30 days' },
        { id: 3, text: '31-60 days' },
        { id: 4, text: '60-90 days' },
    ];

    referenceTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'VAT Invoice', value: 'VatInvoice' },
        { title: 'DebitNote/Invoice', value: 'DebitInvoice' },
        { title: 'SOA', value: 'Soa' },
        { title: 'Receipt No', value: 'ReceiptNo' },
        { title: 'Credit Note', value: 'CreditNote' },
        { title: 'HBL/HAWB', value: 'HBL' },
        { title: 'MBL/MAWB', value: 'MBL' },
        { title: 'JOB No', value: 'JobNo' }
    ];

    showAdvanceSearch: boolean = false;
    
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<IHistoryPaymentState>
    ) {
        super();
        this.requestSearch = this.submitSearch;
        this.requestReset = this.resetSearch;
    }

    ngOnInit(): void {
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);

        this.initForm();
        this.subscriptionSearchParamState();
    }

    initForm() {
        this.formSearch = this._fb.group({
            searchType: [this.referenceTypes[0].value],
            referenceNo: [null],
            partnerId: [],
            issuedDate: [],
            paidDate: [],
            dueDate: [],
            overdueDate: [this.overDueDays[0].id],
            paymentStatus: [[this.payments[1], this.payments[2]]]
        });

        this.partnerId = this.formSearch.controls["partnerId"];
        this.issuedDate = this.formSearch.controls["issuedDate"];
        this.paidDate = this.formSearch.controls["updatedDate"];
        this.dueDate = this.formSearch.controls["dueDate"];
        this.overdueDate = this.formSearch.controls["overdueDate"];
        this.paymentStatus = this.formSearch.controls["paymentStatus"];
        this.searchType = this.formSearch.controls["searchType"];
        this.referenceNo = this.formSearch.controls["referenceNo"];
    }

    // tslint:disable-next-line:no-any
    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.partnerId.setValue((data as Partner).id);
                break;
            default:
                break;
        }
    }

    submitSearch() {
        // tslint:disable-next-line:no-any
        const dataForm: { [key: string]: any } = this.formSearch.getRawValue();
        const status = !!dataForm.paymentStatus ? this.getSearchStatus(dataForm.paymentStatus) : null;
        const body: ISearchAccPayment = {
            // tslint:disable-next-line:no-any
            searchType: dataForm.searchType,
            referenceNos: !!dataForm.referenceNo ? dataForm.referenceNo.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: dataForm.partnerId,
            paymentStatus: status,
            overDueDays: !!dataForm.overdueDate ? +dataForm.overdueDate : OverDueDays.All,
            fromIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.startDate) ? formatDate(this.issuedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            toIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.endDate) ? formatDate(this.issuedDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            fromUpdatedDate: (!!dataForm.paidDate && !!dataForm.paidDate.startDate) ? formatDate(dataForm.paidDate.startDate, 'yyyy-MM-dd', 'en') : null,
            toUpdatedDate: (!!dataForm.paidDate && !!dataForm.paidDate.endDate) ? formatDate(dataForm.paidDate.endDate, 'yyyy-MM-dd', 'en') : null,
            fromDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.startDate) ? formatDate(dataForm.dueDate.startDate, 'yyyy-MM-dd', 'en') : null,
            toDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.endDate) ? formatDate(dataForm.dueDate.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentType: PaymentType.Invoice
        };
        this._store.dispatch(SearchListHistoryPayment(body));
        this.onSearch.emit(body);
    }
    getSearchStatus(paymentStatus: []) {
        let strStatus = null;
        if (!!paymentStatus) {
            strStatus = [];

            paymentStatus.forEach(element => {
                if (element !== 'All') {
                    strStatus.push(element);
                } else {
                    return [];
                }
            });
        }
        return strStatus;

    }

    resetSearch() {
        this.formSearch.reset();
        this.initForm();
        this._store.dispatch(SearchListHistoryPayment({ paymentStatus: this.getSearchStatus(this.paymentStatus.value), paymentType: PaymentType.Invoice, overDueDays: OverDueDays.All }));
        // this.onSearch.emit({ paymentStatus: this.getSearchStatus(this.paymentStatus.value), paymentType: PaymentType.Invoice, overDueDays: OverDueDays.All });
    }

    selelectedStatus(event: string) {
        const currStatus = this.paymentStatus.value;
        if (currStatus.filter(x => x === 'All').length > 0 && event !== 'All') {
            currStatus.splice(0);
            currStatus.push(event);
            this.paymentStatus.setValue(currStatus);

        }
        if (event === 'All') {
            this.paymentStatus.setValue(['All']);
        }

    }
    subscriptionSearchParamState() {
        this._store.select(getDataSearchHistoryPaymentState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (data) {
                        console.log('sub', data)
                        let formData: any = {
                            searchType: data.searchType ? data.searchType : this.referenceTypes[0].value,
                            referenceNo: !!data.referenceNos && !!data.referenceNos.length ? data.referenceNos.join('\n') : null,
                            partnerId: data.partnerId ? data.partnerId : null,
                            paymentStatus: this.paymentStatus.value,
                            overdueDate: data.overDueDays ? data.overDueDays : this.overDueDays[0].id,
                            paidDate: (!!data?.fromPaidDate && !!data?.toPaidDate) ?
                                { startDate: new Date(data?.fromPaidDate), endDate: new Date(data?.toPaidDate) } : null,
                            dueDate: (!!data?.fromDueDate && !!data?.toDueDate) ?
                                { startDate: new Date(data?.fromDueDate), endDate: new Date(data?.toDueDate) } : null,
                            issuedDate: (!!data?.fromIssuedDate && !!data?.toIssuedDate) ?
                                { startDate: new Date(data?.fromIssuedDate), endDate: new Date(data?.toIssuedDate) } : null,
                        };
                        this.formSearch.patchValue(formData);
                    }
                }
            );
    }
}

interface ISearchAccPayment {
    searchType: string;
    referenceNos: string;
    partnerId: string;
    paymentStatus: string[];
    overDueDays: number;
    fromIssuedDate: string;
    toIssuedDate: string;
    fromUpdatedDate: string;
    toUpdatedDate: string;
    fromDueDate: string;
    toDueDate: string;
    paymentType: number;
}
export enum PaymentType {
    Invoice = 0,
    OBH = 1
}


