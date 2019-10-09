import { Component, OnInit, Input, EventEmitter, Output, ViewChild } from '@angular/core';
import { catchError, finalize } from 'rxjs/operators';
import * as dataHelper from 'src/helper/data.helper';
import { BaseService, SortService, DataService } from 'src/app/shared/services';
import { API_MENU } from 'src/constants/api-menu.const';
import { PopupBase } from 'src/app/popup.base';
import { NgForm } from '@angular/forms';
import { CatalogueRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ContainerImportComponent } from './container-import/container-import.component';
import { SystemConstants } from 'src/constants/system.const';

@Component({
    selector: 'app-container-list',
    templateUrl: './container-list.component.html'
})
export class ContainerListComponent extends PopupBase implements OnInit {
    @Input() jobId: string;
    @Input() lstMasterContainers: any[] = [];
    @Output() outputContainers = new EventEmitter<any[]>();
    @ViewChild(ConfirmPopupComponent, { static: false }) popupConfirmDelete: ConfirmPopupComponent;
    @ViewChild(ContainerImportComponent, { static: false }) popupContainerImport: ContainerImportComponent;
    saveContainerSuccess = false;
    listContainerType: any[] = [];
    containerTypes: any[];
    packageTypes: any[] = [];
    weightMesurements: any[];
    listPackageTypes: any[];
    listWeightMesurements: any[];
    packagesUnitActive: any[] = [];
    containerToChangeEdit: any;
    listcommodities: any[] = [];
    lstContainerTemp: any[] = [];
    indexItemConDelete: number;
    listUnits: any[] = [];

    keySearch = '';
    isDisableSave = false;

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _dataService: DataService
    ) {
        super();
    }

    ngOnInit() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.UNIT)) {
            this.listUnits = this._dataService.getDataByKey(SystemConstants.CSTORAGE.UNIT);
            if (this.listUnits != null) {
                this.listWeightMesurements = this.listUnits.filter(x => x.unitType === 'Weight Measurement');
                this.listContainerType = this.listUnits.filter(x => x.unitType === 'Container');
                this.listPackageTypes = this.listUnits.filter(x => x.unitType === 'Package');
            }
        } else {
            this.getUnits();
        }

        this.getComodities();

        this.lstContainerTemp = this.lstMasterContainers;
    }

    showImportPopup() {
        this.popupContainerImport.show({ backdrop: 'static' });
    }

    import(event) {
        if (event) {
            this.getListContainersOfJob();
        }
    }

    getListContainersOfJob() {
        this._documentRepo.getListContainersOfJob({ mblid: this.jobId })
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                })
            )
            .subscribe(
                (res: any) => {
                    this.lstMasterContainers = res;
                },
                (errors: any) => {
                },
                () => { }
            );
    }
    async onSubmitContainer(form: NgForm) {
        for (let i = 0; i < this.lstMasterContainers.length; i++) {
            this.lstMasterContainers[i].verifying = true;
        }
        if (form.valid) {
            let hasItemEdited = false;
            for (let i = 0; i < this.lstMasterContainers.length; i++) {
                if (this.lstMasterContainers[i].allowEdit === true) {
                    hasItemEdited = true;
                    break;
                }
            }
            if (hasItemEdited === false) {
                const response = await this.baseServices.putAsync(this.api_menu.Documentation.CsMawbcontainer.update, { csMawbcontainerModels: this.lstMasterContainers, masterId: this.jobId }, true, false);
                if (response.status) {
                    this.isDisableSave = false;
                    this.outputContainers.emit(this.lstMasterContainers);
                    this.hide();
                }
            } else {
                this.isDisableSave = false;
                this.baseServices.errorToast("Current container must be save!!!");
            }
        }
    }

    closeContainerPopup() {
        const index = this.lstMasterContainers.findIndex(x => x.isNew === true);
        if (index > -1 && this.lstMasterContainers.length > 1) {
            this.lstMasterContainers.splice(index, 1);
            // this.lstMasterContainers.push(this.initNewContainer());
        }
        // this.outputContainers.emit(this.lstMasterContainers);
        this.hide();
    }

    addNewRowContainer() {
        let hasItemEdited = false;
        let index = -1;
        // 1.
        for (let i = 0; i < this.lstMasterContainers.length; i++) {
            if (this.lstMasterContainers[i].allowEdit === true) {
                hasItemEdited = true;
                index = i;
                break;
            }
        }
        // 2.
        if (hasItemEdited === false) {
            this.lstMasterContainers.push(this.initNewContainer());
        } else {
            this.saveNewContainer(index);
            if (this.saveContainerSuccess) {
                this.lstMasterContainers.push(this.initNewContainer());
            }
        }
    }

    async saveNewContainer(index: any) {
        if (this.lstMasterContainers[index].containerNo.length > 0 || this.lstMasterContainers[index].sealNo.length > 0 || this.lstMasterContainers[index].markNo.length > 0) {
            this.lstMasterContainers[index].quantity = 1;
        }
        this.lstMasterContainers[index].verifying = true;
        if (this.lstMasterContainers[index].quantity == null
            || this.lstMasterContainers[index].containerTypeId == null
            || this.lstMasterContainers[index].gw === 0 || this.lstMasterContainers[index].nw === 0
            || this.lstMasterContainers[index].chargeAbleWeight === 0
            || this.lstMasterContainers[index].cbm === 0) {
            this.saveContainerSuccess = false;
            return;
        }
        // Check duplicate( Cont Type && Cont Q'ty && Container No && Package Type)
        let existedItems = this.lstMasterContainers.filter(x => x.containerTypeId === this.lstMasterContainers[index].containerTypeId
            && x.quantity === this.lstMasterContainers[index].quantity);
        if (this.lstMasterContainers[index].containerNo.length !== 0 && this.lstMasterContainers[index].packageTypeId != null) {
            existedItems = existedItems.filter(x => x.containerNo === this.lstMasterContainers[index].containerNo
                && x.packageTypeId === this.lstMasterContainers[index].packageTypeId);
        } else {
            existedItems = [];
        }
        // set row is invalid
        if (existedItems.length > 1) {
            this.lstMasterContainers[index].inValidRow = true;
            this.saveContainerSuccess = false;
        } else {
            // set row is saved
            if (this.lstMasterContainers[index].isNew === true) {
                this.lstMasterContainers[index].isNew = false;
            } else {
                this.lstMasterContainers[index].inValidRow = false;
                this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null ? [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }] : [];
                this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null ? [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }] : [];
                this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureId != null ? [{ id: this.lstMasterContainers[index].unitOfMeasureId, text: this.lstMasterContainers[index].unitOfMeasureName }] : [];
            }
            this.roundWeight(index);
            this.saveContainerSuccess = true;
            this.lstMasterContainers[index].allowEdit = false;
        }
        this.lstContainerTemp = Object.assign([], this.lstMasterContainers);
    }

    roundWeight(index: number) {
        if (this.lstMasterContainers[index].gw != null) {
            this.lstMasterContainers[index].gw = Number(this.lstMasterContainers[index].gw.toFixed(2));
        }
        if (this.lstMasterContainers[index].nw != null) {
            this.lstMasterContainers[index].nw = Number(this.lstMasterContainers[index].nw.toFixed(2));
        }
        if (this.lstMasterContainers[index].cbm != null) {
            this.lstMasterContainers[index].cbm = Number(this.lstMasterContainers[index].cbm.toFixed(2));
        }
        if (this.lstMasterContainers[index].chargeAbleWeight != null) {
            this.lstMasterContainers[index].chargeAbleWeight = Number(this.lstMasterContainers[index].chargeAbleWeight.toFixed(2));
        }
    }

    initNewContainer() {
        const container = {
            mawb: this.jobId,
            containerTypeId: null,
            containerTypeName: '',
            containerTypeActive: [],
            quantity: 1,
            containerNo: '',
            sealNo: '',
            markNo: '',
            unitOfMeasureId: null,
            unitOfMeasureName: '',
            unitOfMeasureActive: [], // [{ "id": 37, "text": "Kilogam" }],
            commodityId: null,
            commodityName: '',
            packageTypeId: null,
            packageTypeName: '',
            packageTypeActive: [],
            packageQuantity: null,
            description: null,
            gw: null,
            nw: null,
            chargeAbleWeight: null,
            cbm: null,
            packageContainer: '',
            allowEdit: true,
            isNew: true,
            verifying: false
        };
        this.getComboboxDataContainer(this.lstMasterContainers.length);
        return container;
    }

    getComboboxDataContainer(index: number) {
        if (this.listContainerType != null) {
            if (this.lstMasterContainers[index] == null) {
                this.containerTypes = dataHelper.prepareNg2SelectData(this.listContainerType, 'id', 'unitNameEn');
                this.packageTypes = dataHelper.prepareNg2SelectData(this.listPackageTypes, 'id', 'unitNameEn');
                this.weightMesurements = dataHelper.prepareNg2SelectData(this.listWeightMesurements, 'id', 'unitNameEn');
            } else {
                if (this.lstMasterContainers[index]["id"] != null) {
                    const contTypes = this.listContainerType.filter(x => x.active === true);
                    const packs = this.listPackageTypes.filter(x => x.active === true);
                    const weights = this.listWeightMesurements.filter(x => x.active === true);
                    this.containerTypes = dataHelper.prepareNg2SelectData(contTypes, 'id', 'unitNameEn');
                    this.packageTypes = dataHelper.prepareNg2SelectData(packs, 'id', 'unitNameEn');
                    this.weightMesurements = dataHelper.prepareNg2SelectData(weights, 'id', 'unitNameEn');
                }
            }
        } else {
            this.containerTypes = [];
        }
    }

    getUnits() {
        this._catalogueRepo.getUnit()
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this.isLoading = false;
                })
            )
            .subscribe(
                (res: any) => {
                    this.listUnits = res;
                    if (this.listUnits != null) {
                        this.listWeightMesurements = this.listUnits.filter(x => x.unitType === 'Weight Measurement');
                        this.listContainerType = this.listUnits.filter(x => x.unitType === 'Container');
                        this.listPackageTypes = this.listUnits.filter(x => x.unitType === 'Package');
                    }
                },
            );
    }

    getComodities() {
        this._catalogueRepo.getCommondity({ active: true, all: null })
            .pipe()
            .subscribe(
                (responses: any) => {
                    this.listcommodities = responses || [];
                    this.listcommodities = this.sortService.sort(responses, 'commodityNameEn', true);
                }
            );
    }

    cancelNewContainer(index: number) {
        if (this.lstMasterContainers[index].isNew === true) {
            this.lstMasterContainers.splice(index, 1);
        } else {
            this.lstMasterContainers[index] = this.containerToChangeEdit;
            this.lstMasterContainers[index].allowEdit = false;
            this.containerToChangeEdit = null;
        }
    }

    changeEditMode(index: any) {
        this.getComboboxDataContainer(index);
        if (this.lstMasterContainers[index].allowEdit === false || this.lstMasterContainers[index].allowEdit == undefined) {
            this.lstMasterContainers[index].allowEdit = true;
            this.lstMasterContainers[index].containerTypeActive = this.lstMasterContainers[index].containerTypeId != null ?
                [{ id: this.lstMasterContainers[index].containerTypeId, text: this.lstMasterContainers[index].containerTypeName }] : [];
            this.lstMasterContainers[index].packageTypeActive = this.lstMasterContainers[index].packageTypeId != null ?
                [{ id: this.lstMasterContainers[index].packageTypeId, text: this.lstMasterContainers[index].packageTypeName }] : [];
            this.lstMasterContainers[index].unitOfMeasureActive = this.lstMasterContainers[index].unitOfMeasureId != null ?
                [{ id: this.lstMasterContainers[index].unitOfMeasureId, text: this.lstMasterContainers[index].unitOfMeasureName }] : [];
            console.log(this.lstMasterContainers[index].unitOfMeasureActive);
            for (let i = 0; i < this.lstMasterContainers.length; i++) {
                if (i !== index) {
                    this.lstMasterContainers[i].allowEdit = false;
                }
            }
        } else {
            this.lstMasterContainers[index].allowEdit = false;
        }
        this.containerToChangeEdit = Object.assign({}, this.lstMasterContainers[index]);
    }

    removeAContainer(index: number) {
        this.indexItemConDelete = index;
        console.log('dadadadadadadd');
        this.popupConfirmDelete.show();
    }

    deleteContainer() {
        this.lstMasterContainers.splice(this.indexItemConDelete, 1);
        this.popupConfirmDelete.hide();
    }


    searchContainer(keySearch: any) {
        console.log(keySearch);
        keySearch = keySearch != null ? keySearch.trim().toLowerCase() : "";
        this.lstMasterContainers = Object.assign([], this.lstContainerTemp).filter(
            item => (item.containerTypeName.toLowerCase().includes(keySearch)
                || (item.quantity != null ? item.quantity.toString() : "").includes(keySearch)
                || (item.containerNo != null ? item.containerNo.toLowerCase() : "").includes(keySearch)
                || (item.sealNo != null ? item.sealNo.toLowerCase() : "").includes(keySearch)
                || (item.markNo != null ? item.markNo.toLowerCase() : "").includes(keySearch)
                || (item.commodityName != null ? item.commodityName.toLowerCase() : "").includes(keySearch)
                || (item.packageTypeName != null ? item.packageTypeName.toLowerCase() : "").includes(keySearch)
                || (item.packageQuantity != null ? item.packageQuantity.toString().toLowerCase() : "").includes(keySearch)
                || (item.description != null ? item.description.toLowerCase() : "").includes(keySearch)
                || (item.nw != null ? item.nw.toString().toLowerCase() : "").includes(keySearch)
                || (item.chargeAbleWeight != null ? item.chargeAbleWeight.toString() : "").toLowerCase().includes(keySearch)
                || (item.gw != null ? item.gw.toString().toLowerCase() : "").includes(keySearch)
                || (item.unitOfMeasureName != null ? item.unitOfMeasureName.toLowerCase() : "").includes(keySearch)
                || (item.cbm != null ? item.cbm.toString().toLowerCase() : "").includes(keySearch)
            )
        );
    }
}
