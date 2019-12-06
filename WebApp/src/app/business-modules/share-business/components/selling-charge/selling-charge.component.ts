import { Component } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { ShareBussinessBuyingChargeComponent } from '../buying-charge/buying-charge.component';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { CsShipmentSurcharge } from 'src/app/shared/models';
import { SystemConstants } from 'src/constants/system.const';
import { CommonEnum } from 'src/app/shared/enums/common.enum';

import { takeUntil, catchError, finalize } from 'rxjs/operators';

import * as fromStore from './../../store';


@Component({
    selector: 'selling-charge',
    templateUrl: './selling-charge.component.html',
    styleUrls: ['./../buying-charge/buying-charge.component.scss']
})

export class ShareBussinessSellingChargeComponent extends ShareBussinessBuyingChargeComponent {

    isShowSyncFreightCharge: boolean = true;
    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
        protected _ngProgressService: NgProgress

    ) {
        super(_catalogueRepo, _store, _documentRepo, _toastService, _sortService, _ngProgressService);
        this._progressRef = this._ngProgressService.ref();

    }

    getSurcharge() {
        this._store.select(fromStore.getSellingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                }
            );
    }

    configHeader() {
        this.headers = [
            { title: 'Partner Name', field: 'partnerName', required: true, sortable: true, width: 150 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 250 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true, width: 150 },
            { title: 'Unit', field: 'unitId', required: true, sortable: true },
            { title: 'Unit Price', field: 'unitPrice', required: true, sortable: true },
            { title: 'Currency', field: 'currencyId', required: true, sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Credit/Debit Note', field: 'cdno', sortable: true },
            { title: 'Settle Payment', field: 'settlementCode', sortable: true },
            { title: 'Exchange Rate Date', field: 'exchangeDate', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Voucher ID Date', field: 'voucherIddate', sortable: true },
            { title: 'Final Exchange Rate', field: 'finalExchangeRate', sortable: true },
        ];
    }

    getCharge() {
        return this._catalogueRepo.getCharges({ active: true, serviceTypeId: this.serviceTypeId, type: CommonEnum.CHARGE_TYPE.CREDIT });
    }

    saveSellingSurCharge() {
        // * Update data 
        this._progressRef.start();
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }

        if (!this.checkDuplicate()) {
            return;
        }

        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.SELLING_RATE);

        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        this.getProfit();

                        this.getSurcharges(CommonEnum.SurchargeTypeEnum.SELLING_RATE);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    syncFreightCharge() {
        this._progressRef.start();

        this._documentRepo.getArrivalInfo(this.hbl.id, CommonEnum.TransactionTypeEnum.SeaFCLImport)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        if (!res.csArrivalFrieghtCharges.length) {
                            this._toastService.warning("Not found freight charge");
                        } else {
                            for (const freightCharge of res.csArrivalFrieghtCharges) {
                                const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(freightCharge);
                                newSurCharge.id = SystemConstants.EMPTY_GUID;
                                newSurCharge.exchangeDate = { startDate: new Date(), endDate: new Date() };
                                newSurCharge.invoiceDate = null;

                                // * Default get partner = customer name's hbl.
                                newSurCharge.partnerName = this.hbl.customerName;
                                newSurCharge.paymentObjectId = this.hbl.customerId;

                                this._store.dispatch(new fromStore.AddSellingSurchargeAction(newSurCharge));
                            }
                        }

                    }
                }
            );
    }
}
