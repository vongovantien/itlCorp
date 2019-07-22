import { Component, OnInit, ViewChild } from "@angular/core";
import { ConfirmPopupComponent, InfoPopupComponent } from "src/app/shared/common/popup";


@Component({
    selector: 'app-statement-of-account',
    templateUrl: './statement-of-account.component.html',
    styleUrls: ['./statement-of-account.component.scss']
})
export class StatementOfAccountComponent implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;

    constructor() {
    }

    ngOnInit() {
    }

    onDeleteSOA() {
        // this.confirmPopup.show();
        // this.infoPopup.show();

    }

}
