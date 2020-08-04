import { Component, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { User, Currency } from 'src/app/shared/models';
import { formatDate } from '@angular/common';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'settle-payment-form-search',
    templateUrl: './form-search-settlement.component.html'
})

export class SettlementFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ISearchSettlePayment> = new EventEmitter<ISearchSettlePayment>();

    formSearch: FormGroup;
    referenceNo: AbstractControl;
    requester: AbstractControl;
    requestDate: AbstractControl;
    modifiedDate: AbstractControl;
    statusApproval: AbstractControl;
    paymentMethod: AbstractControl;
    currencyId: AbstractControl;

    statusApprovals: CommonInterface.ICommonTitleValue[];
    statusPayments: CommonInterface.ICommonTitleValue[];
    paymentMethods: CommonInterface.ICommonTitleValue[] = [];

    userLogged: User;

    currencies: Observable<Currency[]>;
    requesters: any[] = [];
    requesterActive: any[] = [];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,

    ) {
        super();
        this.requestSearch = this.onSubmit;
    }

    ngOnInit() {
        this.initForm();
        this.initDataInform();
        this.getCurrency();
    }

    getCurrency() {
        this.currencies = this._catalogueRepo.getListCurrency();
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
            paymentMethod: [],
            currencyId: [],

        });

        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.requester = this.formSearch.controls['requester'];
        this.requestDate = this.formSearch.controls['requestDate'];
        this.modifiedDate = this.formSearch.controls['modifiedDate'];
        this.statusApproval = this.formSearch.controls['statusApproval'];
        this.paymentMethod = this.formSearch.controls['paymentMethod'];
        this.currencyId = this.formSearch.controls['currencyId'];
    }

    initDataInform() {
        this.statusApprovals = this.getStatusApproval();
        this.statusPayments = this.getStatusPayment();
        this.paymentMethods = this.getMethod();

        this.getUserLogged();
        this.getUsers();
    }

    onSubmit() {
        const body: ISearchSettlePayment = {
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            advanceModifiedDateFrom: !!this.modifiedDate.value && !!this.modifiedDate.value.startDate ? formatDate(this.modifiedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            advanceModifiedDateTo: !!this.modifiedDate.value && !!this.modifiedDate.value.endDate ? formatDate(this.modifiedDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            requestDateFrom: !!this.requestDate.value && !!this.requestDate.value.startDate ? formatDate(this.requestDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            requestDateTo: !!this.requestDate.value && !!this.requestDate.value.endDate ? formatDate(this.requestDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentMethod: !!this.paymentMethod.value ? this.paymentMethod.value.value : 'All',
            statusApproval: !!this.statusApproval.value ? this.statusApproval.value.value : 'All',
            currencyId: !!this.currencyId.value ? this.currencyId.value.id : 'All',
            requester: this.requester.value.length > 0 ? this.requester.value[0].id : this.userLogged.id
        };
        this.onSearch.emit(body);
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
        this.resetFormControl(this.currencyId);

        this.onSearch.emit(<any>{ requester: this.userLogged.id });
    }

    getUsers() {
        this._systemRepo.getSystemUsers({})
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (users: any) => {
                    if (!!users) {
                        this.requesters = users.map((item: any) => ({ text: item.username, id: item.id }));
                        this.requesterActive = [this.requesters.filter(stf => stf.id === this.userLogged.id)[0]];
                        this.requester.setValue(this.requesterActive);
                    }
                },
            );
    }

    onRemoveDataFormInfo(data: any, type: string) {
        if (type === 'requester') {
            this.requester.setValue([]);
        }
    }
}

interface ISearchSettlePayment {
    referenceNos: string[];
    requester: string;
    requestDateFrom: string;
    requestDateTo: string;
    advanceModifiedDateFrom: string;
    advanceModifiedDateTo: string;
    paymentMethod: string;
    statusApproval: string;
    currencyId: string;
}