import { Component, OnInit, ViewChild } from '@angular/core';
import * as lodash from 'lodash';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { Router } from '@angular/router';
import { SortService } from 'src/app/shared/services/sort.service';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { ExcelService } from 'src/app/shared/services/excel.service';
import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { SystemConstants } from 'src/constants/system.const';
import { AppList } from 'src/app/app.list';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize, map, tap } from 'rxjs/operators';
import { Charge } from 'src/app/shared/models';
import { NgProgress } from '@ngx-progressbar/core';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'app-charge',
    templateUrl: './charge.component.html',
    styleUrls: ['./charge.component.scss']
})
export class ChargeComponent extends AppList implements OnInit {
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: any;
    headers: CommonInterface.IHeaderTable[];
    dataSearch: any = {};

    constructor(
        private _progressService: NgProgress,
        private baseServices: BaseService,
        private excelService: ExcelService,
        private api_menu: API_MENU,
        private router: Router,
        private sortService: SortService,
        private cataloguage: CatalogueRepo) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestList = this.searchCharge;
        this.requestSort = this.sortCharge;
    }
    pager: PagerSetting = PAGINGSETTING;
    ListCharges: any = [];
    idChargeToUpdate: any = "";
    idChargeToDelete: any = "";
    idChargeToAdd: any = "";
    searchKey: string = "";
    searchObject: any = {};

    async ngOnInit() {
        this.headers = [
            { title: 'Code', field: 'code', sortable: true },
            { title: 'Name EN', field: 'chargeNameEn', sortable: true },
            { title: 'Name Local', field: 'chargeNameVn', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Status', field: 'active', sortable: true }
        ];
        this.searchCharge(this.dataSearch);
    }
    sortCharge(sort: string): void {
        this.ListCharges = this.sortService.sort(this.ListCharges, sort, this.order);
    }

    onSearchCharge(dataSearch: any) {
        this.dataSearch = {};
        if (dataSearch.type === 'All') {
            this.dataSearch.all = dataSearch.keyword;
        } else {
            this.dataSearch.all = null;
            if (dataSearch.type === 'code') {
                this.dataSearch.code = dataSearch.keyword;
            }
            if (dataSearch.type === 'chargeNameEn') {
                this.dataSearch.chargeNameEn = dataSearch.keyword;
            }
            if (dataSearch.type === 'chargeNameVn') {
                this.dataSearch.chargeNameVn = dataSearch.keyword;
            }
            if (dataSearch.type === 'type') {
                this.dataSearch.type = dataSearch.keyword;
            }
        }
        this.searchCharge(this.dataSearch);
    }

    searchCharge(dataSearch: any) {
        this.isLoading = true;
        this._progressRef.start();
        this.cataloguage.getListCharge(this.page, this.pageSize, Object.assign({}, dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((res: any) => {
                    return {
                        data: res.data != null ? res.data.map((item: any) => new Charge(item)) : [],
                        totalItems: res.totalItems,
                    };
                }),
            ).subscribe(
                (res: any) => {
                    this.totalItems = res.totalItems;
                    this.ListCharges = res.data;
                },
            );
    }

    prepareDeleteCharge(id: any) {
        this.idChargeToDelete = id;
        this.confirmDeletePopup.show();
    }

    async onDeleteCharge() {
        await this.baseServices.deleteAsync(this.api_menu.Catalogue.Charge.delete + this.idChargeToDelete, true, true);
        this.searchCharge(this.dataSearch);
        this.confirmDeletePopup.hide();
    }

    public itemsToString(value: Array<any> = []): string {
        return value
            .map((item: any) => {
                return item.text;
            }).join(',');
    }

    async export() {
        let charges = await this.baseServices.postAsync(this.api_menu.Catalogue.Charge.query, this.searchObject);
        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.ENGLISH_API) {
            charges = lodash.map(charges, function (chrg, index) {
                return [
                    index + 1,
                    chrg['code'],
                    chrg['chargeNameEn'],
                    chrg['chargeNameVn'],
                    chrg['type'],
                    (chrg['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.ENGLISH : SystemConstants.STATUS_BY_LANG.ACTIVE.ENGLISH
                ]
            });
        }

        if (localStorage.getItem(SystemConstants.CURRENT_LANGUAGE) === SystemConstants.LANGUAGES.VIETNAM_API) {
            charges = lodash.map(charges, function (chrg, index) {
                return [
                    index + 1,
                    chrg['code'],
                    chrg['chargeNameEn'],
                    chrg['chargeNameVn'],
                    chrg['type'],
                    (chrg['inactive'] === true) ? SystemConstants.STATUS_BY_LANG.INACTIVE.VIETNAM : SystemConstants.STATUS_BY_LANG.ACTIVE.VIETNAM
                ]
            });
        }
        const exportModel: ExportExcel = new ExportExcel();
        exportModel.title = "Charge List";
        const currrently_user = localStorage.getItem('currently_userName');
        exportModel.author = currrently_user;
        exportModel.header = [
            { name: "No.", width: 10 },
            { name: "Code", width: 20 },
            { name: "English Name", width: 20 },
            { name: "Local Name", width: 20 },
            { name: "Type", width: 20 },
            { name: "Inactive", width: 20 }
        ]
        exportModel.data = charges;
        exportModel.fileName = "Charges";

        this.excelService.generateExcel(exportModel);

    }

}
