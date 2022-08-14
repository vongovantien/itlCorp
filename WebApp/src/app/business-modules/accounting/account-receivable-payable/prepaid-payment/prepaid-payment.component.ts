import { T } from '@angular/cdk/keycodes';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { AccountingConstants } from '@constants';
import { InjectViewContainerRefDirective } from '@directives';
import { AccountingRepo, DocumentationRepo } from '@repositories';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-prepaid-payment',
    templateUrl: './prepaid-payment.component.html',
})
export class ARPrePaidPaymentComponent extends AppList implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;


    debitNotes: Partial<IPrepaidPayment[]> = [];
    selectedCd: IPrepaidPayment = null;

    constructor(
        private readonly _accountingRepo: AccountingRepo,
        private readonly _sortService: SortService,
        private readonly _toast: ToastrService,


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
                    // this.page = data.page;
                    // this.pageSize = data.pageSize;
                    this.totalItems = data.totalItems;
                }
            )
    }

    onSearchData(dataSearch) {
        this.dataSearch = dataSearch;
        this.page = 1;

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

    onSelectCd(cd: IPrepaidPayment) {
        this.selectedCd = cd;
        console.log(this.selectedCd);
    }

    preview(cd: IPrepaidPayment, currency: string = 'VND') {

    }

    confirmItem() {
        const selectedCd = Object.assign({}, this.selectedCd);
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: `Are you sure to confirm <strong>${this.selectedCd.debitNote}</strong>`,
            labelConfirm: 'Yes',
            labelCancel: 'No',
            center: true
        }, () => {
            if (selectedCd) {
                const body = {
                    id: selectedCd.id,
                    status: 'Paid',
                }
                this.confirmData([body]);
            }
        });
    }

    private confirmData(body) {
        this._accountingRepo.confirmCdNotePrepaid(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toast.success(res.message);
                        this.onSearchData(this.dataSearch);
                    } else {
                        this._toast.error(res.message);
                    }
                }
            )
    }

    confirmSelectedItems() {
        const cdList = this.debitNotes.filter(x => x.isSelected && x.status === 'Unpaid');
        if (!cdList.length) {
            this._toast.warning("Please select debit");
            return;
        }

        const hasSynced: boolean = cdList.some(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED);
        if (hasSynced) {
            const debitHasSynced: string = cdList.filter(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED).map(a => a.debitNote).toString();
            this._toast.warning(`${debitHasSynced} had synced, Please recheck!`);
            return;
        }

        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.viewContainerRef.viewContainerRef,
            {
                body: `Are you sure you want to confirm paid <span class="font-weight-bold">${cdList.map(x => x.debitNote).join()}</span> ?`,
                iconConfirm: 'la la-cloud-upload',
                labelConfirm: 'Yes',
                center: true
            },
            (v: boolean) => {
                const body = cdList.map(x => ({
                    id: x.id,
                    status: 'Paid'
                }));
                this.confirmData(body);
            });

    }

    revertSelectedItems() {

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
