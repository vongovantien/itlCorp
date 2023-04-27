import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { finalize, pluck, takeUntil } from 'rxjs/operators';
import { CommercialCreateWorkOrderComponent } from '../create-work-order/create-work-order.component';
import {
    IWorkOrderMngtState,
    LoadDetailWorkOrder,
    LoadDetailWorkOrderSuccess,
    workOrderDetailCodeState,
    workOrderDetailTransactionTypeNameState
} from '../store';
import isUUID from 'validator/es/lib/isUUID';
import { WorkOrder, WorkOrderViewUpdateModel } from '@models';
import _merge from 'lodash-es/merge';
import { formatDate } from '@angular/common';
import { Observable } from 'rxjs';


@Component({
    selector: 'app-detail-work-order',
    templateUrl: './detail-work-order.component.html',
})
export class CommercialWorkOrderDetailComponent extends CommercialCreateWorkOrderComponent implements OnInit {
    workOrderDetail: WorkOrderViewUpdateModel;
    isActive: boolean;
    code$: Observable<string>;
    transactionTypeName$: Observable<string>;

    constructor(
        protected readonly _router: Router,
        protected readonly _activedRouter: ActivatedRoute,
        protected readonly _store: Store<IWorkOrderMngtState>,
        protected readonly _toast: ToastrService,
        protected readonly _documentationRepo: DocumentationRepo,
    ) {
        super(_router, _activedRouter, _store, _toast, _documentationRepo);
    }

    listenParams() {
        this._activedRouter.params
            .pipe(
                takeUntil(this.ngUnsubscribe),
                pluck('id'),
            )
            .subscribe(
                (workOrderId: string) => {
                    console.log(workOrderId);
                    if (isUUID(workOrderId)) {
                        this.workOrderId = workOrderId;
                        this.getDetailWorkOrder(workOrderId);
                    } else {
                        this.gotoList();
                    }
                }
            )

        this.code$ = this._store.select(workOrderDetailCodeState);
        this.transactionTypeName$ = this._store.select(workOrderDetailTransactionTypeNameState);
    }

    dispatchDetail(id: string) {
        this._store.dispatch(LoadDetailWorkOrder({ id }));
    }

    getDetailWorkOrder(id: string) {
        this.isLoading = true;
        this._store.dispatch(LoadDetailWorkOrder({ id }));
        this._documentationRepo.getDetailWorkOrder(id)
            .pipe(
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (res: WorkOrderViewUpdateModel) => {
                    this.workOrderDetail = res;
                    this.isActive = this.workOrderDetail.active || false;
                    this.prices = this.workOrderDetail.listPrice;
                    this.transactionType = this.workOrderDetail.transactionType;
                    this.updateFormWorkOrder(this.workOrderDetail);

                    this._store.dispatch(LoadDetailWorkOrderSuccess(res));
                }
            );
    }

    updateFormWorkOrder(res: WorkOrderViewUpdateModel) {
        const formData: WorkOrderViewUpdateModel | any = {
            workOrderNo: res.code,
            effectiveDate: !!res.effectiveDate ? { startDate: new Date(res.effectiveDate), endDate: new Date(res.effectiveDate) } : null,
            expiredDate: !!res.expiredDate ? { startDate: new Date(res.expiredDate), endDate: new Date(res.expiredDate) } : null,
            polDescription: res.polDescription,
            podDescription: res.podDescription,
        };
        this.formWorkOrder.form.patchValue(Object.assign(_merge(res, formData)));
    }

    onToggleActive($event) {
        console.log($event);
    }


    saveWorkOrder(body: WorkOrderViewUpdateModel) {
        body.id = this.workOrderId;
        body.code = this.workOrderDetail.code;
        // body.userCreated = this.workOrderDetail.userCreated;
        // body.datetimeCreated = this.workOrderDetail.datetimeCreated;

        this._documentationRepo.updateWorkOrder(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toast.success(res.message);
                        this.getDetailWorkOrder(this.workOrderId);
                        return;
                    }
                    this._toast.error(res.message);
                }
            )
    }


}
