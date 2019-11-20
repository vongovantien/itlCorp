import { Component } from '@angular/core';
import { Store } from '@ngrx/store';

import { AppForm } from 'src/app/app.form';
import { DeliveryOrder, User } from 'src/app/shared/models';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { SystemConstants } from 'src/constants/system.const';

import { getHBLState } from '../../../../store';
import { catchError, takeUntil, switchMap, finalize } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';


@Component({
    selector: 'sea-fcl-import-hbl-delivery-order',
    templateUrl: './delivery-order.component.html'
})

export class SeaFClImportDeliveryOrderComponent extends AppForm {

    deliveryOrder: DeliveryOrder = new DeliveryOrder();

    header: string = '';
    footer: string = '';

    userLogged: User;

    constructor(
        private _documentRepo: DocumentationRepo,
        private _store: Store<any>,
        private _ngProgress: NgProgress,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._ngProgress.ref();

    }

    ngOnInit() {
        // * Get User logged.
        this.userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
        this._store.select(getHBLState)
            .pipe(
                catchError(this.catchError),
                takeUntil(this.ngUnsubscribe),
                switchMap((hblDetail: any) => {
                    return this._documentRepo.getDeliveryOrder(hblDetail.data.id || SystemConstants.EMPTY_GUID, CommonEnum.TransactionTypeEnum.SeaFCLImport);

                }) // * Get deliveryOrder info.
            )
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.deliveryOrder = new DeliveryOrder(res);

                        this.deliveryOrder.deliveryOrderPrintedDate = {
                            startDate: new Date(this.deliveryOrder.deliveryOrderPrintedDate),
                            endDate: new Date(this.deliveryOrder.deliveryOrderPrintedDate),
                        };

                        this.deliveryOrder.userDefault = this.userLogged.id;
                        this.deliveryOrder.transactionType = CommonEnum.TransactionTypeEnum.SeaFCLImport;
                    }
                }
            );
    }

    setDefaultHeadeFooter() {
        const body: IDefaultHeaderFooter = {
            transactionType: '' + CommonEnum.TransactionTypeEnum.SeaFCLImport,
            userDefault: this.userLogged.id,
            doheader1: this.deliveryOrder.doheader1,
            doheader2: this.deliveryOrder.doheader2,
            dofooter: this.deliveryOrder.dofooter,
        };

        this._progressRef.start();
        this._documentRepo.setDefaultHeaderFooterDeliveryOrder(body)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    saveDeliveryOrder() {
        this._progressRef.start();
        const printedDate = {
            deliveryOrderPrintedDate: !!this.deliveryOrder.deliveryOrderPrintedDate && !!this.deliveryOrder.deliveryOrderPrintedDate.startDate ? formatDate(this.deliveryOrder.deliveryOrderPrintedDate.startDate, 'yyyy-MM-dd', 'en') : null,
        };

        this._documentRepo.updateDeliveryOrderInfo(Object.assign({}, this.deliveryOrder, printedDate))
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

}


interface IDefaultHeaderFooter {
    transactionType: any;
    userDefault: string;
    doheader1: string;
    doheader2: string;
    dofooter: string;
}

