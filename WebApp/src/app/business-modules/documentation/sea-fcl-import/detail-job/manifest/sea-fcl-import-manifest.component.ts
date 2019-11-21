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

@Component({
    selector: 'app-sea-fcl-import-manifest',
    templateUrl: './sea-fcl-import-manifest.component.html'
})
export class SeaFclImportManifestComponent extends AppList {

    @ViewChild(AddHblToManifestComponent, { static: false }) AddHblToManifestPopup: AddHblToManifestComponent;
    housebills: CsTransactionDetail[] = [];
    housebillsRoot: CsTransactionDetail[] = [];
    housebillsChange: CsTransactionDetail[] = [];
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    jobId: string = '';
    headers: CommonInterface.IHeaderTable[];

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

        this._store.select(getParamsRouterState)
            .subscribe((param: Params) => {
                if (param.id) {
                    this.jobId = param.id;
                    this.getHblList(this.jobId);
                }
            });
    }


    showPopupAddHbl() {
        let arraytoAdd: any[] = [];
        this.AddHblToManifestPopup.checkAll = false;
        if (this.housebillsChange.length > 0) {
            this.housebillsRoot.forEach(item => {
                const existed = this.housebillsChange.find(({ id }) => item.id === id);
                if (existed == null) {
                    arraytoAdd.push(item);
                }

            });
            arraytoAdd.forEach(item => {
                item.isChecked = false;
            });
            this.AddHblToManifestPopup.houseBills = arraytoAdd;
        }

        this.AddHblToManifestPopup.show();

    }

    OnAdd(selectedhouseBills: any[]) {
        this.housebillsChange = selectedhouseBills;
        if (this.housebillsChange.length === 1) {

        }
        this.housebills = this.housebillsChange;




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
                (res: CsTransactionDetail[]) => {
                    this.housebills = (res || []).map((item: CsTransactionDetail) => new CsTransactionDetail(item));
                    this.AddHblToManifestPopup.houseBills = this.housebills;
                    this.housebillsRoot = this.housebills;

                },
            );
    }
}


