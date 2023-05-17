import { Component, ViewChild, ChangeDetectorRef, Inject } from '@angular/core';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from 'src/app/shared/repositories';
import { catchError, concatMap, finalize, mergeMap, takeUntil } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { Params, Router, ActivatedRoute } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { formatDate } from '@angular/common';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { ToastrService } from 'ngx-toastr';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ShareBusinessFormManifestComponent } from 'src/app/business-modules/share-business/components/manifest/form-manifest/components/form-manifest.component';
import { ShareBusinessAddHblToManifestComponent } from 'src/app/business-modules/share-business/components/manifest/popup/add-hbl-to-manifest.popup';
import { CommonEnum } from '@enums';
import { getTransactionLocked, getTransactionPermission, TransactionGetDetailAction } from '@share-bussiness';

import isUUID from 'validator/es/lib/isUUID';
import { RoutingConstants, SystemConstants } from '@constants';
import { InjectViewContainerRefDirective } from '@directives';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
import { HttpResponse } from '@angular/common/http';
import { of } from 'rxjs';

@Component({
    selector: 'app-air-export-manifest',
    templateUrl: './air-export-manifest.component.html'
})
export class AirExportManifestComponent extends AppList implements ICrystalReport {
    @ViewChild(ShareBusinessFormManifestComponent) formManifest: ShareBusinessFormManifestComponent;
    @ViewChild(ShareBusinessAddHblToManifestComponent) AddHblToManifestPopup: ShareBusinessAddHblToManifestComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    housebills: any[] = [];
    housebillsRoot: any[] = [];
    housebillsChange: any[] = [];
    portType: CommonEnum.PORT_TYPE = CommonEnum.PORT_TYPE.AIR;
    manifest: any = {};
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };
    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];
    checkAll = false;
    totalGW = 0;
    totalCBM = 0;
    fistOpen: boolean = true;

    constructor(
        protected _store: Store<any>,
        private _progressService: NgProgress,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        protected _router: Router,
        private cdRef: ChangeDetectorRef,
        private _activedRouter: ActivatedRoute,
        private _exportRepo: ExportRepo,
        private _fileMngtRepo: SystemFileManageRepo

    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true, width: 100 },
            { title: 'No of Pieces', field: 'packageQty', sortable: true },
            { title: 'G.W', field: 'gw', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Destination', field: 'podName', sortable: true },
            { title: 'Shipper', field: 'shipperName', sortable: true },
            { title: 'Consignee', field: 'consigneeName', sortable: true },
            { title: 'Description', field: 'desOfGoods', sortable: true },
            { title: 'Freight Charge', field: '', sortable: true }
        ];

        this.isLocked = this._store.select(getTransactionLocked);
        this.permissionShipments = this._store.select(getTransactionPermission);

    }

    ngAfterViewInit() {
        this._activedRouter.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((param: Params) => {
                if (param.jobId && isUUID(param.jobId)) {
                    this.jobId = param.jobId;

                    this.formManifest.jobId = this.jobId;


                    this._store.dispatch(new TransactionGetDetailAction(this.jobId));

                    this.getHblList(this.jobId);
                    this.getManifest(this.jobId);
                    this.cdRef.detectChanges();

                }
            });
    }

    showPopupAddHbl() {
        this.AddHblToManifestPopup.show();
    }

    removeAllChecked() {
        this.checkAll = false;
    }

    refreshManifest() {
        this.formManifest.getShipmentDetail();
        this.isShowUpdate = false;
        this.getHblList(this.jobId);
    }

    onRefresh() {
        this.refreshManifest();
    }

    showRefreshPopup() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'All manually entered data will be refresh. Are you sure you want to perform this action?',
            labelCancel: 'No'
        }, () => { this.onRefresh(); });
    }

    combackToHBLList() {
        this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`]);
    }

    getTotalWeight() {
        this.totalCBM = 0;
        this.totalGW = 0;
        this.housebills.forEach(x => {
            if (x.isRemoved === false) {
                this.totalGW = this.totalGW + x.gw;
                this.totalCBM = this.totalCBM + x.cbm;
            }
        });
        this.manifest.weight = this.totalGW;
        this.manifest.volume = this.totalCBM;
        this.formManifest.volume.setValue(this.manifest.volume);
        this.formManifest.weight.setValue(this.manifest.weight);

    }

    getManifest(id: string) {
        this._documentationRepo.getManifest(id).subscribe(
            (res: any) => {
                if (!!res) {
                    this.isShowUpdate = true;
                    this.manifest = res;
                    this.formManifest.updateDataToForm(this.manifest);
                } else {
                    this.isShowUpdate = false;
                    this.formManifest.getShipmentDetail();
                }
            }
        );
    }

    addOrUpdateManifest() {
        this.formManifest.isSubmitted = true;
        this.getTotalWeight();

        if (this.formManifest.formGroup.valid) {

            this._progressRef.start();
            const body: any = {
                jobId: this.jobId,
                refNo: this.formManifest.referenceNo.value,
                supplier: this.formManifest.supplier.value,
                attention: this.formManifest.attention.value,
                masksOfRegistration: this.formManifest.marksOfNationality.value,
                voyNo: this.formManifest.vesselNo.value,
                invoiceDate: !!this.formManifest.date.value && this.formManifest.date.value.startDate != null ? formatDate(this.formManifest.date.value.startDate !== undefined ? this.formManifest.date.value.startDate : this.formManifest.date.value, 'yyyy-MM-dd', 'en') : null,
                pol: this.formManifest.pol.value,
                pod: this.formManifest.pod.value,
                paymentTerm: this.formManifest.freightCharge.value,
                consolidator: this.formManifest.consolidator.value,
                deConsolidator: this.formManifest.deconsolidator.value,
                volume: this.formManifest.volume.value,
                weight: this.formManifest.weight.value,
                manifestIssuer: this.formManifest.agent.value,
                csTransactionDetails: this.housebills,
                manifestShipper: this.formManifest.shipperDescription.value
            };
            this._documentationRepo.AddOrUpdateManifest(body)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                            this.getManifest(this.jobId);
                            this.getHblList(this.jobId);
                        } else {
                            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    }
                );

        }
    }

    checkIsChecked() {
        let isChecked = false;
        isChecked = this.housebills.find(t => t.isChecked === true);
        if (!isChecked) {
            return false;
        }
        return true;
    }

    onRemove() {
        if (this.checkIsChecked() === false) {
            return;
        }
        this.housebills.forEach(x => {
            if (x.isChecked) {
                x.isRemoved = true;
                x.isChecked = false;
                x.manifestRefNo = null;
            }

        });

        this.getTotalWeight();
        this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
        this.AddHblToManifestPopup.checkAll = false;
    }

    onAdd() {
        if (this.checkIsChecked() === false) {
            return;
        }
        this.housebills.forEach(x => {
            if (x.isChecked) {
                x.isRemoved = false;
                x.isChecked = false;
            }
        });
        this.getTotalWeight();
        this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
    }

    checkAllChange() {
        if (this.checkAll) {
            this.housebills.forEach(x => {
                x.isChecked = true;
            });
        } else {
            this.housebills.forEach(x => {
                x.isChecked = false;
            });
        }
    }

    sortHouseBills(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.housebills = this._sortService.sort(this.housebills, sortData.sortField, sortData.order);
        }
    }

    getHblList(jobId: string) {
        this._progressRef.start();
        this._documentationRepo.getListHouseBillOfJob({ jobId: jobId })
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        this.AddHblToManifestPopup.houseBills = this.housebills;
                        res.forEach((element: { isChecked: boolean; isRemoved: boolean }) => {
                            element.isChecked = false;
                            if (element["manifestRefNo"] == null) {
                                element.isRemoved = true;
                            } else {
                                element.isRemoved = false;
                            }
                        });
                        this.housebills = res;
                        const hasHbl = this.housebills.some(element => element.isRemoved === false);
                        if (!hasHbl) {
                            this.housebills.forEach(element => {
                                element.isRemoved = false;
                            });
                        }
                        this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
                    }

                },
            );
    }

    previewManifest() {
        if (this.formManifest.referenceNo.value === null) {
            this._toastService.warning('There is no data to display preview');
            return;
        }
        const body: any = {
            jobId: this.jobId,
            refNo: this.formManifest.referenceNo.value,
            supplier: this.formManifest.supplier.value,
            attention: this.formManifest.attention.value,
            masksOfRegistration: this.formManifest.marksOfNationality.value,
            voyNo: this.formManifest.vesselNo.value,
            invoiceDate: !!this.formManifest.date.value && this.formManifest.date.value.startDate != null ? formatDate(this.formManifest.date.value.startDate !== undefined ? this.formManifest.date.value.startDate : this.formManifest.date.value, 'yyyy-MM-dd', 'en') : null,
            pol: this.formManifest.pol.value,
            pod: this.formManifest.pod.value,
            paymentTerm: this.formManifest.freightCharge.value,
            consolidator: this.formManifest.consolidator.value,
            deConsolidator: this.formManifest.deconsolidator.value,
            volume: this.formManifest.volume.value,
            weight: this.formManifest.weight.value,
            manifestIssuer: this.formManifest.agent.value,
            csTransactionDetails: this.housebills.filter(x => x.isRemoved === false),
            manifestShipper: this.formManifest.shipperDescription.value
        };
        this._documentationRepo.previewAirExportManifest(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.viewContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.viewContainerRef.viewContainerRef.clear();
            });

        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.jobId,
                            hblId: SystemConstants.EMPTY_GUID,
                            templateCode: 'MNF',
                            transactionType: 'AE'
                        };
                        return this._fileMngtRepo.uploadPreviewTemplateEdoc([body]);
                    }
                    return of(false);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) return;
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.success(res.message || "Upload fail");
                    }
                },
                (errors) => {
                    console.log("error", errors);
                },
                () => {
                    sub.unsubscribe();
                }
            );
    }
}


