import { Component, OnInit, ViewChild } from '@angular/core';
import { PagingService } from 'src/app/shared/services/paging-service';
import { SortService } from 'src/app/shared/services/sort.service';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { SystemConstants } from 'src/constants/system.const';
import { InfoPopupComponent } from 'src/app/shared/common/popup';
import { AppPage } from 'src/app/app.base';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { forkJoin } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

@Component({
    selector: 'app-job-charge-import',
    templateUrl: './job-charge-import.component.html'
})
export class JobManagementChargeImportComponent extends AppPage implements OnInit {
    @ViewChild(InfoPopupComponent) invaliDataAlert: InfoPopupComponent;
    data: any[];
    pagedItems: any[] = [];
    inValidItems: any[] = [];
    totalValidRows: number = 0;
    totalRows: number = 0;
    isShowInvalid: boolean = true;
    pager: PagerSetting = PAGINGSETTING;
    isDesc = true;
    sortKey: string = 'code';
    transactionType: string = '';

    constructor(
        private _documentRepo: DocumentationRepo,
        private pagingService: PagingService,
        private sortService: SortService,
        private _toastService: ToastrService,
        private route: ActivatedRoute
    ) {
        super();
    }

    ngOnInit() {
        this.pager.totalItems = 0;
        this.subscriptionJobOpsType();
    }

    subscriptionJobOpsType() {
        this.subscription =
            this.route.data
                .pipe(
                    takeUntil(this.ngUnsubscribe)
                ).subscribe((res: any) => { this.transactionType = res.transactionType; }
                );
    }


    chooseFile(file: Event) {
        this.pager.totalItems = 0;
        if (file.target['files'] == null) { return; }
        this._documentRepo.upLoadChargeFile(file.target['files'], this.transactionType)
            .subscribe((response: any) => {
                this.data = response.data;
                this.pager.currentPage = 1;
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
            this.pagingData(this.data);
        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pagingData(this.inValidItems);
            this.pager.totalItems = this.inValidItems.length;
        }
    }


    import(element) {
        if (this.data == null) { return; }
        if (this.totalRows - this.totalValidRows > 0) {
            this.invaliDataAlert.show();
        } else {
            const data = this.data.filter(x => x.isValid);
            const sells = data.filter(x => ['selling', 'sell', 'debit'].includes((x.type || '').toLowerCase()));
            const obhs = data.filter(x => ['obh'].includes((x.type || '').toLowerCase()));

            const sellGrp = sells.reduce((acc, curr) => {
                const key = `${curr.paymentObjectId}_${curr.hblid}`; // * guid_guiid
                if (!acc[key]) {
                    acc[key] = [];
                }
                acc[key].push(curr);
                return acc;
            }, {} as { [key: string]: { hblid: string, paymentObjectId: string }[] });

            const obhGrp = obhs.reduce((acc, curr) => {
                const key = `${curr.paymentObjectId}_${curr.hblid}`; // * guid_guiid
                if (!acc[key]) {
                    acc[key] = [];
                }
                acc[key].push(curr);
                return acc;
            }, {} as { [key: string]: { hblid: string, paymentObjectId: string }[] });

            const sellResult = Object.entries(sellGrp).map(([key]) => {
                const [paymentObjectId, hblid] = key.split("_");
                return {
                    paymentObjectId,
                    hblid,
                    type: 5
                };
            });

            const obhResult = Object.entries(obhGrp).map(([key]) => {
                const [paymentObjectId, hblid] = key.split("_");
                return {
                    paymentObjectId,
                    hblid,
                    type: 9
                };
            });
            const finalResult = [...sellResult, ...obhResult];
            if (!!finalResult.length) {
                const criteriaCheckpointsObs = finalResult.map(x => ({
                    partnerId: x.paymentObjectId,
                    hblId: x.hblid,
                    transactionType: 'CL',
                    type: x.type || 5
                })).map(y => this._documentRepo.validateCheckPointContractPartner(y));

                forkJoin(criteriaCheckpointsObs)
                    .subscribe(
                        (res: CommonInterface.IResult | any) => {
                            if (res.length === criteriaCheckpointsObs.length) {
                                this.handleImportCharge(data);
                                this.reset(element);
                            }
                        }
                    )
            } else {
                this.handleImportCharge(data);
                this.reset(element);
            }
        }
    }

    handleImportCharge(data) {
        this._documentRepo.importCharge(data)
            .subscribe(
                (res) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.pager.totalItems = 0;
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    downloadSample() {
        this._documentRepo.downloadChargeExcel()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, "LogisticsImportChargeTemplate.xlsx");
                },
            );
    }

    reset(element) {
        this.data = null;
        this.pagedItems = null;
        element.value = "";
        this.pager.totalItems = 0;
    }
    selectPageSize() {
        this.pager.currentPage = 1;
        if (this.isShowInvalid) {
            this.pager.totalItems = this.data.length;
            this.pagingData(this.data);

        } else {
            this.inValidItems = this.data.filter(x => !x.isValid);
            this.pagingData(this.inValidItems);
            this.pager.totalItems = this.inValidItems.length;
        }
    }
    pageChanged(event: any): void {
        if (this.pager.currentPage !== event.page || this.pager.pageSize !== event.itemsPerPage) {
            this.pager.currentPage = event.page;
            this.pager.pageSize = event.itemsPerPage;

            this.pagingData(this.data);
        }
    }
}
