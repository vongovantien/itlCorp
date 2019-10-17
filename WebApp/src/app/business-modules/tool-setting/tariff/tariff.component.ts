import { Component } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettingRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/internal/operators/catchError';
import { finalize } from 'rxjs/internal/operators/finalize';
import { map } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from 'src/app/shared/services';

@Component({
    selector: 'app-tariff',
    templateUrl: './tariff.component.html',
})
export class TariffComponent extends AppList {

    tariffs: ITariff[] = [];

    headers: CommonInterface.IHeaderTable[] = [];

    constructor(
        private _settingRepo: SettingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService
    ) {
        super();
        this._progressRef = this._progressService.ref();

        this.requestList = this.searchTariff;
        this.requestSort = this.sortTariff;
    }

    ngOnInit() {
        this.headers = [
            { field: 'tariffName', title: 'Name', sortable: true },
            { field: 'tariffType', title: 'Tariff Type', sortable: true },
            { field: 'customerID', title: 'Customer', sortable: true },
            { field: 'supplierID', title: 'Supplier', sortable: true },
            { field: 'serviceMode', title: 'Shipment Mode', sortable: true },
            { field: 'effectiveDate', title: 'Effect Date', sortable: true },
            { field: 'expiredDate', title: 'Expired Date', sortable: true },
            { field: 'status', title: 'Status', sortable: true },
        ];

        this.searchTariff({});
    }

    searchTariff(dataSearch?: any) {
        this.isLoading = true;
        this._progressRef.start();
        this._settingRepo.getTariff(this.page, this.pageSize, Object.assign({}, dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((data: any) => {
                    return {
                        data: data.data,
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.tariffs = res.data || [];
                    console.log(this.tariffs);
                },
            );
    }

    sortTariff() {
        this.tariffs = this._sortService.sort(this.tariffs, this.sort, this.order);
    }

}

interface ITariff {
    id: string;
    tariffName: string;
    tariffType: string;
    customerID: string;
    supplierID: string;
    officeID: string;
    serviceMode: string;
    effectiveDate: string;
    expiredDate: string;
    datetimeCreated: string;
    status: boolean;
}
