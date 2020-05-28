import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-accounting-management',
    templateUrl: './form-search-accounting-management.component.html'
})

export class AccountingManagementFormSearchComponent extends AppForm implements OnInit {
    constructor() {
        super()
    }

    ngOnInit() { }
}
