import { AppPage } from './app.base';

export abstract class AppList extends AppPage {

  sort: string = null;
  order: any = false;
  keyword: string = '';
 

  setSortBy(sort?: string, order?: boolean): void {
    this.sort = sort ? sort : 'code';
    this.order = order;
  }

  sortBy(sort: string): void {

    if (!!sort) {
      // this.setSortBy(sort, this.sort !== sort ? 'asc' : (this.order === 'desc' ? 'asc' : 'desc'));
      this.setSortBy(sort, this.sort !== sort ? true : !this.order);

      // if (typeof (this.request) === 'function') {
      //   this.request(this.sort, this.order);
      // }
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
