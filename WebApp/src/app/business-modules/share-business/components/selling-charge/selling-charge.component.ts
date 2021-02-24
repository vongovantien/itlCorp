import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { ShareBussinessBuyingChargeComponent } from '../buying-charge/buying-charge.component';
import { CatalogueRepo, DocumentationRepo, AccountingRepo } from '@repositories';
import { SortService } from '@services';
import { CsShipmentSurcharge, Charge, Unit } from '@models';
import { SystemConstants } from '@constants';
import { CommonEnum } from '@enums';

import { takeUntil, catchError, finalize } from 'rxjs/operators';

import * as fromStore from './../../store';
import cloneDeep from 'lodash/cloneDeep';
import { NgxSpinnerService } from 'ngx-spinner';
import { ActivatedRoute } from '@angular/router';
import { getCatalogueCurrencyState, getCatalogueUnitState } from '@store';


@Component({
    selector: 'selling-charge',
    templateUrl: './selling-charge.component.html',
    styleUrls: ['./../buying-charge/buying-charge.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush

})

export class ShareBussinessSellingChargeComponent extends ShareBussinessBuyingChargeComponent {

    @Input() showSyncFreight: boolean = true;
    @Input() showGetCharge: boolean = true;
    @Input() showSyncStandard: boolean = true;
    @Input() allowSaving: boolean = true; // * not allow to save or add Charges without saving the job

    TYPE: any = CommonEnum.SurchargeTypeEnum.SELLING_RATE;

    constructor(
        protected _catalogueRepo: CatalogueRepo,
        protected _store: Store<fromStore.IShareBussinessState>,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _sortService: SortService,
        protected _ngProgressService: NgProgress,
        protected _spinner: NgxSpinnerService,
        protected _accountingRepo: AccountingRepo,
        protected _activedRoute: ActivatedRoute,
        protected _cd: ChangeDetectorRef
    ) {
        super(_catalogueRepo,
            _store,
            _documentRepo,
            _toastService,
            _sortService,
            _ngProgressService,
            _spinner,
            _accountingRepo,
            _activedRoute,
            _cd);
        this._progressRef = this._ngProgressService.ref();

    }

    getPartner() {
        this.isShowLoadingPartner = true;
        this._spinner.show(this.spinnerpartner);

        this._catalogueRepo.getListPartner(null, null, { active: true })
            .pipe(
                catchError(this.catchError), finalize(() => {
                    this._spinner.hide(this.spinnerpartner);
                    this.isShowLoadingPartner = false;
                }))
            .subscribe(
                (partners: any[]) => {
                    this.listPartner = partners;
                }
            );

    }

    getCurrency() {
        this.listCurrency = this._store.select(getCatalogueCurrencyState);
    }

    getUnits() {
        this._store.select(getCatalogueUnitState)
            .pipe(catchError(this.catchError))
            .subscribe(
                (units: Unit[]) => {
                    this.listUnits = units;
                }
            );
    }

    getSurcharge() {
        this._store.select(fromStore.getSellingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    this.charges = buyings;
                    this._cd.markForCheck();

                }
            );
    }

    configHeader() {
        this.headers = [
            { title: 'Partner Name', field: 'partnerShortName', required: true, sortable: true, width: 150 },
            { title: 'Charge Name', field: 'chargeId', required: true, sortable: true, width: 250 },
            { title: 'Quantity', field: 'quantity', required: true, sortable: true, width: 150 },
            { title: 'Unit', field: 'unitId', required: true, sortable: true },
            { title: 'Unit Price', field: 'unitPrice', required: true, sortable: true },
            { title: 'Currency', field: 'currencyId', required: true, sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Total', field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
            { title: 'Fee Type', field: 'chargeGroup', sortable: true },
            { title: 'SOA', field: 'soano', sortable: true },
            { title: 'Credit/Debit Note', field: 'cdno', sortable: true },
            // { title: 'Settle Payment', field: 'settlementCode', sortable: true },
            { title: 'Exchange Rate Date', field: 'exchangeDate', sortable: true },
            { title: 'Final Exchange Rate', field: 'finalExchangeRate', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Invoice Date', field: 'invoiceDate', sortable: true },
            { title: 'Voucher ID', field: 'voucherId', sortable: true },
            { title: 'Voucher ID Date', field: 'voucherIddate', sortable: true },
            { title: 'Net Amount', field: 'netAmount', sortable: true },
        ];
    }

    getCharge() {
        this._catalogueRepo.getCharges({ active: true, serviceTypeId: this.serviceTypeId, type: CommonEnum.CHARGE_TYPE.DEBIT })
            .subscribe(
                (charges: Charge[]) => {
                    this.listCharges = charges;
                }
            );
    }

    saveSellingSurCharge() {
        if (!this.charges.length) {
            this._toastService.warning("Please add charge");
            return;
        }
        // * Update data 
        this.isSubmitted = true;
        if (!this.checkValidate()) {
            return;
        }

        if (!this.checkDuplicate()) {
            return;
        }

        this.updateSurchargeField(CommonEnum.SurchargeTypeEnum.SELLING_RATE);
        this._progressRef.start();

        this._documentRepo.addShipmentSurcharges(this.charges)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        // Tính công nợ
                        this.calculatorReceivable(this.charges);

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
                                newSurCharge.hblno = this.hbl.hwbno || null;
                                newSurCharge.mblno = this.getMblNo(this.shipment, this.hbl);
                                newSurCharge.jobNo = this.shipment.jobNo || null;

                                // * Default get partner = customer name's hbl.
                                newSurCharge.partnerShortName = this.hbl.customerName;
                                newSurCharge.paymentObjectId = this.hbl.customerId;

                                this._store.dispatch(new fromStore.AddSellingSurchargeAction(newSurCharge));
                            }
                        }
                    }
                }
            );
    }

    syncBuyingCharge() {
        this._store.select(fromStore.getBuyingSurChargeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (buyings: CsShipmentSurcharge[]) => {
                    if (!buyings.length) {
                        this._toastService.warning("Not found buying charge");
                        return;
                    }

                    const buyingCharges: CsShipmentSurcharge[] = cloneDeep(buyings);

                    // * update debit charge to chargeId
                    buyingCharges.forEach(c => {
                        c.chargeId = c.debitCharge;
                        c.id = SystemConstants.EMPTY_GUID;
                        c.type = CommonEnum.SurchargeTypeEnum.SELLING_RATE;
                        c.invoiceDate = null;
                        c.creditNo = null;
                        c.debitNo = null;
                        c.settlementCode = null;
                        c.voucherId = null;
                        c.voucherIddate = null;
                        c.voucherIdre = null;
                        c.voucherIdredate = null;
                        c.isFromShipment = true;
                        c.invoiceNo = null;
                        c.invoiceDate = null;
                        c.finalExchangeRate = null;
                        c.acctManagementId = null;

                        // Mặc định lấy customer name của HBL
                        c.paymentObjectId = this.service === 'logistic' ? this.shipment.customerId : this.hbl.customerId;
                        c.partnerName = this.service === 'logistic' ? this.shipment.customerName : this.hbl.customerName;
                        c.partnerShortName = this.service === 'logistic' ? this.shipment.customerName : this.hbl.customerName;
                        c.exchangeDate = { startDate: new Date(), endDate: new Date() };
                        this._store.dispatch(new fromStore.AddSellingSurchargeAction(c));
                    });

                    // this.charges = [...this.charges, ...buyingCharges];
                }
            );
    }
}
