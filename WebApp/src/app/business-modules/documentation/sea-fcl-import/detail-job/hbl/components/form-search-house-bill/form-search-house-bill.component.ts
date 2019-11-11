import { Component, OnInit } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-house-bill',
    templateUrl: './form-search-house-bill.component.html',
    styleUrls: ['./form-search-house-bill.component.scss']
})
export class FormSearchHouseBillComponent extends AppForm {
    filterTypes: CommonInterface.ICommonTitleValue[];
    formSearch: FormGroup;
    searchText: AbstractControl;
    filterType: AbstractControl;
    serviceDate: AbstractControl;
    constructor(
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit() {
        this.initFormSearch();
        this.filterTypes = [
            { title: 'MBL', value: 'mawb' },
            { title: 'Customer', value: 'customerId' },
            { title: 'Saleman', value: 'saleMan_Id' }
        ];
        this.filterType.setValue(this.filterTypes[0]);
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            'searchText': [],
            'filterType': [],
            'serviceDate': [],

        });

        this.searchText = this.formSearch.controls['searchText'];
        this.filterType = this.formSearch.controls['filterType'];
        this.serviceDate = this.formSearch.controls['serviceDate'];
    }
}
interface ISearchDataHbl {
    all: string;
    mawb: string;
    customerName: string;
    salemanName: string;
}

