import { AppPage } from './app.base';

export abstract class AppList extends AppPage {

  page: number = 1;
  totalItems: number = 0;
  numberToShow: number[] = [3, 15, 30,  50];
  pageSize: number = this.numberToShow[0];

  sort: string = null;
  order: any = false;
  keyword: string = '';
  requestList: any = null;

  constructor() {
    super();
  }

  setSortBy(sort?: string, order?: boolean): void {
    this.sort = sort ? sort : 'code';
    this.order = order;
  }

  sortBy(sort: string): void {
    if (!!sort) {
      this.setSortBy(sort, this.sort !== sort ? true : !this.order);

      if (typeof (this.requestList) === 'function') {
        this.requestList(this.sort, this.order);   // local or server
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

  selectPageSize() {
    this.requestList();
  }

}
