import { Component, OnInit, ViewChild } from '@angular/core';
import { forkJoin } from 'rxjs';
import { catchError, finalize, map } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';

import { Warehouse } from '../../../shared/models/catalogue/ware-house.model';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { PagerSetting } from '../../../shared/models/layout/pager-setting.model';
import { SystemConstants } from '../../../../constants/system.const';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { language } from 'src/languages/language.en';
import { CatalogueRepo, ExportRepo } from 'src/app/shared/repositories';
import { AppList } from 'src/app/app.list';
import { FormWarehouseComponent } from './components/form-warehouse.component';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-warehouse',
    templateUrl: './warehouse.component.html',
})
export class WarehouseComponent extends AppList implements OnInit {
    @ViewChild(FormWarehouseComponent, { static: false }) formPopup: FormWarehouseComponent;
    warehouses: Array<Warehouse>;
    warehouse: Warehouse = new Warehouse();
    criteria: any = { placeType: PlaceTypeEnum.Warehouse };
    pager: PagerSetting = PAGINGSETTING;
    addButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.add
    };
    importButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.import
    };
    exportButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.export
    };
    warehouseSettings: ColumnSetting[] = language.Warehouse; // = WAREHOUSECOLUMNSETTING;
    isDesc: boolean = true;
    configSearch: any = {
        settingFields: this.warehouseSettings.filter(x => x.allowSearch === true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
        typeSearch: TypeSearch.outtab
    };

    constructor(private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private toastService: ToastrService,
        private exportRepository: ExportRepo,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.requestWarehouses;
    }

    ngOnInit() {
        this.getDataCombobox();
        this.requestWarehouses();
    }

    getDataCombobox() {
        forkJoin([
            this._catalogueRepo.getCountryByLanguage(),
            this._catalogueRepo.getProvinces(),
            this._catalogueRepo.getDistricts()
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([countries, provinces, districts]) => {
                    this.formPopup.countries = this.utility.prepareNg2SelectData(countries || [], 'id', 'name');
                    this.formPopup.provinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    this.formPopup.districts = this.utility.prepareNg2SelectData(districts || [], 'id', 'name_VN');
                },
                () => { },

            );
    }
    requestWarehouses() {
        this._catalogueRepo.pagingPlace(this.page, this.pageSize, Object.assign({}, this.criteria))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((res: any) => {
                    return {
                        data: res.data,
                        totalItems: res.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.warehouses = res.data;
                },
            );
    }

    onSortChange(column) {
        const property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
    }
    showAdd() {
        this.formPopup.warehouseForm.reset();
        this.formPopup.code.enable();
        [this.formPopup.isUpdate, this.formPopup.isSubmitted] = [false, false];
        this.formPopup.title = "Add Warehouse";
        this.formPopup.show();
    }
    async showDetail(item: Warehouse) {
        this.warehouse = item;
        [this.formPopup.isUpdate, this.formPopup.isSubmitted] = [true, false];
        this.formPopup.title = "Update Warehouse";
        this.formPopup.code.disable();
        this.formPopup.warehouse = item;
        this.formPopup.setFormValue(item);
        this.formPopup.show();
    }
    async onDelete(event) {
        if (event) {
            this._catalogueRepo.deletePlace(this.warehouse.id)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                ).subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this.toastService.success(res.message, '');

                            this.page = 1;
                            this.requestList();
                        } else {
                            this.toastService.error(res.message || 'Có lỗi xảy ra', '');
                        }
                    },
                );
        }
    }
    showConfirmDelete(item: Warehouse) {
        this.warehouse = item;
    }

    resetSearch(event: { field: string; searchString: any; }) {
        this.criteria = {
            placeType: PlaceTypeEnum.Warehouse
        };
        this.onSearch(event);
    }

    onSearch(event: { field: string; searchString: any; }) {
        const curLanguage = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
        if (event.field === "All") {
            this.criteria.all = event.searchString;
        } else {
            this.criteria = {
                placeType: PlaceTypeEnum.Warehouse
            };
            this.criteria[event.field] = event.searchString;
            if (curLanguage === SystemConstants.LANGUAGES.ENGLISH) {
                if (event.field === "countryName") {
                    this.criteria.countryNameEN = event.searchString;
                }
                if (event.field === "provinceName") {
                    this.criteria.provinceNameEN = event.searchString;
                }
                if (event.field === "districtName") {
                    this.criteria.districtNameEN = event.searchString;
                }
            }
            if (curLanguage === SystemConstants.LANGUAGES.VIETNAM) {
                if (event.field === "countryName") {
                    this.criteria.countryNameVN = event.searchString;
                }
                if (event.field === "provinceName") {
                    this.criteria.provinceNameVN = event.searchString;
                }
                if (event.field === "districtName") {
                    this.criteria.districtNameVN = event.searchString;
                }
            }
        }
        this.page = 1;
        this.requestWarehouses();
    }

    /**
     * EXPORT - IMPORT DATA 
     */
    async export() {
        this.exportRepository.exportPortIndex(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "Warehouse.xlsx");
                },
            );
    }
}
