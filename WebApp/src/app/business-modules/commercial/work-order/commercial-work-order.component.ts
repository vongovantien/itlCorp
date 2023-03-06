import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { RoutingConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Store } from '@ngrx/store';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { map, takeUntil, withLatestFrom } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { IWorkOrderListState, LoadListWorkOrder, workOrderListState, workOrderLoadingState, workOrderPagingState, workOrderSearchState } from './store';

@Component({
    selector: 'app-commercial-work-order',
    templateUrl: './commercial-work-order.component.html',
})
export class CommercialWorkOrderComponent extends AppList implements OnInit {

    workOrders: any[] = [];
    services: any[] = [
        { title: 'Air Export', value: 'AE', icon: 'plane' },
        { title: 'Air Import', value: 'AI', icon: 'plane' },
        { title: 'Sea Consol Export', value: 'SCE', icon: 'ship' },
        { title: 'Sea Consol Import', value: 'SCI', icon: 'ship' },
        { title: 'Sea FCL Export', value: 'SFE', icon: 'ship' },
        { title: 'Sea FCL Import', value: 'SFI', icon: 'ship' },
        { title: 'Sea LCL Export', value: 'SLE', icon: 'ship' },
        { title: 'Sea LCL Import', value: 'SLI', icon: 'ship' },
        { title: 'Custom Logistics', value: 'CL', icon: 'truck' },
    ];

    constructor(
        private readonly _toastService: ToastrService,
        private readonly _router: Router,
        private readonly _store: Store<IWorkOrderListState>,
        private readonly _sortService: SortService
    ) {
        super();
        this.requestList = this.requestListWorkOrder;
        this.requestSort = this.sortWorkOrderList;
    }

    ngOnInit(): void {
        this.isLoading = of(false);
        this.headers = [
            { title: 'Work Order No.', field: 'code', sortable: true },
            { title: 'Customer', field: 'partnerName', sortable: true },
            { title: 'Salesman', field: 'salesmanName', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'POL/POD', field: 'polCode', sortable: true },
            // { title: 'Approval Status', field: 'approvedStatus', sortable: true },
            { title: 'Creator', field: 'userNameCreated', sortable: true },
            { title: 'Datetime Created', field: 'datetimeCreated', sortable: true },
        ];
        this.configSearch = {
            settingFields: this.headers.map(x => ({ "fieldName": x.field, "displayName": x.title })),
            typeSearch: CommonEnum.TypeSearch.outtab
        };


        this.isLoading = this._store.select(workOrderLoadingState);
        this.getWorkOrders();


        this._store.select(workOrderSearchState)
            .pipe(
                withLatestFrom(this._store.select(workOrderPagingState)),
                map(([dataSearch, pagingData]) => ({ page: pagingData.page, pageSize: pagingData.pageSize, dataSearch: dataSearch })),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (data) => {
                    if (!!data.dataSearch) {
                        this.dataSearch = data.dataSearch;
                    }

                    this.page = data.page;
                    this.pageSize = data.pageSize;

                    this.requestListWorkOrder();

                }
            );
    }

    requestListWorkOrder() {
        this._store.dispatch(LoadListWorkOrder({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));

    }

    getWorkOrders() {
        this._store.select(workOrderListState)
            .pipe(
                map((data: any) => {
                    return {
                        data: data.data,
                        totalItems: data.totalItems,
                    };
                }),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: any) => {
                    this.workOrders = res.data || [];
                    console.log(this.workOrders);
                    this.totalItems = res.totalItems || 0;
                },
            );
    }

    sortWorkOrderList(sortField: string, order: boolean) {
        this.workOrders = this._sortService.sort(this.workOrders, sortField, order);
    }

    createWO(transactionType: string) {
        this._router.navigate([`${RoutingConstants.COMMERCIAL.WO}/new`, transactionType]);
    }
}
