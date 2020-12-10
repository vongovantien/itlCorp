import { Component, OnInit, ViewChild } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { Store, ActionsSubject } from '@ngrx/store';
import { DocumentationRepo, CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { formatDate } from '@angular/common';

import { AppForm } from '@app';
import { ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { HouseBill } from '@models';
import { DataService } from '@services';
import { RoutingConstants } from '@constants';

import { AirImportHBLFormCreateComponent } from '../components/form-create-house-bill-air-import/form-create-house-bill-air-import.component';
import { ShareBusinessDeliveryOrderComponent, ShareBusinessImportHouseBillDetailComponent, ShareBusinessArrivalNoteAirComponent, getTransactionPermission, IShareBussinessState, TransactionGetDetailAction } from '@share-bussiness';

import { forkJoin, merge } from 'rxjs';
import _merge from 'lodash/merge';
import isUUID from 'validator/lib/isUUID';

import { catchError, finalize, mergeMap, takeUntil } from 'rxjs/operators';


@Component({
    selector: 'app-create-hbl-air-import',
    templateUrl: './create-house-bill.component.html',
})
export class AirImportCreateHBLComponent extends AppForm implements OnInit {
    @ViewChild(AirImportHBLFormCreateComponent) formCreateHBLComponent: AirImportHBLFormCreateComponent;
    @ViewChild(ConfirmPopupComponent) confirmPopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) infoPopup: InfoPopupComponent;
    @ViewChild(ShareBusinessArrivalNoteAirComponent, { static: true }) arrivalNoteComponent: ShareBusinessArrivalNoteAirComponent;
    @ViewChild(ShareBusinessDeliveryOrderComponent, { static: true }) deliveryComponent: ShareBusinessDeliveryOrderComponent;
    @ViewChild(ShareBusinessImportHouseBillDetailComponent) importHouseBillPopup: ShareBusinessImportHouseBillDetailComponent;
    @ViewChild('confirmSaveExistedHbl') confirmExistedHbl: ConfirmPopupComponent;

    jobId: string;
    selectedHbl: any = {};
    isImport: boolean = false;

    activeTab: string = 'hawb';

    constructor(
        protected _progressService: NgProgress,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<IShareBussinessState>,
        protected _documentationRepo: DocumentationRepo,
        protected _catalogueRepo: CatalogueRepo,
        protected _toastService: ToastrService,
        protected _actionStoreSubject: ActionsSubject,
        protected _router: Router,
        protected _dataService: DataService
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this._activedRoute.params
            .subscribe((param: Params) => {
                if (isUUID(param.jobId)) {
                    this.jobId = param.jobId;

                    this._store.dispatch(new TransactionGetDetailAction(this.jobId));

                    this.permissionShipments = this._store.select(getTransactionPermission);
                } else {
                    this.gotoList();
                }
            });

        this.listenShortcutSaveHawb();
    }

    listenShortcutSaveHawb() {
        merge(
            this.createShortcut(['ControlLeft', 'ShiftLeft', 'KeyS']),
        ).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                () => {
                    this.confirmSaveHBL();
                }
            );
    }

    confirmSaveHBL() {
        this.confirmPopup.show();
    }

    onImport(selectedData: any) {
        this.isImport = true;
        this.selectedHbl = selectedData;
        this.selectedHbl.hwbno = null;
        this.formCreateHBLComponent.updateFormValue(this.selectedHbl);
    }

    showImportPopup() {
        const dataSearch = { jobId: this.jobId };
        dataSearch.jobId = this.jobId;
        this.importHouseBillPopup.typeFCL = 'Import';
        this.importHouseBillPopup.selected = - 1;
        this.importHouseBillPopup.getHourseBill(dataSearch);
        this.importHouseBillPopup.show();
    }

    getDataForm() {
        const form: any = this.formCreateHBLComponent.formCreate.getRawValue();
        const formData = {
            customerId: form.customer,
            notifyPartyId: form.notifyId,
            notifyPartyDescription: form.notifyDescription,
            hwbno: form.hawb,

            eta: !!form.eta && !!form.eta.startDate ? formatDate(form.eta.startDate, 'yyyy-MM-dd', 'en') : null,
            arrivalDate: !!form.arrivalDate && !!form.arrivalDate.startDate ? formatDate(form.arrivalDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDate: !!form.flightDate ? formatDate(form.flightDate.startDate, 'yyyy-MM-dd', 'en') : null,
            flightDateOrigin: !!form.flightDateOrigin && form.flightDateOrigin.startDate !== undefined ? formatDate(form.flightDateOrigin.startDate, 'yyyy-MM-dd', 'en') : null,
            issueHBLDate: !!form.issueHBLDate ? formatDate(form.issueHBLDate.startDate, 'yyyy-MM-dd', 'en') : null,

            grossWeight: form.gw,
            finalPOD: form.finalPod,
        };

        const houseBill = new HouseBill(_merge(form, formData));
        return houseBill;
    }

    saveHBL() {
        this.confirmPopup.hide();

        this.formCreateHBLComponent.isSubmitted = true;
        this.arrivalNoteComponent.isSubmitted = true;
        this.deliveryComponent.isSubmitted = true;

        if (!this.checkValidateForm()) {
            this.activeTab = 'hawb';
            this.infoPopup.show();
            return;
        }
        if (!this.arrivalNoteComponent.checkValidate() || !this.arrivalNoteComponent.hblArrivalNote.arrivalNo) {
            this.activeTab = 'arrival';
            this.infoPopup.show();

            return;
        }

        if (!this.arrivalNoteComponent.checkValidateFirstNote() || !this.arrivalNoteComponent.checkValidateSecondtNote()) {
            return;
        }
        if (!this.deliveryComponent.deliveryOrder.deliveryOrderNo) {
            this.activeTab = 'authorize';
            this.infoPopup.show();
            return;
        }
        this._documentationRepo.checkExistedHawbNo(this.formCreateHBLComponent.hwbno.value, this.jobId, null)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res) {
                        this.confirmExistedHbl.show();
                    } else {
                        const houseBill: HouseBill = this.getDataForm();
                        houseBill.jobId = this.jobId;
                        this._catalogueRepo.getSalemanIdByPartnerId(houseBill.customerId, this.jobId).subscribe((res: any) => {
                            if (!!res.salemanId) {
                                if (res.salemanId !== houseBill.saleManId) {
                                    this._toastService.error('Not found contract information, please check!');
                                    return;
                                }
                            }
                            if (!!res.officeNameAbbr) {
                                this._toastService.error('The selected customer not have any agreement for service in office ' + res.officeNameAbbr + '! Please check Again', 'Cannot Create House Bill!');
                            } else {
                                this.createHbl(houseBill);

                            }
                        });
                    }
                }
            );
    }

    confirmSaveData() {
        this.confirmExistedHbl.hide();

        const houseBill: HouseBill = this.getDataForm();
        houseBill.jobId = this.jobId;

        this.createHbl(houseBill);
    }

    checkValidateForm() {
        let valid: boolean = true;
        if (!this.formCreateHBLComponent.formCreate.valid) {
            valid = false;
        }
        return valid;
    }

    createHbl(houseBill: HouseBill) {
        if (this.formCreateHBLComponent.formCreate.valid) {
            this._progressRef.start();
            this._documentationRepo.createHousebill(houseBill)
                .pipe(
                    mergeMap((res: any) => {
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
                        return forkJoin([arrival, delivery]);
                    }),
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe((res: CommonInterface.IResult) => {
                    if (!!res) {
                        this._toastService.success(res[1].message, '');
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.arrivalNoteComponent.hblArrivalNote.hblid}`]);
                    }
                });
        }
    }

    gotoList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl`]);
    }
}
