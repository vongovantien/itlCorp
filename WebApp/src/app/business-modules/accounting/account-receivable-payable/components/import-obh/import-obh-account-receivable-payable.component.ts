import { Component, OnInit } from '@angular/core';
import { AccountingRepo } from '@repositories';

@Component({
    selector: 'app-import-obh-account-receivable-payable',
    templateUrl: './import-obh-account-receivable-payable.component.html'
})

export class AccountReceivablePayableImportOBHPaymentComponent implements OnInit {
    headers: any = [];
    data: IImportOBH[];
    totalRows: number = 0;
    totalValidRows: number = 0;
    constructor(
        private _accountingRepo: AccountingRepo
    ) {

    }

    ngOnInit(): void {
        this.data = [];
        this.headers = [
            { title: 'Soa No', field: 'soaNo', sortable: true },
            { title: 'Partner Id', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Payment Amount', field: 'paymentAmount', sortable: true },
            { title: 'Paid Date', field: 'paidDate', sortable: true },
            { title: 'Payment Type', field: 'paymentType', sortable: true }
        ];
    }

    changeFile(file: Event) {
        if (file.target['files'] == null) { return; }
        this._accountingRepo.getOBHPaymentImport(file.target['files'])
            .subscribe((response: CommonInterface.IResponseImport) => {
                this.data = response.data;
                console.log(this.data);

                this.totalValidRows = response.totalValidRows;
                this.totalRows = this.data.length;
            });

    }
}
interface IImportOBH {
    soaNo: string;
    partnerId: string;
    partnerName: string;
    paymentAmount: number;
    paidDate: string;
    paymentType: string;
}