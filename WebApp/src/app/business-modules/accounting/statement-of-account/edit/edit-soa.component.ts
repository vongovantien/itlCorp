import { Component, OnInit } from '@angular/core';
import moment from 'moment/moment';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'app-statement-of-account-edit',
    templateUrl: './edit-soa.component.html',
    styleUrls: ['./edit-soa.component.scss']
})
export class StatementOfAccountEditComponent extends AppPage {

    constructor() {
        super();
    }

    ngOnInit() {
    }
}
