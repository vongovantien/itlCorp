import { Component, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { Router } from '@angular/router';
import { ConfirmPopupComponent } from '@common';
import { RoutingConstants } from '@constants';
import { ContextMenuDirective, InjectViewContainerRefDirective } from '@directives';
import { CommonEnum } from '@enums';
import { WorkOrderViewModel } from '@models';
import { Store } from '@ngrx/store';
import { DocumentationRepo } from '@repositories';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { map, takeUntil, withLatestFrom } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { IWorkOrderListState, LoadDetailWorkOrder, LoadListWorkOrder, workOrderListState, workOrderLoadingState, workOrderPagingState, workOrderSearchState } from './store';

@Component({
    selector: 'app-commercial-work-order',
    templateUrl: './commercial-work-order.component.html',
})
export class CommercialWorkOrderComponent extends AppList implements OnInit {
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    workOrders: WorkOrderViewModel[] = [];
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


    selectedWorkOrder: WorkOrderViewModel;

    constructor(
        private readonly _toastService: ToastrService,
        private readonly _router: Router,
        private readonly _store: Store<IWorkOrderListState>,
        private readonly _sortService: SortService,
        private readonly _documentationRepo: DocumentationRepo
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
            { title: 'Source', field: 'source', sortable: true },
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

    gotoDetail(workOrder: WorkOrderViewModel) {
        this._router.navigate([`${RoutingConstants.COMMERCIAL.WO}/`, workOrder]);

    }

    onSelectWorkOrder(workOrder: WorkOrderViewModel) {
        this.selectedWorkOrder = workOrder;
        this.clearMenuContext(this.queryListMenuContext);

    }

    activeWorkOrder(wo: WorkOrderViewModel) {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: `${wo.active ? 'Inactive' : 'Active'} Work Order`,
            body: `Are you sure you want to ${wo.active ? 'Inactive' : 'Active'} this work order?`,
            data: null,
            labelConfirm: 'Yes',
            center: true
        }, () => {
            this._documentationRepo.setActiveInActiveWorkOrder({ id: wo.id, active: !wo.active }).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        wo.active = !wo.active;
                        this._toastService.success(res.message);
                        this._store.dispatch(LoadListWorkOrder({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
                        return;
                    }
                    this._toastService.error(res.message);
                }
            )
        });
    }

    viewDetailWorkOrder(wo: WorkOrderViewModel) {
        this._router.navigate([`${RoutingConstants.COMMERCIAL.WO}/`, this.selectedWorkOrder.id]);
    }

    deleteWorkOrder(wo: WorkOrderViewModel) {
        this._documentationRepo.checkAllowDeleteWorkOrder(wo.id)
            .subscribe((value: boolean) => {
                if (value) {
                    this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                        title: 'Delete Work Order',
                        body: `Are you sure you want to delete this work order?`,
                        labelConfirm: 'Yes',
                        center: true
                    }, () => {
                        this._documentationRepo.deleteWorkOrder(wo.id).subscribe(
                            (res: CommonInterface.IResult) => {
                                if (res.status) {
                                    this._toastService.success(res.message);
                                    this._store.dispatch(LoadListWorkOrder({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
                                    return;
                                }
                                this._toastService.error(res.message);
                            }
                        )
                    });
                }
            }
            )

    }


}
