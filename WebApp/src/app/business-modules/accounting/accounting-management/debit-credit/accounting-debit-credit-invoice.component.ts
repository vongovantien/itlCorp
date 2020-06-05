import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Router } from '@angular/router';
import { DocumentationRepo } from '@repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { CDNoteViewModel } from 'src/app/shared/models/accouting/cdnoteview.model';
import { SortService } from '@services';
import { NgProgress } from '@ngx-progressbar/core';
import { AccountingDetailCdNoteComponent } from '../components/popup/detail-cd-note/detail-cd-note.component';

type TAB = 'CDI' | 'VAT' | 'VOUCHER';
@Component({
    selector: 'app-accounting-debit-credit-invoice',
    templateUrl: './accounting-debit-credit-invoice.component.html'
})

export class AccountingManagementDebitCreditInvoiceComponent extends AppList implements OnInit {
    @ViewChild(AccountingDetailCdNoteComponent, { static: false }) cdNoteDetailPopupComponent: AccountingDetailCdNoteComponent;
    selectedTab: TAB = 'CDI';
    cdNotes: CDNoteViewModel[] = [];
    isOpenDetail: boolean = false;

    constructor(
        private _router: Router,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _ngProgressService: NgProgress
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
        this.requestList = this.getCreditDebitInvoices;
        this.requestSort = this.sortCD;
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
        this.getCreditDebitInvoices();
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
    sortCD(sort: string): void {
        if (!!sort) {
            if (!!this.cdNotes.length) {
                this.cdNotes = this._sortService.sort(this.cdNotes, this.sort, this.order);
            }
        }
    }
    searchInvoiceAndCDNotes(event) {
        this.page = 1;
        this.dataSearch = event;
        this._progressRef.start();
        this.getCreditDebitInvoices();
    }
    resetSearch(event) {

        this.page = 1;
        this.dataSearch = event;
        this._progressRef.start();
        this.getCreditDebitInvoices();
    }
    getCreditDebitInvoices() {
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
    showDetailCDNote(jobId: string, refNo: string) {
        this.isOpenDetail = true;
        this.cdNoteDetailPopupComponent.setDefault(refNo);
        this.cdNoteDetailPopupComponent.jobId = jobId;
        this.cdNoteDetailPopupComponent.cdNote = refNo;
        this.cdNoteDetailPopupComponent.getDetailCdNote(jobId, refNo);
        this.cdNoteDetailPopupComponent.show();
    }
}
