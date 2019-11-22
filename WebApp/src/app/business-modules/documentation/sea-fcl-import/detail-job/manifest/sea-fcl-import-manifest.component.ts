import { Component, OnInit, ViewChild, Input, EventEmitter } from '@angular/core';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { CsTransactionDetail } from 'src/app/shared/models';
import { Store } from '@ngrx/store';
import * as fromStore from '../../store';
import { getParamsRouterState } from 'src/app/store';
import { Params } from '@angular/router';
import { SortService } from 'src/app/shared/services';
import { AddHblToManifestComponent } from './popup/add-hbl-to-manifest.popup';
import { FormManifestSeaFclImportComponent } from './components/form-manifest/form-manifest-sea-fcl-import.component';
import { formatDate } from '@angular/common';
import { CsManifest } from 'src/app/shared/models/document/manifest.model';

@Component({
    selector: 'app-sea-fcl-import-manifest',
    templateUrl: './sea-fcl-import-manifest.component.html'
})
export class SeaFclImportManifestComponent extends AppList {
    @ViewChild(FormManifestSeaFclImportComponent, { static: false }) formManifest: FormManifestSeaFclImportComponent;

    @ViewChild(AddHblToManifestComponent, { static: false }) AddHblToManifestPopup: AddHblToManifestComponent;
    housebills: any[] = [];
    housebillsRoot: any[] = [];
    housebillsChange: any[] = [];
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];
    checkAll = false;

    constructor(
        protected _store: Store<fromStore.ISeaFCLImportState>,
        private _progressService: NgProgress,
        private _documentationRepo: DocumentationRepo,
        private _sortService: SortService


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
        this._store.select(getParamsRouterState)
            .subscribe((param: Params) => {
                if (param.id) {
                    this.jobId = param.id;
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

    getManifest(id: string) {
        this._documentationRepo.getManifest(id).subscribe(
            (res: any) => {
                if (!!res) {
                    setTimeout(() => {
                        this.formManifest.supplier.setValue(res.supplier);
                        this.formManifest.referenceNo.setValue(res.refNo);
                        this.formManifest.attention.setValue(res.attention);
                        this.formManifest.marksOfNationality.setValue(res.masksOfRegistration);
                        this.formManifest.vesselNo.setValue(res.voyNo);
                        !!res.invoiceDate ? this.formManifest.date.setValue({ startDate: new Date(res.invoiceDate), endDate: new Date(res.invoiceDate) }) : this.formManifest.date.setValue(null);
                        this.formManifest.selectedPortOfLoading = { field: 'id', value: res.pol };
                        this.formManifest.selectedPortOfDischarge = { field: 'id', value: res.pod };
                        this.formManifest.freightCharge.value.selectedItem = { id: res.paymentTerm, text: res.paymentTerm }

                    }, 500);

                }
            }
        );
    }

    AddOrUpdateManifest() {
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
                paymentTerm: this.formManifest.freightCharge.value[0].text,
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
                            console.log(res);

                        } else {

                        }
                    }
                );

        }
    }


    OnRemove() {
        this.housebills.forEach(x => {
            if (x.isChecked) {
                x.isRemoved = true;
                x.isChecked = false;
            }
        });
        this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
        this.AddHblToManifestPopup.checkAll = false;
    }

    OnAdd() {
        this.housebills.forEach(x => {
            if (x.isChecked) {
                x.isRemoved = false;
                x.isChecked = false;
            }
        });
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
                    this.AddHblToManifestPopup.houseBills = this.housebills.filter(x => x.isRemoved === true);
                },
            );
    }
}


