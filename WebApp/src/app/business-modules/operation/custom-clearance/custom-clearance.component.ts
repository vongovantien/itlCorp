import { Component, ViewChild } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { ToastrService } from 'ngx-toastr';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { catchError, map, finalize } from 'rxjs/operators';
import { CustomDeclaration } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { CDNoteRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

import _map from 'lodash/map';
import { formatDate } from '@angular/common';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-custom-clearance',
    templateUrl: './custom-clearance.component.html',
    styleUrls: ['./custom-clearance.component.scss']
})
export class CustomClearanceComponent extends AppList {
    @ViewChild('confirmConvertPopup', { static: false }) confirmConvertPopup: ConfirmPopupComponent;
    @ViewChild('confirmDeletePopup', { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    listCustomDeclaration: CustomDeclaration[] = [];
    searchObject: any = {};
    listCustomer: any = [];
    listPort: any = [];
    listUnit: any = [];

    headers: CommonInterface.IHeaderTable[];
    constructor(
        private excelService: ExcelService,
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private _sortService: SortService,
        private _toastrService: ToastrService,
        private _cdNoteRepo: CDNoteRepo,
        private _ngProgressService: NgProgress
    ) {
        super();
        this.requestList = this.getListCustomsDeclaration;
        this.requestSort = this.sortCD;
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {

        this.headers = [
            { title: 'Clearance No', field: 'clearanceNo', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Gateway', field: 'gatewayName', sortable: true },
            { title: 'Partner Name', field: 'customerName', sortable: true },
            { title: 'Import Country', field: 'importCountryName', sortable: true },
            { title: 'Export Country', field: 'exportCountryName', sortable: true },
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
            { title: 'Status', field: 'jobNo', sortable: true },
        ];
        this.getListCustomsDeclaration();
        this.getListCustomer();
        this.getListPort();
        this.getListUnit();
    }

    async getListCustomer() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.query, { partnerGroup: PartnerGroupEnum.CUSTOMER }, true, true);
        this.listCustomer = res;
    }
    async getListPort() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, { placeType: PlaceTypeEnum.Port }, true, true);
        this.listPort = res;
    }
    async getListUnit() {
        const res = await this.baseServices.postAsync(this.api_menu.Catalogue.Unit.getAllByQuery, { unitType: 'Package' }, true, true);
        this.listUnit = res;
    }

    getListCustomsDeclaration(dataSearch?: any) {
        this.isLoading = true;
        this._progressRef.start();
        const body = dataSearch || {};
        this._cdNoteRepo.getListCustomDeclaration(this.page, this.pageSize, body)
            .pipe(
                catchError(this.catchError),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new CustomDeclaration(item)),
                        totalItems: data.totalItems,
                    };
                }),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); })
            )
            .subscribe(
                (res: any) => {
                    this.listCustomDeclaration = res.data;
                    this.totalItems = res.totalItems;
                },
                (errors: any) => { 
                    this.handleError(errors, (data) => {
                        this._toastrService.error(data.message, data.title);
                    });
                },
                () => { }
            );
    }

    sortCD(sort: string): void {
        if (!!sort) {
            if (!!this.listCustomDeclaration.length) {
                this.listCustomDeclaration = this._sortService.sort(this.listCustomDeclaration, this.sort, this.order);
            }
        }
    }

    getDataFromEcus() {
        this._cdNoteRepo.importCustomClearanceFromEcus()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    this._toastrService.success(res.message, '');
                    this.getListCustomsDeclaration();
                },
                (errors: any) => { },
                () => { }
            );
    }

    confirmConvert() {
        if (this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo).length > 0) {
            this.confirmConvertPopup.show();
        } else {
            this._toastrService.warning('Custom clearance was not selected');
        }
    }

    deleteClearance() {
        if (this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo).length > 0) {
            this.confirmDeletePopup.show();
        } else {
            this._toastrService.warning(`You haven't selected any custom clearance yet. Please select one or more custom no to delete!`);
        }
    }

    onConfirmDelete() {
        this._progressRef.start();
        this.confirmDeletePopup.hide();
        const customCheckedArray: CustomDeclaration[] = this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo) || [];
        this._cdNoteRepo.deleteMultipleClearance(customCheckedArray || [])
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    this._toastrService.success(res.message, '', { positionClass: 'toast-bottom-right' });
                    this.getListCustomsDeclaration();
                },
                (errors: any) => { 
                    this.handleError(errors, (data) => {
                        this._toastrService.error(data.message, data.title);
                    });
                },
                () => { }
            );
    }

    onComfirmConvertToJobs() {
        this.confirmConvertPopup.hide();
        this._progressRef.start();
        const clearancesToConvert = this.mapClearancesToJobs();
        const clearanceNulls = clearancesToConvert.filter(x => x.opsTransaction == null);
        if (clearanceNulls.length === 0) {
            this._cdNoteRepo.convertClearanceToJob(clearancesToConvert)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); })
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this.getListCustomsDeclaration();
                        }
                    },
                    (errors: any) => {
                        this.handleError(errors, (data) => {
                            this._toastrService.error(data.message, data.title);
                        });
                     },
                    () => { }
                );
        }
    }

    checkUncheckAll() {
        for (const clearance of this.listCustomDeclaration) {
            clearance.isSelected = this.isCheckAll;
        }
    }

    onChangeAction() {
        this.isCheckAll = this.listCustomDeclaration.every((item: CustomDeclaration) => item.isSelected);
    }

    mapClearancesToJobs() {
        const clearancesToConvert = [];
        const customCheckedArray: any[] = this.listCustomDeclaration.filter((item: CustomDeclaration) => item.isSelected && !item.jobNo);
        for (let i = 0; i < customCheckedArray.length; i++) {
            const clearance: CustomDeclaration = customCheckedArray[i];
            let shipment = new OpsTransaction();
            let index = this.listCustomer.findIndex(x => x.taxCode.trim() === clearance.partnerTaxCode.trim());
            if (index !== -1) {
                const customer = this.listCustomer[index];
                shipment.customerId = customer.id;
                shipment.salemanId = customer.salePersonId;
                shipment.serviceMode = clearance.type;
                index = this.listPort.findIndex(x => x.code === clearance.gateway);
                if (index > -1) {
                    if (clearance.type === "Export") {
                        shipment.pol = this.listPort[index].id;
                    }
                    if (clearance.type === "Import") {
                        shipment.pod = this.listPort[index].id;
                    }
                }
                if (clearance.serviceType === "Sea") {
                    if (clearance.cargoType === "FCL") {
                        shipment.productService = "SeaFCL";
                    }
                    if (clearance.cargoType === "LCL") {
                        shipment.productService = "SeaLCL";
                    }
                } else {
                    shipment.productService = clearance.serviceType;
                }
                shipment.shipmentMode = "External";
                shipment.mblno = clearance.mblid;
                shipment.hwbno = clearance.hblid;
                shipment.serviceDate = clearance.clearanceDate;
                shipment.sumGrossWeight = clearance.grossWeight;
                shipment.sumNetWeight = clearance.netWeight;
                shipment.sumCbm = clearance.cbm;
                const claim = localStorage.getItem('id_token_claims_obj');
                const currenctUser = JSON.parse(claim)["id"];
                shipment.billingOpsId = currenctUser;
                index = this.listUnit.findIndex(x => x.code === clearance.unitCode);
                if (index > -1) {
                    shipment.packageTypeId = this.listUnit[index].id;
                }
            } else {
                this.baseServices.errorToast(`Không có customer để tạo job mới`, `${clearance.clearanceNo}`);
                shipment = null;
            }
            if (clearance.mblid == null) {
                this.baseServices.errorToast(`Không có MBL/MAWB để tạo job mới`, `${clearance.clearanceNo} `);
                shipment = null;
            }
            if (clearance.hblid == null) {
                this.baseServices.errorToast(`Không có HBL/HAWB để tạo job mới`, `${clearance.clearanceNo} `);
                shipment = null;
            }
            if (clearance.clearanceDate == null) {
                this.baseServices.errorToast(`Không có clearance date để tạo job mới`, `${clearance.clearanceNo} `);
                shipment = null;
            }
            clearancesToConvert.push({ opsTransaction: shipment, customsDeclaration: clearance });
        }
        return clearancesToConvert;
    }

    async export() {
        let customClearances = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.query, this.searchObject);
        customClearances = _map(customClearances, function (item, index) {
            return [
                index + 1,
                item.clearanceNo,
                item.type,
                item.gateway,
                item.customerName,
                item.importCountryName,
                item.exportCountryName,
                item.jobNo,
                formatDate(item.clearanceDate, 'dd/MM/yyyy', 'en'),
                (item.jobNo != null && item.jobNo != '') ? 'Imported' : 'Not Imported'
            ];
        });

        /**Set up stylesheet */
        const exportModel: ExportExcel = new ExportExcel();
        exportModel.fileName = "Custom Clearance Report";
        const currrently_user = localStorage.getItem('currently_userName');
        exportModel.title = "Custom Clearance Report ";
        exportModel.author = currrently_user;
        exportModel.header = [
            { name: "No.", width: 10 },
            { name: "Clearance No", width: 25 },
            { name: "Type", width: 10 },
            { name: "Gateway", width: 25 },
            { name: "Partner Name", width: 25 },
            { name: "Import Country", width: 25 },
            { name: "Export Country", width: 25 },
            { name: "JOBID", width: 25 },
            { name: "Clearance Date", width: 20 },
            { name: "Status", width: 25 }
        ];

        exportModel.data = customClearances;
        this.excelService.generateExcel(exportModel);
    }

}



