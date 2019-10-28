import { Component } from '@angular/core';
import { PermissionFormSearchComponent } from '../form-search-permission/form-search-permission.component';
import { FormBuilder, FormGroup } from '@angular/forms';

@Component({
    selector: 'form-create-permission',
    templateUrl: './form-create-permission.component.html',
})
export class PermissionFormCreateComponent extends PermissionFormSearchComponent {

    formCreate: FormGroup;
    types: CommonInterface.IValueDisplay[];


    constructor(
        protected _fb: FormBuilder
    ) {
        super(_fb);
        console.log("form create constructor");
    }

    ngOnInit(): void {
        this.types = [
            { displayName: 'Standard', value: 'Standard' },
            { displayName: 'User', value: 'User' },
        ];

        this.initFormCreate();

    }

    initFormCreate() {
        this.formCreate = this._fb.group({
            permissionName: [],
            role: [],
            type: [],
            status: [],
        });

        this.formCreate.controls['type'].setValue(this.types[0]);
    }
}
