import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-create-commercial',
    templateUrl: './create-commercial.component.html',
})
export class CommercialCreateComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
