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
import { formatDate } from '@angular/common';

import * as fromStore from './../../store';


enum QUANTITY_TYPE {
    GW = 'gw',
    NW = 'nw',
    CW = 'cw',
    CBM = 'cbm',
    PACKAGE = 'package',
    CONT = 'cont'
}

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
    }

    ngOnInit() {
        this.headers = [
            { title: 'Receiver', field: 'payerName', required: true, sortable: true, width: 200 },
            { title: 'Payer', field: 'receiverName', required: true, sortable: true, width: 200 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 400 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true, width: 200 },
            { title: 'Unit', field: 'unitId', required: true, sortable: true },
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

        this.headerPartner = [
            { title: 'Name', field: 'partnerNameEn' },
            { title: 'Partner Code', field: 'taxCode' },
        ];

        this.configComboGridCharge = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unitId', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], });

        this.quantityHints = [
            { displayName: 'G.W', value: QUANTITY_TYPE.GW },
            { displayName: 'C.W', value: QUANTITY_TYPE.CW },
            { displayName: 'CBM', value: QUANTITY_TYPE.CBM },
            { displayName: 'P.K', value: QUANTITY_TYPE.PACKAGE },
            { displayName: 'Cont', value: QUANTITY_TYPE.CONT },
            { displayName: 'N.W', value: QUANTITY_TYPE.NW },
        ];

        this.partnerType = [
            { displayName: 'Customer', value: CommonEnum.PartnerGroupEnum.CUSTOMER, fieldName: 'CUSTOMER' },
            { displayName: 'Carrier', value: CommonEnum.PartnerGroupEnum.CARRIER, fieldName: 'CARRIER' },
            { displayName: 'Agent', value: CommonEnum.PartnerGroupEnum.AGENT, fieldName: 'AGENT' },
        ];

        this.getMasterData();


        this._store.select(fromStore.getOBHSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get obh charge from store", this.charges);
                }
            );
    }

    addCharge() {
        this.isSubmitted = false;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge();
        newSurCharge.currencyId = "USD"; // * Set default.
        newSurCharge.quantity = 0;
        newSurCharge.quantityType = null;
        newSurCharge.exchangeDate = { startDate: new Date(), endDate: new Date() };
        newSurCharge.invoiceDate = null;

        this._store.dispatch(new fromStore.AddOBHSurchargeAction(newSurCharge));
    }

    duplicate(index: number) {
        this.isSubmitted = false;
        const newCharge = this.charges[index];
        newCharge.id = SystemConstants.EMPTY_GUID;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(newCharge);

        this._store.dispatch(new fromStore.AddOBHSurchargeAction(newSurCharge));

    }

    deleteCharge(index: number) {
        this._store.dispatch(new fromStore.DeleteOBHSurchargeAction(index));
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
        // * Update data 
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }
        if (this.utility.checkDuplicateInObject("chargeId", this.charges)) {
            this.isDuplicateChargeCode = true;
            return;
        } else {
            this.isDuplicateChargeCode = false;
        }
        if (this.utility.checkDuplicateInObject("invoiceNo", this.charges)) {
            this.isDuplicateInvoice = true;
            return;
        } else {
            this.isDuplicateInvoice = false;
        }

        for (const charge of this.charges) {
            if (!!charge.exchangeDate && !!charge.exchangeDate.startDate) {
                charge.exchangeDate = formatDate(charge.exchangeDate.startDate, 'yyyy-MM-dd', 'en');
            }

            if (!!charge.invoiceDate && !!charge.invoiceDate.startDate) {
                charge.invoiceDate = formatDate(charge.invoiceDate.startDate, 'yyyy-MM-dd', 'en');
            } else {
                charge.invoiceDate = null;
            }

            // Update HBL ID,Type
            charge.hblid = this.hbl.id;
            charge.type = CommonEnum.SurchargeTypeEnum.OBH;
        }

        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        // this._store.dispatch(new fromStore.SaveBuyingSurchargeAction(this.charges));
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
