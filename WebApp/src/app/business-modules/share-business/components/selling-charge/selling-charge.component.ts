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
import { formatDate } from '@angular/common';

enum QUANTITY_TYPE {
    GW = 'gw',
    NW = 'nw',
    CW = 'cw',
    CBM = 'cbm',
    PACKAGE = 'package',
    CONT = 'cont'
}

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
    }

    ngOnInit() {
        this.headers = [
            { title: 'Partner Name', field: 'partnerName', required: true, sortable: true, width: 200 },
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


        this._store.select(fromStore.getSellingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    console.log("get selling charge from store", this.charges);
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

        this._store.dispatch(new fromStore.AddSellingSurchargeAction(newSurCharge));
    }

    duplicate(index: number) {
        this.isSubmitted = false;
        const newCharge = this.charges[index];
        newCharge.id = SystemConstants.EMPTY_GUID;
        const newSurCharge: CsShipmentSurcharge = new CsShipmentSurcharge(newCharge);

        this._store.dispatch(new fromStore.AddSellingSurchargeAction(newSurCharge));

    }

    deleteCharge(index: number) {
        this._store.dispatch(new fromStore.DeleteSellingSurchargeAction(index));
    }

    saveSellingSurCharge() {
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
            charge.type = CommonEnum.SurchargeTypeEnum.SELLING_RATE;
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

}
