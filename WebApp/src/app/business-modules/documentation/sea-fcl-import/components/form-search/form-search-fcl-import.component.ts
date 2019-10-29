import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-fcl-import',
    templateUrl: './form-search-fcl-import.component.html',
})
export class SeaFCLImportManagementFormSearchComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
