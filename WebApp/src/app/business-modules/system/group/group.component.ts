import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Group } from 'src/app/shared/models/system/group';
import { NgProgress } from '@ngx-progressbar/core';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map } from 'rxjs/operators';
import { SortService } from 'src/app/shared/services';

@Component({
  selector: 'app-group',
  templateUrl: './group.component.html',
  styleUrls: ['./group.component.sass']
})
export class GroupComponent extends AppList implements OnInit {

  headers: CommonInterface.IHeaderTable[];
  titleConfirmDelete = 'Do you want to delete?';
  groups: Group[] = [];

  constructor(
    private _progressService: NgProgress,
    private _systemRepo: SystemRepo,
    private _sortService: SortService) {
    super();
    this._progressRef = this._progressService.ref();
    this.requestList = this.searchGroup;
    this.requestSort = this.sortGroups;
  }

  ngOnInit() {
    this.headers = [
      { title: 'Group Code', field: 'code', sortable: true },
      { title: 'Name (EN)', field: 'nameEn', sortable: true },
      { title: 'Name (Local)', field: 'nameVn', sortable: true },
      { title: 'Name Abbr', field: 'shortName', sortable: true },
      { title: 'Department', field: 'departmentName', sortable: true },
      { title: 'Status', field: 'active', sortable: true }
    ];
    this.dataSearch = {
      all: null
    };
    this.searchGroup(this.dataSearch);
  }
  searchGroup(dataSearch?: any) {
    this.isLoading = true;
    this._progressRef.start();
    this._systemRepo.getGroup(this.page, this.pageSize, Object.assign({}, dataSearch))
      .pipe(
        catchError(this.catchError),
        finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
        map((data: any) => {
          return {
            data: data.data.map((item: any) => new Group(item)),
            totalItems: data.totalItems,
          };
        })
      ).subscribe(
        (res: any) => {
          this.totalItems = res.totalItems || 0;
          this.groups = res.data;
          console.log(this.groups);
        },
      );
  }
  sortGroups(sort: string): void {
    this.groups = this._sortService.sort(this.groups, sort, this.order);
  }
  onDelete(event) {
  }
  onSearchGroup(dataSearch: any) {
    this.dataSearch = {};
    if (dataSearch.type === 'All') {
      this.dataSearch.all = dataSearch.keyword;
    } else {
      this.dataSearch.all = null;
      if (dataSearch.type === 'id') {
        this.dataSearch.id = dataSearch.keyword;
      }
      if (dataSearch.type === 'code') {
        this.dataSearch.code = dataSearch.keyword;
      }
      if (dataSearch.type === 'nameEN') {
        this.dataSearch.nameEN = dataSearch.keyword;
      }
      if (dataSearch.type === 'nameVN') {
        this.dataSearch.nameVN = dataSearch.keyword;
      }
      if (dataSearch.type === 'shortName') {
        this.dataSearch.shortName = dataSearch.keyword;
      }
      if (dataSearch.type === 'departmentName') {
        this.dataSearch.departmentName = dataSearch.keyword;
      }
    }
    // this.dataSearch = dataSearch;
    this.searchGroup(this.dataSearch);
  }
}
