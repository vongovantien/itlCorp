import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-permission',
    templateUrl: './form-search-permission.component.html',
})
export class PermissionFormSearchComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
