import { T } from '@angular/cdk/keycodes';
import { Component, OnInit } from '@angular/core';
import { AccountingRepo, DocumentationRepo } from '@repositories';
import { SortService } from '@services';
import { finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-prepaid-payment',
    templateUrl: './prepaid-payment.component.html',
})
export class ARPrePaidPaymentComponent extends AppList implements OnInit {
    debitNotes: Partial<IPrepaidPayment[]> = [];
    constructor(
        private readonly _accountingRepo: AccountingRepo,
        private readonly _sortService: SortService

    ) {
        super();
        this.requestList = this.getPaging;
        this.requestSort = this.sortLocal;
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Job ID', field: '', sortable: true },
            { title: 'MBL - HBL', field: '', sortable: true },
            { title: 'Partner Name', field: '', sortable: true },
            { title: 'Debit Note', field: '', sortable: true },
            { title: 'Debit Amount', field: '', sortable: true },
            { title: 'PrePaid Amount', field: '', sortable: true },
            { title: 'Salesman', field: '', sortable: true },
            { title: 'AR Confirm', field: '', sortable: true },
        ];
        this.dataSearch = {
            keywords: []
        }
        this.getPaging();
    }

    getPaging() {
        this.isLoading = true;
        this._accountingRepo.getPagingCdNotePrepaid(this.dataSearch, this.page, this.pageSize)
            .pipe(
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (data: CommonInterface.IResponsePaging) => {
                    this.debitNotes = data.data || [];
                    this.page = data.page;
                    this.pageSize = data.pageSize;
                }
            )
    }

    onSearchData(dataSearch) {
        this.dataSearch = dataSearch;
        this.page = 1;
        this.pageSize = 30;

        this.requestList();
    }

    sortLocal(sort: string) {
        this.debitNotes = this._sortService.sort(this.debitNotes, sort, this.order);
    }

    checkAllCd() {
        if (this.isCheckAll) {
            this.debitNotes.forEach(x => {
                if (x.status === 'Unpaid') {
                    x.isSelected = true;
                }
            });
        } else {
            this.debitNotes.forEach(x => {
                x.isSelected = false;
            });
        }
    }

    onChangeSelectedCd() {
        this.isCheckAll = this.debitNotes.filter(x => x.status === 'Unpaid').every(x => x.isSelected === true);
    }
}

interface IPrepaidPayment {
    id: string;
    jobNo: string;
    mb: string;
    hbl: string;
    debitNote: string;
    totalAmount: number;
    totalAmountVnd: number;
    totalAmountUsd: number;
    paidAmount: number;
    paidAmountVnd: number;
    paidAmountUsd: number;
    currency: string;
    salesmanName: string;
    status: string;
    syncStatus: string;
    lastSyncDate: Date;
    notes: string;
    partnerName: string;
    [key: string]: any;
}
