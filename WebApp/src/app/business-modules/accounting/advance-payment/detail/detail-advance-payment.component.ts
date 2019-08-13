import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'app-advance-payment-detail',
    templateUrl: './detail-advance-payment.component.html',
    styleUrls: ['./detail-advance-payment.component.scss']
})
export class AdvancePaymentDetailComponent extends AppPage {

    progress: any[] = [];
    constructor() {
        super();
    }

    ngOnInit() {
        this.progress = [1, 2, 3, 4, 5];
    }

}
