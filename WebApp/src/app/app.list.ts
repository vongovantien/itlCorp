import { AppPage } from './app.base';

export abstract class AppList extends AppPage {

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
}
