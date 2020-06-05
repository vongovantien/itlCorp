import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { DocumentationRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { CDNoteViewModel } from 'src/app/shared/models/accouting/cdnoteview.model';

type TAB = 'CDI' | 'VAT' | 'VOUCHER';
@Component({
    selector: 'app-accounting-debit-credit-invoice',
    templateUrl: './accounting-debit-credit-invoice.component.html'
})

export class AccountingManagementDebitCreditInvoiceComponent extends AppList implements OnInit {

    selectedTab: TAB = 'CDI';
    cdNotes: CDNoteViewModel[];

    constructor(
        private _router: Router,
        private _documentationRepo: DocumentationRepo
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Invoice No', field: '', sortable: true },
            { title: 'Job ID', field: '', sortable: true },
            { title: 'HBL', field: '', sortable: true },
            { title: 'Partner Name', field: '', sortable: true },
            { title: 'Total Amount', field: '', sortable: true },
            { title: 'Currency', field: '', sortable: true },
            { title: 'Issue Date', field: '', sortable: true },
            { title: 'Creator', field: '', sortable: true },
            { title: 'Status', field: '', sortable: true },
        ];
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'vat': {
                this._router.navigate([`home/accounting/management/vat-invoice`]);
                break;
            }
            case 'voucher': {
                this._router.navigate([`home/accounting/management/voucher`]);
                break;
            }
        }
    }
    searchInvoiceAndCDNotes(event) {
        this.dataSearch = event;
        this._progressRef.start();
        this._documentationRepo.pagingInvoiceAndCDNotes(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                }),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new CDNoteViewModel(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.cdNotes = res.data;
                },
            );
    }
}
