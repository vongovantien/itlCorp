import { Component, OnInit, ViewChild } from '@angular/core';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { CommodityGroup } from 'src/app/shared/models/catalogue/commonity-group.model';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { COMMODITYGROUPCOLUMNSETTING } from './commonity-group.column';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgForm } from '@angular/forms';
import { Commodity } from 'src/app/shared/models/catalogue/commodity.model';
import { COMMODITYCOLUMNSETTING } from './commodity.column';
import { SelectComponent } from 'ng2-select';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { ExcelService } from 'src/app/shared/services/excel.service';
import _map from 'lodash/map';
import { SystemConstants } from 'src/constants/system.const';
import { SearchOptionsComponent } from '../../../shared/common/search-options/search-options.component';
import { CatalogueRepo } from 'src/app/shared/repositories/catalogue.repo';
import { SortService } from 'src/app/shared/services/sort.service';
import { ToastrService } from 'ngx-toastr';

declare var $: any;

@Component({
  selector: 'app-commodity',
  templateUrl: './commodity.component.html',
})
export class CommodityComponent implements OnInit {

  /*
  declare variable
  */
  @ViewChild(AppPaginationComponent, { static: false }) child;
  @ViewChild(SearchOptionsComponent, { static: false }) searchOption;
  @ViewChild('formCommodity', { static: false }) formCommodity: NgForm;
  @ViewChild('formGroupCommodity', { static: false }) formGroupCommodity: NgForm;
  @ViewChild('chooseGroup', { static: false }) public groupSelect: SelectComponent;
  commodities: Array<Commodity>;
  commodity: Commodity;
  commodityGroups: Array<CommodityGroup>;
  commodityGroup: CommodityGroup;
  pager: PagerSetting = PAGINGSETTING;
  commoditySettings: ColumnSetting[] = COMMODITYCOLUMNSETTING;
  commodityGroupSettings: ColumnSetting[] = COMMODITYGROUPCOLUMNSETTING;
  groups: any[];
  criteria: any = {};
  tabName = {
    commodity: "commodity",
    commodityGroup: "commodityGroup"
  };
  activeTab: string = this.tabName.commodity;
  groupActive: any[] = [];
  addCommodityButtonSetting: ButtonModalSetting = {
    dataTarget: "edit-commodity-modal",
    typeButton: ButtonType.add
  };
  addGroupButtonSetting: ButtonModalSetting = {
    dataTarget: "edit-commodity-group-modal",
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
  configSearchGroup: any = {
    settingFields: this.commodityGroupSettings.filter(x => x.allowSearch == true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
    typeSearch: TypeSearch.intab,
    searchString: ''
  };
  configSearchCommonity: any = {
    settingFields: this.commoditySettings.filter(x => x.allowSearch == true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
    typeSearch: TypeSearch.intab,
    searchString: ''
  };
  isDesc = false;
  /*
  end declare variable
  */

  constructor(private baseService: BaseService,
    private excelService: ExcelService,
    private api_menu: API_MENU,
    private sortService: SortService,
    private catalogueRepo: CatalogueRepo,
    private _toastService: ToastrService, ) {
  }

  ngOnInit() {
    this.pager.totalItems = 0;
    this.getCommodities(this.pager);
    this.getGroups();
  }

  getGroups() {
    this.catalogueRepo.getAllCommodityGroup().subscribe(
      (data: any) => {
        this.groups = data.map(x => ({ "text": x.groupName, "id": x.id }));
      },
    );
  }

  setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    if (this.activeTab == this.tabName.commodityGroup) {
      this.getGroupCommodities(pager);
    }
    if (this.activeTab == this.tabName.commodity) {
      this.getCommodities(pager);
    }
  }
  tabSelect(tabName: string) {
    this.activeTab = tabName;
    this.pager.currentPage = 1;
    this.pager.pageSize = 15;
    this.pager.totalItems = 0;
    this.resetSearch({ field: "All", fieldDisplayName: "All" }, tabName);
  }

  getCommodities(pager: PagerSetting) {
    this.catalogueRepo.getCommodityPaging(pager.currentPage, pager.pageSize, Object.assign({}, this.criteria))
      .subscribe(
        (res: any) => {
          this.pager.totalItems = res.totalItems || 0;
          this.commodities = res.data;
        },
      );
  }

  getGroupCommodities(pager: PagerSetting) {
    this.catalogueRepo.getCommodityGroupPaging(pager.currentPage, pager.pageSize, Object.assign({}, this.criteria))
      .subscribe(
        (res: any) => {
          this.pager.totalItems = res.totalItems || 0;
          this.commodityGroups = res.data;
        },
      );
  }

  onSearch(event, tabName) {
    if (this.activeTab == this.tabName.commodityGroup) {
      this.searchCommodityGroup(event);
    }
    if (this.activeTab == this.tabName.commodity) {
      this.searchCommodity(event);
    }
  }
  searchCommodity(event: any): any {
    if (event.field == "All") {
      this.criteria.all = event.searchString;
    }
    else {
      this.criteria = {};
      if (event.field == "commodityNameEn") {
        this.criteria.commodityNameEn = event.searchString;
      }
      if (event.field == "commodityNameVn") {
        this.criteria.commodityNameVn = event.searchString;
      }
      if (event.field == "commodityGroupNameEn") {
        this.criteria.commodityGroupNameEn = event.searchString;
      }
      if (event.field == "code") {
        this.criteria.code = event.searchString;
      }
    }
    this.pager.totalItems = 0;
    this.getCommodities(this.pager);
  }
  searchCommodityGroup(event: any): any {
    if (event.searchString == "") {
      event.searchString = null;
    }
    if (event.field == "All") {
      this.criteria.all = event.searchString;
    }
    else {
      this.criteria = {};
      if (event.field == "groupNameEn") {
        this.criteria.groupNameEn = event.searchString;
      }
      if (event.field == "groupNameVn") {
        this.criteria.groupNameVn = event.searchString;
      }
    }
    this.pager.currentPage = 1;
    this.getGroupCommodities(this.pager);
  }
  resetSearch(event, tabName) {
    this.criteria = {};
    if (tabName == this.tabName.commodityGroup) {
      this.searchCommodityGroup(event);
    }
    if (tabName == this.tabName.commodity) {
      this.searchCommodity(event);
    }
  }

  onSortChange(column) {
    let property = column.primaryKey;

    this.isDesc = !this.isDesc;
    if (this.activeTab == this.tabName.commodity) {
      this.commodities = this.sortService.sort(this.commodities, property, this.isDesc);
    }
    if (this.activeTab == this.tabName.commodityGroup) {
      this.commodityGroups = this.sortService.sort(this.commodityGroups, property, this.isDesc);
    }
  }
  showDetail(item, tabName) {
    if (tabName == this.tabName.commodityGroup) {
      this.commodityGroup = item;
      this.catalogueRepo.getDetailCommodityGroup(item.id).subscribe(
        (res: any) => {
          console.log(res)
          if (res.id !== 0) {
            this.commodityGroup = res;
          } else {
            this._toastService.error("Not found data");
          }
        },
      );
    }
    if (tabName == this.tabName.commodity) {
      this.commodity = item;
      this.catalogueRepo.getDetailCommodity(item.id).subscribe(
        (res: any) => {
          console.log(res)
          if (res.id !== 0) {
            this.commodity = res;
            let index = this.groups.findIndex(x => x.id == this.commodity.commodityGroupId);
            if (index > -1) {
              this.groupActive = [this.groups[index]];
            }
          } else {
            this._toastService.error("Not found data");
          }
        },
      );

    }
  }
  onDelete(event) {
    if (event) {
      if (this.commodityGroup) {
        this.deleteGroupCommodity();
      }
      if (this.commodity) {
        this.deleteCommodity();
      }
    }
  }

  deleteCommodity() {
    this.catalogueRepo.deleteCommodity(this.commodity.id).subscribe(
      (res: CommonInterface.IResult) => {
        if (res.status) {
          this._toastService.success(res.message, '');
          this.getCommodities(this.pager);
          //this.setPageAfterDelete();
        } else {
          this._toastService.error(res.message || 'Có lỗi xảy ra', '');
        }
      },
    );
  }

  deleteGroupCommodity() {
    this.catalogueRepo.deleteCommodityGroup(this.commodityGroup.id).subscribe(
      (res: CommonInterface.IResult) => {
        if (res.status) {
          this._toastService.success(res.message, '');
          this.getGroups();
          this.getGroupCommodities(this.pager);
          //this.setPageAfterDelete();
        } else {
          this._toastService.error(res.message || 'Có lỗi xảy ra', '');
        }
      },
    );
  }
  setPageAfterDelete() {
    this.pager.totalItems = this.pager.totalItems - 1;
    let totalPages = Math.ceil(this.pager.totalItems / this.pager.pageSize);
    if (totalPages < this.pager.totalPages) {
      this.pager.currentPage = totalPages;
    }
    this.child.setPage(this.pager.currentPage);
  }
  setPageAfterAdd() {
    this.child.setPage(this.pager.currentPage);
    if (this.pager.currentPage < this.pager.totalPages) {
      this.pager.currentPage = this.pager.totalPages;
      this.child.setPage(this.pager.currentPage);
    }
  }
  showAdd(tabName) {
    if (tabName == this.tabName.commodityGroup) {
      this.commodityGroup = new CommodityGroup();
    }
    if (tabName == this.tabName.commodity) {
      this.commodity = new Commodity();
      this.commodity.commodityGroupId = null;
    }
  }
  onSubmit() {
    if (this.formGroupCommodity) {
      this.saveGroupCommodity();
    }
    if (this.formCommodity) {
      this.saveCommodity();
    }
  }
  saveCommodity(): any {
    if (this.formCommodity.valid && this.commodity.commodityGroupId != null) {
      if (this.commodity.id == null) {
        this.addNewCommodity();
      }
      else {
        this.updateCommodity();
      }
    }
  }
  saveGroupCommodity() {
    if (this.formGroupCommodity.valid) {
      if (this.commodityGroup.id == null) {
        this.addNewGroup();
      }
      else {
        this.updateGroup();
      }
    }
  }

  updateCommodity() {
    this.catalogueRepo.updateCommodity(this.commodity.id, this.commodity).subscribe(
      (res: CommonInterface.IResult) => {
        if (res.status) {
          this._toastService.success(res.message);
          this.resetCommodityForm();
          this.getCommodities(this.pager);
        } else {
          this._toastService.error(res.message);
        }
      }
    );
  }

  addNewCommodity() {
    this.catalogueRepo.addNewCommodity(this.commodity).subscribe(
      (res: CommonInterface.IResult) => {
        if (res.status) {
          this._toastService.success(res.message);
          this.resetCommodityForm();
          this.pager.totalItems = 0;
          this.pager.currentPage = 1;
          this.getCommodities(this.pager);
        } else {
          this._toastService.error(res.message);
        }
      }
    );
  }

  resetCommodityForm() {
    this.formCommodity.onReset();
    this.groupSelect.active = [];
    $('#edit-commodity-modal').modal('hide');
  }

  updateGroup() {
    this.catalogueRepo.updateCommodityGroup(this.commodityGroup.id, this.commodityGroup).subscribe(
      (res: CommonInterface.IResult) => {
        if (res.status) {
          this._toastService.success(res.message);
          this.resetCommodityGroupForm();
          this.getGroupCommodities(this.pager);
        } else {
          this._toastService.error(res.message);
        }
      }
    );
  }

  resetCommodityGroupForm() {
    this.formGroupCommodity.onReset();
    $('#edit-commodity-group-modal').modal('hide');
  }

  addNewGroup() {
    this.catalogueRepo.addNewCommodityGroup(this.commodityGroup).subscribe(
      (res: CommonInterface.IResult) => {
        if (res.status) {
          this._toastService.success(res.message);
          this.getGroups();
          this.pager.totalItems = 0;
          this.pager.currentPage = 1;
          this.getGroupCommodities(this.pager);
          this.resetCommodityGroupForm();
        } else {
          this._toastService.error(res.message);
        }
      }
    );
  }

  showConfirmDelete(item, tabName: string) {
    if (tabName == this.tabName.commodityGroup) {
      this.commodityGroup = item;
    }
    if (tabName == this.tabName.commodity) {
      this.commodity = item;
    }
  }
  onCancel(tabName) {
    if (tabName == this.tabName.commodityGroup) {
      this.resetCommodityGroupForm();
    }
    if (tabName == this.tabName.commodity) {
      this.resetCommodityForm();
    }
  }
  public removed(value: any): void {
    console.log('Removed value is: ', value);
  }
  public typed(value: any): void {
    console.log('New search input: ', value);
  }
  value: any
  refreshValue(value: any) {
    this.value = value;
  }
  async exportCom() {
    var commodities = await this.baseService.postAsync(this.api_menu.Catalogue.Commodity.query, this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
      commodities = _map(commodities, function (com, index) {
        return [
          index + 1,
          com['code'],
          com['commodityNameEn'],
          com['commodityNameVn'],
          com['commodityGroupNameEn'],
          (com['inactive'] === true) ? "Inactive" : "Active"
        ]
      });
    }

    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
      commodities = _map(commodities, function (com, index) {
        return [
          index + 1,
          com['code'],
          com['commodityNameEn'],
          com['commodityNameVn'],
          com['commodityGroupNameVn'],
          (com['inactive'] === true) ? "Ngưng Hoạt Động" : "Đang Hoạt Động"
        ]
      });
    }
    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Commodity List";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      { name: "STT", width: 10 },
      { name: "Code", width: 20 },
      { name: "Name EN", width: 20 },
      { name: "Name VN", width: 20 },
      { name: "Commodity Group", width: 30 },
      { name: "Inactive", width: 20 }
    ]
    exportModel.data = commodities;
    exportModel.fileName = "Commodity";

    this.excelService.generateExcel(exportModel);

  }
  async exportComGroup() {
    var commodities_group = await this.baseService.postAsync(this.api_menu.Catalogue.CommodityGroup.query, this.criteria);
    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
      commodities_group = _map(commodities_group, function (com_group, index) {
        return [
          index + 1,
          com_group['groupNameEn'],
          com_group['groupNameVn'],
          (com_group['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
        ]
      });
    }

    if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
      commodities_group = _map(commodities_group, function (com_group, index) {
        return [
          index + 1,
          com_group['groupNameEn'],
          com_group['groupNameVn'],
          (com_group['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
        ]
      });
    }

    const exportModel: ExportExcel = new ExportExcel();
    exportModel.title = "Commodity Group List";
    const currrently_user = localStorage.getItem('currently_userName');
    exportModel.author = currrently_user;
    exportModel.header = [
      { name: "No.", width: 10 },
      { name: "Name EN", width: 20 },
      { name: "Name Local", width: 20 },
      { name: "Inactive", width: 20 }
    ]
    exportModel.data = commodities_group;
    exportModel.fileName = "Commodity Group";

    this.excelService.generateExcel(exportModel);

  }

}
