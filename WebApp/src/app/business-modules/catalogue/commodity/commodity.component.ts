import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { Commodity } from 'src/app/shared/models/catalogue/commodity.model';
import { COMMODITYCOLUMNSETTING } from './commodity.column';
import _map from 'lodash/map';
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

type COMMODITY_TAB = 'Commodity list' | 'Commodity group';
enum CommodityTab {
  LIST = 'Commodity list',
  GROUP = 'Commodity group',
}

@Component({
  selector: 'app-commodity',
  templateUrl: './commodity.component.html',
})
export class CommodityComponent extends AppList {
  @ViewChild(CommodityAddPopupComponent, { static: false }) commodityAddPopupComponent: CommodityAddPopupComponent;
  @ViewChild(CommodityGroupAddPopupComponent, { static: false }) commodityGroupAddPopupComponent: CommodityGroupAddPopupComponent;
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

  commodities: Commodity[] = [];
  commodity: Commodity;
  commoditySettings: ColumnSetting[] = COMMODITYCOLUMNSETTING;

  configSearchCommonity: any = {
    settingFields: this.commoditySettings.filter(x => x.allowSearch === true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
    typeSearch: TypeSearch.intab,
    searchString: ''
  };
  selectedTab: COMMODITY_TAB = CommodityTab.LIST; // Default tab.

  headerCommodity: CommonInterface.IHeaderTable[];
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

    this.getCommodities();
  }

  onSelectTabCommodity(tabname: COMMODITY_TAB) {
    this.selectedTab = tabname;
  }

  ngAfterViewInit() {
    this._cd.detectChanges();
  }

  getCommodities() {
    this._progressRef.start();
    this.catalogueRepo.getCommodityPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
      .pipe(
        catchError(this.catchError),
        finalize(() => {
          this._progressRef.complete();
        }),
        map((data: any) => {
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


  onSearch(event: { field: string, searchString: string, displayName: string }) {
    this.dataSearch = {};
    this.dataSearch[event.field] = event.searchString;
    this.page = 1;
    this.getCommodities();
  }

  resetSearch() {
    this.dataSearch = {};
    this.getCommodities();
  }

  showDetailCommodity(commodity) {
    this.catalogueRepo.getDetailCommodity(commodity.id).subscribe(
      (res: any) => {
        if (res.id !== 0) {
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

  deleteCommodity() {
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

  openPopupAddCommodity() {
    this.commodityAddPopupComponent.action = 'create';
    this.commodityAddPopupComponent.show();
  }

  onRequestCommodity() {
    this.getCommodities();
  }

  showConfirmDelete(data: any) {
    this.commodity = data;
    this.confirmDeletePopup.show();
  }

  onDelete() {
    this.confirmDeletePopup.hide();
    this.deleteCommodity();
  }

  sortCommodity(sortData: CommonInterface.ISortData): void {
    this.commodities = this._sortService.sort(this.commodities, sortData.sortField, sortData.order);
  }

  exportCom() {
    this._exportRepo.exportCommodity(this.dataSearch)
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: any) => {
          this.downLoadFile(res, "application/ms-excel", "Commodity.xlsx");
        },
      );
  }

}
