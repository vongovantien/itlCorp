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
    Over_30
}

enum DebitRates {
    All,
    Between0_50,
    Between50_70,
    Between70_100,
    Between100_120,
    Over_120,
    Other
}

@Component({
    selector: 'form-search-account-receivable',
    templateUrl: './form-search-account-receivable.component.html'
})
export class AccountReceivableFormSearchComponent extends AppForm implements OnInit {

    @Output() onSearchReceivable: EventEmitter<Partial<any>> = new EventEmitter<Partial<any>>();

    formSearch: FormGroup;

    partnerId: AbstractControl;

    overdueDays: AbstractControl;
    fromOverduaDays: AbstractControl;
    toOverduaDays: AbstractControl;

    debitRate: AbstractControl;
    fromDebitRate: AbstractControl;
    toDebitRate: AbstractControl;

    agreementStatus: AbstractControl;

    agreementExpiredDays: AbstractControl;
    fromAgreementExpiredDays: AbstractControl;
    toAgreementExpiredDays: AbstractControl;

    salesManId: AbstractControl;
    officalId: AbstractControl;
    ////seedData
    seedPartner: any[] = [
        {
            partnerId: '111111',
            abbrName: 'Cristian',
            nameLocal: 'Cris',
            taxCode: 123
        },
        {
            partnerId: '222222',
            abbrName: 'Maple',
            nameLocal: 'SwordArt',
            taxCode: 456
        }
    ];
    seedSalesMan: any[] = [
        {
            id: 'SM01',
            userName: 'intern.007',
            fullName: 'Thiện Nguyễn'
        },
        {
            id: 'SM02',
            userName: 'intern.00n',
            fullName: 'Someone'
        }
    ];
    seedOffice: any[] = [
        {
            id: 'Office01',
            abbrName: 'ITL HCM',
            enName: 'Indo Trans Logictis HCMC'
        },
        {
            id: 'Office02',
            abbrName: 'ITL HN',
            enName: 'Indo Trans Logictis HNC'
        }
    ]
    //
    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = [
        { field: 'partnerId', label: 'Partner ID' },
        { field: 'abbrName', label: 'ABBR Name' },
        { field: 'nameLocal', label: 'Name Local' },
        { field: 'taxCode', label: 'Tax Code' },
    ];
    displayFieldsSalesMan: CommonInterface.IComboGridDisplayField[] = [
        { field: 'userName', label: 'User Name' },
        { field: 'fullName', label: 'Full Name' },
    ];
    displayFieldsOffice: CommonInterface.IComboGridDisplayField[] = [
        { field: 'abbrName', label: 'Abbr Name' },
        { field: 'enName', label: 'En Name' },
    ];
    //model search
    overDueDays: CommonInterface.INg2Select[] = [
        { id: '0', text: 'All' },
        { id: 1, text: '01-15 days' },
        { id: 2, text: '16-30 days' },
        { id: 3, text: 'Over 30 days' },
    ];

    debitRates: CommonInterface.INg2Select[] = [
        { id: '0', text: 'All' },
        { id: 1, text: '0% - 50%' },
        { id: 2, text: '50% - 70%' },
        { id: 3, text: '70% - 100%' },
        { id: 4, text: '100% - 120%' },
        { id: 5, text: 'Over 120%' },
        { id: 6, text: 'Other' },
    ];
    agreementStatusList: CommonInterface.INg2Select[] = [
        { id: '0', text: 'All' },
        { id: 1, text: 'Active' },
        { id: 2, text: 'Inactive' },

    ];
    constructor(
        private _fb: FormBuilder,
    ) {
        super();
        this.requestSearch = this.submitSearch;
    }

    ngOnInit(): void {
        this.initForm();
    }
    initForm() {
        this.formSearch = this._fb.group({
            partnerId: [],

            overdueDays: [[this.overDueDays[0]]],
            fromOverdueDays: [],
            toOverdueDays: [],

            debitRate: [[this.debitRates[0]]],
            fromDebitRate: [],
            toDebitRate: [],

            agreementStatus: [[this.agreementStatusList[0]]],

            agreementExpiredDays: [],
            fromAgreementExpiredDays: [],
            toAgreementExpiredDays: [],

            salesManId: [],
            officalId: [],
        });

        this.partnerId = this.formSearch.controls["partnerId"];

        this.overdueDays = this.formSearch.controls["overdueDays"];
        this.fromOverduaDays = this.formSearch.controls["fromOverduaDays"];
        this.toOverduaDays = this.formSearch.controls["toOverduaDays"];

        this.debitRate = this.formSearch.controls["debitRate"];
        this.fromDebitRate = this.formSearch.controls["fromDebitRate"];
        this.toDebitRate = this.formSearch.controls["toDebitRate"];

        this.agreementStatus = this.formSearch.controls["agreementStatus"];

        this.agreementExpiredDays = this.formSearch.controls["agreementExpiredDays"];
        this.fromAgreementExpiredDays = this.formSearch.controls["fromAgreementExpiredDays"];
        this.toAgreementExpiredDays = this.formSearch.controls["toAgreementExpiredDays"];

        this.salesManId = this.formSearch.controls["salesManId"];
        this.officalId = this.formSearch.controls["officalId"];
    }
    //
    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.partnerId.setValue(data.partnerId);
                break;
            case 'salesMan':
                this.salesManId.setValue(data.id);
                break;
            case 'offical':
                this.officalId.setValue(data.id);
                break;
            default:
                break;
        }
    }
    submitSearch() {
        const dataForm: { [key: string]: any } = this.formSearch.getRawValue();
        //format body 
        this.onSearchReceivable.emit(dataForm);

    }

}
interface ISearchAccReceivable {
    partnerId: string;
    overdueDays: string;
    fromOverduaDays: string;
    toOverduaDays: string;

    debitRate: string;
    fromDebitRate: number;
    toDebitRate: number;

    agreementStatus: number;

    agreementExpiredDays: string;
    fromAgreementExpiredDays: string;
    toAgreementExpiredDays: string;

    salesManId: string;
    officalId: string;
}