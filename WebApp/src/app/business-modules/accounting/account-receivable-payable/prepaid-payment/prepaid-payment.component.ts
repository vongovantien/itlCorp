import { G, T } from '@angular/cdk/keycodes';
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { ConfirmPopupComponent, ReportPreviewComponent } from '@common';
import { AccountingConstants } from '@constants';
import { delayTime } from '@decorators';
import { InjectViewContainerRefDirective } from '@directives';
import { ICrystalReport } from '@interfaces';
import { AccountingRepo, DocumentationRepo, ExportRepo } from '@repositories';
import { SortService } from '@services';
import _groupBy from 'lodash/groupBy';
import { ToastrService } from 'ngx-toastr';
import { concatMap, finalize, switchMap } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ARPrePaidPaymentConfirmPopupComponent } from './components/popup-confirm-prepaid/confirm-prepaid.popup';

@Component({
    selector: 'app-prepaid-payment',
    templateUrl: './prepaid-payment.component.html',
})
export class ARPrePaidPaymentComponent extends AppList implements OnInit, ICrystalReport {
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    @ViewChild(ARPrePaidPaymentConfirmPopupComponent) confirmPrepaidPopup: ARPrePaidPaymentConfirmPopupComponent;

    debitNotes: Partial<IPrepaidPayment[]> = [];
    selectedCd: IPrepaidPayment = null;
    totalSelectedItem: number;

    constructor(
        private readonly _accountingRepo: AccountingRepo,
        private readonly _sortService: SortService,
        private readonly _toast: ToastrService,
        private readonly _documentationRepo: DocumentationRepo,
        private readonly _export: ExportRepo,
        private readonly _cd: ChangeDetectorRef


    ) {
        super();
        this.requestList = this.getPaging;
        this.requestSort = this.sortLocal;
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'AR Confirm', field: 'status', sortable: true },
            { title: 'Job ID', field: 'jobNo', sortable: true },
            { title: 'MBL - HBL', field: 'hbl', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Debit Note', field: 'debitNote', sortable: true },
            { title: 'Debit Amount', field: 'totalAmount', sortable: true },
            { title: 'Department', field: 'DeparmentName', sortable: true },
            { title: 'Office', field: 'officeName', sortable: true },
            { title: 'User Created', field: 'userCreatedName', sortable: true },
            { title: 'Issue Date', field: 'datetimeCreated', sortable: true },
            { title: 'Salesman', field: 'salesmanName', sortable: true },
        ];
        this.dataSearch = {
            keywords: []
        }
        this.getPaging();
    }

    getPaging() {
        this.isLoading = true;
        this.totalSelectedItem = 0;
        this.isCheckAll = false;
        this._accountingRepo.getPagingCdNotePrepaid(this.dataSearch, this.page, this.pageSize)
            .pipe(
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (data: CommonInterface.IResponsePaging) => {
                    this.debitNotes = data.data || [];
                    // this.page = data.page;
                    // this.pageSize = data.pageSize;
                    this.totalItems = data.totalItems;
                }
            )
    }

    onSearchData(dataSearch) {
        this.dataSearch = dataSearch;
        this.page = 1;

        this.requestList();
    }

    sortLocal(sort: string) {
        this.debitNotes = this._sortService.sort(this.debitNotes, sort, this.order);
    }

    checkAllCd() {
        if (this.isCheckAll) {
            this.debitNotes.forEach(x => {
                if (x.status === 'Unpaid') {
                    x.isSelected = true;
                }
            });
        } else {
            this.debitNotes.forEach(x => {
                x.isSelected = false;
            });
        }

        this.totalSelectedItem = this.debitNotes.filter(x => x.isSelected === true).length;
    }

    onChangeSelectedCd() {
        const selectedItems = this.debitNotes.filter(x => x.status === 'Unpaid');
        this.isCheckAll = selectedItems.every(x => x.isSelected === true);
        this.totalSelectedItem = selectedItems.filter(x => x.isSelected === true).length;
    }

    onSelectCd(cd: IPrepaidPayment) {
        this.selectedCd = cd;
    }

    export(cd: IPrepaidPayment, format: string) {
        if (!cd) return;
        let url: string;
        let _format = 0;
        switch (format) {
            case 'PDF':
                _format = 5;
                break;
            case 'WORD':
                _format = 3;
                break;
            case 'EXCEL':
                _format = 4;
                break;
            default:
                _format = 5;
                break;
        }
        this._documentationRepo.getDetailsCDNote(cd.jobId, cd.debitNote)
            .pipe(
                switchMap((detail) => {
                    if (cd.transactionType === 'CL') {
                        return this._documentationRepo.previewOPSCdNote({ jobId: cd.jobId, creditDebitNo: cd.debitNote, currency: 'VND', exportFormatType: _format });
                    } else if (cd.transactionType === 'AE' || cd.transactionType === 'AI') {
                        return this._documentationRepo.previewAirCdNote({ jobId: cd.jobId, creditDebitNo: cd.debitNote, currency: 'VND', exportFormatType: _format });
                    }
                    return this._documentationRepo.previewSIFCdNote({ jobId: cd.jobId, creditDebitNo: cd.debitNote, currency: 'VND', exportFormatType: _format });
                }),
                concatMap((x) => {
                    url = x.pathReportGenerate;
                    return this._export.exportCrystalReportPDF(x);
                })
            ).subscribe(
                (res: any) => {

                },
                (error) => {
                    this._export.downloadExport(url);
                },
                () => {
                    console.log(url);
                }
            );
    }

    preview(cd: IPrepaidPayment, currency: string = 'VND') {
        if (!cd) return;
        this._documentationRepo.getDetailsCDNote(cd.jobId, cd.debitNote)
            .pipe(
                switchMap((detail) => {
                    if (cd.transactionType === 'CL') {
                        return this._documentationRepo.previewOPSCdNote({ jobId: cd.jobId, creditDebitNo: cd.debitNote, currency: currency });
                    } else if (cd.transactionType === 'AE' || cd.transactionType === 'AI') {
                        return this._documentationRepo.previewAirCdNote({ jobId: cd.jobId, creditDebitNo: cd.debitNote, currency: currency });
                    }
                    return this._documentationRepo.previewSIFCdNote({ jobId: cd.jobId, creditDebitNo: cd.debitNote, currency: currency });
                }),
            ).subscribe(
                (res: any) => {
                    if (res != null && res?.dataSource.length > 0) {
                        this.dataReport = res;
                        this.renderAndShowReport();
                    } else {
                        this._toast.warning('There is no data to display preview');
                    }
                },
            );
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.viewContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.viewContainerRef.viewContainerRef.clear();
            });
    }

    confirmItem() {
        if (!this.selectedCd) return;
        const selectedCd = Object.assign({}, this.selectedCd);
        if (selectedCd.status !== 'Unpaid') {
            this._toast.warning("Please select debit unpaid");
            return;
        }
        if (selectedCd.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED) {
            this._toast.warning(`${selectedCd.debitNote} was synced, you can not confirm this payment`);
            return;
        }
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: `Are you sure to confirm <strong>${this.selectedCd.debitNote}</strong>`,
            labelConfirm: 'Yes',
            labelCancel: 'No',
            center: true
        }, () => {
            if (selectedCd) {
                const body = {
                    id: selectedCd.id,
                    status: 'Paid',
                }
                this.confirmData([body]);
            }
        });
    }

    private confirmData(body) {
        this._accountingRepo.confirmCdNotePrepaid(body)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toast.success(res.message);
                        this.onSearchData(this.dataSearch);
                    } else {
                        this._toast.error(res.message);
                    }
                }
            )
    }

    confirmSelectedItems() {
        const cdList = this.debitNotes.filter(x => x.isSelected && x.status === 'Unpaid');
        if (!cdList.length) {
            this._toast.warning("Please select debit");
            return;
        }

        const hasSynced: boolean = cdList.some(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED);
        if (hasSynced) {
            const debitHasSynced: string = cdList.filter(x => x.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED).map(a => a.debitNote).toString();
            this._toast.warning(`${debitHasSynced} was synced, you cannot revert this payment`);
            return;
        }

        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.viewContainerRef.viewContainerRef,
            {
                body: `Are you sure you want to confirm paid <span class="font-weight-bold">${cdList.map(x => x.debitNote).join("</br>")}</span>?`,
                iconConfirm: 'la la-cloud-upload',
                labelConfirm: 'Yes',
                center: true

            },
            (v: boolean) => {
                const body = cdList.map(x => ({
                    id: x.id,
                    status: 'Paid'
                }));
                this.confirmData(body);
            });

    }

    revertSelectedItem() {
        const selectedCd = Object.assign({}, this.selectedCd);
        if (selectedCd.status !== 'Paid') {
            this._toast.warning("Please select debit paid");
            return;
        }
        if (selectedCd.syncStatus === AccountingConstants.SYNC_STATUS.SYNCED) {
            this._toast.warning(`${selectedCd.debitNote} had synced, Please recheck!`);
            return;
        }

        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.viewContainerRef.viewContainerRef,
            {
                body: `Are you sure you want to confirm revert paid <span class="font-weight-bold">${selectedCd.debitNote}</span> ?`,
                iconConfirm: 'la la-cloud-upload',
                labelConfirm: 'Yes',
                center: true
            },
            (v: boolean) => {
                const body = {
                    id: selectedCd.id,
                    status: 'Unpaid'
                };
                this.confirmData([body]);
            });
    }

    showUpdatePrepaidPopup() {
        const cdList = this.debitNotes.filter(x => x.isSelected && x.status === 'Unpaid');
        if (!cdList.length) {
            this._toast.warning("Please select debit");
            return;
        }

        const debitGroup = (_groupBy(cdList, 'jobId') || []);

        let results = [];
        Object.entries(debitGroup).forEach((value, index) => {
            const currentDebit = cdList.find(x => x.jobId === value[0]);
            const _total = value[1].reduce((acc, curr) => (acc += curr.totalAmount), 0);
            const itemGrps = {
                job: currentDebit.jobNo,
                partnerName: currentDebit.partnerName,
                currency: currentDebit.currency,
                mbl: currentDebit.mbl,
                hbl: currentDebit.hbl,
                total: _total,
                details: value[1]
            }
            results.push(itemGrps);
        });
        console.log(results);
        this.confirmPrepaidPopup.groupData = [...results];
        this.confirmPrepaidPopup.show();

    }

    onConfirmPaidDataPopup(data) {
        if (!data || !data.length) {
            return;
        }

        const body = data.map(x => ({
            id: x.id,
            status: 'Paid'
        }));
        console.log(body);
        this.confirmData(body);
    }
}

interface IPrepaidPayment {
    id: string;
    jobNo: string;
    jobId: string;
    mbl: string;
    hbl: string;
    debitNote: string;
    totalAmount: number;
    totalAmountVnd: number;
    totalAmountUsd: number;
    paidAmount: number;
    paidAmountVnd: number;
    paidAmountUsd: number;
    currency: string;
    salesmanName: string;
    status: string;
    syncStatus: string;
    lastSyncDate: Date;
    notes: string;
    partnerName: string;
    datetimeCreated: string;
    userCreatedName: string;
    officeName: string;
    departmentName: string;
    transactionType: string;
    [key: string]: any;
}
