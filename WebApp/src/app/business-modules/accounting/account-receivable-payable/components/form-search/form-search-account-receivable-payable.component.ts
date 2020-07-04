import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { formatDate } from '@angular/common';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';

import { Observable } from 'rxjs';

enum OverDueDays {
    All,
    Between1_15,
    Between16_30,
    Between31_60,
    Between61_90
}

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

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = [
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name Local' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    payments: CommonInterface.INg2Select[] = [
        { id: 'All', text: 'All' },
        { id: 'Unpaid', text: 'Unpaid' },
        { id: 'Paid A Part', text: 'Paid A Part' },
        { id: 'Paid', text: 'Paid' },
    ];
    overDueDays: CommonInterface.INg2Select[] = [
        { id: '0', text: 'All' },
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
            paymentStatus: [[this.payments[1], this.payments[2]]]
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
        const status = !!dataForm.paymentStatus ? this.getSearchStatus(dataForm.paymentStatus) : null;
        const body: ISearchAccReceivePayble = {
            referenceNos: !!dataForm.referenceNo ? dataForm.referenceNo.trim().replace(SystemConstants.CPATTERN.LINE, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: dataForm.partnerId,
            paymentStatus: status,
            overDueDays: !!dataForm.overdueDate ? +dataForm.overdueDate[0].id : OverDueDays.All,
            fromIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.startDate) ? formatDate(this.issuedDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            toIssuedDate: (!!this.issuedDate.value && !!this.issuedDate.value.endDate) ? formatDate(this.issuedDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            fromUpdatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.startDate) ? formatDate(dataForm.updatedDate.startDate, 'yyyy-MM-dd', 'en') : null,
            toUpdatedDate: (!!dataForm.updatedDate && !!dataForm.updatedDate.endDate) ? formatDate(dataForm.updatedDate.endDate, 'yyyy-MM-dd', 'en') : null,
            fromDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.startDate) ? formatDate(dataForm.dueDate.startDate, 'yyyy-MM-dd', 'en') : null,
            toDueDate: (!!dataForm.dueDate && !!dataForm.dueDate.endDate) ? formatDate(dataForm.dueDate.endDate, 'yyyy-MM-dd', 'en') : null,
            paymentType: PaymentType.Invoice
        };

        this.onSearch.emit(body);
    }
    getSearchStatus(paymentStatus: []) {
        let strStatus = null;
        if (!!paymentStatus) {
            strStatus = [];

            paymentStatus.forEach(element => {
                if (element['id'] !== 'All') {
                    strStatus.push(element['id']);
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
        this.onSearch.emit({ paymentStatus: this.getSearchStatus(this.paymentStatus.value), paymentType: PaymentType.Invoice, overDueDays: OverDueDays.All });
    }

    selelectedStatus(event) {
        const currStatus = this.paymentStatus.value;
        if (currStatus.filter(x => x.id === 'All').length > 0 && event.id !== 'All') {
            currStatus.splice(0);
            currStatus.push(event);
            this.paymentStatus.setValue(currStatus);

        }
        if (event.id === 'All') {
            const onlyAllObj = currStatus.filter(ele => ele.id === 'All');
            this.paymentStatus.setValue(onlyAllObj);
        }

    }
}

interface ISearchAccReceivePayble {
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


