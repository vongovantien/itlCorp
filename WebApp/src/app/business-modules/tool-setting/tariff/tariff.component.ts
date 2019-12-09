import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { SettingRepo } from 'src/app/shared/repositories';
import { map, catchError, finalize } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from 'src/app/shared/services';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';
import { Tariff } from 'src/app/shared/models';

@Component({
    selector: 'app-tariff',
    templateUrl: './tariff.component.html',
})
export class TariffComponent extends AppList {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: ConfirmPopupComponent;

    tariffs: Tariff[] = [];
    headers: CommonInterface.IHeaderTable[] = [];

    selectedTariff: ITariff;

    constructor(
        private _settingRepo: SettingRepo,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _toastService: ToastrService
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
            { field: 'customerName', title: 'Customer', sortable: true },
            { field: 'supplierName', title: 'Supplier', sortable: true },
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
                        data: (data.data || []).map((item: Tariff) => new Tariff(item)),
                        totalItems: data.totalItems,
                    };
                })
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems || 0;
                    this.tariffs = res.data || [];
                },
            );
    }

    sortTariff() {
        this.tariffs = this._sortService.sort(this.tariffs, this.sort, this.order);
    }

    showDeletePopup(tariff: ITariff) {
        this.selectedTariff = tariff;
        this.confirmDeletePopup.show();
    }

    onDeleteTariff() {
        this._progressRef.start();
        this._settingRepo.deleteTariff(this.selectedTariff.id)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.confirmDeletePopup.hide();
            }))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.confirmDeletePopup.hide();
                        this._toastService.success(res.message);
                        this.searchTariff({});
                    } else {
                        this._toastService.warning(res.message);
                    }
                },
            );
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
