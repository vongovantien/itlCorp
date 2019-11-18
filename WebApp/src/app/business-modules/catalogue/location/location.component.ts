import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import _findIndex from 'lodash/findIndex';
import _map from 'lodash/map';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import * as dataHelper from 'src/helper/data.helper';
import { SystemConstants } from 'src/constants/system.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ActivatedRoute } from '@angular/router';
import { AddCountryComponent } from './country/add-country/add-country.component';
import { AddProvinceComponent } from './province/add-province/add-province.component';
import { UpdateProvinceComponent } from './province/update-province/update-province.component';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { UpdateCountryComponent } from './country/update-country/update-country.component';
import { AddDistrictComponent } from './district/add-district/add-district.component';
import { UpdateDistrictComponent } from './district/update-district/update-district.component';
import { AddWardComponent } from './ward/add-ward/add-ward.component';
import { UpdateWardComponent } from './ward/update-ward/update-ward.component';
declare var $: any;

@Component({
	selector: 'app-location',
	templateUrl: './location.component.html'
})
export class LocationComponent implements OnInit, AfterViewInit {
	@ViewChild(AddCountryComponent, { static: false }) addCountryPopup: AddCountryComponent;
	@ViewChild(AddProvinceComponent, { static: false }) addProvincePopup: AddProvinceComponent;
	@ViewChild(AddDistrictComponent, { static: false }) addDistrictPopup: AddDistrictComponent;
	@ViewChild(AddWardComponent, { static: false }) addWardPopup: AddWardComponent;
	@ViewChild(UpdateProvinceComponent, { static: false }) editProvincePopup: UpdateProvinceComponent;
	@ViewChild(UpdateCountryComponent, { static: false }) editCountryPopup: UpdateCountryComponent;
	@ViewChild(UpdateDistrictComponent, { static: false }) editDistrictPopup: UpdateDistrictComponent;
	@ViewChild(UpdateWardComponent, { static: false }) editWardPopup: UpdateWardComponent;
	ngAfterViewInit(): void { }

    /**
     *START VARIABLES DEFINITIONS
     */
	ListCountries: any = [];
	ConstListCountries: any = [];
	indexCountryDelete: Number = -1;
	indexCountryUpdate: Number = -1;
	ObjectToDelete: String = "";
	deleteWhat: any = null;
	idCountryToDelete: any = null;

	ListProvinceCities: any = [];
	ConstListProvinceCities: any = [];

	ListDistricts: any = [];
	ConstListDistrict: any = [];

	ListWards: any = [];
	ConstListWards: any = [];

	importButtonSetting: ButtonModalSetting = {
		typeButton: ButtonType.import
	};
	exportButtonSetting: ButtonModalSetting = {
		typeButton: ButtonType.export
	};

	pager: PagerSetting = PAGINGSETTING;

	searchKey: string = "";

	searchKeyCountryTab: string = "";
	searchKeyProvinceTab: string = "";
	searchKeyDistrictTab: string = "";
	searchKeyWardTab: string = "";
	CURRENT_LANGUAGE = localStorage.getItem(SystemConstants.CURRENT_LANGUAGE);
	// idDistrictToUpdate: string = "";

	fieldNameByLanguage(field) {
		if (this.CURRENT_LANGUAGE.toLowerCase() === SystemConstants.DEFAULT_LANGUAGE.toLowerCase()) {
			return field + "EN";
		} else {
			return field + "VN";
		}
	}

	listFilterCountryTab = [
		{ filter: "All", field: "all" }, { filter: "Code", field: "code" },
		{ filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" }];
	selectedFilterCountryTab = this.listFilterCountryTab[0].filter;

	listFilterProvinceCityTab = [
		{ filter: "All", field: "all" }, { filter: "Code", field: "code" },
		{ filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" },
		{ filter: "Country", field: this.fieldNameByLanguage("countryName") }];
	selectedFilterProvinceCityTab = this.listFilterProvinceCityTab[0].filter;

	listFilterDistrictTab = [
		{ filter: "All", field: "all" }, { filter: "Code", field: "code" },
		{ filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" },
		{ filter: "Country", field: this.fieldNameByLanguage("countryName") }, { filter: "Province-City", field: this.fieldNameByLanguage("provinceName") }];
	selectedFilterDistrictTab = this.listFilterDistrictTab[0].filter;

	listFilterWardTab = [
		{ filter: "All", field: "all" }, { filter: "Code", field: "code" },
		{ filter: "English Name", field: "nameEn" }, { filter: "Local Name", field: "nameVn" },
		{ filter: "Country", field: this.fieldNameByLanguage("countryName") }, { filter: "Province-City", field: this.fieldNameByLanguage("provinceName") }, { filter: "District", field: this.fieldNameByLanguage("districtName") }];
	selectedFilterWardTab = this.listFilterWardTab[0].filter;

	countrySearchObject = {
		code: "",
		nameVn: "",
		nameEn: "",
		condition: "OR",
	}
	searchObject: any = { condition: "OR" };
	@ViewChild(AppPaginationComponent, { static: false }) child;

	/**
	 * END OF VARIABLES DEFINITIONS
	 */

	constructor(
		private route: ActivatedRoute,
		private excelService: ExcelService,
		private baseServices: BaseService,
		private api_menu: API_MENU,
		private sortService: SortService,
		private catalogueRepo: CatalogueRepo) {

	}

	showAddProvince() {
		this.addProvincePopup.show();
	}
	ngSelectData(sourceData: any) {
		var dataReturn: any = null;
		var isCountry: Boolean = false;

		if (sourceData[0].placeTypeID == undefined) {
			isCountry = true;
		}

		if (this.CURRENT_LANGUAGE.toLowerCase() == SystemConstants.DEFAULT_LANGUAGE.toLowerCase()) {
			if (isCountry) {
				dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.nameEn, "id": x.id }));
			} else {
				dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.nameEn, "id": x.id }));
			}
		} else {
			if (isCountry) {
				dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.nameVn, "id": x.id }));
			} else {
				dataReturn = sourceData.map(x => ({ "text": x.code + " - " + x.nameVn, "id": x.id }));
			}
		}
		return dataReturn;
	}

	async ngOnInit() {
		this.pager.totalItems = 0;
		await this.getCountries();
		this.getProvinceCities();
		this.getDistrict();
		this.getWards();
		this.getAllCountries();
		this.getAllProvinces();
		this.getAllDistricts();
	}
	activeTab: string = "country";
	changeTab(activeTab) {
		this.activeTab = activeTab;
		if (activeTab === "country") {
			this.pager.currentPage = 1;
			this.pager.totalItems = this.totalItemCountries;
			this.child.setPage(this.pager.currentPage);
		}
		if (activeTab === "province") {
			this.pager.currentPage = 1;
			this.pager.totalItems = this.totalItemProvinces;
			this.child.setPage(this.pager.currentPage);
		}
		if (activeTab === "district") {
			this.pager.currentPage = 1;
			this.pager.totalItems = this.totalItemDistricts;
			this.child.setPage(this.pager.currentPage);
		}
		if (activeTab === "ward") {
			this.pager.currentPage = 1;
			this.pager.totalItems = this.totalItemWards;
			this.child.setPage(this.pager.currentPage);
		}
	}


	async setPage(pager: PagerSetting) {
		this.pager.currentPage = pager.currentPage;
		this.pager.totalPages = pager.totalPages;
		this.pager.pageSize = pager.pageSize
		if (this.activeTab === "country") {
			this.ListCountries = await this.getCountries();
		}
		if (this.activeTab === "province") {
			this.ListProvinceCities = await this.getProvinceCities();
		}
		if (this.activeTab === "district") {
			this.ListDistricts = await this.getDistrict();
		}
		if (this.activeTab === "ward") {
			this.ListWards = await this.getWards();
		}

	}

	setPageAfterAdd() {
		this.child.setPage(this.pager.currentPage);
		if (this.pager.currentPage < this.pager.totalPages) {
			this.pager.currentPage = this.pager.totalPages;
			this.child.setPage(this.pager.currentPage);
		}
	}

	setPageAfterDelete() {
		this.child.setPage(this.pager.currentPage);
		if (this.pager.currentPage > this.pager.totalPages) {
			this.pager.currentPage = this.pager.totalPages;
			this.child.setPage(this.pager.currentPage);
		}
	}





	/**
	 * BEGIN COUNTRY METHODS
	 */

	async searchInCountryTab() {
		this.searchObject = {};
		if (this.selectedFilterCountryTab == "All") {
			this.searchObject.condition = "OR";
			for (var i = 1; i < this.listFilterCountryTab.length; i++) {
				this.searchObject[this.listFilterCountryTab[i].field] = this.searchKeyCountryTab;
				// eval("this.searchObject[this.listFilterCountryTab[i].field]=this.searchKeyCountryTab");
			}

		} else {

			this.searchObject.condition = "AND";
			for (var i = 1; i < this.listFilterCountryTab.length; i++) {
				console.log(this.listFilterCountryTab[i].field);
				if (this.selectedFilterCountryTab == this.listFilterCountryTab[i].filter) {
					this.searchObject[this.listFilterCountryTab[i].field] = this.searchKeyCountryTab;
					// eval("this.searchObject[this.listFilterCountryTab[i].field]=this.searchKeyCountryTab");
				}

			}
		}
		await this.getCountries();
	}

	async resetCountryTab() {
		this.pager.totalItems = 0;
		this.pager.currentPage = 1;
		this.searchKeyCountryTab = "";
		this.searchObject = {};
		this.selectedFilterCountryTab = this.listFilterCountryTab[0].filter;
		await this.getCountries();
	}

	totalItemCountries: number = null;
	totalItemProvinces: number = null;
	totalItemDistricts: number = null;
	totalItemWards: number = null;

	async getCountries() {
		const response = await this.baseServices.postAsync(this.api_menu.Catalogue.Country.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject, false, true);
		this.ListCountries = response.data;
		this.totalItemCountries = response.totalItems;
		this.pager.totalItems = this.totalItemCountries;
		this.ConstListCountries = response.data;
		return response.data;
	}
	showAddCountryPopup() {
		this.addCountryPopup.show();
	}

	async showUpdateCountry(id) {
		let countryToUpdate = null;
		// this.CountryToUpdate = await this.baseServices.getAsync(this.api_menu.Catalogue.Country.getById + id, true, true);
		this.catalogueRepo.getDetailCountry(id)
			.pipe(
				finalize(() => {
					if (countryToUpdate != null) {
						this.editCountryPopup.currentId = id;
						this.editCountryPopup.countryToUpdate = countryToUpdate;
						this.editCountryPopup.setValueFormGroup(countryToUpdate);
						this.editCountryPopup.show();
					}
				})
			).subscribe(
				(res: any) => {
					countryToUpdate = res;
				});
	}

	prepareDeleteCountry(id) {
		this.idCountryToDelete = id;
		this.ObjectToDelete = "country";
	}

	async delete(action) {
		if (action == "yes") {
			if (this.ObjectToDelete == "country") {
				await this.baseServices.deleteAsync(this.api_menu.Catalogue.Country.delete + this.idCountryToDelete);
				await this.getCountries();
				this.setPageAfterDelete();
			}

			if (this.ObjectToDelete == "province-city") {
				await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete + this.idProvinceCityToDelete);
				await this.getProvinceCities();
				this.setPageAfterDelete();
			}

			if (this.ObjectToDelete == "district") {
				await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete + this.idDistrictToDelete);
				await this.getDistrict();
				this.setPageAfterDelete();
			}

			if (this.ObjectToDelete == "ward") {
				await this.baseServices.deleteAsync(this.api_menu.Catalogue.CatPlace.delete + this.idWardToDelete);
				await this.getWards();
				this.setPageAfterDelete();
			}

		}
	}




	/**
	 * END COUNTRY METHODS 
	 */


	/**
	 * START PROVINCE-CITY METHODS
	 */

	async searchInProvinceCityTab() {
		this.pager.currentPage = 1;
		this.searchObject = {};
		this.searchObject.placeType = PlaceTypeEnum.Province; //9;
		if (this.selectedFilterProvinceCityTab == "All") {
			this.searchObject.All = this.searchKeyProvinceTab;
		} else {
			this.searchObject = {};
			for (var i = 1; i < this.listFilterProvinceCityTab.length; i++) {
				if (this.selectedFilterProvinceCityTab == this.listFilterProvinceCityTab[i].filter) {
					this.searchObject[this.listFilterProvinceCityTab[i].field] = this.searchKeyProvinceTab;
					// eval("this.searchObject[this.listFilterProvinceCityTab[i].field]=this.searchKeyProvinceTab");
				}

			}
		}
		await this.getProvinceCities();
	}

	async resetProvinceCityTab() {
		this.pager.totalItems = 0;
		this.pager.currentPage = 1;
		this.searchKeyProvinceTab = "";
		this.searchObject = {};
		this.selectedFilterProvinceCityTab = this.listFilterProvinceCityTab[0].filter;
		await this.getProvinceCities();
	}

	async getProvinceCities() {
		this.searchObject.placeType = PlaceTypeEnum.Province; //9;
		const response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject);
		this.ListProvinceCities = response.data;
		this.ConstListProvinceCities = response.data;
		this.totalItemProvinces = response.totalItems;
		this.pager.totalItems = this.totalItemProvinces;
		return response.data;
	}

	async showUpdateProvince(id) {
		let provinceCityToUpdate = null;
		this.catalogueRepo.getDetailPlace(id)
			.pipe(
				finalize(() => {
					if (provinceCityToUpdate != null) {
						const countryId = provinceCityToUpdate.countryId;
						const indexCurrentCountry = _findIndex(this.ngSelectDataCountries, function (o) {
							return o['id'] === countryId;
						});
						if (indexCurrentCountry > -1) {
							this.editProvincePopup.currentActiveCountry = [this.ngSelectDataCountries[indexCurrentCountry]];
						}
						this.editProvincePopup.currentId = id;
						this.editProvincePopup.provinceCityToUpdate = provinceCityToUpdate;
						this.editProvincePopup.setValueFormGroup(this.editProvincePopup.provinceCityToUpdate);
						this.editProvincePopup.show();
					}
				})
			).subscribe(
				(res: any) => {
					provinceCityToUpdate = res;
				});
	}

	idProvinceCityToDelete: string = "";
	prepareDeleteProvince(id) {
		this.idProvinceCityToDelete = id;
		this.ObjectToDelete = "province-city";
	}


	/**
	 * END PROVINCE-CITY METHODS
	 */


	/**
	 * START DISTRICT METHODS
	 */


	async searchInDistrictTab() {

		this.searchObject = {};
		this.searchObject.placeType = PlaceTypeEnum.District; //9;
		if (this.selectedFilterDistrictTab == "All") {
			this.searchObject.All = this.searchKeyDistrictTab;
		} else {
			this.searchObject = {};
			for (var i = 1; i < this.listFilterDistrictTab.length; i++) {
				console.log(this.listFilterDistrictTab[i].field);
				if (this.selectedFilterDistrictTab == this.listFilterDistrictTab[i].filter) {
					this.searchObject[this.listFilterDistrictTab[i].field] = this.searchKeyDistrictTab;
					// eval("this.searchObject[this.listFilterDistrictTab[i].field]=this.searchKeyDistrictTab");
				}

			}
		}
		await this.getDistrict();
	}

	async resetDistrictTab() {
		this.pager.totalItems = 0;
		this.pager.currentPage = 1;
		this.searchKeyDistrictTab = "";
		this.searchObject = {};
		this.selectedFilterDistrictTab = this.listFilterDistrictTab[0].filter;
		await this.getDistrict();
	}

	public async getDistrict() {
		this.searchObject.placeType = PlaceTypeEnum.District;
		const response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject);
		this.ListDistricts = response.data;
		this.totalItemDistricts = response.totalItems;
		this.pager.totalItems = this.totalItemDistricts;
		return response.data;
	}

	async showUpdateDistrict(id) {
		let districtToUpdate = null;
		this.catalogueRepo.getDetailPlace(id)
			.pipe(
				finalize(() => {
					if (districtToUpdate != null) {
						const countryId = districtToUpdate.countryId;
						const provinceId = districtToUpdate.provinceId;
						this.editDistrictPopup.getProvinceByCountry(countryId);
						const indexCurrentCountry = _findIndex(this.ngSelectDataCountries, function (o) {
							return o['id'] === countryId;
						});
						if (indexCurrentCountry > -1) {
							this.editDistrictPopup.currentActiveCountry = [this.ngSelectDataCountries[indexCurrentCountry]];
						}
						const indexCurrentProvince = _findIndex(this.editDistrictPopup.ngSelectDataProvinces, function (o) {
							return o['id'] === provinceId;
						});
						if (indexCurrentProvince > -1) {
							this.editDistrictPopup.currentActiveProvince = [this.editDistrictPopup.ngSelectDataProvinces[indexCurrentProvince]];
						}
						this.editDistrictPopup.currentId = id;
						this.editDistrictPopup.ngSelectDataCountries = this.ngSelectDataCountries;
						this.editDistrictPopup.districtToUpdate = districtToUpdate;
						this.editDistrictPopup.setValueFormGroup(districtToUpdate);
						this.editDistrictPopup.show();
					}
				})
			).subscribe(
				(res: any) => {
					districtToUpdate = res;
				});
	}
	idDistrictToDelete: string = "";
	prepareDeleteDistrict(id) {
		this.idDistrictToDelete = id;
		this.ObjectToDelete = "district";
	}


	ngSelectDataCountries: any[] = [];
	ngSelectDataProvinces: any = [];
	ngSelectDataDistricts: any = [];

	async getAllCountries() {
		const countries = await this.baseServices.getAsync(this.api_menu.Catalogue.Country.getAll, false, false);
		this.ngSelectDataCountries = this.ngSelectData(countries);
		this.addProvincePopup.ngSelectDataCountries = this.ngSelectDataCountries;
		this.editProvincePopup.ngSelectDataCountries = this.ngSelectDataCountries;

		this.addDistrictPopup.ngSelectDataCountries = this.ngSelectDataCountries;
		this.addDistrictPopup.ngSelectDataProvinces = [];
		this.editDistrictPopup.ngSelectDataCountries = this.ngSelectDataCountries;

		this.addWardPopup.ngSelectDataCountries = this.ngSelectDataCountries;
		this.addWardPopup.ngSelectDataProvinces = [];
		this.addWardPopup.ngSelectDataDistricts = [];
		this.editDistrictPopup.ngSelectDataCountries = this.ngSelectDataCountries;
	}

	async getAllProvinces() {
		let searchObj = {
			placeType: PlaceTypeEnum.Province
		};
		let provinces = [];
		this.catalogueRepo.getPlace(searchObj)
			.pipe(
				finalize(() => {
					this.addDistrictPopup.provinces = provinces;
					this.editDistrictPopup.provinces = provinces;
					this.addWardPopup.provinces = provinces;
					this.editWardPopup.provinces = provinces;
				})
			).subscribe(
				(res: any) => {
					provinces = res;
				});
	}

	async getAllDistricts() {
		let searchObj = {
			placeType: PlaceTypeEnum.District
		};
		let districts = [];
		this.catalogueRepo.getPlace(searchObj)
			.pipe(
				finalize(() => {
					this.addWardPopup.districts = districts;
					this.editWardPopup.districts = districts;
				})
			).subscribe(
				(res: any) => {
					districts = res;
				});
	}

	/**
	 * END DISTRICT METHODS
	 */

	/**
	 * BEGIN WARD METHODS
	 */
	async searchInWardTab() {

		this.searchObject = {};
		this.searchObject.placeType = PlaceTypeEnum.Ward; //9;
		if (this.selectedFilterWardTab == "All") {
			this.searchObject.All = this.searchKeyWardTab;
		} else {
			this.searchObject = {};
			for (var i = 1; i < this.listFilterWardTab.length; i++) {
				if (this.selectedFilterWardTab == this.listFilterWardTab[i].filter) {
					this.searchObject[this.listFilterWardTab[i].field] = this.searchKeyWardTab;
					// eval("this.searchObject[this.listFilterWardTab[i].field]=this.searchKeyWardTab");
				}

			}
		}
		console.log(this.searchObject);
		await this.getWards();
	}

	async resetWardTab() {
		this.pager.totalItems = 0;
		this.pager.currentPage = 1;
		this.searchKeyWardTab = "";
		this.searchObject = {};
		this.selectedFilterWardTab = this.listFilterWardTab[0].filter;
		await this.getWards();
	}


	public async getWards() {
		this.searchObject.placeType = PlaceTypeEnum.Ward;
		const response = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + this.pager.currentPage + "&size=" + this.pager.pageSize, this.searchObject);
		this.ListWards = response.data;
		this.ConstListWards = response.data;
		this.totalItemWards = response.totalItems;
		this.pager.totalItems = this.totalItemWards;
		return response.data;
	}

	async showUpdateWard(id) {
		let wardToUpdate = null;
		this.catalogueRepo.getDetailPlace(id)
			.pipe(
				finalize(() => {
					if (wardToUpdate != null) {
						this.editWardPopup.currentId = id;
						this.editWardPopup.ngSelectDataCountries = this.ngSelectDataCountries;
						this.editWardPopup.wardToUpdate = wardToUpdate;

						const countryId = wardToUpdate.countryId;
						const provinceId = wardToUpdate.provinceId;
						const districtId = wardToUpdate.districtId;

						const indexCurrentCountry = _findIndex(this.ngSelectDataCountries, function (o) {
							return o['id'] === countryId;
						});
						if (indexCurrentCountry > -1) {
							this.editWardPopup.currentActiveCountry = [this.ngSelectDataCountries[indexCurrentCountry]];
						}
						this.editWardPopup.getProvinceByCountry(countryId);
						var indexCurrentProvince = _findIndex(this.editWardPopup.ngSelectDataProvinces, function (o) {
							return o['id'] == provinceId;
						});
						if (indexCurrentProvince > -1) {
							this.editWardPopup.currentActiveProvince = [this.editWardPopup.ngSelectDataProvinces[indexCurrentProvince]];
						}
						this.editWardPopup.getWardByProvince(provinceId);
						var indexCurrentDistrict = _findIndex(this.editWardPopup.ngSelectDataDistricts, function (o) {
							return o['id'] == districtId;
						});
						if (indexCurrentDistrict > -1) {
							this.editWardPopup.currentActiveDistrict = [this.editWardPopup.ngSelectDataDistricts[indexCurrentDistrict]];
						}
						this.editWardPopup.setValueFormGroup(wardToUpdate);
						this.editWardPopup.show();
					}
				})
			).subscribe(
				(res: any) => {
					wardToUpdate = res;
				});
	}
	idWardToDelete: any = null;
	prepareDeleteWard(id) {
		this.idWardToDelete = id;
		this.ObjectToDelete = "ward";
	}





	/**
	 * END WARD METHODS
	 */


	private value: any = {};
	private _disabledV: string = '0';
	private disabled: boolean = false;
	isDesc = true;
	sortKey: string = "code";
	sort(property) {
		this.isDesc = !this.isDesc;
		this.sortKey = property;
		if (this.activeTab === "country") {
			this.ListCountries = this.sortService.sort(this.ListCountries, property, this.isDesc);
		}
		if (this.activeTab === "province") {
			this.ListProvinceCities = this.sortService.sort(this.ListProvinceCities, property, this.isDesc);
		}
		if (this.activeTab === "district") {
			this.ListDistricts = this.sortService.sort(this.ListDistricts, property, this.isDesc);
		}
		if (this.activeTab === "ward") {
			this.ListWards = this.sortService.sort(this.ListWards, property, this.isDesc);
		}
	}



	async import() {

	}

	async export() {
		if (this.activeTab === 'country') {
			await this.exportCountry();
		}
		if (this.activeTab === 'province') {
			await this.exportProvince();
		}
		if (this.activeTab === 'district') {
			await this.exportDistrict();
		}
		if (this.activeTab === 'ward') {
			await this.exportTownWard()
		}
	}

	async exportCountry() {
		var countries = await this.baseServices.postAsync(this.api_menu.Catalogue.Country.query, this.searchObject);
		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
			countries = _map(countries, function (country, index) {
				return [
					index + 1,
					country['code'],
					country['nameEn'],
					country['nameVn'],
					(country['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH
				]
			});
		}

		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
			countries = _map(countries, function (country, index) {
				return [
					index + 1,
					country['code'],
					country['nameEn'],
					country['nameVn'],
					(country['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM
				]
			});
		}

		/**Set up stylesheet */
		var exportModel: ExportExcel = new ExportExcel();
		exportModel.fileName = "Countries List";
		const currrently_user = localStorage.getItem('currently_userName');
		exportModel.title = "Countries List";
		exportModel.author = currrently_user;
		exportModel.sheetName = "Sheet 1";
		exportModel.header = [
			{ name: "No.", width: 10 },
			{ name: "Country Code", width: 10 },
			{ name: "English Name", width: 20 },
			{ name: "Local Name", width: 20 },
			{ name: "Status", width: 20 }
		]
		exportModel.data = countries;
		this.excelService.generateExcel(exportModel);
	}


	async exportProvince() {
		this.searchObject.placeType = PlaceTypeEnum.Province;
		var provinces = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, this.searchObject);
		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
			provinces = _map(provinces, function (province, index) {
				return [
					index + 1,
					province['code'],
					province['nameEn'],
					province['nameVn'],
					province['countryNameEN'],
					(province['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH
				]
			});
		}

		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
			provinces = _map(provinces, function (province, index) {
				return [
					index + 1,
					province['code'],
					province['nameEn'],
					province['nameVn'],
					province['countryNameVN'],
					(province['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM
				]
			});
		}

		/**Set up stylesheet */
		var exportModel: ExportExcel = new ExportExcel();
		exportModel.fileName = "Provinces List";
		const currrently_user = localStorage.getItem('currently_userName');
		exportModel.title = "Provinces List";
		exportModel.author = currrently_user;
		exportModel.sheetName = "Sheet 1";
		exportModel.header = [
			{ name: "No.", width: 10 },
			{ name: "Province Code", width: 20 },
			{ name: "English Name", width: 20 },
			{ name: "Local Name", width: 20 },
			{ name: "Country", width: 20 },
			{ name: "Status", width: 20 }
		]
		exportModel.data = provinces;
		this.excelService.generateExcel(exportModel);

	}

	async exportDistrict() {
		this.searchObject.placeType = PlaceTypeEnum.District;
		var districts = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, this.searchObject);
		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
			districts = _map(districts, function (dist, index) {
				return [
					index + 1,
					dist['code'],
					dist['nameEn'],
					dist['nameVn'],
					dist['provinceNameEN'],
					dist['countryNameEN'],
					(dist['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH
				]
			});
		}

		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
			districts = _map(districts, function (dist, index) {
				return [
					index + 1,
					dist['code'],
					dist['nameEn'],
					dist['nameVn'],
					dist['provinceNameVN'],
					dist['countryNameVN'],
					(dist['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM
				]
			});
		}

		/**Set up stylesheet */
		var exportModel: ExportExcel = new ExportExcel();
		exportModel.fileName = "Districts List";
		const currrently_user = localStorage.getItem('currently_userName');
		exportModel.title = "Districts List";
		exportModel.author = currrently_user;
		exportModel.sheetName = "Sheet 1";
		exportModel.header = [
			{ name: "No.", width: 10 },
			{ name: "District Code", width: 20 },
			{ name: "English Name", width: 20 },
			{ name: "Local Name", width: 20 },
			{ name: "Province", width: 20 },
			{ name: "Country", width: 20 },
			{ name: "Status", width: 20 }
		]
		exportModel.data = districts;
		this.excelService.generateExcel(exportModel);

	}


	async exportTownWard() {
		this.searchObject.placeType = PlaceTypeEnum.Ward;
		var wards = await this.baseServices.postAsync(this.api_menu.Catalogue.CatPlace.query, this.searchObject);
		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
			wards = _map(wards, function (ward, index) {
				return [
					index + 1,
					ward['code'],
					ward['nameEn'],
					ward['nameVn'],
					ward['districtNameEN'],
					ward['provinceNameEN'],
					ward['countryNameEN'],
					(ward['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH
				]
			});
		}

		if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
			wards = _map(wards, function (ward, index) {
				return [
					index + 1,
					ward['code'],
					ward['nameEn'],
					ward['nameVn'],
					ward['districtNameVN'],
					ward['provinceNameVN'],
					ward['countryNameVN'],
					(ward['active'] === true) ? SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM
				]
			});
		}

		/**Set up stylesheet */
		var exportModel: ExportExcel = new ExportExcel();
		exportModel.fileName = "Town-Ward List";
		const currrently_user = localStorage.getItem('currently_userName');
		exportModel.title = "Town-Ward List";
		exportModel.author = currrently_user;
		exportModel.sheetName = "Sheet 1";
		exportModel.header = [
			{ name: "No.", width: 10 },
			{ name: "Town-Ward Code", width: 20 },
			{ name: "English Name", width: 20 },
			{ name: "Local Name", width: 20 },
			{ name: "District", width: 20 },
			{ name: "Province", width: 20 },
			{ name: "Country", width: 20 },
			{ name: "Status", width: 20 }
		]
		exportModel.data = wards;
		this.excelService.generateExcel(exportModel);

	}
	async saveProvince(event) {
		if (event) {
			this.ListProvinceCities = await this.getProvinceCities();
		}
	}
	async saveCountry(event) {
		if (event == true) {
			this.ListCountries = await this.getCountries();
		}
	}
	async saveDistrict(event) {
		if (event == true) {
			this.ListDistricts = await this.getDistrict();
		}
	}
	async saveWard(event) {
		if (event == true) {
			this.ListWards = await this.getWards();
		}
	}
	showAddDistrict() {
		this.addDistrictPopup.show();
	}
	showAddWard() {
		this.addWardPopup.show();
	}
}
