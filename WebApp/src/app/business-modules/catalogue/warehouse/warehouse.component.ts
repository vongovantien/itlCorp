import { Component, OnInit, ViewChild } from '@angular/core';
import { Warehouse } from '../../../shared/models/catalogue/ware-house.model';
import { ColumnSetting } from '../../../shared/models/layout/column-setting.model';
import { SortService } from '../../../shared/services/sort.service';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { PagerSetting } from '../../../shared/models/layout/pager-setting.model';
import { BaseService } from 'src/app/shared/services/base.service';
import { NgForm } from '@angular/forms';
import { SystemConstants } from '../../../../constants/system.const';
import { API_MENU } from '../../../../constants/api-menu.const';
import { SelectComponent } from 'ng2-select';
import { PaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
declare var $: any;
import * as lodash from 'lodash';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { language } from 'src/languages/language.en';
import * as dataHelper from 'src/helper/data.helper';
import { forkJoin } from 'rxjs';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-warehouse',
    templateUrl: './warehouse.component.html',
})
export class WarehouseComponent extends AppList implements OnInit {
    warehouses: Array<Warehouse>;
    countries: any[] = [];
    countryActive: any[] = [];
    provinces: any[] = [];
    provinceActive: any[] = [];
    districts: any[] = [];
    districtActive: any[] = [];
    warehouse: Warehouse = new Warehouse();
    showModal: boolean = false;
    provinceLookup: any;
    districtLookup: any;
    criteria: any = { placeType: PlaceTypeEnum.Warehouse };
    pager: PagerSetting = PAGINGSETTING;
    addButtonSetting: ButtonModalSetting = {
        dataTarget: "add-ware-house-modal",
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

    @ViewChild('chooseCountry', { static: false }) public ngSelectCountry: SelectComponent;
    @ViewChild('chooseProvince', { static: false }) public ngSelectProvince: SelectComponent;
    @ViewChild('chooseDistrict', { static: false }) public ngSelectDistrict: SelectComponent;
    @ViewChild(PaginationComponent, { static: false }) child;
    @ViewChild('formAddEdit', { static: false }) form: NgForm;
    warehouseSettings: ColumnSetting[] = language.Warehouse;//= WAREHOUSECOLUMNSETTING;
    isDesc: boolean = true;
    configSearch: any = {
        settingFields: this.warehouseSettings.filter(x => x.allowSearch == true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
        typeSearch: TypeSearch.outtab
    };

    constructor(private sortService: SortService,
        private excelService: ExcelService,
        private baseService: BaseService,
        private _catalogueRepo: CatalogueRepo,
        private api_menu: API_MENU) {
        super();
    }

    ngOnInit() {
        this.initNewPager();
        this.warehouse.placeType = PlaceTypeEnum.Warehouse;
        this.getWarehouses(this.pager);
        this.getDataCombobox();
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
                    this.countries = this.utility.prepareNg2SelectData(countries || [], 'id', 'name');
                    this.provinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'name_VN');
                    this.districts = this.utility.prepareNg2SelectData(districts || [], 'id', 'name_VN');
                },
                (errors: any) => { },

            );

        // this.getCountries();
        // this.getProvinces();
        // this.getDistricts();
    }

    async getCountries() {
        let responses = await this.baseService.getAsync(this.api_menu.Catalogue.Country.getAllByLanguage, false, true);
        if (responses != null) {
            this.countries = dataHelper.prepareNg2SelectData(responses, 'id', 'name');
        }
        else {
            this.countries = [];
        }
    }
    async getProvinces(countryId?: number) {
        let url = this.api_menu.Catalogue.CatPlace.getProvinces;
        if (countryId != undefined) {
            url = url + "?countryId=" + countryId;
        }
        let responses = await this.baseService.getAsync(url, false, true);
        if (responses != null) {
            this.provinces = dataHelper.prepareNg2SelectData(responses, 'id', 'name_VN');
        }
        else {
            this.provinces = [];
        }
    }
    async getDistricts(provinceId?: any) {
        let url = this.api_menu.Catalogue.CatPlace.getDistricts;
        if (provinceId != undefined) {
            url = url + "?provinceId=" + provinceId;
        }
        let responses = await this.baseService.getAsync(url, false, true);
        if (responses != null) {
            this.districts = dataHelper.prepareNg2SelectData(responses, 'id', 'name_VN');
        }
        else {
            this.districts = [];
        }
    }
    async getWarehouses(pager: PagerSetting) {
        let responses = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
        if (responses != null) {
            this.warehouses = responses.data;
            this.pager.totalItems = responses.totalItems;
        }
        else {
            this.warehouses = [];
            this.pager.totalItems = 0;
        }
    }
    onSortChange(column) {
        let property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.warehouses = this.sortService.sort(this.warehouses, property, this.isDesc);
    }
    async showDetail(item: Warehouse) {
        this.warehouse = item;
        if (this.warehouse.countryID != null) {
            await this.getProvinces(this.warehouse.countryID);
            this.countryActive = this.getCountryActive(this.warehouse.countryID);
        }
        if (this.warehouse.provinceID != null) {
            await this.getDistricts(this.warehouse.provinceID);
            this.provinceActive = this.getProvinceActive(this.warehouse.provinceID);
        }
        this.districtActive = this.getDistrictActive(this.warehouse.districtID);
    }
    getDistrictActive(districtID: string) {
        let indexOfDistrictActive = this.districts.findIndex(x => x.id == districtID);
        if (indexOfDistrictActive > -1) {
            return [this.districts[indexOfDistrictActive]];
        }
        else {
            return [];
        }
    }
    getProvinceActive(provinceID: string) {
        let indexOfProvinceActive = this.provinces.findIndex(x => x.id == provinceID);
        if (indexOfProvinceActive > -1) {
            return [this.provinces[indexOfProvinceActive]];
        }
        else {
            return [];
        }
    }
    getCountryActive(countryID: number) {
        let indexOfCountryActive = this.countries.findIndex(x => x.id == countryID);
        if (indexOfCountryActive > -1) {
            return [this.countries[indexOfCountryActive]];
        }
        else {
            return [];
        }
    }
    resetWarehouse() {
        this.warehouse = {
            id: null,
            code: null,
            nameEn: null,
            nameVn: null,
            countryID: null,
            districtID: null,
            provinceID: null,
            countryName: null,
            provinceName: null,
            districtName: null,
            address: null,
            placeType: PlaceTypeEnum.Warehouse
        };
        this.provinces = [];
        this.districts = [];
        this.countryActive = [];
        this.provinceActive = [];
        this.districtActive = [];
        this.warehouse.countryID = null;
        this.warehouse.provinceID = null;
        this.warehouse.districtID = null;
        this.ngSelectCountry.active = [];
        this.ngSelectProvince.active = [];
        this.ngSelectDistrict.active = [];
        this.form.onReset();
    }
    async onDelete(event) {
        console.log(event);
        if (event) {
            this.baseService.spinnerShow();
            this.baseService.delete(this.api_menu.Catalogue.CatPlace.delete + this.warehouse.id).subscribe((response: any) => {
                this.baseService.successToast(response.message);
                this.baseService.spinnerHide();
                this.setPageAfterDelete();
            }, err => {
                this.baseService.spinnerHide();
                this.baseService.handleError(err);
            });
        }
    }
    setPageAfterDelete() {
        this.pager.totalItems = this.pager.totalItems - 1;
        let totalPages = Math.ceil(this.pager.totalItems / this.pager.pageSize);
        if (totalPages < this.pager.totalPages) {
            this.pager.currentPage = totalPages;
        }
        this.child.setPage(this.pager.currentPage);
    }
    showConfirmDelete(item: Warehouse) {
        this.warehouse = item;
    }

    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.totalPages = pager.totalPages;
        this.pager.pageSize = pager.pageSize
        this.getWarehouses(pager);
    }
    onSubmit() {
        if (this.form.valid && this.warehouse.countryID != null && this.warehouse.provinceID != null && this.warehouse.districtID != null) {
            if (this.warehouse.id == null) {
                this.addNew();
            } else {
                this.update();
            }
        }
    }
    async update() {
        const response = await this.baseService.putAsync(this.api_menu.Catalogue.CatPlace.update + this.warehouse.id, this.warehouse, true, true);
        if (response != null) {
            if (response.status) {
                $('#edit-ware-house-modal').modal('hide');
                this.getWarehouses(this.pager);
                this.resetWarehouse();
            }
        }
    }
    async addNew() {
        let response = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.add, this.warehouse, true, true);
        if (response != null) {
            if (response.status) {
                this.initNewPager();
                this.getWarehouses(this.pager);
                this.resetWarehouse();
                $('#' + this.addButtonSetting.dataTarget).modal('hide');
            }
        }
    }
    initNewPager() {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
    }

    resetSearch(event: { field: string; searchString: any; }) {
        this.criteria = {
            placeType: PlaceTypeEnum.Warehouse
        };
        this.onSearch(event);
    }
    onSearch(event: { field: string; searchString: any; }) {
        if (event.field == "All") {
            this.criteria.all = event.searchString;
        }
        else {
            this.criteria = {
                placeType: PlaceTypeEnum.Warehouse
            };
            this.criteria[event.field] = event.searchString;
            let language = localStorage.getItem(SystemConstants.CURRENT_CLIENT_LANGUAGE);
            if (language == SystemConstants.LANGUAGES.ENGLISH) {
                if (event.field == "countryName") {
                    this.criteria.countryNameEN = event.searchString;
                }
                if (event.field == "provinceName") {
                    this.criteria.provinceNameEN = event.searchString;
                }
                if (event.field == "districtName") {
                    this.criteria.districtNameEN = event.searchString;
                }
            }
            if (language == SystemConstants.LANGUAGES.VIETNAM) {
                if (event.field == "countryName") {
                    this.criteria.countryNameVN = event.searchString;
                }
                if (event.field == "provinceName") {
                    this.criteria.provinceNameVN = event.searchString;
                }
                if (event.field == "districtName") {
                    this.criteria.districtNameVN = event.searchString;
                }
            }
        }
        this.initNewPager();
        this.getWarehouses(this.pager);
    }
    onCancel() {
        this.form.onReset();
        this.resetWarehouse();
        this.getWarehouses(this.pager);
    }
    onChange(value, name: any) {
        if (name == 'country') {
            this.warehouse.countryID = value.id;
            this.getProvinces(value.id);
            this.chooseCountryReset();
        }
        if (name == 'province') {
            this.warehouse.provinceID = value.id;
            this.getDistricts(value.id);
            this.chooseProvinceReset();
        }
        if (name == 'district') {
            this.warehouse.districtID = value.id;
        }
    }
    showAdd() {
        this.resetWarehouse();
        this.showModal = true;
    }

    value: any = {};
    public refreshValue(value: any, name: any): void {
        this.value = value;
    }
    public removed(value: any, name: any): void {
        if (name == 'country') {
            this.warehouse.countryID = null;
            this.warehouse.provinceID = null;
            this.warehouse.districtID = null;
            this.chooseCountryReset();
        }
        if (name == 'province') {
            this.warehouse.provinceID = null;
            this.warehouse.districtID = null;
            this.chooseProvinceReset();
        }
        if (name == 'district') {
            this.warehouse.districtID = null;
        }
    }
    public typed(value: any): void {
    }
    chooseCountryReset() {
        this.ngSelectProvince.active = [];
        this.ngSelectDistrict.active = [];
        this.provinces = [];
        this.districts = [];
        this.provinceActive = [];
        this.districtActive = [];
    }
    chooseProvinceReset() {
        this.ngSelectDistrict.active = [];
        this.districts = [];
        this.districtActive = [];
    }

    /**
     * EXPORT - IMPORT DATA 
     */
    async export() {
        var warehouseData = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.query, this.criteria);
        console.log(warehouseData);
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == SystemConstants.LANGUAGES.ENGLISH_API) {
            warehouseData = lodash.map(warehouseData, function (item, index) {
                return [
                    index + 1,
                    item['code'],
                    item['nameEn'],
                    item['nameVn'],
                    item['address'],
                    item['districtNameEN'],
                    item['provinceNameEN'],
                    item['countryNameEN'],
                    (item['inactive'] == true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
                ]
            });
        }
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) == SystemConstants.LANGUAGES.VIETNAM_API) {
            warehouseData = lodash.map(warehouseData, function (item, index) {
                return [
                    index + 1,
                    item['code'],
                    item['nameEn'],
                    item['nameVn'],
                    item['address'],
                    item['districtNameVN'],
                    item['provinceNameVN'],
                    item['countryNameVN'],
                    (item['inactive'] == true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
                ]
            });
        }
        const exportModel: ExportExcel = new ExportExcel();
        exportModel.title = "Warehouse list";
        const currrently_user = localStorage.getItem('currently_userName');
        exportModel.author = currrently_user;
        exportModel.header = [
            { name: "No.", width: 10 },
            { name: "Code", width: 20 },
            { name: "Name EN", width: 20 },
            { name: "Name VN", width: 20 },
            { name: "Address", width: 30 },
            { name: "Disctric", width: 20 },
            { name: "City/Province", width: 20 },
            { name: "Country", width: 20 },
            { name: "Status", width: 20 }
        ]
        exportModel.data = warehouseData;
        exportModel.fileName = "Warehouse";

        this.excelService.generateExcel(exportModel);
    }
}
