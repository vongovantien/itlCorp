import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { AbstractControl, FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
    selector: 'detail-account-receivable-popup',
    templateUrl: './detail-account-receivable.popup.html',
})
export class AccountReceivableDetailPopupComponent extends PopupBase implements OnInit {
    constructor() {
        super();
    }
    ngOnInit() {

    }
}