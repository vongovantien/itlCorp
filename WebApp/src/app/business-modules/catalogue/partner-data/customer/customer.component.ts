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
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map, } from 'rxjs/operators';
import _map from 'lodash/map';
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
        private excelService: ExcelService,
        private api_menu: API_MENU,
        private _progressService: NgProgress,
        private sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
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
        this.getService();
    }

    async getPartnerData(pager: PagerSetting, criteria?: any) {
        if (criteria != undefined) {
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
        var customers = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.query, this.criteria);
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
            customers = _map(customers, function (cus, index) {
                return [
                    index + 1,
                    cus['id'],
                    cus['partnerNameEn'],
                    cus['shortName'],
                    cus['addressEn'],
                    cus['taxCode'],
                    cus['tel'],
                    cus['fax'],
                    cus['userCreatedName'],
                    cus['datetimeModified'],
                    (cus['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
                ]
            });
        }
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
            customers = _map(customers, function (cus, index) {
                return [
                    index + 1,
                    cus['id'],
                    cus['partnerNameVn'],
                    cus['shortName'],
                    cus['addressVn'],
                    cus['taxCode'],
                    cus['tel'],
                    cus['fax'],
                    cus['userCreatedName'],
                    cus['datetimeModified'],
                    (cus['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
                ]
            });
        }


        const exportModel: ExportExcel = new ExportExcel();
        exportModel.title = "Partner Data - Customers";
        exportModel.sheetName = "Customers"
        const currrently_user = localStorage.getItem('currently_userName');
        exportModel.author = currrently_user;
        exportModel.header = [
            { name: "No.", width: 10 },
            { name: "Partner ID", width: 20 },
            { name: "Full Name", width: 60 },
            { name: "Short Name", width: 20 },
            { name: "Billing Address", width: 60 },
            { name: "Tax Code", width: 20 },
            { name: "Tel", width: 30 },
            { name: "Fax", width: 30 },
            { name: "Creator", width: 30 },
            { name: "Modify", width: 30 },
            { name: "Inactive", width: 20 }
        ]
        exportModel.data = customers;
        exportModel.fileName = "Partner Data - Customers";
        this.excelService.generateExcel(exportModel);
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
