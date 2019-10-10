import { Component, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { Router } from '@angular/router';
import { AppList } from 'src/app/app.list';
import { SystemRepo } from 'src/app/shared/repositories';
import { Department } from 'src/app/shared/models/system/department';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize, map } from 'rxjs/operators';

@Component({
  selector: 'app-department',
  templateUrl: './department.component.html',
})
export class DepartmentComponent extends AppList {
  @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

  headers: CommonInterface.IHeaderTable[];

  departments: Department[] = [];

  constructor(private _router: Router,
    private _systemRepo: SystemRepo,
    private _progressService: NgProgress, ) {
    super();
    this.requestList = this.searchDepartment;
    this._progressRef = this._progressService.ref();
  }

  ngOnInit() {
    this.headers = [
      { title: 'Department Code', field: 'code', sortable: true },
      { title: 'Name EN', field: 'deptNameEN', sortable: true },
      { title: 'Name Local', field: 'deptName', sortable: true },
      { title: 'Name Abbr', field: 'deptNameAbbr', sortable: true },
      { title: 'Office', field: 'officeName', sortable: true },
      { title: 'Status', field: 'active', sortable: true },
    ];
    this.dataSearch = {
      type: 'All'
    };
    this.searchDepartment(this.dataSearch);
  }

  showDeletePopup() {
    this.confirmDeletePopup.show();
  }

  onSearchDepartment(data: any) {
    console.log(data);
    this.page = 1; // reset page.
    this.searchDepartment(data);
  }

  searchDepartment(dataSearch?: any) {
    //this.isLoading = true;
    this._progressRef.start();
    this._systemRepo.getDepartment(this.page, this.pageSize, Object.assign({}, dataSearch))
      .pipe(
        catchError(this.catchError),
        finalize(() => {
          //this.isLoading = false; 
          this._progressRef.complete(); 
        }),
        map((data: any) => {
          return {
            data: data.data.map((item: any) => new Department(item)),
            totalItems: data.totalItems,
          };
        })
      ).subscribe(
        (res: any) => {
          this.totalItems = res.totalItems || 0;
          this.departments = res.data;
          console.log(this.departments);
        },
      );
  }

  gotoDetailDepartment(id: number) {
    console.log(id)
    this._router.navigate([`home/system/department/${id}`]);//([`${id}`])//
  }

}
