import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';

import { Observable } from 'rxjs';
import { formatDate } from '@angular/common';


@Component({
    selector: 'form-search-account-receivable-payable',
    templateUrl: './form-search-account-receivable-payable.component.html'
})
export class AccountReceivePayableFormSearchComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<Partial<ISearchAccReceivePayble>> = new EventEmitter<Partial<ISearchAccReceivePayble>>();

    formSearch: FormGroup;

    partnerId: AbstractControl;
    referenceNo: AbstractControl;
    issuedDate: AbstractControl;
    updatedDate: AbstractControl;
    dueDate: AbstractControl;
    overdueDate: AbstractControl;
    paymentStatus: AbstractControl;

    partners: Observable<Partner[]>;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    payments: CommonInterface.INg2Select[] = [
        { id: '', text: 'All' },
        { id: 'Unpaid', text: 'UnPaid' },
        { id: 'Paid A Part', text: 'Paid A Part' },
        { id: 'Paid', text: 'Paid' },
    ];
    overDueDays: CommonInterface.INg2Select[] = [
        { id: 0, text: 'All' },
        { id: 1, text: '01-15 days' },
        { id: 2, text: '16-30 days' },
        { id: 3, text: '31-60 days' },
        { id: 4, text: '60-90 days' },
    ];

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
        this.requestSearch = this.submitSearch;
        this.requestReset = this.resetSearch;
    }

    ngOnInit(): void {
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL);

        this.initForm();
    }

    initForm() {
        this.formSearch = this._fb.group({
            referenceNo: [],
            partnerId: [],
            issuedDate: [],
            updatedDate: [],
            dueDate: [],
            overdueDate: [[this.overDueDays[0]]],
            paymentStatus: [[this.payments[0]]]
        });

        this.partnerId = this.formSearch.controls["partnerId"];
        this.issuedDate = this.formSearch.controls["issuedDate"];
        this.updatedDate = this.formSearch.controls["updatedDate"];
        this.dueDate = this.formSearch.controls["dueDate"];
        this.overdueDate = this.formSearch.controls["overdueDate"];
        this.paymentStatus = this.formSearch.controls["paymentStatus"];
    }

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
        const dataForm: { [key: string]: any } = this.formSearch.getRawValue();

        const body: ISearchAccReceivePayble = {
            referenceNos: !!dataForm.referenceNo ? dataForm.referenceNo.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: dataForm.partnerId,
            paymentStatus: !!dataForm.paymentStatus ? dataForm.paymentStatus : null,
            overDueDays: !!dataForm.overdueDate ? dataForm.overdueDate[0].id : null,
            fromIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.startDate) ? formatDate(this.issuedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            toIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.endDate) ? formatDate(this.issuedDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            fromUpdatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.startDate) ? formatDate(dataForm.updatedDate.startDate, 'yyyy-MM-dd', 'en') : null,
            toUpdatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.endDate) ? formatDate(dataForm.updatedDate.endDate, 'yyyy-MM-dd', 'en') : null,
            fromDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.startDate) ? formatDate(dataForm.dueDate.startDate, 'yyyy-MM-dd', 'en') : null,
            toDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.endDate) ? formatDate(dataForm.dueDate.endDate, 'yyyy-MM-dd', 'en') : null
        };
        console.log(body);

        this.onSearch.emit(body);
    }

    resetSearch() {
        this.resetKeywordSearchCombogrid();
        this.formSearch.reset();
        this.onSearch.emit({});
    }
}

interface ISearchAccReceivePayble {
    referenceNos: string;
    partnerId: string;
    paymentStatus: string;
    overDueDays: number;
    fromIssuedDate: string;
    toIssuedDate: string;
    fromUpdatedDate: string;
    toUpdatedDate: string;
    fromDueDate: string;
    toDueDate: string;
}

