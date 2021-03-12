import { Component, ViewChild, ViewChildren, QueryList, Input } from '@angular/core';
import { coerceBooleanProperty } from "@angular/cdk/coercion";
import { takeUntil } from 'rxjs/operators';
import { AppList } from '@app';
import { Surcharge, Partner } from '@models';
import { SortService, DataService } from '@services';
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
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }

    get readonly(): boolean {
        return this._readonly;
    }

    private _readonly: boolean = false;

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
        private _dataService: DataService
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

        this.subscriptionDuplicateChargeState();
    }

    showExistingCharge() {
        this.existingChargePopup.show();
    }

    showCreateCharge() {
        this.tableListChargePopup.isSubmitted = false;
        this.tableListChargePopup.isUpdate = false;
        this.tableListChargePopup.formGroup.reset();
        this.tableListChargePopup.initTableListCharge();
        this.tableListChargePopup.settlementCode = this.settlementCode || null;
        this.tableListChargePopup.show();
    }

    onRequestSurcharge(surcharge: any) {
        // this.surcharges.push(surcharge);
        this.surcharges = [...this.surcharges, ...surcharge];
        this.surcharges.forEach(x => x.isSelected = false);
        this.TYPE = 'LIST'; // * SWITCH UI TO LIST
    }

    onUpdateSurchargeFromTableChargeList(charges: Surcharge[]) {
        if (charges.length) {
            this.selectedIndexSurcharge = -1;

            const surchargeFromShipment = this.surcharges.filter(x => x.isFromShipment);

            const hblIds: string[] = charges.map(x => x.hblid);
            if (charges[0].isChangeShipment) {
                this.surcharges = this.surcharges.filter(x => hblIds.indexOf(x.hblid) && x.isChangeShipment === false);
            } else {
                const jobNos: string[] = charges.map(x => x.jobNo);

                this.surcharges = this.surcharges.filter(x => hblIds.indexOf(x.hblid) && jobNos.indexOf(x.jobId));
            }

            this.surcharges = [...charges, ...this.surcharges];
            this.surcharges.forEach(c => c.isChangeShipment = undefined)

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
        if (this.STATE !== 'WRITE') { return; }
        this.formChargePopup.selectedSurcharge = surcharge;
        this.openSurchargeDetail(surcharge, null, 'copy');
    }

    switchToGroup() {
        if (this.TYPE === 'GROUP') {
            this.TYPE = 'LIST';
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
            const surchargesFromShipment: Surcharge[] = this.surcharges.filter((surcharge: Surcharge) => surcharge.hblid === charge.hblid && surcharge.isFromShipment);

            this.listChargeFromShipmentPopup.charges = cloneDeep(surchargesFromShipment);
            this.listChargeFromShipmentPopup.show();
        } else {
            const shipment = this.tableListChargePopup.shipments.find(s => s.jobId === charge.jobId && s.hbl === charge.hbl && s.mbl === charge.mbl);
            if (!!shipment) {
                this.tableListChargePopup.selectedShipment = shipment;
                this.tableListChargePopup.settlementCode = this.settlementCode || null;

                // * Filter charge with hblID.
                const surcharges: Surcharge[] = this.surcharges.filter((surcharge: Surcharge) => surcharge.hblid === charge.hblid && !surcharge.isFromShipment);
                if (!!surcharges.length) {
                    const hblIds: string[] = surcharges.map(x => x.hblid);

                    this.surcharges.forEach(x => {
                        if (hblIds.indexOf(x.hblid)) {
                            x.isChangeShipment = false; //  * Mark item is editing.
                        }
                    });

                    this.tableListChargePopup.charges = cloneDeep(surcharges);

                    this.tableListChargePopup.charges.forEach(item => {
                        item.isDuplicate = false; // * Reset duplicate state

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
                    if (!!this.settlementCode) {
                        if (charge.advanceNo) {
                            this.tableListChargePopup.getAdvances(shipment.jobId, !!charge.advanceNo, this.settlementCode);
                        } else {
                            this.tableListChargePopup.advs.length = 0;
                        }
                    } else {
                        if (charge.advanceNo) {
                            this.tableListChargePopup.getAdvances(shipment.jobId, !!charge.advanceNo);
                        } else {
                            this.tableListChargePopup.advs.length = 0;
                        }
                    }

                    const selectedCD = this.tableListChargePopup.cds.find(x => x.clearanceNo === surcharges[0].clearanceNo);
                    if (!!selectedCD) {
                        this.tableListChargePopup.selectedCD = selectedCD;
                    }
                    // * Update value form.
                    this.tableListChargePopup.formGroup.patchValue({
                        shipment: shipment.hblid,
                        advanceNo: !!charge.advanceNo ? charge.advanceNo : null,
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
        const hblidsSurchargefromshipmentUpdate = surchargeFromShipment.map(x => x.hblid);

        const surChargeFromShipmentAnother = this.surcharges.filter(x => x.isFromShipment && !hblidsSurchargefromshipmentUpdate.includes(x.hblid));

        this.surcharges.length = 0;
        this.surcharges = [...surChargeisNotFromShipment, ...surchargeFromShipment, ...surChargeFromShipmentAnother];

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

    subscriptionDuplicateChargeState() {
        this._dataService.currentMessage
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (error: any) => {
                    if (error?.duplicateChargeSettlement?.data) {
                        let chargeDup: Partial<DuplicateShipmentSettlementResultModel> = {};
                        if (error?.duplicateChargeSettlement?.data?.length) {
                            chargeDup = error?.duplicateChargeSettlement?.data[0];
                        } else {
                            let errorData = error?.duplicateChargeSettlement?.data;
                            chargeDup.chargeId = errorData?.chargeId;
                            chargeDup.jobNo = errorData?.jobId;
                            chargeDup.mblNo = errorData?.mbl;
                            chargeDup.hblNo = errorData?.hbl;
                        }
                        this.surcharges.forEach(c => {
                            if ((c.jobNo || c.jobId) === chargeDup.jobNo
                                && (c.mblno || c.mbl) === chargeDup.mblNo
                                && (c.hblno || c.hbl) === chargeDup.hblNo
                                && (c.chargeId) === chargeDup.chargeId) {
                                c.isDuplicate = true;
                            } else {
                                c.isDuplicate = false;
                            }
                        })
                    }
                }
            )
    }
}

interface DuplicateShipmentSettlementResultModel {
    jobNo: string;
    jobId?: string;
    mblNo: string;
    hblNo: string;
    chargeId: string;
}


