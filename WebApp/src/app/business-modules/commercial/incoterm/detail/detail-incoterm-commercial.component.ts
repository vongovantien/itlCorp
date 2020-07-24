import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'detail-incoterm-commercial',
    templateUrl: './detail-incoterm-commercial.component.html',
})
export class CommercialDetailIncotermComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
