import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { SortService, BaseService } from 'src/app/shared/services';
import { AdvancePaymentFormsearchComponent } from './components/form-search-advance-payment/form-search-advance-payment.component';
import { AdvancePayment, AdvancePaymentRequest, User } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { NgProgress } from '@ngx-progressbar/core';
import { Router } from '@angular/router';

@Component({
    selector: 'app-advance-payment',
    templateUrl: './advance-payment.component.html',
})
export class AdvancePaymentComponent extends AppList {
    @ViewChild(AdvancePaymentFormsearchComponent, { static: false }) formSearch: AdvancePaymentFormsearchComponent;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    headers: CommonInterface.IHeaderTable[];
    headerGroupRequest: CommonInterface.IHeaderTable[];

    advancePayments: AdvancePayment[] = [];
    selectedAdv: AdvancePayment;

    groupRequest: AdvancePaymentRequest[] = [];
    userLogged: User;

    dataSearch: any = {};

    constructor(
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _baseService: BaseService,
        private _router: Router
    ) {
        super();
        this.requestList = this.getListAdvancePayment;
        this.requestSort = this.sortAdvancePayment;
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'Advance No', field: 'advanceNo', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'advanceCurrency', sortable: true },
            { title: 'Requester', field: 'requester', sortable: true },
            { title: 'Request Date', field: 'requestDate', sortable: true },
            { title: 'DeadLine Date', field: 'deadlinePayment', sortable: true },
            { title: 'Modified Date', field: 'advanceDatetimeModified', sortable: true },
            { title: 'Status Approval', field: 'statusApprovalName', sortable: true },
            { title: 'Status Payment', field: 'advanceStatusPayment', sortable: true },
            { title: 'Payment Method', field: 'paymentMethod', sortable: true },
            { title: 'Description', field: 'advanceNote', sortable: true },

        ];

        this.headerGroupRequest = [
            { title: 'JobId', field: 'jobId', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'Amount', field: 'amount', sortable: true },
            { title: 'Currency', field: 'requestCurrency', sortable: true },
            { title: 'Status Payment', field: 'statusPayment', sortable: true },
        ];
        this.getUserLogged();
        this.getListAdvancePayment();
    }

    onSearchAdvPayment(data: any) {
        this.dataSearch = data;
        this.getListAdvancePayment(this.dataSearch);
    }

    getListAdvancePayment(dataSearch?: any) {
        this.isLoading = true;
        this._progressRef.start();
        this._accoutingRepo.getListAdvancePayment(this.page, this.pageSize, Object.assign({}, dataSearch, { requester: this.userLogged.id }))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new AdvancePayment(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.advancePayments = res.data || [];
                    console.log(this.advancePayments);
                    this.totalItems = res.totalItems || 0;
                },
            );
    }

    sortAdvancePayment(sort: string): void {
        if (!!sort) {
            this.advancePayments = this._sortService.sort(this.advancePayments, this.sort, this.order);
        }
    }

    sortByCollapse(sort: CommonInterface.ISortData): void {
        if (!!sort.sortField) {
            this.groupRequest = this._sortService.sort(this.groupRequest, sort.sortField, sort.order);
        }
    }

    onDeleteAdvPayment() {
        this._progressRef.start();
        this._accoutingRepo.deleteAdvPayment(this.selectedAdv.advanceNo)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.confirmDeletePopup.hide();
                    finalize(() => this._progressRef.complete());
                })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message, 'Delete Success');
                        this.getListAdvancePayment();
                    }
                },
            );
    }

    deleteAdvancePayment(selectedAdv: AdvancePayment) {
        this.confirmDeletePopup.show();
        this.selectedAdv = new AdvancePayment(selectedAdv);
    }

    getRequestAdvancePaymentGroup(advanceNo: string, index: number) {
        if (!!this.advancePayments[index].advanceRequests.length) {
            this.groupRequest = this.advancePayments[index].advanceRequests;
        } else {
            this.isLoading = true;
            this._progressRef.start();
            this._accoutingRepo.getGroupRequestAdvPayment(advanceNo)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); this.isLoading = false; })
                )
                .subscribe(
                    (res: any) => {
                        this.groupRequest = res;
                        this.advancePayments[index].advanceRequests = res;
                    },
                );
        }
    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin() || 'admin';
    }

    gotoDetailAdvPayment(adv: AdvancePayment) {
        switch (adv.statusApproval) {
            case 'New':
            case 'Denied':
                this._router.navigate([`home/accounting/advance-payment/${adv.id}`]);
                break;
            default:
                this._router.navigate([`home/accounting/advance-payment/${adv.id}/approve`]);
                break;
        }
    }
}


