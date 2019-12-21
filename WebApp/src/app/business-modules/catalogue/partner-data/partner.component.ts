import { Component, OnInit, ViewChild } from '@angular/core';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { PARTNERDATACOLUMNSETTING } from './partner-data.columns';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { Partner } from 'src/app/shared/models/catalogue/partner.model';
import { Router } from '@angular/router';
import { SystemConstants } from 'src/constants/system.const';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { PartnerListComponent } from './components/partner-list/partner-list.component';
import { NgProgress } from '@ngx-progressbar/core';
import { AppList } from 'src/app/app.list';
import { ExportRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { DeleteConfirmModalComponent, ConfirmPopupComponent } from '@common';
type PARTNERDATA_TAB = 'allTab' | 'Customer' | 'Agent' | 'Carrier' | 'Consginee' | 'Shipper';
enum PartnerDataTab {
    ALL = 'allTab',
    CUSTOMER = 'Customer',
    AGENT = 'Agent',
    CARRIER = 'Carrier',
    CONSIGNEE = 'Consignee',
    SHIPPER = 'Shipper'
}

@Component({
    selector: 'app-partner',
    templateUrl: './partner.component.html'
})
export class PartnerComponent extends AppList implements OnInit {
    pager: PagerSetting = PAGINGSETTING;
    partnerDataSettings: ColumnSetting[] = PARTNERDATACOLUMNSETTING;
    configSearch: any = {
        settingFields: this.partnerDataSettings.filter(x => x.allowSearch === true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
        typeSearch: TypeSearch.intab
    };

    criteria: any = { partnerGroup: PartnerGroupEnum.CUSTOMER };
    partner: Partner;
    tabName = {
        customerTab: "customerTab",
        agentTab: "agentTab",
        carrierTab: "carrierTab",
        consigneeTab: "consigneeTab",
        shipperTab: "shipperTab",
        allTab: "allTab"
    };
    addButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.add
    };
    importButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.import
    };
    exportButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.export
    };
    activeTab: string = this.tabName.allTab;
    @ViewChild(PartnerListComponent, { static: false }) allPartnerComponent: any;
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmDeletePopup: any;

    selectedTab: PARTNERDATA_TAB = PartnerDataTab.ALL; // Default tab.

    constructor(private baseService: BaseService,
        private api_menu: API_MENU,
        private router: Router,
        private _ngProgressService: NgProgress,
        private _exportRepo: ExportRepo) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit() {
        this.tabSelect(this.activeTab);
    }
    resetSearch(event) {
        this.onSearch(event);
    }
    onSearch(event) {
        if (event.field === "All") {
            this.criteria.all = event.searchString;
        } else {
            const currentTab = this.criteria.partnerGroup;
            this.criteria = {};
            this.criteria.partnerGroup = currentTab;
            if (event.field === "id") {
                this.criteria.id = event.searchString;
            }
            if (event.field === "shortName") {
                this.criteria.shortName = event.searchString;
            }
            if (event.field === "addressVn") {
                this.criteria.addressVn = event.searchString;
            }
            if (event.field === "taxCode") {
                this.criteria.taxCode = event.searchString;
            }
            if (event.field === "tel") {
                this.criteria.tel = event.searchString;
            }
            if (event.field === "fax") {
                this.criteria.fax = event.searchString;
            }
            if (event.field === "userCreatedName") {
                this.criteria.userCreated = event.searchString;
            }
        }
        this.allPartnerComponent.dataSearch = this.criteria;
        this.allPartnerComponent.getPartners();
        this.baseService.spinnerHide();
    }
    tabSelect(tabName) {
        this.pager.currentPage = 1;
        this.pager.pageSize = SystemConstants.OPTIONS_PAGE_SIZE;
        this.activeTab = tabName;

        if (tabName === this.tabName.customerTab) {
            this.criteria.partnerGroup = PartnerGroupEnum.CUSTOMER;
        }
        if (tabName === this.tabName.agentTab) {
            this.criteria.partnerGroup = PartnerGroupEnum.AGENT;
        }
        if (tabName === this.tabName.carrierTab) {
            this.criteria.partnerGroup = PartnerGroupEnum.CARRIER;
        }
        if (tabName === this.tabName.consigneeTab) {
            this.criteria.partnerGroup = PartnerGroupEnum.CONSIGNEE;
        }
        if (tabName === this.tabName.shipperTab) {
            this.criteria.partnerGroup = PartnerGroupEnum.SHIPPER;
        }
        if (tabName === this.tabName.allTab) {
            this.criteria.partnerGroup = PartnerGroupEnum.ALL;
        }
        this.baseService.spinnerHide();
    }

    showConfirmDelete(event) {
        this.partner = event;
        this.confirmDeletePopup.show();
    }
    showDetail(event) {
        this.partner = event;
        this.router.navigate([`/home/catalogue/partner-data/detail/${this.partner.id}`]);
    }
    async onDelete() {
        this.baseService.delete(this.api_menu.Catalogue.PartnerData.delete + this.partner.id).subscribe((response: any) => {
            this.tabSelect(this.activeTab);
            this.baseService.successToast(response.message);

        }, err => {
            this.baseService.errorToast(err.error.message);
        });
    }

    addPartner() {
        this.router.navigate(["/home/catalogue/partner-data/add", { partnerType: this.criteria.partnerGroup }]);
    }
    async export() {
        this._progressRef.start();
        this._exportRepo.exportPartner(this.criteria)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'partner.xlsx');
                }
            );
    }
}
