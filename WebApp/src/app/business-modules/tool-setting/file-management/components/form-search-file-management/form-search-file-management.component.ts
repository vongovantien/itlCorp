import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup } from '@angular/forms';
import { JobConstants } from '@constants';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { DataService } from '@services';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-file-management',
    templateUrl: './form-search-file-management.component.html',
})

export class FormSearchFileManagementComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<Partial<IFileManageSearch> | any> = new EventEmitter<Partial<IFileManageSearch> | any>();
    @Output() onReset: EventEmitter<Partial<IFileManageSearch> | any> = new EventEmitter<Partial<IFileManageSearch> | any>();
    @Input() tabType: string = '';
    referenceTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Job ID', value: 0 },
        { title: 'House Bill', value: 1 },
        { title: 'Mater Bill', value: 2 },
    ];
    referenceNo: AbstractControl;
    searchType: AbstractControl;
    dateModes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Create Date', value: 0 },]
    dateMode: AbstractControl;
    date: AbstractControl;
    accountantType: AbstractControl;
    accountantTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Cash Receipt', value: 0 },
        { title: 'Cash Payment', value: 1 },
        { title: 'Debit Slip', value: 2 },
        { title: 'Credit Slip', value: 3 },
        { title: 'Purchasing Note', value: 4 },
        { title: 'Other Entry', value: 5 },
        { title: 'All', value: 6 },
    ]
    formSearchFile: FormGroup;
    isAcc: boolean = false;
    ruleName: AbstractControl;
    serviceBuying: AbstractControl;
    serviceSelling: AbstractControl;
    //accTypeSelected: number[] = [];


    constructor(
        protected _dataService: DataService,
        protected _catalogueRepo: CatalogueRepo,
        protected _systemRepo: SystemRepo,
        protected _fb: FormBuilder
    ) {
        super();
        this.requestSearch = this.submitSearch;
        this.requestReset = this.submitReset;
    }

    ngOnInit(): void {
        this.initFormSearch();
        this.submitSearch(this.formSearchFile.value);
    }

    initValue() {
    }

    initFormSearch() {
        if (this.tabType === 'fileAccManage') {
            this.referenceTypes = [
                { title: 'Accountant No', value: 3 },
                { title: 'Invoice No', value: 4 },
            ]
            this.isAcc = true;
            this.dateModes = [
                { title: 'Create Date', value: 0 },
                { title: 'Accounting Date', value: 1 },];
        }
        this.formSearchFile = this._fb.group({
            dateMode: [this.dateModes[0].value],
            date: [{
                startDate: new Date(JobConstants.DEFAULT_RANGE_DATE_SEARCH.fromDate),
                endDate: new Date(JobConstants.DEFAULT_RANGE_DATE_SEARCH.toDate),
            }],
            searchType: [this.referenceTypes[0].value],
            referenceNo: [],
            accountantType: [[this.accountantTypes[6].value]]
        });
        this.dateMode = this.formSearchFile.controls['dateMode'];
        this.date = this.formSearchFile.controls['date'];
        this.searchType = this.formSearchFile.controls["searchType"];
        this.referenceNo = this.formSearchFile.controls["referenceNo"];
        this.accountantType = this.formSearchFile.controls["accountantType"];

    }

    submitSearch(formSearch: any) {
        // if (this.isAcc) {
        //     this.accountantType.setValue([this.accountantTypes[6].value]);
        // }
        const bodySearch: Partial<IFileManageSearch> = {
            referenceNo: formSearch.referenceNo,
            referenceType: formSearch.searchType,
            fromDate: !!formSearch.date?.startDate ? formatDate(formSearch.date.startDate, "yyyy-MM-dd", 'en') : new Date(JobConstants.DEFAULT_RANGE_DATE_SEARCH.fromDate),
            toDate: !!formSearch.date?.endDate ? formatDate(formSearch.date.endDate, "yyyy-MM-dd", 'en') : new Date(JobConstants.DEFAULT_RANGE_DATE_SEARCH.toDate),
            dateMode: formSearch.dateMode,
            accountantTypes: this.isAcc ? this.accountantType.value : null,
        };
        this.onSearch.emit(bodySearch);
    }

    submitReset() {
        const bodySearch: Partial<IFileManageSearch> = {
            referenceNo: null,
            referenceType: this.referenceTypes[0].value,
            fromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            toDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0),
            dateMode: this.accountantTypes[0].value,
            accountantTypes: this.isAcc ? [6] : null,
        };
        this.referenceNo.setValue(null)
        this.date.setValue({
            startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            endDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)
        });
        this.searchType.setValue(this.referenceTypes[0].value);
        this.dateMode.setValue(this.dateModes[0].value);
        if (this.isAcc) {
            this.accountantType.setValue([this.accountantTypes[6].value]);
        }
        this.onReset.emit(bodySearch);
    }

    resetDate(control: FormControl | AbstractControl) {
        this.date.setValue({
            startDate: null,
            endDate: null
        });
        control.setValue(null);
    }

    selelectedSelect(event: any) {
        const currTrans = this.accountantType.value;
        console.log(currTrans);
        console.log(event);
        if (currTrans.filter(x => x === 6).length > 0 && event !== 6) {
            currTrans.splice(0);
            currTrans.push(event.value);
            this.accountantType.setValue(currTrans);
            console.log(currTrans);
        }
        console.log(currTrans);

        if (event.value === 6) {
            this.accountantType.setValue([6]);
        }
    }

}
interface IFileManageSearch {
    referenceType: string,
    referenceNo: string,
    fromDate: any,
    toDate: any,
    dateMode: string,
    accountantTypes: number[]
}
