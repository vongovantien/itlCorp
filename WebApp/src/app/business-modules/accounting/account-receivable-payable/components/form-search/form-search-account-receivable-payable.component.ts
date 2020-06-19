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
        { id: 'Part A Part', text: 'Part A Part' },
        { id: 'Paid', text: 'Paid' },
    ];
    overDueDays: CommonInterface.INg2Select[] = [
        { id: '', text: 'All' },
        { id: '1-15', text: '01-15 days' },
        { id: '16-30', text: '16-30 days' },
        { id: '31-60', text: '31-60 days' },
        { id: '60-90', text: '60-90 days' },
    ];

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo
    ) {
        super();
        this.requestSearch = this.submitSearch;
        this.requestReset = this.resetSearrch;
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
            issuedDate: (!!dataForm.issuedDate && !!dataForm.issuedDate.startDate) ? formatDate(dataForm.issuedDate.startDate, 'yyyy-MM-dd', 'en') : null,
            updatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.startDate) ? formatDate(dataForm.updatedDate.startDate, 'yyyy-MM-dd', 'en') : null,
            dueDate: (!!dataForm.dueDate && !!dataForm.dueDate.startDate) ? formatDate(dataForm.dueDate.startDate, 'yyyy-MM-dd', 'en') : null,
            status: dataForm.paymentStatus[0].id,
            overdueDate: dataForm.overdueDate[0].id
        };
        console.log(body);

        this.onSearch.emit(body);
    }

    resetSearrch() {
        this.resetKeywordSearchCombogrid();
        this.formSearch.reset();
        this.onSearch.emit({});
    }
}

interface ISearchAccReceivePayble {
    referenceNos: string;
    partnerId: string;
    issuedDate: string;
    status: string;
    dueDate: string;
    overdueDate: string;
    updatedDate: string;
}

