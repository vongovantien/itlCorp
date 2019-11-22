import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-create-sea-fcl-export',
    templateUrl: './form-create-fcl-export.component.html'
})

export class SeaFCLExportFormCreateComponent extends AppForm implements OnInit {

    constructor() {
        super();
    }

    ngOnInit() { }
}
