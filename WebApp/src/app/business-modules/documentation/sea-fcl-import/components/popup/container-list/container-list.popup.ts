import { Component, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Store } from '@ngrx/store';

import { PopupBase } from 'src/app/popup.base';
import { Container } from 'src/app/shared/models/document/container.model';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { Unit } from 'src/app/shared/models';

import { catchError, takeUntil } from 'rxjs/operators';
import { forkJoin } from 'rxjs';

import * as fromStore from './../../../store';
import { Commodity } from 'src/app/shared/models/catalogue/commodity.model';
import { DataService, SortService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { ShareContainerImportComponent } from 'src/app/business-modules/share-business';
import { CommonEnum } from 'src/app/shared/enums/common.enum';


@Component({
    selector: 'container-list-popup',
    templateUrl: './container-list.popup.html',
})
export class SeaFCLImportContainerListPopupComponent extends PopupBase {

    @ViewChild(ShareContainerImportComponent, { static: false }) containerImportPopup: ShareContainerImportComponent;

    mblid: string = null;
    hblid: string = null;
    containers: Container[] = [];
    initContainers: Container[] = [];

    headers: CommonInterface.IHeaderTable[] = [];

    containerUnits: Unit[] = new Array<Unit>();
    packageUnits: Unit[] = new Array<Unit>();
    weightUnits: Unit[] = new Array<Unit>();
    commodities: any[] = new Array<any>();

    isSubmitted: boolean = false;
    isAdd: boolean = false;

    isDuplicateContPakage: boolean = false;


    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<fromStore.IContainerState>,
        private cdRef: ChangeDetectorRef,
        private _dataService: DataService,
        private _sortService: SortService,
    ) {
        super();

        this.requestSort = this.sortContainer;
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Cont Type', field: 'containerTypeId', sortable: true, required: true },
            { title: 'Cont Q`Ty', field: 'quantity', required: true, sortable: true },
            { title: 'G.W', field: 'gw', sortable: true, },
            { title: 'C.W', field: 'chargeAbleWeight', sortable: true, },
            { title: 'CBM', field: 'cbm', sortable: true, },
            { title: 'Package Q`Ty', field: 'packageQuantity', sortable: true, },
            { title: 'Package Type', field: 'packageTypeId', sortable: true, },
            { title: 'Container No', field: 'containerNo', sortable: true, },
            { title: 'Seal No', field: 'sealNo', sortable: true, },
            { title: 'Mark No', field: 'markNo', sortable: true, },
            { title: 'Commodity', field: 'commodityId', sortable: true, },
            { title: 'Description', field: 'description', sortable: true, },
            { title: 'N.W', field: 'nw', sortable: true, },
            { title: 'Unit', field: 'unitOfMeasureId', sortable: true, },
        ];
        this.cdRef.detectChanges(); // * tell ChangeDetect update view in app-table-header (field required).
        this.getMasterData();


        // * GET DATA FROM STORE.
        this._store.select(fromStore.getContainerSaveState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: fromStore.IContainerState | any) => {
                    this.containers = res;
                    if (!this.initContainers.length) {
                        this.initContainers = res;
                    }
                }
            );
    }

    addNewContainer() {
        this.isSubmitted = false;
        this._store.dispatch(new fromStore.AddContainerAction(new Container({ nw: null, cbm: null, chargeAbleWeight: null, gw: null, unitOfMeasureId: 119, unitOfMeasureName: 'Kilogram' }))); // * DISPATCH Add ACTION 
    }

    deleteContainerItem(index: number) {
        this._store.dispatch(new fromStore.DeleteContainerAction(index)); // * DISPATCH DELETE ACTION
    }

    getMasterData() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.UNIT) && !!this._dataService.getDataByKey(SystemConstants.CSTORAGE.UNIT).length) {
            const res: any[] = this._dataService.getDataByKey(SystemConstants.CSTORAGE.UNIT);
            this.containerUnits = res[0] || [];
            this.packageUnits = res[1] || [];
            this.weightUnits = res[2] || [];
            this.commodities = res[3] || [];
        } else {
            forkJoin([
                this._catalogueRepo.getUnit({ active: true, unitType: CommonEnum.UnitType.CONTAINER }),
                this._catalogueRepo.getUnit({ active: true, unitType: CommonEnum.UnitType.PACKAGE }),
                this._catalogueRepo.getUnit({ active: true, unitType: CommonEnum.UnitType.WEIGHT }),
                this._catalogueRepo.getCommondity({ active: true }),
            ])
                .pipe(
                    catchError(this.catchError)
                )
                .subscribe(
                    (res: any[] = [[], [], [], []]) => {
                        this.containerUnits = res[0];
                        this.packageUnits = res[1];
                        this.weightUnits = res[2];
                        this.commodities = res[3];

                        this._dataService.setDataService(SystemConstants.CSTORAGE.UNIT, [...res]);
                    }
                );
        }
    }

    onSaveContainerList() {
        this.isSubmitted = true;
        // if (!this.containers.length) {
        //     return;
        // }

        if (this.checkValidateContainer()) {
            // * DISPATCH SAVE ACTION
            if (this.checkDuplicate()) {
                for (const container of this.containers) {
                    container.commodityName = this.getCommodityName(container.commodityId);
                    container.containerTypeName = this.getContainerTypeName(container.containerTypeId);
                    container.packageTypeName = this.getPackageTypeName(container.packageTypeId);
                }
                this._store.dispatch(new fromStore.SaveContainerAction(this.containers));

                this.initContainers = this.containers;

                this.isSubmitted = false;
                this.hide();
            }
        }
    }

    checkValidateContainer() {
        let valid: boolean = true;
        for (const container of this.containers) {
            if (
                !container.containerTypeId
                || !container.quantity
                || container.chargeAbleWeight < 0
                || container.gw < 0
                || container.cbm < 0
                || container.nw < 0
            ) {
                valid = false;
                break;
            }
        }

        return valid;
    }

    checkDuplicate() {
        let valid: boolean = true;
        if (
            this.utility.checkDuplicateInObject("containerTypeId", this.containers)
            && this.utility.checkDuplicateInObject("packageTypeId", this.containers)
            && this.utility.checkDuplicateInObject("packageQuantity", this.containers)
            && this.utility.checkDuplicateInObject("containerNo", this.containers)
        ) {
            this.isDuplicateContPakage = true;
            valid = false;
            return;
        } else {
            valid = true;
            this.isDuplicateContPakage = false;
        }

        return valid;
    }

    getCommodityName(id: string | number) {
        const commodities: Commodity[] = this.commodities.filter(c => c.id === id);
        if (!!commodities.length) {
            return commodities[0].commodityNameEn;
        } else {
            return null;
        }
    }

    getContainerTypeName(containerId: string | number) {
        const containers: Unit[] = this.containerUnits.filter(c => c.id === containerId);
        if (!!containers.length) {
            return containers[0].unitNameEn;
        } else {
            return null;
        }
    }

    getPackageTypeName(packageId: string | number) {
        const packages: Unit[] = this.packageUnits.filter(c => c.id === packageId);
        if (!!packages.length) {
            return packages[0].unitNameEn;
        } else {
            return null;
        }
    }

    closePopup() {
        this.isSubmitted = false;
        if (!this.isAdd) {
            this._store.dispatch(new fromStore.GetContainerSuccessAction(this.initContainers));
        } else {
            // this._store.dispatch(new fromStore.GetContainerSuccessAction([]));
        }
        this.hide();
    }

    showImportPopup() {
        this.containerImportPopup.mblid = this.mblid;
        this.containerImportPopup.hblid = this.hblid;
        this.containerImportPopup.show();
    }

    sortContainer(sortField: string) {
        this.containers = this._sortService.sort(this.containers, sortField, this.order);
    }

    onChangeDataFormTable() {
        this.isDuplicateContPakage = false;
    }

}
