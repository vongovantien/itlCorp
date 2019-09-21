import { Component, ViewChild, Input, ViewChildren, QueryList } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettlementExistingChargePopupComponent } from '../popup/existing-charge/existing-charge.popup';
import { SettlementFormChargePopupComponent } from '../popup/form-charge/form-charge.popup';
import { Surcharge, Currency } from 'src/app/shared/models';
import { BehaviorSubject } from 'rxjs';
import { SettlementPaymentManagementPopupComponent } from '../popup/payment-management/payment-management.popup';
import { SettlementTableSurchargeComponent } from '../table-surcharge/table-surcharge.component';
import { SettlementShipmentItemComponent } from '../shipment-item/shipment-item.component';
import { SortService } from 'src/app/shared/services';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'settle-payment-list-charge',
    templateUrl: './list-charge-settlement.component.html',
})

export class SettlementListChargeComponent extends AppList {


    @ViewChild(SettlementExistingChargePopupComponent, { static: true }) existingChargePopup: SettlementExistingChargePopupComponent;
    @ViewChild(SettlementFormChargePopupComponent, { static: false }) formChargePopup: SettlementFormChargePopupComponent;
    @ViewChild(SettlementPaymentManagementPopupComponent, { static: false }) paymentManagementPopup: SettlementPaymentManagementPopupComponent;

    @ViewChildren('tableSurcharge') tableSurchargeComponent: QueryList<SettlementTableSurchargeComponent>;
    @ViewChildren('headingShipmentGroup') headingShipmentGroup: QueryList<SettlementShipmentItemComponent>;

    groupShipments: any[] = [];
    headers: CommonInterface.IHeaderTable[];

    surcharges: Surcharge[] = [];
    selectedSurcharge: Surcharge = new Surcharge();
    selectedIndexSurcharge: number = 0;

    stateFormCharge: string = 'create';

    openAllCharge: BehaviorSubject<boolean> = new BehaviorSubject(false);
    settlementCode: string = '';

    TYPE: string = 'LIST';
    STATE: string = 'WRITE';  // * list'state READ/WRITE
    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'JobId', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'Qty', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unitName', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currencyId', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Amount', field: 'total', sortable: true },
            { title: 'Payer', field: 'payer', sortable: true },
            { title: 'OBH Partner', field: 'obhPartnerName', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Series No', field: 'seriesNo', sortable: true },
            { title: 'Inv Date', field: 'invoiceDate', sortable: true },
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Cont No', field: 'contNo', sortable: true },
            { title: 'Note', field: 'notes', sortable: true },
        ];
        this.selectedSurcharge = this.surcharges[0];

    }

    showExistingCharge() {
        this.existingChargePopup.show();
    }

    showCreateCharge() {
        this.formChargePopup.settlementCode = this.settlementCode;
        this.stateFormCharge = 'create';
        this.formChargePopup.action = 'create';
        this.formChargePopup.show();
    }

    onRequestSurcharge(surcharge: any) {
        this.surcharges.push(...surcharge);
        this.TYPE = 'LIST'; // * SWITCH UI TO LIST
    }

    onUpdateRequestSurcharge(surcharge: any) {
        this.TYPE = 'LIST'; // * SWITCH UI TO LIST
        this.surcharges[this.selectedIndexSurcharge] = surcharge;
    }

    openSurchargeDetail(surcharge: Surcharge, index?: number, action?: string) {
        if (this.STATE !== 'WRITE') {
            return;
        } else {
            // * CHECK SURCHARGE IS FROM SHIPMENT.
            if (surcharge.isFromShipment) {
                return;
            } else if (this.TYPE === 'LIST') {
                this.selectedIndexSurcharge = index;
            } else {
                const indexSurcharge: number = this.surcharges.findIndex(item => item.id === surcharge.id);
                if (indexSurcharge !== - 1) {
                    this.selectedIndexSurcharge = indexSurcharge;
                }
            }
            this.selectedSurcharge = surcharge;
            this.stateFormCharge = 'update';

            this.formChargePopup.action = action;
            this.formChargePopup.settlementCode = this.selectedSurcharge.settlementCode;
            this.formChargePopup.initFormUpdate(this.selectedSurcharge);
            this.formChargePopup.calculateTotalAmount();

            this.formChargePopup.show();
        }
    }

    changeCurrency(currency: Currency) {
        this.formChargePopup.currency.setValue(currency.id);
    }

    returnShipmet(item: any) {
        return item.shipment.jobId;
    }

    onClickHeadingShipment(data: any): boolean {
        // * prevent collapse/expand within accordion-heading
        data.event.stopPropagation();
        data.event.preventDefault();

        this.paymentManagementPopup.getDataPaymentManagement(data.data.jobId, data.data.hbl, data.data.mbl);
        setTimeout(() => {
            this.paymentManagementPopup.show();
        }, 500);
        return false;
    }

    // * Handle checkbox from heading
    onCheckBoxShipmentItemInGroupShipment(isCheck: boolean, indexShipmentItem: number): any {
        const tableChargeChildComponent: SettlementTableSurchargeComponent[] = this.tableSurchargeComponent.toArray();
        tableChargeChildComponent[indexShipmentItem].isCheckAll = isCheck;
        tableChargeChildComponent[indexShipmentItem].checkUncheckAllCharge();

        this.groupShipments[indexShipmentItem].isSelected = true;
    }

    // * Handle checkbox from listCharge in group.
    onChangeCheckBoxSurChargeListInGroupShipment(isCheckAll: boolean, indexShipmentItem: number) {
        const headingShipmentComponent: SettlementShipmentItemComponent[] = this.headingShipmentGroup.toArray();
        headingShipmentComponent[indexShipmentItem].isCheckAll = isCheckAll;
    }

    deleteShipmentItem() {
        if (this.TYPE === 'GROUP') {
            this.surcharges = [];
            const lastGroupShipment: any[] = this.groupShipments.filter((groupItem: any) => !groupItem.isSelected);

            for (const groupShipment of this.groupShipments) {
                groupShipment.chargeSettlements = this.returnChargeFromShipment(groupShipment);
            }

            // * UPDATE SURCHARGE LIST.
            for (const groupShipmentItem of lastGroupShipment) {
                this.surcharges.push(...groupShipmentItem.chargeSettlements);
            }

            // * UPDATE GROUP SHIPMENT LIST
            this.groupShipments = this.groupShipments.filter((groupItem: any) => groupItem.chargeSettlements.length);
        } else {
            const surchargeSelected: Surcharge[] = this.surcharges.filter((surcharge: Surcharge) => surcharge.isSelected && !surcharge.isFromShipment);

            if (!!surchargeSelected.length) {
                this.surcharges = this.surcharges.filter((surcharge: Surcharge) => !surcharge.isSelected);
            } else {
                this._toastService.warning(`Don't have any charges in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
                return;
            }
        }
        const headingShipmentComponent: SettlementShipmentItemComponent[] = this.headingShipmentGroup.toArray();

        // * Reset check all in heading shipment group.
        for (const item of headingShipmentComponent) {
            item.isCheckAll = false;
        }
    }

    returnChargeFromShipment(groupShipment: any) {
        return groupShipment.chargeSettlements.filter((surcharge: Surcharge) => !surcharge.isSelected);
    }

    sortSurcharge(sortData: any) {
        this.surcharges = this._sortService.sort(this.surcharges, sortData.sortField, sortData.order);
    }

    checkUncheckAllCharge() {
        for (const charge of this.surcharges) {
            if (!charge.isFromShipment) {
                charge.isSelected = this.isCheckAll;
            }
        }
    }

    onChangeCheckBoxCharge() {
        this.isCheckAll = this.surcharges.every((item: Surcharge) => item.isSelected);
    }

    showPaymentManagement(surcharge: Surcharge) {
        this.paymentManagementPopup.getDataPaymentManagement(surcharge.jobId, surcharge.hbl, surcharge.mbl);
        this.paymentManagementPopup.show();
    }

    openCopySurcharge(surcharge: Surcharge) {
        this.formChargePopup.selectedSurcharge = surcharge;
        this.openSurchargeDetail(surcharge, null, 'copy');
    }
}


