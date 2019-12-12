import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { FormGroup, FormBuilder } from '@angular/forms';

@Component({
    selector: 'form-add-partner',
    templateUrl: './form-add-partner.component.html'
})

export class FormAddPartnerComponent extends AppList {
    formGroup: FormGroup;
    isSubmitted: boolean = false;
    constructor(
        private _fb: FormBuilder
    ) {
        super();
    }
    ngOnInit() { }

    initForm() {

    }

}