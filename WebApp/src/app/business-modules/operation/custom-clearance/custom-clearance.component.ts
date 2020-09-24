import { Component, ViewChild } from '@angular/core';
import { SortService } from 'src/app/shared/services/sort.service';
import { ToastrService } from 'ngx-toastr';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { catchError, map, finalize, takeUntil } from 'rxjs/operators';
import { CustomDeclaration } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { OperationRepo, DocumentationRepo, CatalogueRepo, ExportRepo } from 'src/app/shared/repositories';
import { ConfirmPopupComponent, Permission403PopupComponent } from 'src/app/shared/common/popup';
import _map from 'lodash/map';
import { NgProgress } from '@ngx-progressbar/core';
import { Router } from '@angular/router';
import { IAppState, getMenuUserPermissionState } from '@store';
import { Store } from '@ngrx/store';
import { SystemConstants } from 'src/constants/system.const';
import { formatDate } from '@angular/common';

@Component({
    selector: 'app-custom-clearance',
    templateUrl: './custom-clearance.component.html',
})
export class CustomClearanceComponent extends AppList {
    @ViewChild('confirmConvertPopup', { static: false }) confirmConvertPopup: ConfirmPopupComponent;
    @ViewChild('confirmDeletePopup', { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(Permission403PopupComponent, { static: false }) canNotAllowActionPopup: Permission403PopupComponent;
    listCustomDeclaration: CustomDeclaration[] = [];
    searchObject: any = {};
    listPort: any = [];
    listUnit: any = [];
    menuPermission: SystemInterface.IUserPermission;
    messageConvertError: string = '';
    clearancesToConvert: CustomDeclaration[] = [];
    headers: CommonInterface.IHeaderTable[];
    constructor(
        private _store: Store<IAppState>,
        private _sortService: SortService,
        private _toastrService: ToastrService,
        private _operationRepo: OperationRepo,
        private _ngProgressService: NgProgress,
        private _documentRepo: DocumentationRepo,
        private _catalogueRepo: CatalogueRepo,
        private _exportRepo: ExportRepo,
        private _router: Router,
    ) {
        super();
        this.requestList = this.getListCustomsDeclaration;
        this.requestSort = this.sortCD;
        this._progressRef = this._ngProgressService.ref();
        this.dataSearch = {
            fromClearanceDate: formatDate(new Date(new Date().setDate(new Date().getDate() - 30)), 'yyyy-MM-dd', 'en'),
            toClearanceDate: formatDate(new Date(), 'yyyy-MM-dd', 'en'),
            imPorted: null,
            personHandle: JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS)).id
        };
    }

    ngOnInit() {
        this._store.select(getMenuUserPermissionState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: SystemInterface.IUserPermission) => {
                    if (res !== null && res !== undefined) {
                        this.menuPermission = res;
                    }
                }
            );
        this.headers = [
            { title: 'Clearance No', field: 'clearanceNo', sortable: true },
            { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Clearance Location', field: 'gatewayName', sortable: true },
            { title: 'Partner Name', field: 'customerName', sortable: true },
            { title: 'Package Qty', field: 'pcs', sortable: true },
            { title: 'GW', field: 'grossWeight', sortable: true },
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'creator', field: 'userCreatedName', sortable: true },
            { title: 'Status', field: 'jobNo', sortable: true },
            { title: 'Import Country', field: 'importCountryName', sortable: true },
            { title: 'Export Country', field: 'exportCountryName', sortable: true },
        ];
        this.getListCustomsDeclaration();
        this.getListPort();
        this.getListUnit();
    }

    getListPort() {
        this._catalogueRepo.getListPort({ placeType: PlaceTypeEnum.Port })
            .subscribe((res: any) => { this.listPort = res; });
    }

    getListUnit() {
        this._catalogueRepo.getUnit({ unitType: 'Package' })
            .subscribe((res: any) => { this.listUnit = res; });
    }

    onSearchClearance(dataSearch?: any) {
        this.dataSearch = dataSearch;
        this.page = 1;
        this.getListCustomsDeclaration();
    }

    getListCustomsDeclaration() {
        this.isLoading = true;
        this._progressRef.start();
        const body = this.dataSearch || {};
        this._operationRepo.getListCustomDeclaration(this.page, this.pageSize, body)
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
            );
    }

    sortCD(sort: string): void {
        if (!!sort) {
            if (!!this.listCustomDeclaration.length) {
                this.listCustomDeclaration = this._sortService.sort(this.listCustomDeclaration, this.sort, this.order);
            }
        }
    }
    showDetail(id) {
        this._operationRepo.checkViewDetailPermission(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this._router.navigate(['/home/operation/custom-clearance/detail', id]);
                    } else {
                        this.canNotAllowActionPopup.show();
                    }
                },
            );
    }
    getDataFromEcus() {
        this._progressRef.start();
        this._operationRepo.importCustomClearanceFromEcus()
            .pipe(catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!!res.message) {
                        this._toastrService.success(res.message, '');
                    }
                    this.getListCustomsDeclaration();
                },
            );
    }

    confirmConvert() {
        this._toastrService.clear();
        if (this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo).length > 0) {
            this.clearancesToConvert = this.checkValidClearancesToJobs();
            if (this.clearancesToConvert === null) { return; }

            if (this.messageConvertError.length > 0) {
                this._toastrService.error(this.messageConvertError, '', { enableHtml: true });
                this.messageConvertError = '';
                return;
            } else {
                this._documentRepo.checkAllowConvertJob(this.clearancesToConvert)
                    .pipe(
                        catchError(this.catchError),
                        finalize(() => this._progressRef.complete())
                    ).subscribe(
                        (res: any) => {
                            if (res.status) {
                                this.confirmConvertPopup.show();
                            } else {
                                if (res.data === 403) {
                                    this._toastrService.error(res.message, '', { enableHtml: true });
                                } else {
                                    this.canNotAllowActionPopup.show();
                                }
                            }
                        },
                    );
            }
        } else {

            this.canNotAllowActionPopup.show();
        }
    }

    deleteClearance() {
        const customCheckedArray: CustomDeclaration[] = this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo) || [];
        if (this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo).length > 0) {
            this._operationRepo.checkDeletePermission(customCheckedArray)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe(
                    (res: any) => {
                        if (res.success) {
                            this.confirmDeletePopup.show();
                        } else {
                            // this._toastrService.error(res.message);
                            this.canNotAllowActionPopup.show();
                        }
                    },
                );
            // this.confirmDeletePopup.show();
        } else {

            this.canNotAllowActionPopup.show();
            // this._toastrService.warning(`You haven't selected any custom clearance yet. Please select one or more custom no to delete!`);
        }
    }

    onConfirmDelete() {
        this._progressRef.start();
        this.confirmDeletePopup.hide();
        const customCheckedArray: CustomDeclaration[] = this.listCustomDeclaration.filter(i => i.isSelected && !i.jobNo) || [];
        this._operationRepo.deleteMultipleClearance(customCheckedArray || [])
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    // this._toastrService.success(res.message, '');
                    if (res.status) {
                        this.getListCustomsDeclaration();
                    } else {
                        this.canNotAllowActionPopup.show();
                    }
                },
            );
    }

    onComfirmConvertToJobs() {
        this.confirmConvertPopup.hide();
        this._progressRef.start();
        this._documentRepo.convertExistedClearanceToJob(this.clearancesToConvert)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastrService.success('Convert Success');
                        this.getListCustomsDeclaration();
                    }
                },
            );
    }

    checkUncheckAll() {
        for (const clearance of this.listCustomDeclaration) {
            clearance.isSelected = this.isCheckAll;
        }
    }

    onChangeAction() {
        this.isCheckAll = this.listCustomDeclaration.every((item: CustomDeclaration) => item.isSelected);
    }
    checkValidClearancesToJobs() {
        const customCheckedArray: CustomDeclaration[] = this.listCustomDeclaration.filter((item: CustomDeclaration) => item.isSelected && !item.jobNo);
        if (customCheckedArray === null) {
            return customCheckedArray;
        }
        for (const row of customCheckedArray) {
            const clearance: CustomDeclaration = row;

            if (clearance.mblid === null || clearance.mblid.length === 0) {
                this.messageConvertError = this.messageConvertError + clearance.clearanceNo + ` Không có MBL/MAWB để tạo job mới <br />`;
            }
            if (clearance.hblid === null || clearance.hblid.length === 0) {
                this.messageConvertError = this.messageConvertError + clearance.clearanceNo + ` Không có HBL/HAWB để tạo job mới <br />`;
            }
            if (clearance.clearanceDate === null) {
                this.messageConvertError = this.messageConvertError + clearance.clearanceNo + ` Không có clearance date để tạo job mới <br />`;
            }
            if (clearance.partnerTaxCode === null || clearance.partnerTaxCode.length === 0) {
                this.messageConvertError = this.messageConvertError + clearance.clearanceNo + ` Không có customer để tạo job mới <br />`;
            }
            if (clearance.serviceType !== "Air" && clearance.serviceType !== "Express") {
                if (clearance.cargoType === null) {
                    this.messageConvertError = this.messageConvertError + clearance.clearanceNo + ` Không có cargo type để tạo mới <br />`;
                }
            }
            if (clearance.type === null) {
                this.messageConvertError = this.messageConvertError + clearance.clearanceNo + ` Không có Type để tạo job mới <br />`;
            }
        }
        return customCheckedArray;
    }

    export() {
        const body = this.dataSearch || {};
        this._exportRepo.exportCustomClearance(body)
            .subscribe(
                (response: ArrayBuffer) => {
                    this.downLoadFile(response, "application/ms-excel", 'CustomClearance.xlsx');
                },
                (errors: any) => {
                },
                () => { }
            );
    }

    gotoCreateCD() {
        this._router.navigate(["home/operation/custom-clearance/new"]);
    }

    import() {
        this._router.navigate(["home/operation/custom-clearance/import"]);

    }


}



