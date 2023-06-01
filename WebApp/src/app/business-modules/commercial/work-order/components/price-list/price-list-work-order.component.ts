import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { WorkOrderPriceModel } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { Observable } from 'rxjs';
import { AppList } from 'src/app/app.list';
import {
    AddPriceItemWorkOrderSuccess,
    DeletePriceItemWorkOrder,
    DeletePriceItemWorkOrderSuccess,
    IWorkOrderMngtState,
    SelectPriceItemWorkOrder,
    UpdatePriceItemWorkOrderSuccess,
    WorkOrderActionTypes,
    workOrderDetailIsReadOnlyState,
    workOrderDetailLoadingState,
    WorkOrderListPricestate
} from '../../store';
import { CommercialPriceItemWorkOrderPopupComponent } from '../popup/price-item/price-item-work-order.component';
import isUUID from 'validator/es/lib/isUUID';
import { InjectViewContainerRefDirective } from '@directives';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { filter, map, takeUntil, tap, withLatestFrom } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'price-list-work-order',
    templateUrl: './price-list-work-order.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class CommercialPriceListWorkOrderComponent extends AppList implements OnInit {

    @ViewChild(CommercialPriceItemWorkOrderPopupComponent) priceItemPopup: CommercialPriceItemWorkOrderPopupComponent;
    @ViewChild(InjectViewContainerRefDirective) containerRef: InjectViewContainerRefDirective;

    prices: Observable<WorkOrderPriceModel[]>;
    selectedIndexPriceItem: number;


    constructor(
        private readonly _store: Store<IWorkOrderMngtState>,
        private readonly _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Co-Loader', field: 'partnerName' },
            { title: 'Quantity Type', field: 'quantityType' },
            { title: 'Fee Type', field: 'type' },
            { title: 'Unit', field: 'unitCode' },
            { title: 'Unit Price Buying', field: 'unitPriceBuying' },
            { title: 'Unit Price Selling', field: 'unitPriceSelling' },
            { title: 'Notes', field: 'notes' },
        ];

        this.prices = this._store.select(WorkOrderListPricestate);
        this.isLoading = this._store.select(workOrderDetailLoadingState);
        this.isReadonly = this._store.select(workOrderDetailIsReadOnlyState);


    }

    openAddNewPrice() {
        this.selectedIndexPriceItem = null;
        this.priceItemPopup.isSubmitted = false;
        this.priceItemPopup.form.reset();
        this.priceItemPopup.buyings.length = 0;
        this.priceItemPopup.sellings.length = 0;

        this.priceItemPopup.initFreightCharge(this.priceItemPopup.transactionType);
        this.priceItemPopup.ACTION = 'CREATE';
        this.priceItemPopup.title = 'New Price';
        this.priceItemPopup.show();
    }

    updatePriceItem(index: number) {
        this._toastService.clear();
        this.selectedIndexPriceItem = index;
        this._store.dispatch(SelectPriceItemWorkOrder({ index }));

        this.priceItemPopup.isSubmitted = false;
        this.priceItemPopup.ACTION = 'UPDATE';
        this.priceItemPopup.title = 'Update Price';
        this.priceItemPopup.show();
    }

    deletePriceItem(item: WorkOrderPriceModel, index: number) {
        if (isUUID(item.id) && item.id !== SystemConstants.EMPTY_GUID) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.containerRef.viewContainerRef, {
                title: 'Delete Price Item',
                body: 'Are you sure to delete?',
                labelConfirm: 'Yes',
                center: true
            }, () => {
                this._store.dispatch(DeletePriceItemWorkOrder({ index: index, id: item.id }));
            });
        } else {
            this._store.dispatch(DeletePriceItemWorkOrderSuccess({ index }));
            this.selectedIndexPriceItem = null;
        }
    }
}
