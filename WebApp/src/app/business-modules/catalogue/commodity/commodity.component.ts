import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { CommodityGroup } from 'src/app/shared/models/catalogue/commonity-group.model';
import { COMMODITYGROUPCOLUMNSETTING } from './commonity-group.column';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { Commodity } from 'src/app/shared/models/catalogue/commodity.model';
import { COMMODITYCOLUMNSETTING } from './commodity.column';
import _map from 'lodash/map';
import { SearchOptionsComponent } from '../../../shared/common/search-options/search-options.component';
import { CatalogueRepo } from 'src/app/shared/repositories/catalogue.repo';
import { SortService } from 'src/app/shared/services/sort.service';
import { ToastrService } from 'ngx-toastr';
import { ExportRepo } from 'src/app/shared/repositories';
import { AppList } from 'src/app/app.list';
import { catchError, finalize, map } from 'rxjs/operators';
import { CommodityAddPopupComponent } from './components/form-create-commodity/form-create-commodity.popup';
import { CommodityGroupAddPopupComponent } from './components/form-create-commodity-group/form-create-commodity-group.popup';
import { NgProgress } from '@ngx-progressbar/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
  selector: 'app-commodity',
  templateUrl: './commodity.component.html',
})
export class CommodityComponent extends AppList {

  /*
  declare variable
  */
  @ViewChild(AppPaginationComponent, { static: false }) child;
  @ViewChild(SearchOptionsComponent, { static: false }) searchOption;
  @ViewChild(CommodityAddPopupComponent, { static: false }) commodityAddPopupComponent: CommodityAddPopupComponent;
  @ViewChild(CommodityGroupAddPopupComponent, { static: false }) commodityGroupAddPopupComponent: CommodityGroupAddPopupComponent;
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
  
  commodities: Commodity[] = [];
  commodity: Commodity;
  commodityGroups: CommodityGroup[] = [];
  commodityGroup: CommodityGroup;
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

  headerCommodity: CommonInterface.IHeaderTable[];
  headerCommodityGroup: CommonInterface.IHeaderTable[];
  /*
  end declare variable
  */

  constructor(private catalogueRepo: CatalogueRepo,
    private _toastService: ToastrService,
    private _exportRepo: ExportRepo,
    private _cd: ChangeDetectorRef,
    private _ngProgessSerice: NgProgress,
    private _sortService: SortService,) {
    super();
    this._progressRef = this._ngProgessSerice.ref();
    this.requestList = this.getCommodities;
    this.requestSort = this.sortCommodity;
  }

  ngOnInit() {
    this.headerCommodity = [
      { title: 'Code', sortable: true, field: 'code' },
      { title: 'Name(EN)', sortable: true, field: 'commodityNameEn' },
      { title: 'Name(Local)', sortable: true, field: 'commodityNameVn' },
      { title: 'Group', sortable: true, field: 'commodityGroupNameVn' },
      { title: 'Status', sortable: true, field: 'active' },
    ];
    this.headerCommodityGroup = [
      { title: 'Id', sortable: true, field: 'id' },
      { title: 'Name(EN)', sortable: true, field: 'groupNameEn' },
      { title: 'Name(Local)', sortable: true, field: 'groupNameVn' },
      { title: 'Status', sortable: true, field: 'active' },
    ];
    this.getCommodities();
    this.getGroupCommodities();
  }

  ngAfterViewInit() {
    this._cd.detectChanges();
  }

  getCommodities() {
    this._progressRef.start();
    this.catalogueRepo.getCommodityPaging(this.page, this.pageSize, Object.assign({}, this.criteria))
      .pipe(
        catchError(this.catchError),
        finalize(() => {
          this._progressRef.complete();
        }),
        map((data: any) => {
          console.log(data)
          return {
            data: data.data.map((item: any) => new Commodity(item)),
            totalItems: data.totalItems,
          };
        })
      ).subscribe(
        (res: any) => {
          this.totalItems = res.totalItems || 0;
          this.commodities = res.data;
        },
      );
  }

  getGroupCommodities() {
    this._progressRef.start();
    this.catalogueRepo.getCommodityGroupPaging(this.page, this.pageSize, Object.assign({}, this.criteria))
    .pipe(
      catchError(this.catchError),
      finalize(() => {
        this._progressRef.complete();
      }),
      map((data: any) => {
        console.log(data)
        return {
          data: data.data.map((item: any) => new CommodityGroup(item)),
          totalItems: data.totalItems,
        };
      })
    ).subscribe(
        (res: any) => {
          this.totalItems = res.totalItems || 0;
          this.commodityGroups = res.data;
        },
      );
  }
  onSearch(event: { field: string, searchString: string, displayName: string }) {
    this.criteria = {};
    this.criteria[event.field] = event.searchString;
    if (this.activeTab == this.tabName.commodityGroup) {
      this.getGroupCommodities();
    }
    if (this.activeTab == this.tabName.commodity) {
      this.getCommodities();
    }
  }

  resetSearch() {
    this.criteria = {};
    if (this.activeTab == this.tabName.commodityGroup) {
      this.getGroupCommodities();
    }
    if (this.activeTab == this.tabName.commodity) {
      this.getCommodities();
    }
  }

  // searchCommodity(event: any): any {
  //   if (event.field == "All") {
  //     this.criteria.all = event.searchString;
  //   }
  //   else {
  //     this.criteria = {};
  //     if (event.field == "commodityNameEn") {
  //       this.criteria.commodityNameEn = event.searchString;
  //     }
  //     if (event.field == "commodityNameVn") {
  //       this.criteria.commodityNameVn = event.searchString;
  //     }
  //     if (event.field == "commodityGroupNameEn") {
  //       this.criteria.commodityGroupNameEn = event.searchString;
  //     }
  //     if (event.field == "code") {
  //       this.criteria.code = event.searchString;
  //     }
  //   }
  //   this.getCommodities();
  // }
  // searchCommodityGroup(event: any): any {
  //   if (event.searchString == "") {
  //     event.searchString = null;
  //   }
  //   if (event.field == "All") {
  //     this.criteria.all = event.searchString;
  //   }
  //   else {
  //     this.criteria = {};
  //     if (event.field == "groupNameEn") {
  //       this.criteria.groupNameEn = event.searchString;
  //     }
  //     if (event.field == "groupNameVn") {
  //       this.criteria.groupNameVn = event.searchString;
  //     }
  //   }
  //   this.pager.currentPage = 1;
  //   this.getGroupCommodities();
  // }

  showDetail(item, tabName) {
    if (tabName == this.tabName.commodityGroup) {
      this.commodityGroup = item;
      this.catalogueRepo.getDetailCommodityGroup(item.id)
        .pipe(catchError(this.catchError))
        .subscribe(
          (res: any) => {
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

  showDetailCommodity(commodity){
    this.catalogueRepo.getDetailCommodity(commodity.id).subscribe(
      (res: any) => {
        if (res.id !== 0) {
          console.log(res)
          var _commodity = new Commodity(res);
          this.commodityAddPopupComponent.action = "update";
          this.commodityAddPopupComponent.commodity = _commodity;
          this.commodityAddPopupComponent.getDetail();
          this.commodityAddPopupComponent.show();
        } else {
          this._toastService.error("Not found data");
        }
      },
    );
  }

  showDetailCommodityGroup(commodityGroup){
    console.log(commodityGroup);
    this.catalogueRepo.getDetailCommodityGroup(commodityGroup.id)
        .pipe(catchError(this.catchError))
        .subscribe(
          (res: any) => {
            console.log(res)
            if (res.id !== 0) {
              var _commodityGroup = new CommodityGroup(res);
              this.commodityGroupAddPopupComponent.action = "update"
              this.commodityGroupAddPopupComponent.commodityGroup = _commodityGroup;
              this.commodityGroupAddPopupComponent.getDetail();
              this.commodityGroupAddPopupComponent.show();
            } else {
              this._toastService.error("Not found data");
            }
          },
        );
  }

  deleteCommodity() {
    console.log(this.commodity)
    this.catalogueRepo.deleteCommodity(this.commodity.id)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: CommonInterface.IResult) => {
          if (res.status) {
            this._toastService.success(res.message, '');
            this.getCommodities();
          } else {
            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
          }
        },
      );
  }

  deleteGroupCommodity() {
    console.log(this.commodityGroup)
    this.catalogueRepo.deleteCommodityGroup(this.commodityGroup.id)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: CommonInterface.IResult) => {
          if (res.status) {
            this._toastService.success(res.message, '');
            this.getGroupCommodities();
          } else {
            this._toastService.error(res.message || 'Có lỗi xảy ra', '');
          }
        },
      );
  }

  // showConfirmDelete(item, tabName: string) {
  //   if (tabName == this.tabName.commodityGroup) {
  //     this.commodityGroup = item;
  //   }
  //   if (tabName == this.tabName.commodity) {
  //     this.commodity = item;
  //   }
  // }

  openPopupAddCommodity() {
    this.commodityAddPopupComponent.action = 'create';
    this.commodityAddPopupComponent.show();
  }

  onRequestCommodity() {
    console.log('reload list commodity');
    this.getCommodities();
  }

  openPopupAddCommodityGroup() {
    this.commodityGroupAddPopupComponent.action = 'create';
    this.commodityGroupAddPopupComponent.show();
  }

  onRequestCommodityGroup() {
    console.log('reload list commodity group');
    this.getGroupCommodities();
  }

  showConfirmDelete(data: any, tabName: string){
    this.activeTab = tabName;
    if(this.activeTab == this.tabName.commodity){
      this.commodity = data;
    } else {
      this.commodityGroup = data;
    }
    console.log(data)
    this.confirmDeletePopup.show();
  }

  onDelete(){
    console.log(this.activeTab)
    this.confirmDeletePopup.hide();
    if(this.activeTab == this.tabName.commodity){
      this.deleteCommodity();
    } else {
      this.deleteGroupCommodity();
    }   
  }

  sortCommodity(sort: string): void {
    this.commodities = this._sortService.sort(this.commodities, sort, this.order);
  }

  sortCommodityGroup(sort: string): void {
    this.commodityGroups = this._sortService.sort(this.commodityGroups, sort, this.order);
  }

  exportCom() {
    this._exportRepo.exportCommodity(this.criteria)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: any) => {
          this.downLoadFile(res, "application/ms-excel", "Commodity.xlsx");
        },
      );

  }
  exportComGroup() {
    this._exportRepo.exportCommodityGroup(this.criteria)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: any) => {
          this.downLoadFile(res, "application/ms-excel", "CommodityGroup.xlsx");
        },
      );
  }

}
