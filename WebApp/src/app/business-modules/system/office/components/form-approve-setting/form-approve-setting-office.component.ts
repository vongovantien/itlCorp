import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-approve-setting-office',
    templateUrl: './form-approve-setting-office.component.html',
})
export class OfficeFormApproveSettingComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
