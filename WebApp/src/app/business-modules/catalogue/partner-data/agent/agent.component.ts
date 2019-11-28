import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { SortService } from 'src/app/shared/services/sort.service';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';

import { ExportExcel } from 'src/app/shared/models/layout/exportExcel.models';
import { AppList } from 'src/app/app.list';
import { ExportRepo } from 'src/app/shared/repositories/export.repo';
import { catchError } from 'rxjs/operators';

@Component({
    selector: 'app-agent',
    templateUrl: './agent.component.html',
    styleUrls: ['./agent.component.scss']
})
export class AgentComponent extends AppList implements OnInit {
    agents: Array<Partner>;
    pager: PagerSetting = PAGINGSETTING;
    partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
    criteria: any = { partnerGroup: PartnerGroupEnum.AGENT };
    isDesc: boolean = false;
    keySortDefault = "id";
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
    async getPartnerData(pager: PagerSetting, criteria?: any) {
        if (criteria !== undefined) {
            this.criteria = criteria;
        }
        const responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
        this.agents = responses.data;
        this.pager.totalItems = responses.totalItems;
    }
    onSortChange(column) {
        const property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.agents = this.sortService.sort(this.agents, property, this.isDesc);
    }
    showConfirmDelete(item) {
        this.deleteConfirm.emit(item);
    }
    showDetail(item) {
        this.detail.emit(item);
    }

    async exportAgents() {
        this.criteria.author = localStorage.getItem("currently_userName");
        this.criteria.partnerType = "Agents";
        this._exportRepository.exportPartner(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PartnerData.xlsx");
                },
            );
    }
}
