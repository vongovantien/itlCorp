import { Component, OnInit, ViewChild } from '@angular/core';
import { PagingService } from 'src/app/shared/services/paging-service';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { SystemConstants } from 'src/constants/system.const';
import { AppPaginationComponent } from 'src/app/shared/common/pagination/pagination.component';
import { NgProgress, NgProgressComponent } from '@ngx-progressbar/core';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { AppPage } from 'src/app/app.base';
import { finalize, catchError } from 'rxjs/operators';
@Component({
  selector: 'app-stage-import',
  templateUrl: './stage-import.component.html'
})
export class StageImportComponent extends AppPage implements OnInit {
  @ViewChild(InfoPopupComponent, { static: false }) importAlert: InfoPopupComponent;
  data: any[];
  pagedItems: any[] = [];
  inValidItems: any[] = [];
  totalValidRows: number = 0;
  totalRows: number = 0;
  isShowInvalid: boolean = true;
  pager: PagerSetting = PAGINGSETTING;

  constructor(
    public ngProgress: NgProgress,
    private pagingService: PagingService,
    private sortService: SortService,
    private _progressService: NgProgress,
    private catalogueRepo: CatalogueRepo,
    private _toastService: ToastrService
  ) { 
    super();
    this._progressRef = this._progressService.ref();
  }
  @ViewChild(AppPaginationComponent, { static: false }) child: any;
  @ViewChild('form', { static: false }) form: any;
  @ViewChild(NgProgressComponent, { static: false }) progressBar: NgProgressComponent;
  @ViewChild(InfoPopupComponent, { static: false }) invaliDataAlert: InfoPopupComponent;

  isDesc = true;
  sortKey: string;
  inputFile: string;

  ngOnInit() {
    this.pager.totalItems = 0;
  }

  chooseFile(file: Event) {
    // if (file.target['files'] == null) { return; }
    // this.progressBar.start();
    // this.baseService.uploadfile(this.menu_api.Catalogue.Stage_Management.uploadExel, file.target['files'], "uploadedFile")
    //   .subscribe(res => {
    //     this.data = res['data'];
    //     this.pager.totalItems = this.data.length;
    //     this.totalValidRows = res['totalValidRows'];
    //     this.totalRows = this.data.length;
    //     this.pagingData(this.data);
    //     this.progressBar.complete();
    //   }, err => {
    //     this.progressBar.complete();
    //     this.baseService.handleError(err);
    //   })
    this.pager.totalItems = 0;
    if (file.target['files'] == null) { return; }
    this._progressRef.start();
    this.catalogueRepo.upLoadStageFile(file.target['files'])
      .pipe(
        finalize(() => {
          this._progressRef.complete();
        })
      )
      .subscribe((response: any) => {
        this.data = response.data;
        this.pager.totalItems = this.data.length;
        this.totalValidRows = response.totalValidRows;
        this.totalRows = this.data.length;
        this.pagingData(this.data);
      }, () => {
      });
  }

  pagingData(data: any[]) {
    this.pager = this.pagingService.getPager(this.pager.totalItems, this.pager.currentPage, this.pager.pageSize);
    this.pager.numberPageDisplay = SystemConstants.OPTIONS_NUMBERPAGES_DISPLAY;
    this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
    this.pagedItems = data.slice(this.pager.startIndex, this.pager.endIndex + 1);
  }

  async setPage(pager: PagerSetting) {
    this.pager.currentPage = pager.currentPage;
    this.pager.pageSize = pager.pageSize;
    this.pager.totalPages = pager.totalPages;
    if (this.isShowInvalid) {
      this.pager = this.pagingService.getPager(this.data.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.data.slice(this.pager.startIndex, this.pager.endIndex + 1);
    } else {
      this.pager = this.pagingService.getPager(this.inValidItems.length, this.pager.currentPage, this.pager.pageSize, this.pager.numberPageDisplay);
      this.pager.numberToShow = SystemConstants.ITEMS_PER_PAGE;
      this.pagedItems = this.inValidItems.slice(this.pager.startIndex, this.pager.endIndex + 1);
    }
  }
  sort(property: string) {
    this.isDesc = !this.isDesc;
    this.sortKey = property;
    this.pagedItems = this.sortService.sort(this.pagedItems, property, this.isDesc);
  }

  hideInvalid() {
    if (this.data == null) { return; }
    this.isShowInvalid = !this.isShowInvalid;
    this.sortKey = '';
    if (this.isShowInvalid) {
      this.pager.totalItems = this.data.length;
    } else {
      this.inValidItems = this.data.filter(x => !x.isValid);
      this.pager.totalItems = this.inValidItems.length;
    }
    this.child.setPage(this.pager.currentPage);
  }


  async import(element) {
    // if (this.data == null) { return; }
    // if (this.totalRows - this.totalValidRows > 0) {
    //   this.invaliDataAlert.show();
    // } else {
    //   const validItems = this.data.filter(x => x.isValid);
    //   const response = await this.baseService.postAsync(this.menu_api.Catalogue.Stage_Management.import, validItems, true, true);
    //   if (response) {
    //     this.baseService.successToast(language.NOTIFI_MESS.IMPORT_SUCCESS);
    //     this.pager.totalItems = 0;
    //     this.reset();
    //   }
    // }
    if (this.data == null) { return; }
    if (this.totalRows - this.totalValidRows > 0) {
      this.importAlert.show();
    } else {
      this._progressRef.start();
      const data = this.data.filter(x => x.isValid);
      this.catalogueRepo.importStage(data)
        .pipe(
          finalize(() => {
            this._progressRef.complete();
          })
        )
        .subscribe(
          (res) => {
            if (res.success) {
              this._toastService.success('Import commodity successful');
              this.pager.totalItems = 0;
              this.reset(element);
            } else {
              this._toastService.error(res.message);
            }
          }
        );
    }
  }

  async downloadSample() {
    //await this.baseService.downloadfile(this.menu_api.Catalogue.Stage_Management.downloadExcel, 'ImportStageTemplate.xlsx');
    this.catalogueRepo.downloadStageExcel()
      .pipe(catchError(this.catchError))
      .subscribe(
        (res: any) => {
          this.downLoadFile(res, "application/ms-excel", "ImportStageTemplate.xlsx");
        },
      );
  }

  reset(element) {
    this.data = null;
    this.pagedItems = null;
    element.value = "";
    this.pager.totalItems = 0;
  }

}
