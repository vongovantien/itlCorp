import { Component, ViewChild} from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { AdvancePaymentListRequestComponent } from '../../advance-payment/components/list-advance-payment-request/list-advance-payment-request.component';

@Component({
    selector: 'app-approve-advance',
    templateUrl: './approve-advance.component.html',
    styleUrls: ['./approve-advance.component.scss']
})

export class ApproveAdvancePaymentComponent extends AppPage {

    @ViewChild( AdvancePaymentListRequestComponent, {static: false}) advRequestListComponent: AdvancePaymentListRequestComponent;
    progress: any[] = [1, 2, 3, 4, 5, 6];

    constructor() {
        super();
    }

    ngOnInit() { 
        console.log(this.advRequestListComponent);
    }

    ngAfterContentInit() {
        console.log(this.advRequestListComponent);
    }
}
