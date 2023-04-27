import { formatDate } from '@angular/common';
import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { AppForm, AppPage } from '@app';
import { WorkOrder, WorkOrderPrice } from '@models';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { takeUntil } from 'rxjs/operators';
import { CommercialFormCreateWorkOrderComponent } from '../components/form-create/form-create-work-order.component';
import { InitPriceListWorkOrder, InitWorkOrder } from '../store/actions';
import { IWorkOrderMngtState, WorkOrderListPricestate } from '../store/reducers';
import _merge from 'lodash-es/merge'
import { DocumentationRepo } from '@repositories';
import { RoutingConstants, SystemConstants } from '@constants';

@Component({
    selector: ' app-create-work-order',
    templateUrl: './create-work-order.component.html',
})
export class CommercialCreateWorkOrderComponent extends AppForm implements OnInit {

    @ViewChild(CommercialFormCreateWorkOrderComponent) formWorkOrder: CommercialFormCreateWorkOrderComponent;

    transactionType: string;
    prices: WorkOrderPrice[] = [];
    workOrderId: string = SystemConstants.EMPTY_GUID;

    constructor(
        protected readonly _router: Router,
        protected readonly _activedRouter: ActivatedRoute,
        protected readonly _store: Store<IWorkOrderMngtState>,
        protected readonly _toast: ToastrService,
        protected readonly _documentationRepo: DocumentationRepo
    ) {
        super();
    }

    ngOnInit(): void {
        this._store.dispatch(InitWorkOrder());
        this._store.dispatch(InitPriceListWorkOrder({ data: [] }));

        this.listenParams();
        this.listenPriceList();

        this.initSubmitClickSubscription((action: string) => { this.submitWorkOrder(action) });
    }

    listenParams() {
        this._activedRouter.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (data: any) => {
                    this.transactionType = data?.transactionType;
                    console.log(this.transactionType);
                }
            );
    }

    listenPriceList() {
        this._store.select(WorkOrderListPricestate)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res) => {
                    this.prices = res;
                }
            )
    }

    ngAfterViewInit(): void {

    }

    sendRequest() { }

    submitWorkOrder(action: string) {
        this._toast.clear();
        this.formWorkOrder.isSubmitted = true;
        if (!this.formWorkOrder.form.valid) {
            this._toast.warning(this.invalidFormText);
            return;
        }

        if (!this.prices.length) {
            this._toast.warning('Please add price list');
            return;
        }

        const form = this.formWorkOrder.form.getRawValue();

        const formData = {
            id: this.workOrderId,
            effectiveDate: !!form.effectiveDate && !!form.effectiveDate.startDate ? formatDate(form.effectiveDate.startDate, 'yyyy-MM-dd', 'en') : null,
            expiredDate: !!form.expiredDate && !!form.expiredDate.startDate ? formatDate(form.expiredDate.startDate, 'yyyy-MM-dd', 'en') : null,
        };
        const workOrder: WorkOrder = new WorkOrder(Object.assign(_merge(form, formData)));
        workOrder.transactionType = this.transactionType;

        const body = {
            ...workOrder,
            listPrice: this.prices
        }
        console.log(body);

        this.saveWorkOrder(body);
    }

    saveWorkOrder(body) {
        this._documentationRepo.addWorkOrder(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        // this._store.dispatch(ResetInvoiceList());
                        this._router.navigate([`${RoutingConstants.COMMERCIAL.WO}/${res.data.id}`]);
                        this._toast.success(res.message);
                        return;
                    }
                    this._toast.error(res.message);
                }
            )
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.COMMERCIAL.WO}`]);
    }
}
