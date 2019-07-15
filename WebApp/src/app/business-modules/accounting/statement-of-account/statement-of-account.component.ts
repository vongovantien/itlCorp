import { Component, OnInit, ViewChild } from '@angular/core';
import { ConfirmDeletePopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-statement-of-account',
    templateUrl: './statement-of-account.component.html',
    styleUrls: ['./statement-of-account.component.scss']
})
export class StatementOfAccountComponent implements OnInit {
    @ViewChild(ConfirmDeletePopupComponent, { static: false }) confirmPopup: ConfirmDeletePopupComponent;


    constructor() {
    }

    ngOnInit() {
    }

    onDeleteSOA() {
        this.confirmPopup.show();
    }

}
