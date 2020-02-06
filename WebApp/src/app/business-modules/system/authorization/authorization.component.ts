import { AppList } from "src/app/app.list";
import { Component } from "@angular/core";
import { Router } from "@angular/router";
import { NgProgress } from "@ngx-progressbar/core";
import { SystemRepo } from "@repositories";
import { SortService } from "@services";
import { ToastrService } from "ngx-toastr";
import { catchError, finalize, map } from "rxjs/operators";
import { Authorization } from "src/app/shared/models/system/authorization";

@Component({
  selector: 'app-authorization',
  templateUrl: './authorization.component.html',
})
export class AuthorizationComponent extends AppList {

  headers: CommonInterface.IHeaderTable[];

  authorizations: Authorization[] = [];
  selectedAuthorization: Authorization;

  constructor(private _router: Router,
    private _systemRepo: SystemRepo,
    private _sortService: SortService,
    private _progressService: NgProgress,
    private _toastService: ToastrService) {
    super();
    this._progressRef = this._progressService.ref();
    this.requestList = this.searchAuthorization;
    this.requestSort = this.sortAuthorization;
  }

  ngOnInit() {
    this.headers = [
      { title: 'Service', field: 'servicesName', sortable: true },
      { title: 'Person In Charge', field: 'userId', sortable: true },
      { title: 'Authorized Person', field: 'assignTo', sortable: true },
      { title: 'Name', field: 'name', sortable: true },
      { title: 'Effective Date', field: 'startDate', sortable: true },
      { title: 'Expiration Date', field: 'endDate', sortable: true },
      { title: 'Status', field: 'active', sortable: true },
    ];

    this.searchAuthorization(this.dataSearch);
  }

  onSearchAuthorization(data: any) {
    console.log(data);
    this.page = 1; // reset page.
    this.searchAuthorization(data);
  }

  searchAuthorization(dataSearch?: any) {
    this._progressRef.start();
    this._systemRepo.getAuthorization(this.page, this.pageSize, Object.assign({}, dataSearch))
      .pipe(
        catchError(this.catchError),
        finalize(() => {
          this._progressRef.complete();
        }),
        map((data: any) => {
          return {
            data: data.data.map((item: any) => new Authorization(item)),
            totalItems: data.totalItems,
          };
        })
      ).subscribe(
        (res: any) => {
          this.totalItems = res.totalItems || 0;
          this.authorizations = res.data;
        },
      );
  }

  sortAuthorization(sort: string): void {
    this.authorizations = this._sortService.sort(this.authorizations, sort, this.order);
  }
}