import { Component, OnInit, ViewChild } from '@angular/core';
import { Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';

import { DocumentationRepo, AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { PartnerOfAcctManagementResult, CDNoteViewModel } from '@models';

import { AppList } from 'src/app/app.list';
import { IAccountingManagementPartnerState, SelectPartner } from '../store';

import { AccountingManagementSelectPartnerPopupComponent } from '../components/popup/select-partner/select-partner.popup';
import { AccountingDetailCdNoteComponent } from '../components/popup/detail-cd-note/detail-cd-note.component';

import { catchError, finalize, map } from 'rxjs/operators';


type TAB = 'CDI' | 'VAT' | 'VOUCHER';
@Component({
    selector: 'app-accounting-debit-credit-invoice',
    templateUrl: './accounting-debit-credit-invoice.component.html'
})

export class AccountingManagementDebitCreditInvoiceComponent extends AppList implements OnInit {

    @ViewChild(AccountingDetailCdNoteComponent, { static: false }) cdNoteDetailPopupComponent: AccountingDetailCdNoteComponent;
    @ViewChild(AccountingManagementSelectPartnerPopupComponent, { static: false }) selectPartnerPopup: AccountingManagementSelectPartnerPopupComponent;

    selectedTab: TAB = 'CDI';
    cdNotes: CDNoteViewModel[] = [];

    constructor(
        private _router: Router,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _ngProgressService: NgProgress,
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _store: Store<IAccountingManagementPartnerState>
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
        this.requestList = this.getCreditDebitInvoices;
        this.requestSort = this.sortCD;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Invoice No', field: 'referenceNo', sortable: true },
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'HBL', field: 'hblNo', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Total Amount', field: 'total', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Issue Date', field: 'issuedDate', sortable: true },
            { title: 'Creator', field: 'creator', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
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
        this.getCreditDebitInvoices();
    }

    resetSearch(event) {
        this.page = 1;
        this.dataSearch = event;
        this.getCreditDebitInvoices();
    }

    getCreditDebitInvoices() {
        this.isLoading = true;
        this._progressRef.start();
        this._documentationRepo.pagingInvoiceAndCDNotes(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                }),
                map((data: CommonInterface.IResponsePaging) => {
                    return {
                        data: (data.data || []).map((item: any) => new CDNoteViewModel(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.totalItems = res.totalItems || 0;
                    this.cdNotes = res.data;
                },
            );
    }

    showDetailCDNote(jobId: string, refNo: string) {
        this.cdNoteDetailPopupComponent.setDefault(refNo);
        this.cdNoteDetailPopupComponent.jobId = jobId;
        this.cdNoteDetailPopupComponent.cdNote = refNo;
        this.cdNoteDetailPopupComponent.getDetailCdNote(jobId, refNo);
        this.cdNoteDetailPopupComponent.show();
    }

    issueVatInvoice() {
        const cdNotes: CDNoteViewModel[] = this.cdNotes.filter(x => x.isSelected);
        if (!!cdNotes.length) {
            this.searchRef(cdNotes.map(x => x.referenceNo));
        }
    }

    issueVoucher() {

    }

    searchRef(cdNotes: string[]) {
        const body: AccountingInterface.IPartnerOfAccountingManagementRef = {
            cdNotes: cdNotes,
            soaNos: null,
            jobNos: null,
            hbls: null,
            mbls: null,
            settlementCodes: null
        };
        this._accountingRepo.getChargeSellForInvoiceByCriteria(body)
            .subscribe(
                (res: PartnerOfAcctManagementResult[]) => {
                    if (!!res && !!res.length) {
                        if (res.length === 1) {
                            this._store.dispatch(SelectPartner(res[0]));
                            this._router.navigate(["home/accounting/management/vat-invoice/new"]);
                            return;
                        } else {
                            this.selectPartnerPopup.listPartners = res;
                            this.selectPartnerPopup.selectedPartner = null;

                            this.selectPartnerPopup.show();
                        }
                    } else {
                        this._toastService.warning("Not found data charge");
                    }
                }
            );
    }

    onSelectPartner() {
        this._router.navigate(["home/accounting/management/vat-invoice/new"]);
    }
}
