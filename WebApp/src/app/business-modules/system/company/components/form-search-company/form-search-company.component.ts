import { Component } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-search-company',
    templateUrl: './form-search-company.component.html',
})
export class CompanyInfomationFormSearchComponent extends AppForm {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
