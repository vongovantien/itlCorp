import { Component, Output, EventEmitter } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { AppForm } from 'src/app/app.form';
import { User, Currency } from 'src/app/shared/models';

import { formatDate } from '@angular/common';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { catchError, concatMap, finalize, map, mergeMap, takeUntil } from 'rxjs/operators';
import { combineLatest } from 'rxjs';
import { Store } from '@ngrx/store';
import { getAdvancePaymentSearchParamsState, IAdvancePaymentState, reducer } from '../../store/reducers';
import { SearchList } from '../../store/actions';


@Component({
    selector: 'adv-payment-form-search',
    templateUrl: './form-search-advance-payment.component.html'
})

export class AdvancePaymentFormsearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchAdvancePayment> = new EventEmitter<ISearchAdvancePayment>();

    formSearch: FormGroup;
    referenceNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    modifiedDate: AbstractControl;
    statusApproval: AbstractControl;
    statusPayment: AbstractControl;
    paymentMethod: AbstractControl;
    currencyId: AbstractControl;

    statusApprovals: CommonInterface.ICommonTitleValue[];
    statusPayments: CommonInterface.ICommonTitleValue[];
    paymentMethods: CommonInterface.ICommonTitleValue[] = [];

    userLogged: User;

    currencies: Currency[] = [];
    requesters: any[] = [];
    requesterActive: any[] = [];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _store: Store<IAdvancePaymentState>,
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.initDataInform();
        this._store.select(getAdvancePaymentSearchParamsState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (Object.keys(data).length === 0 && data.constructor === Object) {
                        this.onSearch.emit(<any>{ requester: this.userLogged.id });
                    } else {
                        this.onSearch.emit(data);
                        this.setDataSearchFromRedux(data);
                    }
                }
            );
    }

    initForm() {
        this.formSearch = this._fb.group({
            // referenceNo: [, Validators.compose([
            //     Validators.pattern(/^[\w '_"/*\\\.,-]*$/),
            // ])],
            referenceNo: [],
            requester: [this.requesterActive],
            requestDate: [],
            modifiedDate: [],
            statusApproval: [],
            statusPayment: [],
            paymentMethod: [],
            currencyId: [],
        });

        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.requester = this.formSearch.controls['requester'];
        this.requestDate = this.formSearch.controls['requestDate'];
        this.modifiedDate = this.formSearch.controls['modifiedDate'];
        this.statusApproval = this.formSearch.controls['statusApproval'];
        this.statusPayment = this.formSearch.controls['statusPayment'];
        this.paymentMethod = this.formSearch.controls['paymentMethod'];
        this.currencyId = this.formSearch.controls['currencyId'];

    }

    initDataInform() {
        this.statusApprovals = this.getStatusApproval();
        this.statusPayments = this.getStatusPayment();
        this.paymentMethods = this.getMethod();

        this.getUserLogged();
        this.getCurrencyAndUsers();
        //this.getUsers();
    }

    setDataSearchFromRedux(data: any) {

        this.formSearch.patchValue({
            referenceNo: !!data.referenceNos && !!data.referenceNos.length ? data.referenceNos.join('\n') : null,

            requestDate: !!data.requestDateFrom && !!data.requestDateTo ? { startDate: new Date(data.requestDateFrom), endDate: new Date(data.requestDateTo) } : null,
            modifiedDate: !!data.advanceModifiedDateFrom && !!data.advanceModifiedDateTo ? { startDate: new Date(data.advanceModifiedDateFrom), endDate: new Date(data.advanceModifiedDateTo) } : null,
            statusApproval: !!data.statusApproval && data.statusApproval !== 'All' ? this.statusApprovals.filter(e => e.value === data.statusApproval)[0] : null,
            statusPayment: !!data.statusPayment && data.statusPayment !== 'All' ? this.statusPayments.filter(e => e.value === data.statusPayment)[0] : null,
            paymentMethod: !!data.paymentMethod && data.paymentMethod !== 'All' ? this.paymentMethods.filter(e => e.value === data.paymentMethod)[0] : null,

        });
    }

    onSubmit() {
        const body: ISearchAdvancePayment = {
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            advanceModifiedDateFrom: !!this.modifiedDate.value && !!this.modifiedDate.value.startDate ? formatDate(this.modifiedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            advanceModifiedDateTo: !!this.modifiedDate.value && !!this.modifiedDate.value.endDate ? formatDate(this.modifiedDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            requestDateFrom: !!this.requestDate.value && !!this.requestDate.value.startDate ? formatDate(this.requestDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            requestDateTo: !!this.requestDate.value && !!this.requestDate.value.endDate ? formatDate(this.requestDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentMethod: !!this.paymentMethod.value ? this.paymentMethod.value.value : 'All',
            statusApproval: !!this.statusApproval.value ? this.statusApproval.value.value : 'All',
            statusPayment: !!this.statusPayment.value ? this.statusPayment.value.value : 'All',
            currencyId: !!this.currencyId.value ? this.currencyId.value.id : 'All',
            requester: this.requester.value.length > 0 ? this.requester.value[0].id : this.userLogged.id
        };

        //this.onSearch.emit(body);

        this._store.dispatch(SearchList({ payload: body }));
    }

    getUserLogged() {
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
    }

    getStatusApproval(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'New', value: 'New' },
            { title: 'Request Approval', value: 'Request Approval' },
            { title: 'Leader Approved', value: 'Leader Approved' },
            { title: 'Department Manager Approved', value: 'Department Manager Approved' },
            { title: 'Accountant Manager Approved', value: 'Accountant Manager Approved' },
            { title: 'Done ', value: 'Done' },
            { title: 'Denied  ', value: 'Denied' },
        ];
    }

    getStatusPayment(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Settled', value: 'Settled' },
            { title: 'Not Settled', value: 'NotSettled' },
            { title: 'Partial Settlement', value: 'PartialSettlement' },
        ];
    }

    getMethod(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Cash', value: 'Cash' },
            { title: 'Bank Transfer', value: 'Bank' },
        ];
    }

    getCurrencyAndUsers() {
        combineLatest([
            this._catalogueRepo.getListCurrency(),
            this._systemRepo.getSystemUsers({}),
            this._store.select(getAdvancePaymentSearchParamsState)
        ]).pipe(
            map((cur, param) => ({ ...cur, param }))
        ).subscribe(
            (res) => {
                this.currencies = res[0] || [];
                this.requesters = res[1].map((item: any) => ({ text: item.username, id: item.id }));

                if (Object.keys(res[2]).length === 0 && res[2].constructor === Object) {
                    this.requester.setValue([this.requesters.filter(stf => stf.id === this.userLogged.id)[0]]);
                    this.currencyId.setValue(null);
                } else {
                    const requesterTemp = [this.requesters.filter(e => e.id === res[2].requester)[0]];
                    const currencyTemp = this.currencies.filter(e => e.id === res[2].currencyId).length <= 0 ? null
                        : this.currencies.filter(e => e.id === res[2].currencyId)[0];

                    this.requester.setValue(requesterTemp);
                    this.currencyId.setValue(currencyTemp);

                }
            }
        );
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.initDataInform();
        this.resetFormControl(this.requestDate);
        this.resetFormControl(this.modifiedDate);
        this.resetFormControl(this.referenceNo);
        this.resetFormControl(this.paymentMethod);
        this.resetFormControl(this.statusApproval);
        this.resetFormControl(this.statusPayment);
        this.resetFormControl(this.currencyId);

        this._store.dispatch(SearchList({
            payload: {}
        }));
        this.onSearch.emit(<any>{ requester: this.userLogged.id });
    }

    // getUsers() {
    //     this._systemRepo.getSystemUsers({})
    //         .pipe(
    //             catchError(this.catchError),
    //             finalize(() => { }),
    //         ).subscribe(
    //             (users: any) => {
    //                 if (!!users) {
    //                     this.requesters = users.map((item: any) => ({ text: item.username, id: item.id }));
    //                     this.requesterActive = [this.requesters.filter(stf => stf.id === this.userLogged.id)[0]];
    //                     this.requester.setValue(this.requesterActive);
    //                 }
    //             },
    //         );
    // }

    onRemoveDataFormInfo(data: any, type: string) {
        if (type === 'requester') {
            this.requester.setValue([]);
        }
    }
}

interface ISearchAdvancePayment {
    referenceNos: string[];
    requester: string;
    requestDateFrom: string;
    requestDateTo: string;
    advanceModifiedDateFrom: string;
    advanceModifiedDateTo: string;
    paymentMethod: string;
    statusApproval: string;
    statusPayment: string;
    currencyId: string;
}

