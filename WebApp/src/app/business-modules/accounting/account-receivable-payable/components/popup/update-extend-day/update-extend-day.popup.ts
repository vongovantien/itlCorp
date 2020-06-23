import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'update-extend-day-popup',
    templateUrl: './update-extend-day.popup.html',
})
export class AccountReceivablePayableUpdateExtendDayPopupComponent extends PopupBase implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
