import { Component, OnInit } from '@angular/core';
import { AccountingRepo, DocumentationRepo } from '@repositories';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-prepaid-payment',
    templateUrl: './prepaid-payment.component.html',
})
export class ARPrePaidPaymentComponent extends AppList implements OnInit {
    debitNotes: any = [];
    constructor(
        private readonly _accountingRepo: AccountingRepo
    ) {
        super();
        this.requestList = this.getPaging
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Job ID', field: '', sortable: true },
            { title: 'MBL - HBL', field: '', sortable: true },
            { title: 'Partner Name', field: '', sortable: true },
            { title: 'Debit Note', field: '', sortable: true },
            { title: 'Debit Amount', field: '', sortable: true },
            { title: 'PrePaid Amount', field: '', sortable: true },
            { title: 'Currency', field: '', sortable: true },
            { title: 'Salesman', field: '', sortable: true },
            { title: 'AR Confirm', field: '', sortable: true },
        ];

        this.getPaging();
    }

    getPaging() {
        const body = {
            partnerId: 'fbc21fda-fd9c-45bf-b9ec-c68016476bdf'
        }
        this._accountingRepo.getPagingCdNotePrepaid(body, this.page, this.pageSize)
            .subscribe(
                (data: any) => {
                    this.debitNotes = data || [];
                    this.page = data.page;
                    this.pageSize = data.pageSize;
                }
            )
    }
}
