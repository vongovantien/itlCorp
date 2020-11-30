import { Component, OnInit, ViewChild } from '@angular/core';
import { AccountingRepo, SystemRepo } from '@repositories';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { AppList } from '@app';
import { catchError, finalize, map } from 'rxjs/operators';
import { User } from '@models';
import { SystemConstants, RoutingConstants } from '@constants';
import { formatDate } from '@angular/common';
import { Router } from '@angular/router';
import { ConfirmBillingDatePopupComponent } from '../components/poup/confirm-billing-date/confirm-billing-date.popup';
import { InfoPopupComponent } from '@common';
@Component({
    selector: 'confirm-billing',
    templateUrl: './confirm-billing.component.html',
    styleUrls: ['./confirm-billing.component.scss']
})
export class ConfirmBillingComponent extends AppList implements OnInit {
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmBillingDatePopupComponent, { static: false }) confirmBillingDatePopup: ConfirmBillingDatePopupComponent;
    invoices: any[] = [];
    userLogged: User;
    checkAll = false;
    constructor(
        private _accountingRepo: AccountingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _sysRepo: SystemRepo,
        private _router: Router
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.getListInvoice;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Reference No', field: 'invoiceNoReal', sortable: true },
            { title: 'Partner ID', field: 'partnerId', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Amount', field: 'totalAmount', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Date', field: 'date', sortable: true },
            { title: 'Payment Term', field: 'paymentTerm', sortable: true },
            { title: 'Confirm Billing', field: 'confirmBillingDate', sortable: true },
            { title: 'Due Date', field: 'dueDate', sortable: true },
            { title: 'Payment Status', field: 'paymentStatus', sortable: true },
        ];
        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this.dataSearch = {
            dateType: 'VAT Invoice Date',
            // fromDate: formatDate(new Date(), 'yyyy-MM-dd', 'en'),
            // toDate: formatDate(new Date(), 'yyyy-MM-dd', 'en'),
            csHandling: this.userLogged.id
        };
        this.getListInvoice();
    }

    getListInvoice() {
        this._progressRef.start();
        this._accountingRepo.getListConfirmBilling(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                }),
                map((data: any) => {
                    return {
                        data: data.data,
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.invoices = res.data;
                },
            );
    }

    onSearchConfirmBilling(data: any) {
        this.page = 1;
        this.dataSearch = data;
        this.getListInvoice();
    }

    sortLocal(sort: string): void {
        this.invoices = this._sortService.sort(this.invoices, sort, this.order);
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'confirm-billing': {
                this._router.navigate([`${RoutingConstants.ACCOUNTING.STATEMENT_OF_ACCOUNT}/confirm-billing`]);
                break;
            }
            case 'soa': {
                this._router.navigate([`${RoutingConstants.ACCOUNTING.STATEMENT_OF_ACCOUNT}`]);
                break;
            }
        }
    }

    viewDetail(invoice: any): void {
        this._accountingRepo.checkDetailAcctMngtPermission(invoice.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this._router.navigate([`${RoutingConstants.ACCOUNTING.STATEMENT_OF_ACCOUNT}/confirm-billing/${invoice.id}`]);
                }
                // } else {
                //     this.popup403.show();
                // }
            });
    }

    showConfirmBilling() {
        const objChecked = this.invoices.find(x => x.isSelected);
        const invoiceIds = [];
        if (!objChecked) {
            this.infoPopup.show();
            return;
        } else {
            this.invoices.forEach(item => {
                if (item.isSelected) {
                    invoiceIds.push(item.id);
                }
            });
            this.confirmBillingDatePopup.invoiceIds = invoiceIds;
            console.log(invoiceIds);
            this.confirmBillingDatePopup.show();
        }
    }

    checkAllBilling() {
        if (this.checkAll) {
            this.invoices.forEach(x => {
                x.isSelected = true;
            });
        } else {
            this.invoices.forEach(x => {
                x.isSelected = false;
            });
        }
    }

    removeAllChecked() {
        this.checkAll = false;
    }
}
