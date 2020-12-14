import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { formatDate } from '@angular/common';
import { ActionsSubject, Store } from '@ngrx/store';
import { ToastrService } from 'ngx-toastr';

import { DocumentationRepo, CatalogueRepo } from '@repositories';
import { AppForm } from 'src/app/app.form';
import { InfoPopupComponent, ConfirmPopupComponent } from '@common';
import { Container, CsTransaction } from '@models';
import { RoutingConstants, SystemConstants } from '@constants';
import { DataService } from '@services';

import {
    ShareBusinessArrivalNoteComponent,
    ShareBusinessDeliveryOrderComponent,
    ShareBusinessImportHouseBillDetailComponent,
    ShareBussinessHBLGoodSummaryLCLComponent,
    getTransactionPermission,
} from '@share-bussiness';

import { ShareSeaServiceFormCreateHouseBillSeaImportComponent } from 'src/app/business-modules/documentation/share-sea/components/form-create-hbl-sea-import/form-create-hbl-sea-import.component';

import { catchError, takeUntil, mergeMap, finalize } from 'rxjs/operators';
import { forkJoin } from 'rxjs';

import * as fromShareBussiness from '../../../../../share-business/store';
import isUUID from 'validator/lib/isUUID';
import groupBy from 'lodash/groupBy';

enum HBL_TAB {
    DETAIL = 'DETAIL',
    ARRIVAL = 'ARRIVAL',
    DELIVERY = 'DELIVERY'

}

@Component({
    selector: 'sea-lcl-import-create-house-bill',
    templateUrl: './sea-lcl-import-create-house-bill.component.html',
})
export class SeaLCLImportCreateHouseBillComponent extends AppForm {

    @ViewChild(ShareSeaServiceFormCreateHouseBillSeaImportComponent) formHouseBill: ShareSeaServiceFormCreateHouseBillSeaImportComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmCreatePopup: ConfirmPopupComponent;
    @ViewChild(ShareBussinessHBLGoodSummaryLCLComponent) hblGoodsSummaryComponent: ShareBussinessHBLGoodSummaryLCLComponent;
    @ViewChild(ShareBusinessArrivalNoteComponent) arrivalNoteComponent: ShareBusinessArrivalNoteComponent;
    @ViewChild(ShareBusinessDeliveryOrderComponent) deliveryComponent: ShareBusinessDeliveryOrderComponent;
    @ViewChild(ShareBusinessImportHouseBillDetailComponent) importHouseBillPopup: ShareBusinessImportHouseBillDetailComponent;

    jobId: string = '';
    shipmentDetail: CsTransaction;
    selectedHbl: any = {}; // TODO model.
    containers: Container[] = [];
    selectedTab: string = HBL_TAB.DETAIL;

    constructor(
        protected _progressService: NgProgress,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _activedRoute: ActivatedRoute,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _store: Store<fromShareBussiness.ITransactionState>,
        protected _cd: ChangeDetectorRef,
        protected _dataService: DataService


    ) {
        super();
        this._progressRef = this._progressService.ref();

        this._actionStoreSubject
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (action: fromShareBussiness.ContainerAction) => {
                    if (action.type === fromShareBussiness.ContainerActionTypes.SAVE_CONTAINER) {
                        this.containers = action.payload;

                        if (!!this.containers) {
                            this.containers.forEach(c => {
                                c.id = c.mblid = SystemConstants.EMPTY_GUID;
                            });
                        }
                        if (this.containers.length === 0) {
                            const packageName = this.hblGoodsSummaryComponent.packages.find(x => x.id == this.hblGoodsSummaryComponent.selectedPackage).code;
                            const data = { 'package': packageName, 'quantity': this.hblGoodsSummaryComponent.packageQty };
                            this.formHouseBill.formGroup.controls["inWord"].setValue(this.handleStringPackage(data));
                        }
                        if (this.containers.length > 0) {
                            // * Update field inword with container data.
                            this.formHouseBill.formGroup.controls["inWord"].setValue(this.updateInwordField(this.containers));
                        }
                    }
                });
    }

    ngOnInit() {
        this._activedRoute.params.subscribe((param: Params) => {
            if (param.jobId && isUUID(param.jobId)) {
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                this.permissionShipments = this._store.select(getTransactionPermission);
            } else {
                this.combackToHBLList();
            }
        });
    }

    showImportPopup() {
        const dataSearch = { jobId: this.jobId };
        dataSearch.jobId = this.jobId;
        this.importHouseBillPopup.typeFCL = 'Import';
        this.importHouseBillPopup.selected = - 1;
        this.importHouseBillPopup.typeTransaction = 9;
        this.importHouseBillPopup.getHourseBill(dataSearch);
        this.importHouseBillPopup.show();
    }

    onSelectTab(tabName: HBL_TAB | string) {
        this.selectedTab = tabName;
    }

    ngAfterViewInit() {
        this._store.dispatch(new fromShareBussiness.GetDetailHBLSuccessAction({}));

        this.hblGoodsSummaryComponent.initContainer();
        this.hblGoodsSummaryComponent.description = "AS PER BILL";
        this.hblGoodsSummaryComponent.containerDetail = 'A PART OF CONTAINER S.T.C';
        this.formHouseBill.notifyPartyDescription.setValue("SAME AS CONSIGNEE");
        this.formHouseBill.type = 'SLI';
        this._cd.detectChanges();
    }

    checkValidateForm() {
        let valid: boolean = true;

        if (!this.formHouseBill.formGroup.valid) {
            valid = false;
        }
        if (
            this.hblGoodsSummaryComponent.grossWeight === null
            || this.hblGoodsSummaryComponent.totalCBM === null
            || this.hblGoodsSummaryComponent.packageQty === null
            || this.hblGoodsSummaryComponent.grossWeight < 0
            || this.hblGoodsSummaryComponent.totalCBM < 0
            || this.hblGoodsSummaryComponent.packageQty < 0
        ) {
            valid = false;
        }
        return valid;
    }

    oncreate() {
        this.confirmCreatePopup.hide();
        this.formHouseBill.isSubmited = true;
        this.hblGoodsSummaryComponent.isSubmitted = true;

        if (!this.checkValidateForm() || !this.arrivalNoteComponent.checkValidate() || !this.deliveryComponent.deliveryOrder.deliveryOrderNo) {
            this.arrivalNoteComponent.isSubmitted = true;
            this.deliveryComponent.isSubmitted = true;
            this.infoPopup.show();
        } else {
            const body = this.onsubmitData();
            this._catalogueRepo.getSalemanIdByPartnerId(body.customerId, this.jobId).subscribe((res: any) => {
                if (!!res.salemanId) {
                    if (res.salemanId !== body.saleManId) {
                        this._toastService.error('Not found contract information, please check!');
                        return;
                    }
                }
                if (!!res.officeNameAbbr) {
                    this._toastService.error('The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again', 'Cannot Create House Bill!');
                } else {
                    this.createHbl(body);

                }
            });
        }
    }

    onImport(selectedData: any) {
        this.selectedHbl = selectedData;
        if (!!this.selectedHbl) {
            this.formHouseBill.onUpdateDataToImport(this.selectedHbl);
        }
    }

    showCreatePpoup() {
        this.confirmCreatePopup.show();
    }

    combackToHBLList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}/hbl`]);
    }

    createHbl(body: any) {
        if (this.formHouseBill.formGroup.valid) {

            this._progressRef.start();
            this._documentationRepo.createHousebill(body)
                .pipe(
                    mergeMap((res: CommonInterface.IResult) => {
                        const dateNotice = {
                            arrivalFirstNotice: !!this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice && !!this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice.startDate ? formatDate(this.arrivalNoteComponent.hblArrivalNote.arrivalFirstNotice.startDate, 'yyyy-MM-dd', 'en') : formatDate(new Date(), 'yyyy-MM-dd', 'en'),
                            arrivalSecondNotice: !!this.arrivalNoteComponent.hblArrivalNote.arrivalSecondNotice && <any>!!this.arrivalNoteComponent.hblArrivalNote.arrivalSecondNotice.startDate ? formatDate(this.arrivalNoteComponent.hblArrivalNote.arrivalSecondNotice.startDate, 'yyyy-MM-dd', 'en') : null,
                        };
                        this.arrivalNoteComponent.hblArrivalNote.hblid = res.data;
                        const arrival = this._documentationRepo.updateArrivalInfo(Object.assign({}, this.arrivalNoteComponent.hblArrivalNote, dateNotice));
                        const printedDate = {
                            deliveryOrderPrintedDate: !!this.deliveryComponent.deliveryOrder.deliveryOrderPrintedDate && !!this.deliveryComponent.deliveryOrder.deliveryOrderPrintedDate.startDate ? formatDate(this.deliveryComponent.deliveryOrder.deliveryOrderPrintedDate.startDate, 'yyyy-MM-dd', 'en') : null,
                        };
                        this.deliveryComponent.deliveryOrder.hblid = res.data;
                        const delivery = this._documentationRepo.updateDeliveryOrderInfo(Object.assign({}, this.deliveryComponent.deliveryOrder, printedDate));

                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}/hbl/${res.data}`]);

                        return forkJoin([arrival, delivery]);
                    }),
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe(result => {
                    this._toastService.success(result[0].message, '');
                }
                );
        }
    }

    selectedPackage($event: any) {
        const data = $event;
        if (!data.quantity) {
            this.formHouseBill.formGroup.controls["inWord"].setValue(null);
            return;
        }
        this.formHouseBill.formGroup.controls["inWord"].setValue(this.handleStringPackage(data));
    }

    updateInwordField(containers: Container[]) {
        console.log(containers);

        if (!containers.length) {
            return null;
        }

        let containerDetail = '';

        const contObject = (containers || []).map((container: Container) => ({
            package: container.packageTypeName,
            quantity: container.packageQuantity,
        }));

        const contData = [];
        for (const keyName of Object.keys(groupBy(contObject, 'package'))) {
            contData.push({
                package: keyName,
                quantity: groupBy(contObject, 'package')[keyName].map(i => i.quantity).reduce((a: any, b: any) => a += b),
            });
        }
        for (const item of contData) {

            containerDetail += this.handleStringPackage(item);
            if (contData.length > 1) {
                containerDetail += " AND ";
            }
        }

        if (contData.length > 1) {
            containerDetail = containerDetail.substring(0, containerDetail.length - 5);
        }

        return containerDetail;
    }

    handleStringPackage(contOb: { package: string, quantity: number }) {
        return `${this.utility.convertNumberToWords(contOb.quantity)}${contOb.package}`;
    }

    onsubmitData() {
        const body = {
            id: SystemConstants.EMPTY_GUID,
            jobId: this.jobId,
            etd: !!this.formHouseBill.etd.value && this.formHouseBill.etd.value.startDate != null ? formatDate(this.formHouseBill.etd.value.startDate !== undefined ? this.formHouseBill.etd.value.startDate : this.formHouseBill.etd.value, 'yyyy-MM-dd', 'en') : null,
            eta: !!this.formHouseBill.eta.value ? formatDate(this.formHouseBill.eta.value.startDate !== undefined ? this.formHouseBill.eta.value.startDate : this.formHouseBill.eta.value, 'yyyy-MM-dd', 'en') : null,
            etawarehouse: !!this.formHouseBill.etawarehouse.value && this.formHouseBill.etawarehouse.value.startDate != null ? formatDate(this.formHouseBill.etawarehouse.value.startDate !== undefined ? this.formHouseBill.etawarehouse.value.startDate : this.formHouseBill.etawarehouse.value, 'yyyy-MM-dd', 'en') : null,
            issueHBLDate: !!this.formHouseBill.issueHBLDate.value && this.formHouseBill.issueHBLDate.value.startDate != null ? formatDate(this.formHouseBill.issueHBLDate.value.startDate !== undefined ? this.formHouseBill.issueHBLDate.value.startDate : this.formHouseBill.issueHBLDate.value, 'yyyy-MM-dd', 'en') : null,
            documentDate: !!this.formHouseBill.documentDate.value && this.formHouseBill.documentDate.value.startDate != null ? formatDate(this.formHouseBill.documentDate.value.startDate !== undefined ? this.formHouseBill.documentDate.value.startDate : this.formHouseBill.documentDate.value, 'yyyy-MM-dd', 'en') : null,

            hbltype: this.formHouseBill.hbltype.value,
            servicetype: this.formHouseBill.servicetype.value,
            originBLNumber: this.formHouseBill.originBLNumber.value,

            mawb: this.formHouseBill.mtBill.value,
            saleManId: !!this.formHouseBill.saleMan.value ? this.formHouseBill.saleMan.value : null,
            shipperId: !!this.formHouseBill.shipper.value ? this.formHouseBill.shipper.value : null,
            shipperDescription: !!this.formHouseBill.shipperDescription.value ? this.formHouseBill.shipperDescription.value : null,
            consigneeId: this.formHouseBill.consignee.value,
            consigneeDescription: !!this.formHouseBill.consigneeDescription.value ? this.formHouseBill.consigneeDescription.value : null,
            notifyPartyId: !!this.formHouseBill.notifyParty.value ? this.formHouseBill.notifyParty.value : null,
            notifyPartyDescription: !!this.formHouseBill.notifyPartyDescription.value ? this.formHouseBill.notifyPartyDescription.value : null,
            alsoNotifyPartyId: !!this.formHouseBill.alsoNotifyParty.value ? this.formHouseBill.alsoNotifyParty.value : null,
            alsoNotifyPartyDescription: !!this.formHouseBill.alsonotifyPartyDescription.value ? this.formHouseBill.alsonotifyPartyDescription.value : null,
            hwbno: this.formHouseBill.hwbno.value,
            pickupPlace: this.formHouseBill.pickupPlace.value,
            pol: this.formHouseBill.pol.value,
            pod: this.formHouseBill.pod.value,
            finalDestinationPlace: this.formHouseBill.finalDestinationPlace.value,
            coloaderId: this.formHouseBill.supplier.value !== undefined ? this.formHouseBill.supplier.value : null,
            localVessel: this.formHouseBill.localVessel.value,
            localVoyNo: this.formHouseBill.localVoyNo.value,
            oceanVessel: this.formHouseBill.oceanVessel.value,
            documentNo: this.formHouseBill.documentNo.value,
            inWord: this.formHouseBill.inWord.value,
            shippingMark: this.formHouseBill.shippingMark.value,
            remark: this.formHouseBill.remark.value,
            issueHBLPlace: !!this.formHouseBill.placeOfIssues.value ? this.formHouseBill.placeOfIssues.value : null,
            referenceNo: this.formHouseBill.referenceNo.value,
            customerId: this.formHouseBill.customer.value,
            oceanVoyNo: this.formHouseBill.oceanVoyNo.value,
            csMawbcontainers: this.containers,
            commodity: this.hblGoodsSummaryComponent.commodities,
            packageContainer: this.hblGoodsSummaryComponent.containerDetail,
            desOfGoods: this.hblGoodsSummaryComponent.description,
            contSealNo: this.hblGoodsSummaryComponent.containerDescription,
            cbm: this.hblGoodsSummaryComponent.totalCBM,
            grossWeight: this.hblGoodsSummaryComponent.grossWeight,
            netWeight: this.hblGoodsSummaryComponent.netWeight,
            packageQty: this.hblGoodsSummaryComponent.packageQty,
            packageType: +this.hblGoodsSummaryComponent.selectedPackage,
            arrivalSecondNotice: null,
            arrivalNo: null,
            arrivalHeader: null,
            arrivalFooter: null,
            arrivalFirstNotice: null,
            deliveryOrderNo: null,
            deliveryOrderPrintedDate: null,
            dofooter: null,
            dosentTo1: null,
            dosentTo2: null,
        };
        return body;
    }
}
