import { Component } from '@angular/core';

import { ShareBussinessBuyingChargeComponent } from '../buying-charge/buying-charge.component';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';

import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';

import * as fromStore from './../../store';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { takeUntil, catchError } from 'rxjs/operators';
import { CsShipmentSurcharge } from 'src/app/shared/models';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'selling-charge',
    templateUrl: './selling-charge.component.html',
    styleUrls: ['./../buying-charge/buying-charge.component.scss']
})

export class ShareBussinessSellingChargeComponent extends ShareBussinessBuyingChargeComponent {

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
    ) {
        super(_catalogueRepo, _store, _documentRepo, _toastService, _sortService);

        this._store.select(fromStore.getSellingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get selling charge from store", this.charges);
                }
            );
    }

    configHeader() {
        this.headers = [
            { title: 'Partner Name', field: 'partnerName', required: true, sortable: true, width: 200 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 400 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true, width: 200 },
            { title: 'Unit', field: 'unitId', required: true, sortable: true, width: 200 },
            { title: 'Unit Price', field: 'unitPrice', required: true, sortable: true },
            { title: 'Currency', field: 'currencyId', required: true, sortable: true },
            { title: 'VAT', field: 'vatrate', required: true, sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Series No', field: 'seriesNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Exchange Rate Date', field: 'exchangeDate', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Credit/Debit Note', field: 'cdno', sortable: true },
            { title: 'Settle Payment', field: 'settlementCode', sortable: true },
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Voucher ID Date', field: 'voucherIddate', sortable: true },
            { title: 'Voucher IDRE', field: 'voucherIdre', sortable: true },
            { title: 'Voucher IDRE Date', field: 'voucherIdredate', sortable: true },
            { title: 'Final Exchange Rate', field: 'finalExchangeRate', sortable: true },
        ];
    }

    duplicate(index: number) {
        this.isSubmitted = false;
        const newCharge = this.charges[index];
        newCharge.id = SystemConstants.EMPTY_GUID;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(newCharge);

        this._store.dispatch(new fromStore.AddSellingSurchargeAction(newSurCharge));

    }

    saveSellingSurCharge() {
        // * Update data 
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }

        if (!this.checkDuplicate()) {
            return;
        }

        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.SELLING_RATE);

        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // * Get Profit
                        this._store.dispatch(new fromStore.GetProfitAction(this.hbl.id));
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

}
