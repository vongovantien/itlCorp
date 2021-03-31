import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'shipment-existed-popup',
    templateUrl: './shipment-existed.popup.html',
})
export class AdvancePaymentShipmentExistedPopupComponent extends PopupBase implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
