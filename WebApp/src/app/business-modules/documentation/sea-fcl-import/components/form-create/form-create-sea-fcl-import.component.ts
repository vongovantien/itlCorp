import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-create-sea-fcl-import',
    templateUrl: './form-create-sea-fcl-import.component.html',
})
export class SeaFClImportFormCreateComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
