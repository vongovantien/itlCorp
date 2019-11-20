import { Component, OnInit } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'form-manifest-sea-fcl-import',
    templateUrl: './form-manifest-sea-fcl-import.component.html'
})
export class FormManifestSeaFclImportComponent extends AppList {
    formGroup: FormGroup;
    constructor() {
        super();
    }

    ngOnInit() {

    }

}
