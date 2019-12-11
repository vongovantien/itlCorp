import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PORTINDEXCOLUMNSETTING } from './port-index.columns';
import { PortIndex } from 'src/app/shared/models/catalogue/port-index.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { SystemConstants } from 'src/constants/system.const';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import _map from 'lodash/map';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
declare var $: any;
import * as dataHelper from 'src/helper/data.helper';
import { NgForm } from '@angular/forms';
import { SelectComponent } from 'ng2-select';
import { CatalogueRepo, ExportRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

@Component({
    selector: 'app-port-index',
    templateUrl: './port-index.component.html'
})
export class PortIndexComponent extends AppList implements OnInit {
    @ViewChild('formAddEdit', { static: false }) form: NgForm;
    @ViewChild('chooseCountry', { static: false }) public ngSelectCountry: SelectComponent;
    @ViewChild('chooseArea', { static: false }) public ngSelectArea: SelectComponent;
    @ViewChild('chooseMode', { static: false }) public ngSelectMode: SelectComponent;
    portIndexSettings: ColumnSetting[] = PORTINDEXCOLUMNSETTING;
    portIndexs: Array<PortIndex>;
    portIndex: PortIndex = new PortIndex();
    pager: PagerSetting = PAGINGSETTING;
    criteria: any = { placeType: PlaceTypeEnum.Port };
    addButtonSetting: ButtonModalSetting = {
        dataTarget: 'edit-port-index-modal',
        typeButton: ButtonType.add
    };
    importButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.import
    };
    exportButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.export
    };
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    configSearch: any = {
        settingFields: this.portIndexSettings.filter(x => x.allowSearch == true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
        typeSearch: TypeSearch.outtab
    };
    countries: any[];
    areas: any[];
    modes: any[];
    countryActive: any[] = [];
    areaActive: any[] = [];
    isDesc: boolean = false;
    modeActive: any[] = [];

    constructor(private baseService: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private excelService: ExcelService,
        private exportRepository: ExportRepo,
        private catalogueRepo: CatalogueRepo,
        private toastService: ToastrService,
        private _progressService: NgProgress) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.requestPortIndex;
    }

    ngOnInit() {
        this.requestPortIndex();
        this.getDataCombobox();
    }
    requestPortIndex() {
        this.catalogueRepo.pagingPlace(this.page, this.pageSize, Object.assign({}, this.criteria))
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
                    this.portIndexs = res.data;
                },
            );
    }
    onSearch(event) {
        this.criteria = {
            placeType: PlaceTypeEnum.Port
        };
        if (event.field === "All") {
            this.criteria.all = event.searchString;
        } else {
            const language = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
            if (language === SystemConstants.LANGUAGES.ENGLISH) {
                if (event.field === "countryName") {
                    this.criteria.countryNameEN = event.searchString;
                }
                if (event.field === "areaName") {
                    this.criteria.areaNameEN = event.searchString;
                }
            } else {
                if (event.field === "countryName") {
                    this.criteria.countryNameVN = event.searchString;
                }
                if (event.field === "areaName") {
                    this.criteria.areaNameVN = event.searchString;
                }
            }
            if (language === SystemConstants.LANGUAGES.VIETNAM) {
            }
            if (event.field === "code") {
                this.criteria.code = event.searchString;
            }
            if (event.field === "nameEn") {
                this.criteria.nameEN = event.searchString;
            }
            if (event.field === "nameVn") {
                this.criteria.nameVN = event.searchString;
            }
            if (event.field === "modeOfTransport") {
                this.criteria.modeOfTransport = event.searchString;
            }
        }
        this.page = 1;
        this.requestList();
    }
    resetSearch(event) {
        this.criteria = {
            placeType: PlaceTypeEnum.Port
        };
        this.onSearch(event);
    }
    showAdd() {
        this.initPortIndex();
    }
    initPortIndex() {
        this.form.onReset();
        this.portIndex = new PortIndex();
        this.portIndex.placeType = PlaceTypeEnum.Port;
        this.modeActive = [];
        this.countryActive = [];
        this.areaActive = [];
        this.portIndex.countryID = null;
        this.portIndex.areaID = null;
        this.portIndex.modeOfTransport = null;
    }
    onSubmit() {
        if (this.form.valid && this.portIndex.countryID != null && this.portIndex.modeOfTransport != null) {
            if (this.portIndex.id == null) {
                this.addNew();
            } else {
                this.update();
            }
        }
    }

    async update() {
        this.catalogueRepo.updatePlace(this.portIndex.id, this.portIndex)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.toastService.success(res.message, '');
                        // this.isSubmitted = false;
                        // this.initForm();
                        // this.isSaveSuccess.emit(true);
                        // this.hide();
                        this.initPortIndex();
                        $('#edit-port-index-modal').modal('hide');
                        this.requestList();
                        // this.getPortIndexs(this.pager);
                    } else {
                        this.toastService.error(res.message, '');
                    }
                }
            );
    }

    async addNew() {
        this.catalogueRepo.addPlace(this.portIndex)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.toastService.success(res.message, '');
                        // this.initPager();
                        // this.getPortIndexs(this.pager);
                        this.requestList();
                        $('#edit-port-index-modal').modal('hide');
                    } else {
                        this.toastService.error(res.message, '');
                    }
                }
            );
    }

    onCancel() {
        this.initPortIndex();
        this.requestList();
    }

    getDataCombobox() {
        this.getCountries();
        this.getAreas();
        this.getModeOfTransport();
    }
    async getModeOfTransport() {
        const responses = await this.baseService.getAsync(this.api_menu.Catalogue.CatPlace.getModeOfTransport, false, false);
        if (responses != null) {
            this.modes = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
        } else {
            this.modes = [];
        }
    }
    async getAreas() {
        const responses = await this.baseService.getAsync(this.api_menu.Catalogue.Area.getAllByLanguage, false, false);
        if (responses != null) {
            this.areas = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
        } else {
            this.areas = [];
        }
    }
    async getCountries() {
        const responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage, false, true);
        if (responses != null) {
            this.countries = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
        } else {
            this.countries = [];
        }
    }
    value: any = {};
    refreshValue(value: any): void {
        this.value = value;
    }
    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }
    public typed(value: any): void {
        console.log('New search input: ', value);
    }
    showConfirmDelete(item) {
        this.portIndex = item;
    }
    showDetail(item: PortIndex) {
        this.portIndex = item;
        this.countryActive = this.getCountryActive(this.portIndex.countryID);
        this.areaActive = this.getDistrictAactive(this.portIndex.areaID);
        this.modeActive = this.getModeActive(this.portIndex.modeOfTransport);
    }
    getModeActive(modeOfTransport: string) {
        const index = this.modes.findIndex(x => x.id === modeOfTransport);
        if (index > -1) {
            return [this.modes[index]];
        } else {
            return [];
        }
    }
    getDistrictAactive(areaID: string) {
        const index = this.areas.findIndex(x => x.id === areaID);
        if (index > -1) {
            return [this.areas[index]];
        } else {
            return [];
        }
    }
    getCountryActive(countryID: number) {
        const index = this.countries.findIndex(x => x.id === countryID);
        if (index > -1) {
            return [this.countries[index]];
        } else {
            return [];
        }
    }


    onDelete(event) {
        this.catalogueRepo.deletePlace(this.portIndex.id)
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

    onSortChange(column) {
        const property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.portIndexs = this.sortService.sort(this.portIndexs, property, this.isDesc);
    }
    export() {
        this.exportRepository.exportPortIndex(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PortIndex.xlsx");
                },
            );


    }

}
