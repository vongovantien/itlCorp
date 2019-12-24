import { Component, ViewChild } from '@angular/core';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { getParamsRouterState } from 'src/app/store';
import { Params, Router } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { formatDate } from '@angular/common';
import { Crystal } from 'src/app/shared/models/report/crystal.model';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { ToastrService } from 'ngx-toastr';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ShareBusinessFormManifestComponent } from 'src/app/business-modules/share-business/components/manifest/form-manifest/components/form-manifest.component';
import { ShareBusinessAddHblToManifestComponent } from 'src/app/business-modules/share-business/components/manifest/popup/add-hbl-to-manifest.popup';

@Component({
    selector: 'app-air-export-manifest',
    templateUrl: './air-export-manifest.component.html'
})
export class AirExportManifestComponent extends AppList {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmCreatePopup: ConfirmPopupComponent;

    @ViewChild(ShareBusinessFormManifestComponent, { static: false }) formManifest: ShareBusinessFormManifestComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;
    @ViewChild(ShareBusinessAddHblToManifestComponent, { static: false }) AddHblToManifestPopup: ShareBusinessAddHblToManifestComponent;
    housebills: any[] = [];
    housebillsRoot: any[] = [];
    housebillsChange: any[] = [];
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
    dataReport: Crystal;

    constructor(
        protected _store: Store<any>,
        private _progressService: NgProgress,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        protected _router: Router
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {

        this.headers = [
            { title: 'HBL No', field: 'hwbNo', sortable: true, width: 100 },
            { title: 'No of Pieces', field: 'packageContainer', sortable: true },
            { title: 'G.W', field: 'grossWeight', sortable: true },
            { title: 'CBM', field: 'cbm', sortable: true },
            { title: 'Destination', field: 'podName', sortable: true },
            { title: 'Shipper', field: 'shipperName', sortable: true },
            { title: 'Consignee', field: 'consignee', sortable: true },
            { title: 'Description', field: 'desOfGoods', sortable: true },
            { title: 'Freight Charge', field: '', sortable: true },

        ];

    }
    ngAfterViewInit() {
        this.formManifest.isAir = true;
        this._store.select(getParamsRouterState)
            .subscribe((param: Params) => {
                if (param.jobId) {
                    this.jobId = param.jobId;

                    this.formManifest.jobId = this.jobId;
                    this.formManifest.getShipmentDetail(this.formManifest.jobId);
                    this.getHblList(this.jobId);
                    this.getManifest(this.jobId);

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
        this.getManifest(this.jobId);
        this.getHblList(this.jobId);
    }

    onRefresh() {
        this.confirmCreatePopup.hide();
        this.refreshManifest();
    }

    showRefreshPopup() {
        this.confirmCreatePopup.show();
    }

    combackToHBLList() {
        this._router.navigate([`/home/documentation/air-export/${this.jobId}`]);
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
                    this.manifest = res;
                    setTimeout(() => {
                        this.formManifest.supplier.setValue(res.supplier);
                        this.formManifest.referenceNo.setValue(res.refNo);
                        this.formManifest.attention.setValue(res.attention);
                        this.formManifest.marksOfNationality.setValue(res.masksOfRegistration);
                        this.formManifest.vesselNo.setValue(res.voyNo);
                        !!res.invoiceDate ? this.formManifest.date.setValue({ startDate: new Date(res.invoiceDate), endDate: new Date(res.invoiceDate) }) : this.formManifest.date.setValue(null);
                        this.formManifest.selectedPortOfLoading = { field: 'id', value: res.pol };
                        this.formManifest.selectedPortOfDischarge = { field: 'id', value: res.pod };
                        this.formManifest.freightCharge.setValue([<CommonInterface.INg2Select>{ id: res.paymentTerm, text: res.paymentTerm }]);
                        this.formManifest.consolidator.setValue(res.consolidator);
                        this.formManifest.deconsolidator.setValue(res.deConsolidator);
                        this.formManifest.agent.setValue(res.manifestIssuer);
                        this.formManifest.weight.setValue(res.weight);
                        this.formManifest.volume.setValue(res.volume);

                    }, 500);

                }
            }
        );
    }

    addOrUpdateManifest() {
        this.formManifest.isSubmitted = true;
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
                pol: !!this.formManifest.selectedPortOfLoading.value ? this.formManifest.selectedPortOfLoading.value : null,
                pod: !!this.formManifest.selectedPortOfDischarge.value ? this.formManifest.selectedPortOfDischarge.value : null,
                paymentTerm: this.formManifest.freightCharge.value !== "" ? this.formManifest.freightCharge.value[0].text : null,
                consolidator: this.formManifest.consolidator.value,
                deConsolidator: this.formManifest.deconsolidator.value,
                volume: this.formManifest.volume.value,
                weight: this.formManifest.weight.value,
                manifestIssuer: this.formManifest.agent.value,
                csTransactionDetails: this.housebills
            };
            this._documentationRepo.AddOrUpdateManifest(body)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message, '');
                        } else {
                            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    }
                );

        }
    }


    onRemove() {
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
                    console.log(this.housebills);
                    this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
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
            pol: !!this.formManifest.selectedPortOfLoading.value ? this.formManifest.selectedPortOfLoading.value : null,
            pod: !!this.formManifest.selectedPortOfDischarge.value ? this.formManifest.selectedPortOfDischarge.value : null,
            paymentTerm: this.formManifest.freightCharge.value !== "" ? this.formManifest.freightCharge.value[0].text : null,
            consolidator: this.formManifest.consolidator.value,
            deConsolidator: this.formManifest.deconsolidator.value,
            volume: this.formManifest.volume.value,
            weight: this.formManifest.weight.value,
            manifestIssuer: this.formManifest.agent.value,
            csTransactionDetails: this.housebills.filter(x => x.isRemoved === false)
        };
        this._documentationRepo.previewFCLImportManifest(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res != null) {
                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.reportPopup.frm.nativeElement.submit();
                                this.reportPopup.show();
                            }, 1000);
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
    }
}


