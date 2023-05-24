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

    actionDispatchType: WorkOrderActionTypes;

    constructor(
        private readonly _store: Store<IWorkOrderMngtState>,
        private readonly _actionStoreSubject: ActionsSubject,
        private readonly _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Co-Loader', field: 'partnerName' },
            { title: 'Quantity Range', field: '' },
            { title: 'Unit', field: 'unitCode' },
            { title: 'Unit Price Buying', field: 'unitPriceBuying' },
            { title: 'Unit Price Selling', field: 'unitPriceSelling' },
            { title: 'Notes', field: 'notes' },
        ];

        this.prices = this._store.select(WorkOrderListPricestate);
        this.isLoading = this._store.select(workOrderDetailLoadingState);
        this.isReadonly = this._store.select(workOrderDetailIsReadOnlyState);

        // * Listen event dispatch data and check duplicate
        this._actionStoreSubject
            .pipe(
                filter(
                    (x: { type: WorkOrderActionTypes, data: WorkOrderPriceModel }) =>
                        x.type === WorkOrderActionTypes.ADD_PRICE_ITEM || x.type === WorkOrderActionTypes.UPDATE_PRICE_ITEM
                ),
                tap((x: { type: WorkOrderActionTypes, data: WorkOrderPriceModel }) => {
                    this.actionDispatchType = x.type;
                }),
                map(d => d.data),
                withLatestFrom(this._store.select(WorkOrderListPricestate)),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                ([priceUpdate, currentPrices]) => {
                    switch (this.actionDispatchType) {
                        case WorkOrderActionTypes.ADD_PRICE_ITEM:
                            if (currentPrices.length === 0) {
                                this._store.dispatch(AddPriceItemWorkOrderSuccess({ data: priceUpdate }));
                                return;
                            }
                            const isDuplicate = currentPrices.some((price) => price.partnerId === priceUpdate.partnerId
                                && price.quantityFromValue == priceUpdate.quantityFromValue
                                && price.quantityToValue == priceUpdate.quantityToValue
                            );
                            if (!isDuplicate) {
                                this._store.dispatch(AddPriceItemWorkOrderSuccess({ data: priceUpdate }));
                                return;
                            }
                            this._toastService.warning('This price item is already existed');
                            break;
                        case WorkOrderActionTypes.UPDATE_PRICE_ITEM:
                            const isDuplicateUpdate = currentPrices.some((price) => price.id !== priceUpdate.id
                                && price.partnerId === priceUpdate.partnerId
                                && price.quantityFromValue == priceUpdate.quantityFromValue
                                && price.quantityToValue == priceUpdate.quantityToValue
                            );
                            if (!isDuplicateUpdate) {
                                this._store.dispatch(UpdatePriceItemWorkOrderSuccess({ data: priceUpdate }));
                                return;
                            }
                            this._toastService.warning('This price item is already existed');
                            break;
                        default:
                            break;
                    }

                }
            )
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
