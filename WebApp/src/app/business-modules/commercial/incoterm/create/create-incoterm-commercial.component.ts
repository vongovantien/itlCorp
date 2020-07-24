import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'create-incoterm-commercial',
    templateUrl: './create-incoterm-commercial.component.html',
})
export class CommercialCreateIncotermComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }

    onSave() {

    }

    gotoList() {

    }

}
