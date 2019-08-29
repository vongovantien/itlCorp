import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SortService, BaseService } from 'src/app/shared/services';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { API_MENU } from 'src/constants/api-menu.const';
import { finalize, catchError, takeUntil } from 'rxjs/operators';
import { CustomDeclarationRepo } from 'src/app/shared/repositories';

@Component({
  selector: 'app-add-more-modal',
  templateUrl: './add-more-modal.component.html'
})
export class AddMoreModalComponent extends PopupBase implements OnInit {
  @Input() currentJob: OpsTransaction;
  @Output() isCloseModal = new EventEmitter();
  notImportedData: any[];
  page: number = 1;
  totalItems: number = 0;
  numberToShow: number[] = [3, 15, 30, 50];
  pageSize: number = this.numberToShow[1];

  sort: string = null;
  order: any = false;
  keyword: string = '';
  requestList: any = null;
  requestSort: any = null;
  notImportedCustomClearances: any[];
  checkAllNotImported = false;
  headers: CommonInterface.IHeaderTable[];
  dataNotImportedSearch: any[];

  constructor(
    private _sortService: SortService,
    private api_menu: API_MENU,
    private baseServices: BaseService,
    private customClearanceRepo: CustomDeclarationRepo) {
    super();

    this.requestSort = this.sortLocal;
    this.requestList = this.getDataNotImported;
  }
  ngOnInit() {
    this.headers = [
      { title: 'Custom No', field: 'clearanceNo', sortable: true },
      { title: 'Import Date', field: 'datetimeCreated', sortable: true },
      { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
      { title: 'HBL No', field: 'hblid', sortable: true },
      { title: 'Export Country', field: 'exportCountryCode', sortable: true },
      { title: 'Import Country', field: 'importCountryCode', sortable: true },
      { title: 'Commodity Code', field: 'commodityCode', sortable: true },
      { title: 'Qty', field: 'qtyCont', sortable: true },
      { title: 'Parentdoc', field: 'firstClearanceNo', sortable: true },
      { title: 'Note', field: 'note', sortable: true },
    ];
  }
  sortLocal(sort: string): void {
    this.notImportedCustomClearances = this._sortService.sort(this.notImportedCustomClearances, sort, this.order);
  }
  async refreshData() {
    this.keyword = '';
    this.getListCleranceNotImported();
  }
  changeAllNotImported() {
    if (this.checkAllNotImported) {
      this.notImportedCustomClearances.forEach(x => {
        x.isChecked = true;
      });
    } else {
      this.notImportedCustomClearances.forEach(x => {
        x.isChecked = false;
      });
    }
    const checkedData = this.notImportedCustomClearances.filter(x => x.isChecked === true);
    if (checkedData.length > 0) {
      for (let i = 0; i < checkedData.length; i++) {
        const index = this.notImportedData.indexOf(x => x.id === checkedData[i].id);
        if (index > -1) {
          this.notImportedData[index] = true;
        }
      }
    }
  }
  searchClearanceNotImported(event) {
    const keySearch = this.keyword.trim().toLocaleLowerCase();
    if (keySearch !== null && keySearch.length < 1 && keySearch.length > 0) {
      return 0;
    }
    this.dataNotImportedSearch = this.notImportedData.filter(item => item.clearanceNo.includes(keySearch)
      || (item.hblid == null ? '' : item.hblid.toLocaleLowerCase()).includes(keySearch)
      || (item.exportCountryCode == null ? '' : item.exportCountryCode.toLocaleLowerCase()).includes(keySearch)
      || (item.importCountryCode == null ? '' : item.importCountryCode.toLocaleLowerCase()).includes(keySearch)
      || (item.commodityCode == null ? '' : item.commodityCode.toLocaleLowerCase()).includes(keySearch)
      || (item.firstClearanceNo == null ? '' : item.firstClearanceNo.toLocaleLowerCase()).includes(keySearch)
      || (item.qtyCont == null ? '' : item.qtyCont.toString()).includes(keySearch)
    );
    this.totalItems = this.dataNotImportedSearch.length;
    this.notImportedCustomClearances = this.dataNotImportedSearch.slice(0, (this.pageSize - 1));
  }
  removeAllChecked() {
    this.checkAllNotImported = false;
    const checkedData = this.notImportedCustomClearances.filter(x => x.isChecked === true);
    if (checkedData.length > 0) {
      for (let i = 0; i < checkedData.length; i++) {
        const index = this.notImportedData.indexOf(x => x.id === checkedData[i].id);
        if (index > -1) {
          this.notImportedData[index] = true;
        }
      }
    }
  }
  getDataNotImported() {
    if (this.notImportedData != null) {
      this.totalItems = this.notImportedData.length;
      this.page = 0;
      console.log(this.notImportedData);
      const end = this.page + (this.pageSize - 1);
      this.notImportedCustomClearances = this.notImportedData.slice(this.page, end);
    }
  }
  async updateJobToClearance() {
    const dataToUpdate = this.notImportedData.filter(x => x.isChecked === true);
    if (dataToUpdate.length > 0) {
      dataToUpdate.forEach(x => {
        x.jobNo = this.currentJob.jobNo;
      });
      const responses = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.updateToAJob, dataToUpdate, false, true);
      if (responses.success === true) {
        this.isCloseModal.emit(true);
        this.hide();
      }
    }
  }
  getListCleranceNotImported() {
    this.customClearanceRepo.getListNotImportToJob(false).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { })
    ).subscribe(
      (res: any) => {
        this.totalItems = res.length;
        this.notImportedData = this.dataNotImportedSearch = res;
        this.page = 0;
        this.notImportedCustomClearances = this.dataNotImportedSearch.slice(this.page, (this.pageSize - 1));
      }
    );
  }

  // app list
  setSortBy(sort?: string, order?: boolean): void {
    this.sort = sort ? sort : 'code';
    this.order = order;
  }

  sortBy(sort: string): void {
    if (!!sort) {
      this.setSortBy(sort, this.sort !== sort ? true : !this.order);

      if (typeof (this.requestSort) === 'function') {
        this.requestSort(this.sort, this.order);   // sort Local
      }
    }
  }

  sortClass(sort: string): string {
    if (!!sort) {
      let classes = 'sortable ';
      if (this.sort === sort) {
        classes += ('sort-' + (this.order ? 'asc' : 'desc') + ' ');
      }

      return classes;
    }
    return '';
  }

  pageChanged(event: any): void {
    if (this.page !== event.page || this.pageSize !== event.itemsPerPage) {
      this.page = event.page;
      this.pageSize = event.itemsPerPage;

      this.requestList();
    }
  }

  selectPageSize(pageSize: number, data?: any) {
    this.pageSize = pageSize;
    this.requestList(data);
  }
  close() {
    this.keyword = '';
    this.hide();
  }
}
