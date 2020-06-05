import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { ToastrService } from 'ngx-toastr';
import { PartnerOfAcctManagementResult } from '@models';

import { AccountingManagementSelectPartnerPopupComponent } from '../../components/popup/select-partner/select-partner.popup';


@Component({
    selector: 'app-accounting-create-vat-invoice',
    templateUrl: './accounting-create-vat-invoice.component.html',
})
export class AccountingManagementCreateVATInvoiceComponent extends AppForm implements OnInit {

    constructor(
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void { }

    getListPartnerCharge(data: PartnerOfAcctManagementResult[]) {
        console.log(data);
    }
}
