import { Component } from '@angular/core';

import { ShareBussinessBuyingChargeComponent } from '../buying-charge/buying-charge.component';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';

import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';

import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { takeUntil, catchError } from 'rxjs/operators';
import { CsShipmentSurcharge, Partner } from 'src/app/shared/models';
import { SystemConstants } from 'src/constants/system.const';

import * as fromStore from './../../store';

@Component({
    selector: 'obh-charge',
    templateUrl: './obh-charge.component.html',
    styleUrls: ['./../buying-charge/buying-charge.component.scss']
})

export class ShareBussinessOBHChargeComponent extends ShareBussinessBuyingChargeComponent {

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
    ) {
        super(_catalogueRepo, _store, _documentRepo, _toastService, _sortService);

        this._store.select(fromStore.getOBHSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get obh charge from store", this.charges);
                }
            );

    }

    configHeader() {
        this.headers = [
            { title: 'Receiver', field: 'payerName', required: true, sortable: true, width: 200 },
            { title: 'Payer', field: 'receiverName', required: true, sortable: true, width: 200 },
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
            { title: 'KB', field: 'kickBack', sortable: true },
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

        this._store.dispatch(new fromStore.AddOBHSurchargeAction(newSurCharge));

    }

    selectPartnerTypes(partnerType: CommonInterface.IValueDisplay, chargeItem: CsShipmentSurcharge, type: string) {
        chargeItem.objectBePaid = null;
        switch (type) {
            case 'receiver':
                switch (partnerType.value) {
                    case CommonEnum.PartnerGroupEnum.CUSTOMER:
                        chargeItem.receiverName = this.hbl.customerName;
                        chargeItem.paymentObjectId = this.hbl.customerId;
                        break;
                    case CommonEnum.PartnerGroupEnum.CARRIER:
                        chargeItem.receiverName = this.shipment.supplierName;
                        chargeItem.paymentObjectId = this.shipment.coloaderId;
                        break;
                    case CommonEnum.PartnerGroupEnum.AGENT:
                        chargeItem.receiverName = this.shipment.agentName;
                        chargeItem.paymentObjectId = this.shipment.agentId;
                        break;
                    default:
                        break;
                }
                break;
            case 'payer':
                switch (partnerType.value) {
                    case CommonEnum.PartnerGroupEnum.CUSTOMER:
                        chargeItem.payerName = this.hbl.customerName;
                        chargeItem.payerId = this.hbl.customerId;
                        break;
                    case CommonEnum.PartnerGroupEnum.CARRIER:
                        chargeItem.payerName = this.shipment.supplierName;
                        chargeItem.payerId = this.shipment.coloaderId;
                        break;
                    case CommonEnum.PartnerGroupEnum.AGENT:
                        chargeItem.payerName = this.shipment.agentName;
                        chargeItem.payerId = this.shipment.agentId;
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    onSelectPayerReceiver(partnerData: Partner, chargeItem: CsShipmentSurcharge, type: string) {
        chargeItem.objectBePaid = null;
        switch (type) {
            case 'receiver':
                if (!!partnerData) {
                    chargeItem.receiverName = partnerData.shortName;
                    chargeItem.paymentObjectId = partnerData.id;
                }
                break;
            case 'payer':
                if (!!partnerData) {
                    chargeItem.payerName = partnerData.shortName;
                    chargeItem.payerId = partnerData.id;
                }
                break;
            default:
                break;
        }
    }

    saveOBHSurCharge() {
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (!this.checkDuplicate) {
            return;
        }

        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.OBH);

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

    checkValidate() {
        let valid: boolean = true;
        for (const charge of this.charges) {
            if (
                !charge.paymentObjectId
                || !charge.payerId
                || !charge.unitId
                || !charge.chargeId
                || !charge.chargeCode
                || !charge.receiverName
                || !charge.payerName
                || charge.unitPrice === null
                || charge.unitPrice < 0
                || charge.vatrate === null
                || charge.vatrate > 100
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

}
