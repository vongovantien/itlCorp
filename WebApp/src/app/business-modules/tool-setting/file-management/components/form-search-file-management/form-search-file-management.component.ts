import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { DataService } from '@services';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-file-management',
    templateUrl: './form-search-file-management.component.html',
})

export class FormSearchFileManagementComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<Partial<IFileManageSearch> | any> = new EventEmitter<Partial<IFileManageSearch> | any>();
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
    ]
    @Output() onSearchF: EventEmitter<Partial<IFileManageSearch> | any> = new EventEmitter<Partial<IFileManageSearch> | any>();
    formSearchFileManagement: FormGroup;
    @Input() tabType: string = '';
    isAcc: boolean = false;
    ruleName: AbstractControl;
    serviceBuying: AbstractControl;
    serviceSelling: AbstractControl;

    constructor(
        protected _dataService: DataService,
        protected _catalogueRepo: CatalogueRepo,
        protected _systemRepo: SystemRepo,
        protected _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit(): void {
        this.initFormSearch();
        this.submitSearch(this.formSearchFileManagement.value);
    }

    initValue() {
    }

    initFormSearch() {

        if (this.tabType === 'fileAccManage') {
            this.referenceTypes = [
                { title: 'Accountant No', value: 3 },
            ]
            this.isAcc = true;
            this.dateModes = [
                { title: 'Create Date', value: 0 },
                { title: 'Accounting Date', value: 1 },]
            //this.submitSearch(this.formSearchFileManagement?.value);
        }
        this.formSearchFileManagement = this._fb.group({
            dateMode: [this.dateModes[0].value],
            date: [{
                startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
                endDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)
            }],
            searchType: [this.referenceTypes[0].value],
            referenceNo: [],
            accountantType: [this.accountantTypes[0].value]
        });
        this.dateMode = this.formSearchFileManagement.controls['dateMode'];
        this.date = this.formSearchFileManagement.controls['date'];
        this.searchType = this.formSearchFileManagement.controls["searchType"];
        this.referenceNo = this.formSearchFileManagement.controls["referenceNo"];
        this.accountantType = this.formSearchFileManagement.controls["accountantType"];
    }

    submitSearch(formSearch: any) {
        console.log(formSearch.accountantType);

        const bodySearch: Partial<IFileManageSearch> = {
            referenceNo: formSearch.referenceNo,
            referenceType: formSearch.searchType,
            fromDate: !!formSearch.date?.startDate ? formatDate(formSearch.date.startDate, "yyyy-MM-dd", 'en') : null,
            toDate: !!formSearch.date?.endDate ? formatDate(formSearch.date.endDate, "yyyy-MM-dd", 'en') : null,
            dateMode: formSearch.dateMode,
            accountantTypes: this.isAcc ? formSearch.accountantType == 0 ? [0] : formSearch.accountantType : null
        };
        console.log(bodySearch);
        this.onSearch.emit(bodySearch);
    }

    submitReset() {
        const bodySearch: Partial<IFileManageSearch> = {
            referenceNo: null,
            referenceType: this.referenceTypes[0].value,
            fromDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            toDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0),
            dateMode: this.accountantTypes[0].value,
            accountantTypes: this.isAcc ? [0] : null
        };
        this.referenceNo.setValue(null)
        this.date.setValue({
            startDate: new Date(new Date().getFullYear(), new Date().getMonth(), 1),
            endDate: new Date(new Date().getFullYear(), new Date().getMonth() + 1, 0)
        });
        this.searchType.setValue(this.referenceTypes[0].value);
        this.dateMode.setValue(this.dateModes[0].value);
        if (this.isAcc) {
            this.accountantType.setValue(this.accountantTypes[0].value);
        }
        this.onSearch.emit(bodySearch);
    }

    // ngOnChanges(changes: any): void {
    //     console.log(changes);

    //     if (changes.tabType.currentValue === 'fileAccManage') {
    //         this.referenceTypes = [
    //             { title: 'House Bill', value: 1 },
    //             { title: 'Mater Bill', value: 2 },
    //             { title: 'Accountant No', value: 3 },
    //         ]
    //         this.isAcc = true;
    //         this.submitSearch(this.formSearchFileManagement?.value);
    //     }
    // }
}
interface IFileManageSearch {
    referenceType: string,
    referenceNo: string,
    fromDate: any,
    toDate: any,
    dateMode: string,
    accountantTypes: number[],
}
