import { Component, OnInit, ViewChild } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { SortService } from '@services';
import { CatalogueRepo } from '@repositories';
import { Router } from '@angular/router';
import { AppList } from 'src/app/app.list';
import { ConfirmPopupComponent } from '@common';
import { ChartOfAccounts } from 'src/app/shared/models/catalogue/catChartOfAccounts.model';
import { catchError, finalize, map } from 'rxjs/operators';
import { FormCreateChartOfAccountsPopupComponent } from './components/form-create-chart-of-accounts/form-create-chart-of-accounts.popup';

@Component({
    selector: 'app-chart-of-accounts',
    templateUrl: 'chart-of-accounts.component.html'
})

export class ChartOfAccountsComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(FormCreateChartOfAccountsPopupComponent, { static: false }) formPopup: FormCreateChartOfAccountsPopupComponent;

    chartOfAccounts: ChartOfAccounts[] = [];
    constructor(private _progressService: NgProgress,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _router: Router) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchChart;
        this.requestSort = this.sortLocal;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Account Code', field: 'accountCode', sortable: true, width: 100 },
            { title: 'Account Name Local', field: 'accountNameLocal', sortable: true },
            { title: 'Account Name EN', field: 'accountNameEn', sortable: true },
            { title: 'Status', field: 'active', sortable: true },

        ];
        this.dataSearch = {
            type: 'All'
        };
        this.searchChart();
    }

    sortLocal(sort: string): void {
        this.chartOfAccounts = this._sortService.sort(this.chartOfAccounts, sort, this.order);
    }

    showAdd() {
        [this.formPopup.isUpdate, this.formPopup.isSubmitted] = [false, false];


        this.formPopup.formChart.reset();
        this.formPopup.show();
    }

    searchChart() {
        this.isLoading = true;
        this._progressRef.start();
        this._catalogueRepo.getListChartOfAccounts(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((res: any) => {
                    return {
                        data: res.data != null ? res.data.map((item: any) => new ChartOfAccounts(item)) : [],
                        totalItems: res.totalItems,
                    };
                }),
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems;
                    this.chartOfAccounts = res.data || [];
                },
            );
    }


    onSearchCharge(dataSearch: any) {
        this.dataSearch = {};
        if (dataSearch.type === 'All') {
            this.dataSearch.all = dataSearch.keyword;
        } else {
            this.dataSearch.all = null;
            if (dataSearch.type === 'accountCode') {
                this.dataSearch.accountCode = dataSearch.keyword;
            }
            if (dataSearch.type === 'accountNameLocal') {
                this.dataSearch.accountNameLocal = dataSearch.keyword;
            }
            if (dataSearch.type === 'accountNameEn') {
                this.dataSearch.accountNameEn = dataSearch.keyword;
            }
            if (dataSearch.type === 'status') {
                this.dataSearch.active = dataSearch.keyword;
            }
        }
        this.searchChart();
    }
}