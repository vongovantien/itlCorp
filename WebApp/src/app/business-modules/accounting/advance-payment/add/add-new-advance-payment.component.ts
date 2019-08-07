import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AdvancePaymentAddPopupComponent } from '../components/popup/add-advance-payment/add-advance-payment.popup';

@Component({
    selector: 'app-advance-payment-new',
    templateUrl: './add-new-advance-payment.component.html',
})

export class AdvancePaymentAddNewComponent extends AppPage {

    @ViewChild(AdvancePaymentAddPopupComponent, { static: false }) addNewPopup: AdvancePaymentAddPopupComponent;

    constructor() {
        super();
    }

    ngOnInit() { }

    openPopupAdd() {
        this.addNewPopup.show({backdrop: 'static'});
    }

}

