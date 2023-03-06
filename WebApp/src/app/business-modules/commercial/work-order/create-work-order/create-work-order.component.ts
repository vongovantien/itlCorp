import { formatDate } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { AppForm, AppPage } from '@app';
import { WorkOrder, WorkOrderPrice } from '@models';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';
import { CommercialFormCreateWorkOrderComponent } from '../components/form-create/form-create-work-order.component';
import { InitPriceListWorkOrder } from '../store/actions';
import { IWorkOrderMngtState, WorkOrderListPricestate } from '../store/reducers';
import _merge from 'lodash/merge'
import { DocumentationRepo } from '@repositories';
import { SystemConstants } from '@constants';

@Component({
    selector: ' app-create-work-order',
    templateUrl: './create-work-order.component.html',
})
export class CommercialCreateWorkOrderComponent extends AppForm implements OnInit {

    transactionType: string;
    @ViewChild(CommercialFormCreateWorkOrderComponent) formWorkOrder: CommercialFormCreateWorkOrderComponent;

    prices: WorkOrderPrice[] = [];

    constructor(
        private readonly _activedRouter: ActivatedRoute,
        private readonly _store: Store<IWorkOrderMngtState>,
        private readonly _toast: ToastrService,
        private readonly _documentationRepo: DocumentationRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this._activedRouter.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: any) => {
                    console.log(data);
                    this.transactionType = data?.transactionType;
                }
            );

        this._store.dispatch(InitPriceListWorkOrder({ data: [] }));

        this._store.select(WorkOrderListPricestate)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res) => {
                    console.log(res);
                    this.prices = res;
                }
            )
    }

    ngAfterViewInit(): void {

    }

    sendRequest() { }

    save() {
        this.formWorkOrder.isSubmitted = true;
        if (!this.formWorkOrder.form.valid) {
            this._toast.warning(this.invalidFormText);
            return;
        }
        const form = this.formWorkOrder.form.getRawValue();

        const formData = {
            id: SystemConstants.EMPTY_GUID,
            effectiveDate: !!form.effectiveDate && !!form.effectiveDate.startDate ? formatDate(form.effectiveDate.startDate, 'yyyy-MM-dd', 'en') : null,
            expiredDate: !!form.expiredDate && !!form.expiredDate.startDate ? formatDate(form.expiredDate.startDate, 'yyyy-MM-dd', 'en') : null,
        };
        const workOrder: WorkOrder = new WorkOrder(Object.assign(_merge(form, formData)));
        workOrder.transactionType = this.transactionType;

        const body = {
            ...workOrder,
            ListPrice: this.prices
        }
        console.log(body);
        this._documentationRepo.addWorkOrder(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toast.success(res.message);
                        return;
                    }
                    this._toast.error(res.message);
                }
            )

    }
}
