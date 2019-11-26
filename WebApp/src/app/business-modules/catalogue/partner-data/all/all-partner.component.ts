import { Component, OnInit, ViewChild, EventEmitter, Output } from '@angular/core';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { PARTNERDATACOLUMNSETTING } from '../partner-data.columns';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { ExportRepo } from 'src/app/shared/repositories/export.repo';
import { AppList } from 'src/app/app.list';
import { catchError } from 'rxjs/operators';


@Component({
    selector: 'app-all-partner',
    templateUrl: './all-partner.component.html',
    styleUrls: ['./all-partner.component.scss']
})
export class AllPartnerComponent extends AppList implements OnInit {
    partners: Array<Partner>;
    pager: PagerSetting = PAGINGSETTING;
    partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
    criteria: any = { partnerGroup: PartnerGroupEnum.ALL };
    isDesc: boolean = false;

    @ViewChild(AppPaginationComponent, { static: false }) child;
    @Output() deleteConfirm = new EventEmitter<Partner>();
    @Output() detail = new EventEmitter<Partner>();
    constructor(private baseService: BaseService,
        private api_menu: API_MENU,
        private sortService: SortService,
        private _exportRepository: ExportRepo) {
        super();
    }

    ngOnInit() {
    }
    setPageAll(pager: PagerSetting): any {
        this.getPartnerData(pager, this.criteria);
    }
    async getPartnerData(pager: PagerSetting, criteria?: any) {
        if (criteria !== undefined) {
            this.criteria = criteria;
        }
        const responses = await this.baseService.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.criteria, false, true);
        this.partners = responses.data;
        this.pager.totalItems = responses.totalItems;
    }
    onSortChange(column) {
        const property = column.primaryKey;
        this.isDesc = !this.isDesc;
        this.partners = this.sortService.sort(this.partners, property, this.isDesc);
    }
    showConfirmDelete(item) {
        this.deleteConfirm.emit(item);
    }
    showDetail(item) {
        this.detail.emit(item);
    }

    async exportAll() {
        this.criteria.author = localStorage.getItem("currently_userName");
        this.criteria.partnerType = "All";
        this._exportRepository.exportPartner(this.criteria)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/ms-excel", "PartnerData.xlsx");
                },
            );
    }

}
