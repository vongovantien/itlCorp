import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { SortService, BaseService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import { Router } from '@angular/router';
import { catchError, finalize, map, } from 'rxjs/operators';
import { AdvancePayment, User, SettlementPayment } from 'src/app/shared/models';

@Component({
    selector: 'app-settlement-payment',
    templateUrl: './settlement-payment.component.html',
})
export class SettlementPaymentComponent extends AppList {

    headers: CommonInterface.IHeaderTable[];
    settlements: any[] = [];
    dataSearch: any = {};

    userLogged: User;
    constructor(
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _progressService: NgProgress,
        private _baseService: BaseService,
        private _router: Router
    ) {
        super();
        this.requestList = this.getListSettlePayment;
        // this.requestSort = this.sortAdvancePayment;
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: '', field: '' },
            { title: 'Settlemenent No', field: 'settlementNo',sortable: true },

            { title: 'Amount', field: 'amount' , sortable: true },
            { title: 'Currency', field: 'chargeCurrency' , sortable: true },
            { title: 'Requester', field: 'requester' , sortable: true },
            { title: 'Request Date', field: 'requestDate' , sortable: true },
            { title: 'Status Approval', field: 'statusApproval', sortable: true },
            { title: 'Payment method', field: 'paymentMethod',sortable: true },
            { title: 'Description', field: 'note',sortable: true },
        ];
        this.getUserLogged();
        this.getListSettlePayment();

    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin() || 'admin';
    }

    getRequestAdvancePaymentGroup() {

    }

    deleteAdvancePayment() {

    }

    onSearchSettlement(data: any) {
        this.dataSearch = data;
        this.getListSettlePayment(this.dataSearch);
    }

    getListSettlePayment(dataSearch?: any) {
        this.isLoading = true;
        this._progressRef.start();
        this._accoutingRepo.getListSettlementPayment(this.page, this.pageSize, Object.assign({}, dataSearch, { requester: this.userLogged.id }))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new SettlementPayment(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    console.log(res);
                    this.totalItems = res.totalItems || 0;
                    this.settlements = res.data;
                },
            );

    }


    sortLocal(sort: string): void {
        this.settlements = this._sortService.sort(this.settlements, sort, this.order);
    }


}
