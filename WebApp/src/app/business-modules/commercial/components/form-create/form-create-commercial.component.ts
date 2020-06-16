import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'form-create-commercial',
    templateUrl: './form-create-commercial.component.html',
})
export class CommercialFormCreateComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
