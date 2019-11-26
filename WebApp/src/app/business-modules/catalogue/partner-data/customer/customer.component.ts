import { Component, ViewChild, Output, EventEmitter } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo, ExportRepo } from 'src/app/shared/repositories';
import { catchError, finalize, } from 'rxjs/operators';
import { Saleman } from 'src/app/shared/models/catalogue/saleman.model';


@Component({
    selector: 'app-customer',
    templateUrl: './customer.component.html',
    styleUrls: ['./customer.component.scss']
})
export class CustomerComponent extends AppList {
    customers: Customer[] = [];
    pager: PagerSetting = PAGINGSETTING;
    partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
    criteria: any = { partnerGroup: PartnerGroupEnum.CUSTOMER };
    saleMans: any[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    headers: CommonInterface.IHeaderTable[];
    services: any[] = [];
    @ViewChild(AppPaginationComponent, { static: false }) child;
    @Output() deleteConfirm = new EventEmitter<Partner>();
    @Output() detail = new EventEmitter<any>();
    constructor(
        private baseService: BaseService,
        private api_menu: API_MENU,
        private _progressService: NgProgress,
        private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _exportRepository: ExportRepo
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortCustomers;

    }


    ngOnInit() {
        this.headerSaleman = [
            { title: 'No', field: '', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'CreateDate', field: 'createDate', sortable: true }
        ];

        this.headers = [
            { title: 'Partner ID', field: 'id', sortable: true },
            { title: 'Name ABBR', field: 'shortName', sortable: true },
            { title: 'Billing Address', field: 'addressVn', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Tel', field: 'tel', sortable: true },
            { title: 'Fax', field: 'fax', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },
            { title: 'Modify', field: 'datetimeModified', sortable: true },
            { title: 'Status', field: 'active', sortable: true },

        ];
    }
    async getPartnerData(pager: PagerSetting, criteria?: any) {
        if (criteria !== undefined) {
            this.criteria = criteria;
        }
        const responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
        this.customers = (responses.data || []).map(i => new Customer(i));
        console.log(this.customers);
        this.pager.totalItems = responses.totalItems;
    }
    showConfirmDelete(item) {
        this.deleteConfirm.emit(item);
    }
    showDetail(item) {
        this.detail.emit(item);
    }

    replaceService() {
        for (const item of this.saleMans) {

            this.services.forEach(itemservice => {
                if (item.service === itemservice.id) {
                    item.service = itemservice.text;
                }
            });
        }
    }

    showSaleman(partnerId: string, indexs: number) {
        if (!!this.customers[indexs].saleManRequests.length) {
            this.saleMans = this.customers[indexs].saleManRequests;
            this.replaceService();
        } else {
            this._progressRef.start();
            this._catalogueRepo.getListSaleman(partnerId)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => this._progressRef.complete())
                ).subscribe(
                    (res: Saleman[]) => {
                        this.saleMans = res || [];
                        this.customers[indexs].saleManRequests = this.saleMans;
                        this.replaceService();
                    }
                );
        }
    }

    sortBySaleman(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.saleMans = this.sortService.sort(this.saleMans, sortData.sortField, sortData.order);
        }
    }
    async exportCustomers() {
        this.criteria.author = localStorage.getItem("currently_userName");
        this.criteria.partnerType = "Customers";
        this._exportRepository.exportPartner(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PartnerData.xlsx");
                },
            );
    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                    }
                },
            );
    }

    sortCustomers(sort: string): void {
        this.customers = this.sortService.sort(this.customers, sort, this.order);
    }
}
