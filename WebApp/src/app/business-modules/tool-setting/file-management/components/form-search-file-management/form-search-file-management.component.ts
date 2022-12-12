import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { DataService } from '@services';
import { AppForm } from 'src/app/app.form';
import { RuleLinkFee } from 'src/app/shared/models/tool-setting/rule-link-fee';

@Component({
    selector: 'form-search-file-management',
    templateUrl: './form-search-file-management.component.html',
})

export class FormSearchFileManagementComponent extends AppForm implements OnInit {

    @Output() onSearch: EventEmitter<Partial<IFileManageSearch> | any> = new EventEmitter<Partial<IFileManageSearch> | any>();
    referenceTypes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Job ID', value: 0 }
    ];
    referenceNo: AbstractControl;
    searchType: AbstractControl;
    dateModes: CommonInterface.ICommonTitleValue[] = [
        { title: 'Create Date', value: 0 },
        { title: 'Accounting Date', value: 1 },]
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
    @Output() onSearchF: EventEmitter<Partial<IFileManageSearch> | any> = new EventEmitter<Partial<IFileManageSearch> | any>(); formSearchFileManagement: FormGroup;
    @Input() tabType: string = '';
    rule: RuleLinkFee = new RuleLinkFee();
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
                { title: 'House Bill', value: 0 },
                { title: 'Mater Bill', value: 1 },
                { title: 'Accountant No', value: 2 },
            ]
            this.isAcc = true;
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
        const bodySearch: Partial<IFileManageSearch> = {
            referenceNo: formSearch.referenceNo,
            referenceType: formSearch.searchType,
            fromDate: !!formSearch.date?.startDate ? formatDate(formSearch.date.startDate, "yyyy-MM-dd", 'en') : null,
            toDate: !!formSearch.date?.endDate ? formatDate(formSearch.date.endDate, "yyyy-MM-dd", 'en') : null,
            dateMode: formSearch.dateMode,
            accountantType: this.isAcc ? formSearch.accountantType : null
        };
        console.log(bodySearch);
        this.onSearch.emit(bodySearch);
    }

    submitReset() {
        this.formSearchFileManagement.reset();
        const bodySearch: Partial<IFileManageSearch> = {
        };
        console.log(bodySearch);
        this.onSearch.emit(bodySearch);
    }

    resetDate() {
        this.date.setValue(null);
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
    fromDate: string,
    toDate: string,
    dateMode: string,
    accountantType: number[],
}
