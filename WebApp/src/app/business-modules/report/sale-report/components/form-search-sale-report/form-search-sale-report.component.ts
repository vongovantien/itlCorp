import { Component } from "@angular/core";
import { AppForm } from "src/app/app.form";
import { FormBuilder } from "@angular/forms";

@Component({
    selector: 'sale-report-form-search',
    templateUrl: './form-search-sale-report.component.html'
})

export class SaleReportFormSearchComponent extends AppForm {
    constructor(
        private _fb: FormBuilder
    ) {
        super();
    }

    ngOnInit() {
    }
}