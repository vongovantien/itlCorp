import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-create-job-fcl-import',
    templateUrl: './create-job-fcl-import.component.html',
})
export class SeaFCLImportCreateJobComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
