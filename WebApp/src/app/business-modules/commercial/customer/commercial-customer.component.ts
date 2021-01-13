import { Component, OnInit, ViewChild, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

import { AppList } from 'src/app/app.list';

import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { Customer, Contract } from '@models';
import { CatalogueRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import { CommonEnum } from '@enums';
import { RoutingConstants, SystemConstants } from '@constants';
import { Permission403PopupComponent, ConfirmPopupComponent, SearchOptionsComponent } from '@common';

import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { SearchList } from './store/actions/customer.action';
import { ICustomerState, getCustomerSearchParamsState } from './store';


@Component({
    selector: 'app-commercial-customer',
    templateUrl: './commercial-customer.component.html',
})
export class CommercialCustomerComponent extends AppList implements OnInit {

    @ViewChild(Permission403PopupComponent) info403Popup: Permission403PopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(SearchOptionsComponent, { static: true }) searchOptionsComponent: SearchOptionsComponent;

    customers: Customer[] = [];
    saleMans: Contract[] = [];

    dataSearchs: any = [];


    selectedCustomer: Customer;

    headerSalemans: CommonInterface.IHeaderTable[];

    headerSearch: CommonInterface.IHeaderTable[];

    constructor(private _ngProgressService: NgProgress,
        private _store: Store<ICustomerState>,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private _router: Router,
        private _toastService: ToastrService,
        private _exportRepo: ExportRepo,
        private _cd: ChangeDetectorRef) {
        super();

        this._progressRef = this._ngProgressService.ref();
        this.requestSearch = this.onSearch;
        this.requestList = this.getPartners;
        this.requestSort = this.sortPartners;
    }


    ngOnInit(): void {
        this._store.select(getCustomerSearchParamsState)
            .pipe(
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (!!data && !!data.keyword) {
                        this.dataSearchs = data;
                    }

                }
            );
        this.headerSalemans = [
            { title: 'No', field: '', sortable: true },
            { title: 'Salesman', field: 'username', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'CreateDate', field: 'createDate', sortable: true }
        ];
        this.headers = [
            { title: 'Partner ID', field: 'accountNo', sortable: true },
            { title: 'Name ABBR', field: 'shortName', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },
            { title: 'Billing Address', field: 'addressVn', sortable: true },

            { title: 'Modify', field: 'datetimeModified', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];

        this.headerSearch = [
            { title: 'Partner ID', field: 'accountNo', sortable: true },
            { title: 'Name ABBR', field: 'shortName', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },
            { title: 'Salesman', field: 'Saleman', sortable: true },
        ];

        this.configSearch = {
            settingFields: this.headerSearch.map(x => ({ "fieldName": x.field, "displayName": x.title })),
            typeSearch: CommonEnum.TypeSearch.outtab
        };
        localStorage.removeItem('success_add_sub');
        this.dataSearch = { All: '' };
        this.dataSearch.partnerType = 'Customer';
        // this.getPartners();
        this.onSearch(this.dataSearch);

    }

    onSearch(event: CommonInterface.ISearchOption) {
        this.dataSearch = {};
        this.dataSearch.partnerType = 'Customer';
        this.dataSearch[event.field || "All"] = event.searchString || '';

        if (!!event.field && event.searchString === "") {
            this.dataSearchs.keyword = "";
        }
        const searchData: ISearchGroup = {
            type: !!event.field ? event.field : this.dataSearchs.type,
            keyword: !!event.searchString ? event.searchString : this.dataSearchs.keyword
        };
        this.page = 1;
        this._store.dispatch(SearchList({ payload: searchData }));
        if (Object.keys(this.dataSearchs).length > 0) {
            const type = this.dataSearchs.type === "userCreatedName" ? "userCreated" : this.dataSearchs.type;
            this.dataSearch[type] = this.dataSearchs.keyword;
        }

        this.requestList();
    }

    ngAfterViewInit() {
        if (Object.keys(this.dataSearchs).length > 0) {
            this.searchOptionsComponent.searchObject.searchString = this.dataSearchs.keyword;
            const type = this.dataSearchs.type === "userCreated" ? "userCreatedName" : this.dataSearchs.type;
            this.searchOptionsComponent.searchObject.field = this.dataSearchs.type;
            this.searchOptionsComponent.searchObject.displayName = this.dataSearchs.type !== "All" ? this.headerSearch.find(x => x.field === type).title : this.dataSearchs.type;
        }
        this._cd.detectChanges();
    }

    getPartners() {
        this.isLoading = true;
        this._progressRef.start();
        this._catalogueRepo.getListPartner(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            })).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.customers = res.data || [];
                    this.totalItems = res.totalItems;
                }
            );
    }

    sortPartners() {
        this.customers = this._sortService.sort(this.customers, this.sort, this.order);
    }

    sortBySaleman(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.saleMans = this._sortService.sort(this.saleMans, sortData.sortField, sortData.order);
        }
    }

    resetSearch(event) {
        this.dataSearch = {};
        this.dataSearchs = {};
        this.onSearch(event);
    }

    showSaleman(customerId: string) {
        this._catalogueRepo.getListContract(customerId)
            .pipe(catchError(this.catchError), finalize(() => {
            })).subscribe(
                (res: any) => {
                    this.saleMans = res || [];
                }
            );
    }

    showConfirmDelete(customer: Customer) {
        this.selectedCustomer = customer;
        this._catalogueRepo.checkDeletePartnerPermission(this.selectedCustomer.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.confirmDeletePopup.show();
                    } else {
                        if (res.data === 403) {
                            this.info403Popup.show();
                        } else {
                            this._toastService.warning("This Customer " + res.message);
                        }
                    }
                }
            );
    }

    onDelete() {
        this._catalogueRepo.checkDeletePartnerPermission(this.selectedCustomer.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) {
                        if (res.data === 403) {
                            this.info403Popup.show();
                        } else {
                            this._toastService.warning("This Customer " + res.message);
                        }
                        this.confirmDeletePopup.hide();
                        return;
                    } else {
                        this.confirmDeletePopup.hide();
                        this._progressRef.start();
                        this._catalogueRepo.deletePartner(this.selectedCustomer.id)
                            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                            .subscribe(
                                (res: CommonInterface.IResult) => {
                                    if (res.status) {
                                        this._toastService.success(res.message);

                                        this.resetSearch({});
                                    } else {
                                        this._toastService.error(res.message);
                                    }
                                }
                            );
                    }
                },
            );
    }

    showDetail(customer: Customer) {
        this.selectedCustomer = customer;
        this._catalogueRepo.checkViewDetailPartnerPermission(this.selectedCustomer.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${this.selectedCustomer.id}`]);
                    } else {
                        if (res.data === 403) {
                            this.info403Popup.show();
                        } else {
                            this._toastService.warning("This Customer " + res.message);
                        }
                    }
                },
            );
    }

    export() {
        this._progressRef.start();
        this._exportRepo.exportPartner(this.dataSearch)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'eFms-commercial-customer.xlsx');
                }
            );
    }
}
interface ISearchGroup {
    type: string;
    keyword: string;
}
