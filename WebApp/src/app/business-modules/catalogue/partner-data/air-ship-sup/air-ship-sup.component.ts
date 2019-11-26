import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';

import { ExportRepo } from 'src/app/shared/repositories/export.repo';
import { catchError } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-air-ship-sup',
    templateUrl: './air-ship-sup.component.html',
    styleUrls: ['./air-ship-sup.component.scss']
})
export class AirShipSupComponent extends AppList implements OnInit {
    airShips: Array<Partner>;
    pager: PagerSetting = PAGINGSETTING;
    partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
    criteria: any = { partnerGroup: PartnerGroupEnum.AIRSHIPSUP };
    isDesc: boolean = false;
    keySortDefault: string = "id";
    @ViewChild(AppPaginationComponent, { static: false }) child;
    @Output() deleteConfirm = new EventEmitter<any>();
    @Output() detail = new EventEmitter<any>();
    constructor(private baseService: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private _exportRepository: ExportRepo) {
        super();
    }

    ngOnInit() {
    }
    setPage(pager: PagerSetting): any {
        this.getPartnerData(pager, this.criteria);
    }
    async getPartnerData(pager: PagerSetting, criteria?: any) {
        if (criteria !== undefined) {
            this.criteria = criteria;
        }
        const responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
        this.airShips = responses.data;
        this.pager.totalItems = responses.totalItems;
    }
    onSortChange(column) {
        const property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.airShips = this.sortService.sort(this.airShips, property, this.isDesc);
    }

    showConfirmDelete(item) {
        this.deleteConfirm.emit(item);
    }
    showDetail(item) {
        this.detail.emit(item);
    }

    async exportAirShipSup() {
        this.criteria.author = localStorage.getItem("currently_userName");
        this.criteria.partnerType = "Air-Ship-Sup";
        this._exportRepository.exportPartner(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PartnerData.xlsx");
                },
            );
    }
}
