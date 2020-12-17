import { Component, ViewChild, ViewChildren, QueryList, Output, EventEmitter } from '@angular/core';
import { AppList } from '@app';
import { Surcharge, Partner } from '@models';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { CommonEnum } from '@enums';
import { delayTime } from '@decorators';
import { DocumentationRepo } from '@repositories';
import { ReportPreviewComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { ICrystalReport } from '@interfaces';

import { SettlementExistingChargePopupComponent } from '../popup/existing-charge/existing-charge.popup';
import { SettlementFormChargePopupComponent } from '../popup/form-charge/form-charge.popup';
import { SettlementPaymentManagementPopupComponent } from '../popup/payment-management/payment-management.popup';
import { SettlementTableSurchargeComponent } from '../table-surcharge/table-surcharge.component';
import { SettlementShipmentItemComponent } from '../shipment-item/shipment-item.component';
import { SettlementFormCopyPopupComponent } from '../popup/copy-settlement/copy-settlement.popup';
import { SettlementTableListChargePopupComponent } from '../popup/table-list-charge/table-list-charge.component';
import { SettlementChargeFromShipmentPopupComponent } from '../popup/charge-from-shipment/charge-form-shipment.popup';

import cloneDeep from 'lodash/cloneDeep';
import { BehaviorSubject } from 'rxjs';

@Component({
    selector: 'settle-payment-list-charge',
    templateUrl: './list-charge-settlement.component.html',
})

export class SettlementListChargeComponent extends AppList implements ICrystalReport {
    @Output() onChangeType: EventEmitter<any> = new EventEmitter<any>();

    @ViewChild(SettlementExistingChargePopupComponent) existingChargePopup: SettlementExistingChargePopupComponent;
    @ViewChild(SettlementFormChargePopupComponent) formChargePopup: SettlementFormChargePopupComponent;
    @ViewChild(SettlementPaymentManagementPopupComponent) paymentManagementPopup: SettlementPaymentManagementPopupComponent;
    @ViewChild(SettlementFormCopyPopupComponent) copyChargePopup: SettlementFormCopyPopupComponent;
    @ViewChild(SettlementTableListChargePopupComponent) tableListChargePopup: SettlementTableListChargePopupComponent;
    @ViewChild(SettlementChargeFromShipmentPopupComponent) listChargeFromShipmentPopup: SettlementChargeFromShipmentPopupComponent;
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;

    @ViewChild(InjectViewContainerRefDirective) public reportContainerRef: InjectViewContainerRefDirective;

    @ViewChildren('tableSurcharge') tableSurchargeComponent: QueryList<SettlementTableSurchargeComponent>;
    @ViewChildren('headingShipmentGroup') headingShipmentGroup: QueryList<SettlementShipmentItemComponent>;

    groupShipments: any[] = [];
    headers: CommonInterface.IHeaderTable[];

    surcharges: Surcharge[] = [];
    selectedSurcharge: Surcharge = new Surcharge();
    selectedIndexSurcharge: number;

    stateFormCharge: string = 'create';

    openAllCharge: BehaviorSubject<boolean> = new BehaviorSubject(false);
    settlementCode: string = '';

    TYPE: string = 'LIST';
    STATE: string = 'WRITE';  // * list'state READ/WRITE

    isShowButtonCopyCharge: boolean = false;

    constructor(
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _documenRepo: DocumentationRepo,
    ) {
        super();
    }


    ngOnInit() {
        this.headers = [
            { title: 'JobId - HBL - MBL', field: 'jobId', sortable: true, width: 200 },
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
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
        this.tableListChargePopup.isSubmitted = false;
        this.tableListChargePopup.isUpdate = false;
        this.tableListChargePopup.formGroup.reset();
        this.tableListChargePopup.initTableListCharge();
        this.tableListChargePopup.show();
    }

    onRequestSurcharge(surcharge: any) {
        // this.surcharges.push(surcharge);
        this.surcharges = [...this.surcharges, ...surcharge];
        this.surcharges.forEach(x => x.isSelected = false);
        this.TYPE = 'LIST'; // * SWITCH UI TO LIST
    }

    onUpdateSurchargeFromTableChargeList(charges: Surcharge[]) {
        if (charges.length === 1) {
            const indexChargeUpdating: number = this.surcharges.findIndex(item => item.hblid === charges[0].hblid);
            if (indexChargeUpdating !== -1) {
                this.surcharges[indexChargeUpdating] = charges[0];
                this.surcharges = [...this.surcharges];
            }
        } else {
            const hblIds: string[] = charges.map(i => i.hblid);
            this.surcharges = [...this.surcharges].filter(x => !hblIds.includes(x.hblid));
            this.surcharges = [...this.surcharges, ...charges];
        }
    }
    onUpdateRequestSurcharge(surcharge: any) {
        this.TYPE = 'LIST'; // * SWITCH UI TO LIST
        this.surcharges[this.selectedIndexSurcharge] = surcharge;
        this.surcharges = [...this.surcharges];

        if (this.formChargePopup.isContinue) {
            // * Update next charge.
            this.openSurchargeDetail(this.surcharges[this.selectedIndexSurcharge + 1], this.selectedIndexSurcharge + 1, 'update');
        }
    }

    openSurchargeDetail(surcharge: Surcharge, index?: number, action?: string) {
        if (this.STATE !== 'WRITE') {
            return;
        } else {
            // * CHECK SURCHARGE IS FROM SHIPMENT.
            if (!surcharge) {
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

            this.formChargePopup.isFromshipment = surcharge.isFromShipment;

            this.formChargePopup.show();
        }
    }

    changeCurrency(currency: string) {
        // this.formChargePopup.currency.setValue(currency.id);
        this.tableListChargePopup.currencyId = currency || 'VND';
    }

    returnShipmet(item: any) {
        return item.shipment.jobId;
    }

    onClickHeadingShipment(data: any): boolean {
        // * prevent collapse/expand within accordion-heading
        data.event.stopPropagation();
        data.event.preventDefault();

        this.paymentManagementPopup.getDataPaymentManagement(data.data.jobId, data.data.hbl, data.data.mbl);

        this.showPaymentManagementPopup();
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
            const surchargeSelected: Surcharge[] = this.surcharges.filter((surcharge: Surcharge) => surcharge.isSelected);

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
            charge.isSelected = this.isCheckAll;
        }
    }

    onChangeCheckBoxCharge() {
        this.isCheckAll = this.surcharges.every((item: Surcharge) => item.isSelected);
    }

    showPaymentManagement(surcharge: Surcharge) {
        this.paymentManagementPopup.getDataPaymentManagement(surcharge.jobId, surcharge.hbl, surcharge.mbl);
        this.showPaymentManagementPopup();
    }



    openCopySurcharge(surcharge: Surcharge) {
        this.formChargePopup.selectedSurcharge = surcharge;
        this.openSurchargeDetail(surcharge, null, 'copy');
    }

    switchToGroup() {
        if (this.TYPE === 'GROUP') {
            this.TYPE = 'LIST';
        } else if (this.STATE !== 'READ') {
            this.onChangeType.emit();
        } else {
            this.TYPE = 'GROUP';
        }

        this.selectedIndexSurcharge = null;
    }

    showCopyCharge() {
        this.copyChargePopup.show();
    }

    updateChargeWithJob(charge: Surcharge, index?: number) {
        if (this.STATE !== 'WRITE') { return; }
        this.selectedIndexSurcharge = index;
        if (!charge) {
            return;
        }
        if (charge.isFromShipment) {

            const surchargesFromShipment: Surcharge[] = this.surcharges.filter((surcharge: Surcharge) => surcharge.isFromShipment);
            this.listChargeFromShipmentPopup.charges = cloneDeep(surchargesFromShipment);
            this.listChargeFromShipmentPopup.show();
        } else {
            const shipment = this.tableListChargePopup.shipments.find(s => s.jobId === charge.jobId && s.hbl === charge.hbl && s.mbl === charge.mbl);
            if (!!shipment) {
                this.tableListChargePopup.selectedShipment = shipment;

                // * Filter charge with hblID.
                const surcharges: Surcharge[] = this.surcharges.filter((surcharge: Surcharge) => surcharge.hblid === charge.hblid);
                if (!!surcharges.length) {
                    this.tableListChargePopup.charges = cloneDeep(surcharges);

                    this.tableListChargePopup.charges.forEach(item => {

                        if (item.type.toLowerCase() === CommonEnum.CHARGE_TYPE.OBH.toLowerCase()) {
                            // get partner theo payerId.
                            const partner: Partner = this.tableListChargePopup.listPartner.find(p => p.id === item.payerId);
                            if (!!partner) {
                                // swap để map field cho chage obh
                                item.payer = partner.shortName;
                                item.obhId = item.paymentObjectId;
                                item.paymentObjectId = item.payerId;
                            }
                        } else {
                            // get partner theo paymentObjectId.
                            const partner: Partner = this.tableListChargePopup.listPartner.find(p => p.id === item.paymentObjectId);
                            if (!!partner) {
                                item.payer = partner.shortName;
                            }
                        }

                        if (!!item.invoiceDate && typeof item.invoiceDate === 'string') {
                            item.invoiceDate = { startDate: new Date(item.invoiceDate), endDate: new Date(item.invoiceDate) };
                        }
                    });
                    this.tableListChargePopup.getAdvances(shipment.jobId);

                    const selectedCD = this.tableListChargePopup.cds.find(x => x.clearanceNo === surcharges[0].clearanceNo);
                    if (!!selectedCD) {
                        this.tableListChargePopup.selectedCD = selectedCD;
                    }
                    // * Update value form.
                    this.tableListChargePopup.formGroup.patchValue({
                        shipment: shipment.hblid,
                        advanceNo: surcharges[0].advanceNo,
                        customNo: !!surcharges[0].clearanceNo ? surcharges[0].clearanceNo : null
                    });
                    this.tableListChargePopup.isUpdate = true;
                    this.tableListChargePopup.show();
                }
            } else {
                // * Not found shipment in list PIC
                this._toastService.warning("Please check again!", 'Not found shipment');
            }
        }
    }

    onUpdateChargeFromShipment(surchargeFromShipment: Surcharge[]) {
        this.selectedIndexSurcharge = null;

        const surChargeisNotFromShipment = this.surcharges.filter(x => !x.isFromShipment);
        this.surcharges.length = 0;
        this.surcharges = [...surChargeisNotFromShipment, ...surchargeFromShipment];

    }

    previewPLsheet(data: any, currency: string) {
        if (data.type === 'DOC') {
            this._documenRepo.previewSIFPLsheet(data.shipmentId, data.hblId, currency)
                .subscribe(
                    (res: any) => {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
            return;
        }
        if (data.type === "OPS") {
            this._documenRepo.previewPL(data.shipmentId, currency)
                .subscribe(
                    (res: any) => {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
            return;
        }
        if (data.typeService === "OPS") {
            this._documenRepo.previewPL(data.shipmentId, currency)
                .subscribe(
                    (res: any) => {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
            return;
        } else {
            this._documenRepo.previewSIFPLsheet(data.shipmentId, data.hblid, currency)
                .subscribe(
                    (res: any) => {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
            return;
        }

    }

    @delayTime(500)
    showPaymentManagementPopup() {
        this.paymentManagementPopup.show();
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.reportContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.reportContainerRef.viewContainerRef.clear();
            });

    }
}


