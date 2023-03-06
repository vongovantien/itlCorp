import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';
import { AppList } from 'src/app/app.list';
import { DeletePriceItemWorkOrder, IWorkOrderMngtState, UpdatePriceItemWorkOrder, WorkOrderListPricestate } from '../../store';
import { CommercialPriceItemWorkOrderPopupComponent } from '../popup/price-item/price-item-work-order.component';

@Component({
    selector: 'price-list-work-order',
    templateUrl: './price-list-work-order.component.html',
})
export class CommercialPriceListWorkOrderComponent extends AppList implements OnInit {

    prices: any;
    @ViewChild(CommercialPriceItemWorkOrderPopupComponent) priceItemPopup: CommercialPriceItemWorkOrderPopupComponent;

    selectedIndexPriceItem: number;
    constructor(
        private readonly _store: Store<IWorkOrderMngtState>
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Co-Loader', field: '' },
            { title: 'Weight Range', field: '' },
            { title: 'Unit Price', field: '' },
        ];

        this.prices = this._store.select(WorkOrderListPricestate)
    }

    openAddNewPrice() {
        this.priceItemPopup.isSubmitted = false;
        this.priceItemPopup.form.reset();
        this.priceItemPopup.buyings.length = 0;
        this.priceItemPopup.sellings.length = 0;

        this.priceItemPopup.initFreightCharge(this.priceItemPopup.transactionType);
        this.priceItemPopup.ACTION = 'CREATE';
        this.priceItemPopup.show();
    }

    onAddNewPriceItem($event) {
        this.prices.push($event);
    }

    updatePriceItem(index: number) {
        this.selectedIndexPriceItem = index;
        this._store.dispatch(UpdatePriceItemWorkOrder({ index }));

        this.priceItemPopup.ACTION = 'UPDATE';
        this.priceItemPopup.show();
    }

    deletePriceItem(index: number) {
        this._store.dispatch(DeletePriceItemWorkOrder({ index }));
        this.selectedIndexPriceItem = null;
    }
}
