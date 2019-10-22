import { Component, Output, EventEmitter } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
    selector: 'form-search-permission',
    templateUrl: './form-search-permission.component.html',
})
export class PermissionFormSearchComponent extends AppForm {

    @Output() onSearch: EventEmitter<ISearchPermission | any> = new EventEmitter<ISearchPermission | any>();

    formSearch: FormGroup;
    roles: CommonInterface.IValueDisplay[];
    statuss: CommonInterface.IValueDisplay[];

    constructor(
        protected _fb: FormBuilder
    ) {
        super();

        this.requestReset = this.resetFormSearch;
        this.requestSearch = this.submitSearch;
        this.roles = [
            { displayName: 'Customer Service', value: 'CS' },
            { displayName: 'Field OPS', value: 'OPS' },
            { displayName: 'Sale', value: "Sale" },
            { displayName: 'Accountant', value: 'Accountant' },
            { displayName: 'Admin', value: 'Admin' },
        ];

        this.statuss = [
            { displayName: 'Active', value: true },
            { displayName: 'Inactive', value: false }
        ];
    }

    ngOnInit(): void {

        this.initFormSearch();
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            permissionName: [],
            role: [],
            status: [],
        });
    }


    submitSearch() {
        const body: ISearchPermission = {
            permissionName: '',
            status: true,
            role: '',
        };

        console.log("submit Search", body);

        this.onSearch.emit(body);
    }

    resetFormSearch() {
        this.formSearch.reset();
        this.onSearch.emit({});
    }
}

interface ISearchPermission {
    permissionName: string;
    status: boolean;
    role: string;
}


