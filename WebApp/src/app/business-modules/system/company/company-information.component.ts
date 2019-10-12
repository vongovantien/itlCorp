import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Company } from 'src/app/shared/models';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { map } from 'rxjs/internal/operators/map';
import { NgProgress } from '@ngx-progressbar/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'app-company-info',
    templateUrl: './company-information.component.html',
    styleUrls: ['./company-information.component.scss']
})
export class ComanyInformationComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    headers: CommonInterface.IHeaderTable[];

    companies: Company[] = [];
    selectedCompany: Company;

    constructor(
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _router: Router,
        private _toastService: ToastrService,
        private _sortService: SortService

    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.searchCompany;
        this.requestSort = this.sortCompany;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Company Code', field: 'code', sortable: true },
            { title: 'Name EN', field: 'bunameEn', sortable: true },
            { title: 'Name Local', field: 'bunameVn', sortable: true },
            { title: 'Name Abbr', field: 'bunameAbbr', sortable: true },
            { title: 'Logo', field: 'logoPath', sortable: true },
            { title: 'Website', field: 'website', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];
        this.dataSearch = { All: null };
        this.searchCompany(this.dataSearch);
    }

    onSearchCompany(dataSearch: any) {
        this.dataSearch = dataSearch;
        this.searchCompany(this.dataSearch);
    }

    searchCompany(dataSearch?: any) {
        this.isLoading = true;
        this._progressRef.start();
        this._systemRepo.getCompany(this.page, this.pageSize, Object.assign({}, dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    return {
                        data: data.data.map((item: any) => new Company(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.companies = res.data;
                },
            );
    }

    sortCompany() {
        this.companies = this._sortService.sort(this.companies, this.sort, this.order);
    }

    showDeletePopup(company: Company) {
        this.selectedCompany = new Company(company);
        this.confirmDeletePopup.show();
    }

    gotoDetail(company: Company) {
        this._router.navigate([`${company.id}`]);
    }

    onDelete() {
        this.confirmDeletePopup.hide();
        this._progressRef.start();
        this._systemRepo.deleteCompany(this.selectedCompany.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.searchCompany();
                    } else {
                        this._toastService.warning(res.message);
                    }
                }
            );
    }

}

