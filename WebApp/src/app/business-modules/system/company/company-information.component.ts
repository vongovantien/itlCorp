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

@Component({
    selector: 'app-company-info',
    templateUrl: './company-information.component.html',
    styleUrls: ['./company-information.component.scss']
})
export class ComanyInformationComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    headers: CommonInterface.IHeaderTable[]

    companies: Company[] = [];

    constructor(
        private _systemRepo: SystemRepo,
        private _progressService: NgProgress,
        private _router: Router

    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.searchCompany;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Company Code', field: 'code' },
            { title: 'Name EN', field: 'code' },
            { title: 'Name Local', field: 'code' },
            { title: 'Name Abbr', field: 'code' },
            { title: 'Logo', field: 'code' },
            { title: 'Website', field: 'code' },
            { title: 'Status', field: 'code' },
        ];
        this.dataSearch = {
            type: 'All'
        };
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
                    console.log(this.companies);
                },
            );
    }

    showDeletePopup() {
        this.confirmDeletePopup.show();
    }


    gotoDetail(company: Company) {
        this._router.navigate([`${company.id}`]);
    }

}

