import { Component, OnInit, OnChanges } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';

@Component({
  selector: 'app-add-more-modal',
  templateUrl: './add-more-modal.component.html'
})
export class AddMoreModalComponent extends PopupBase implements OnInit, OnChanges {
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

  constructor(
    private _sortService: SortService) {
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
    console.log('init add more');
    // if (this.notImportedData) {

    //   this.totalItems = this.notImportedData.length;
    //   this.notImportedCustomClearances = this.notImportedData.slice(this.page - 1, this.pageSize);
    // }
  }
  ngOnChanges() {
    // console.log('change add more');
    // this.totalItems = this.notImportedData.length;
    // this.notImportedCustomClearances = this.notImportedData.slice(this.page - 1, this.pageSize);
  }
  sortLocal(sort: string): void {
    this.notImportedCustomClearances = this._sortService.sort(this.notImportedCustomClearances, sort, this.order);
  }
  changeAllNotImported() { }
  removeAllChecked() { }
  getDataNotImported() {
    if (this.notImportedData != null) {
      this.totalItems = this.notImportedData.length;
      console.log(this.notImportedData);
      const end = (this.page - 1) + this.pageSize;
      this.notImportedCustomClearances = this.notImportedData.slice(this.page - 1, end);
    }
  }
  setSortBy(sort?: string, order?: boolean): void {
    this.sort = sort ? sort : 'code';
    this.order = order;
  }

  sortBy(sort: string): void {
    if (!!sort) {
      this.setSortBy(sort, this.sort !== sort ? true : !this.order);

      if (typeof (this.requestSort) === 'function') {
        // this.requestList(this.sort, this.order);   // sort server
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
    this.notImportedCustomClearances = null;
    this.hide();
  }
}
