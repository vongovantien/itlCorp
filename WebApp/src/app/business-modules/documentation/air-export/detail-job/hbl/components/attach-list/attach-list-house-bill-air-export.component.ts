import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'air-export-hbl-attach-list',
    templateUrl: './attach-list-house-bill-air-export.component.html',
})
export class AirExportHBLAttachListComponent extends AppForm implements OnInit {

    attachList: string = '';

    constructor() {
        super();
    }

    ngOnInit(): void { }
}
