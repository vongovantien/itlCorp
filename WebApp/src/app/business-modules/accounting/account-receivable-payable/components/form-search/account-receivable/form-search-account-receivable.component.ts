import { Component, OnInit, EventEmitter, Output } from '@angular/core';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { formatDate } from '@angular/common';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { AppForm } from 'src/app/app.form';

import { Observable } from 'rxjs';


//set value to binding input
const OverDueDaysValues = [
    { from: null, to: null },
    { from: 1, to: 15 },
    { from: 16, to: 30 },
    { from: 30, to: null },
];
const DebitRatesValues = [
    { from: null, to: null },
    { from: 0, to: 50 },
    { from: 50, to: 70 },
    { from: 70, to: 100 },
    { from: 100, to: 120 },
    { from: 120, to: null },
    { from: null, to: null },
];




@Component({
    selector: 'form-search-account-receivable',
    templateUrl: './form-search-account-receivable.component.html'
})
export class AccountReceivableFormSearchComponent extends AppForm implements OnInit {

    @Output() onSearchReceivable: EventEmitter<Partial<any>> = new EventEmitter<Partial<any>>();



    formSearch: FormGroup;

    arType: CommonEnum.TabTypeAccountReceivableEnum = CommonEnum.TabTypeAccountReceivableEnum.TrialOrOffical;

    partnerId: AbstractControl;

    overdueDays: AbstractControl;
    fromOverdueDays: AbstractControl;
    toOverdueDays: AbstractControl;

    debitRate: AbstractControl;
    fromDebitRate: AbstractControl;
    toDebitRate: AbstractControl;

    agreementStatus: AbstractControl;

    agreementExpiredDays: AbstractControl;


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
        { id: 'All', text: 'All' },
        { id: 'Active', text: 'Active' },
        { id: 'Inactive', text: 'Inactive' },

    ];
    agreementExpiredDayList: CommonInterface.INg2Select[] = [
        { id: 'All', text: 'All' },
        { id: 'Normal', text: 'Normal' },
        { id: '30Days', text: '< (-30) Days' },
        { id: '15Days', text: '< (-15) Days' },
        { id: 'Expired', text: 'Expired' },
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

            agreementExpiredDays: [[this.agreementExpiredDayList[0]]],


            salesManId: [],
            officalId: [],
        });

        this.partnerId = this.formSearch.controls["partnerId"];

        this.overdueDays = this.formSearch.controls["overdueDays"];
        this.fromOverdueDays = this.formSearch.controls["fromOverdueDays"];
        this.toOverdueDays = this.formSearch.controls["toOverdueDays"];

        this.debitRate = this.formSearch.controls["debitRate"];
        this.fromDebitRate = this.formSearch.controls["fromDebitRate"];
        this.toDebitRate = this.formSearch.controls["toDebitRate"];

        this.agreementStatus = this.formSearch.controls["agreementStatus"];

        this.agreementExpiredDays = this.formSearch.controls["agreementExpiredDays"];


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
    //
    onSelectBindingInput(item: any, fieldName: string) {
        switch (fieldName) {
            case 'OverDueDays':
                this.fromOverdueDays.setValue(item.id === '0' ? OverDueDaysValues[0].from : OverDueDaysValues[item.id].from);
                this.toOverdueDays.setValue(item.id === '0' ? OverDueDaysValues[0].to : OverDueDaysValues[item.id].to);
                break;
            case 'DebitRates':
                this.fromDebitRate.setValue(item.id === '0' ? DebitRatesValues[0].from : DebitRatesValues[item.id].from);
                this.toDebitRate.setValue(item.id === '0' ? DebitRatesValues[0].to : DebitRatesValues[item.id].to);
                break;
            default:
                break;
        }
    }
    //
    //
    submitSearch() {
        const dataForm: { [key: string]: any } = this.formSearch.getRawValue();
        const body = {
            arType: this.arType,
            acRefId: dataForm.partnerId,
            overDueDay: dataForm.overdueDays[0].id === '0' ? 0 : dataForm.overdueDays[0].id,
            debitRateFrom: dataForm.fromDebitRate,
            debitRateTo: dataForm.toDebitRate,
            agreementStatus: dataForm.agreementStatus[0].id,
            agreementExpiredDay: dataForm.agreementExpiredDays[0].id,
            salesmanId: dataForm.salesManId,
            officeId: dataForm.officalId,
        }
        //format body 
        this.onSearchReceivable.emit(body);

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
