import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'hbl-goods-summary',
    templateUrl: './hbl-good-summary.component.html'
})

export class ShareBussinessHBLGoodSummaryComponent extends AppForm implements OnInit {
    constructor() {
        super();
    }

    ngOnInit() { }
}