import { Component, OnInit, ChangeDetectorRef, Input, ViewChild } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.mode';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagingService } from 'src/app/shared/services/paging-service';
import { SystemConstants } from 'src/constants/system.const';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { SortService } from 'src/app/shared/services/sort.service';
import { AddMoreModalComponent } from './add-more-modal/add-more-modal.component';
import { CustomDeclarationRepo } from 'src/app/shared/repositories';
import { catchError, finalize, takeUntil } from 'rxjs/operators';
import { AppPage } from 'src/app/app.base';

@Component({
  selector: 'app-billing-custom-declaration',
  templateUrl: './billing-custom-declaration.component.html',
  styleUrls: ['./billing-custom-declaration.component.scss']
})
export class BillingCustomDeclarationComponent extends AppPage implements OnInit {
  @ViewChild(AddMoreModalComponent, { static: false }) poupAddMore: AddMoreModalComponent;
  @Input() currentJob: OpsTransaction;
  notImportedCustomClearances: any[];
  customClearances: any[];
  pagerMaster: PagerSetting = PAGINGSETTING;
  notImportedData: any[];
  importedData: any[];
  searchImportedString: string = '';
  checkAllImported = false;
  dataImportedSearch: any[];

  constructor(private baseServices: BaseService,
    private api_menu: API_MENU,
    private pagerService: PagingService,
    private sortService: SortService,
    private customClearanceRepo: CustomDeclarationRepo) {
    super();
  }

  async ngOnInit() {
    this.pagerMaster.currentPage = 1;
    this.pagerMaster.totalItems = 0;
    if (this.currentJob != null) {
      this.getCustomClearanesOfJob(this.currentJob.jobNo);
      this.getListCleranceNotImported();
    }
  }

  async getCustomClearanesOfJob(jobNo: string) {
    this.customClearanceRepo.getListImportedInJob(jobNo).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { })
    ).subscribe(
      (res: any) => {
        this.importedData = res;
        if (this.importedData != null) {
          this.importedData.forEach(element => {
            element.isChecked = false;
          });
        } else {
          this.importedData = [];
        }
        this.dataImportedSearch = this.importedData;
        this.setPageMaster(this.pagerMaster);
      }
    );
  }
  removeChecked() {
    this.checkAllImported = false;
    const checkedData = this.importedData.filter(x => x.isChecked === true);
    if (checkedData.length > 0) {
      for (let i = 0; i < checkedData.length; i++) {
        const index = this.importedData.indexOf(x => x.id === checkedData[i].id);
        if (index > -1) {
          this.importedData[index] = true;
        }
      }
    }
  }
  isDesc = true;
  sortKey: string = "";
  sort(property) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.customClearances = this.sortService.sort(this.customClearances, property, this.isDesc);
  }
  async removeImported() {
    const dataToUpdate = this.importedData.filter(x => x.isChecked === true);
    if (dataToUpdate.length > 0) {
      dataToUpdate.forEach(x => {
        x.jobNo = null;
      });
      const responses = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.updateToAJob, dataToUpdate, false, true);
      if (responses.success === true) {
        await this.getCustomClearanesOfJob(this.currentJob.jobNo);
        this.updateShipmentVolumn();
      }
    }
  }
  getListCleranceNotImported() {
    this.customClearanceRepo.getListNotImportToJob(false).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { })
    ).subscribe(
      (res: any) => {
        this.notImportedData = res;
      }
    );
  }
  async showPopupAdd() {
    this.customClearanceRepo.getListNotImportToJob(false).pipe(
      takeUntil(this.ngUnsubscribe),
      catchError(this.catchError),
      finalize(() => { })
    ).subscribe(
      (res: any) => {
        if (res) {
          this.poupAddMore.notImportedData = this.poupAddMore.dataNotImportedSearch = this.notImportedData;
          this.poupAddMore.getDataNotImported();
          this.poupAddMore.show({ backdrop: 'static' });
        }
      }
    );
  }
  changeAllImported() {
    if (this.checkAllImported) {
      this.customClearances.forEach(x => {
        x.isChecked = true;
      });
    } else {
      this.customClearances.forEach(x => {
        x.isChecked = false;
      });
    }
    const checkedData = this.customClearances.filter(x => x.isChecked === true);
    if (checkedData.length > 0) {
      for (let i = 0; i < checkedData.length; i++) {
        const index = this.importedData.indexOf(x => x.id === checkedData[i].id);
        if (index > -1) {
          this.importedData[index] = true;
        }
      }
    }
  }
  async closeAddMore(event) {
    if (event) {
      await this.getCustomClearanesOfJob(this.currentJob.jobNo);
      this.updateShipmentVolumn();
    }
  }
  async updateShipmentVolumn() {
    if (this.importedData != null) {
      this.currentJob.sumGrossWeight = 0;
      this.currentJob.sumNetWeight = 0;
      this.currentJob.sumCbm = 0;
      if (this.importedData.length > 0) {
        for (let i = 0; i < this.importedData.length; i++) {
          this.currentJob.sumGrossWeight = this.currentJob.sumGrossWeight + this.importedData[i].grossWeight == null ? 0 : this.importedData[i].grossWeight;
          this.currentJob.sumNetWeight = this.currentJob.sumNetWeight + this.importedData[i].netWeight == null ? 0 : this.importedData[i].netWeight;
          this.currentJob.sumCbm = this.currentJob.sumCbm + this.importedData[i].cbm == null ? 0 : this.importedData[i].cbm;
        }
      }
      if (this.currentJob.sumGrossWeight === 0) {
        this.currentJob.sumGrossWeight = null;
      }
      if (this.currentJob.sumNetWeight === 0) {
        this.currentJob.sumNetWeight = null;
      }
      if (this.currentJob.sumCbm === 0) {
        this.currentJob.sumCbm = null;
      }
      await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.currentJob, false, false);
    }
  }
  setPageMaster(pager: PagerSetting) {
    this.pagerMaster = this.pagerService.getPager(this.dataImportedSearch.length, pager.currentPage, this.pagerMaster.pageSize, this.pagerMaster.totalPageBtn);
    this.pagerMaster.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pagerMaster.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    if (this.dataImportedSearch != null) {
      this.dataImportedSearch = this.sortService.sort(this.dataImportedSearch, 'clearanceNo', true);
      this.customClearances = this.dataImportedSearch.slice(this.pagerMaster.startIndex, this.pagerMaster.endIndex + 1);
    }
  }
  searchClearanceImported(event) {
    this.pagerMaster.totalItems = 0;
    const keySearch = this.searchImportedString.trim().toLocaleLowerCase();
    if (keySearch !== null && keySearch.length < 2 && keySearch.length > 0) {
      return 0;
    }
    this.dataImportedSearch = this.importedData.filter(item => item.clearanceNo.includes(keySearch)
      || (item.hblid == null ? '' : item.hblid.toLocaleLowerCase()).includes(keySearch)
      || (item.exportCountryCode == null ? '' : item.exportCountryCode.toLocaleLowerCase()).includes(keySearch)
      || (item.importCountryCode == null ? '' : item.importCountryCode.toLocaleLowerCase()).includes(keySearch)
      || (item.commodityCode == null ? '' : item.commodityCode.toLocaleLowerCase()).includes(keySearch)
      || (item.firstClearanceNo == null ? '' : item.firstClearanceNo.toLocaleLowerCase()).includes(keySearch)
      || (item.qtyCont == null ? '' : item.qtyCont.toString()).includes(keySearch));
    this.pagerMaster.currentPage = 1;
    this.pagerMaster.totalItems = this.dataImportedSearch.length;
    this.setPageMaster(this.pagerMaster);
  }
}
