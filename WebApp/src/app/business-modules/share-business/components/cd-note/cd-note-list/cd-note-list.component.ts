import { HttpResponse } from '@angular/common/http';
import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ChargeConstants, SystemConstants } from '@constants';
import { delayTime } from '@decorators';
import { InjectViewContainerRefDirective } from '@directives';
import { Crystal } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import _uniq from 'lodash/uniq';
import { ToastrService } from 'ngx-toastr';
import { combineLatest, of } from 'rxjs';
import { catchError, concatMap, finalize, map, mergeMap, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ConfirmPopupComponent, InfoPopupComponent } from 'src/app/shared/common/popup';
import { ReportPreviewComponent } from 'src/app/shared/common/report-preview/report-preview.component';
import { TransactionTypeEnum } from 'src/app/shared/enums';
import { AcctCDNote } from 'src/app/shared/models/document/acctCDNote.model';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from 'src/app/shared/repositories';
import { SortService } from 'src/app/shared/services';
import { TransactionActions } from '../../../store';
import { ShareBussinessCdNoteAddPopupComponent } from '../add-cd-note/add-cd-note.popup';
import { ShareBussinessCdNoteDetailPopupComponent } from '../detail-cd-note/detail-cd-note.popup';

@Component({
    selector: 'cd-note-list',
    templateUrl: './cd-note-list.component.html',
    styleUrls: ['./cd-note-list.component.scss']
})
export class ShareBussinessCdNoteListComponent extends AppList {
    @ViewChild(ShareBussinessCdNoteAddPopupComponent) cdNoteAddPopupComponent: ShareBussinessCdNoteAddPopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmDeleteCdNotePopup: ConfirmPopupComponent;
    @ViewChild(InfoPopupComponent) canNotDeleteCdNotePopup: InfoPopupComponent;
    @ViewChild(ShareBussinessCdNoteDetailPopupComponent) cdNoteDetailPopupComponent: ShareBussinessCdNoteDetailPopupComponent;
    @ViewChild('popupDataCombine') reportPrePopup: ReportPreviewComponent;
    @ViewChild(InjectViewContainerRefDirective) public reportContainerRef: InjectViewContainerRefDirective;

    headers: CommonInterface.IHeaderTable[];
    idMasterBill: string = '';
    cdNoteGroups: any[] = [];
    initGroup: any[] = [];
    deleteMessage: string = '';
    selectedCdNoteId: string = '';
    transactionType: TransactionTypeEnum = 0;
    cdNotePrint: AcctCDNote[] = [];

    isDesc = true;
    sortKey: string = '';

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _progressService: NgProgress,
        private _sortService: SortService,
        private _store: Store<TransactionActions>,
        private _activedRouter: ActivatedRoute,
        private _exportRepo: ExportRepo,
        private _fileMngtRepo: SystemFileManageRepo
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit(): void {
        this.subscription = combineLatest([
            this._activedRouter.params,
            this._activedRouter.data,
            this._activedRouter.queryParams
        ]).pipe(
            map(([params, data, qParams]) => ({ ...params, ...data, ...qParams })),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (params: any) => {
                const jobId = params.id || params.jobId;
                const cdNo = params.view;
                const currencyId = params.export;
                if (!!cdNo && !!currencyId) {
                    this.transactionType = +params.transactionType || 0;
                    this.idMasterBill = jobId;
                    this.getListCdNoteWithPreview(this.idMasterBill, cdNo, currencyId)
                } else {
                    if (jobId) {
                        this.transactionType = +params.transactionType || 0;
                        this.idMasterBill = jobId;
                        this.getListCdNote(this.idMasterBill);
                    }
                }
            }
        );

        this.headers = [
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Note No', field: 'code', sortable: true },
            { title: 'Charges Count', field: 'total_charge', sortable: true, },
            { title: 'Balance Amount', field: 'total', sortable: true, width: 220 },
            { title: 'Creator', field: 'userCreated', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'SOA', field: 'soaNo', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'Sync Status', field: 'syncStatus', sortable: true },
            { title: 'Last Sync', field: 'lastSyncDate', sortable: true },
        ];
    }

    ngAfterViewInit() {
        this.cdNoteAddPopupComponent.getListSubjectPartner(this.idMasterBill);
        this.cdNoteDetailPopupComponent.cdNoteEditPopupComponent.getListSubjectPartner(this.idMasterBill);
    }

    openPopupAdd() {
        this.cdNoteAddPopupComponent.action = 'create';
        this.cdNoteAddPopupComponent.transactionType = this.transactionType;
        this.cdNoteAddPopupComponent.currentMBLId = this.idMasterBill;
        this.cdNoteAddPopupComponent.setHeader();
        this.cdNoteAddPopupComponent.show();
    }

    openPopupDetail(jobId: string, cdNote: string) {
        this.cdNoteDetailPopupComponent.jobId = jobId;
        this.cdNoteDetailPopupComponent.cdNote = cdNote;
        this.cdNoteDetailPopupComponent.transactionType = this.transactionType;
        this.cdNoteDetailPopupComponent.setHeader();
        this.cdNoteDetailPopupComponent.getDetailCdNote(jobId, cdNote);
        this.cdNoteDetailPopupComponent.show();
    }

    getListCdNote(id: string) {
        this.isLoading = true;
        const isShipmentOperation = false;
        this._documentationRepo.getListCDNote(id, isShipmentOperation)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.cdNoteGroups = res;
                    this.initGroup = res;
                    const selected = { isSelected: false };
                    this.cdNoteGroups.forEach(element => {
                        element.listCDNote.forEach((item: any[]) => {
                            Object.assign(item, selected);
                        });
                    });
                },
            );
    }

    getListCdNoteWithPreview(id: string, cdNo: string, currencyId: string) {
        this.isLoading = true;
        const isShipmentOperation = false;
        this._documentationRepo.getListCDNote(id, isShipmentOperation)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
            ).subscribe(
                (res: any) => {
                    this.cdNoteGroups = res;
                    this.initGroup = res;
                    const selected = { isSelected: false };
                    this.cdNoteGroups.forEach(element => {
                        element.listCDNote.forEach((item: any[]) => {
                            Object.assign(item, selected);
                        });
                    });
                    let isExist = false;
                    this.cdNoteGroups.forEach(element => {
                        const item = element.listCDNote.filter(cdNote => cdNote.code === cdNo);
                        if (item.length > 0) {
                            isExist = true;
                            element.listCDNote.filter(cdNote => cdNote.code === cdNo).map(cdNote => cdNote.isSelected = true);
                            this.transactionType = item.transactionTypeEnum;
                            this.previewCdNote(currencyId);
                        }
                    }
                    );
                    if (!isExist) {
                        this._toastService.error("CD Note " + cdNo + "does not existed!");
                    }
                },
            );
    }

    checkDeleteCdNote(id: string) {
        this._progressRef.start();
        this._documentationRepo.checkCdNoteAllowToDelete(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            ).subscribe(
                (res: any) => {
                    if (res) {
                        this.selectedCdNoteId = id;
                        this.deleteMessage = `All related information will be lost? Are you sure you want to delete this Credit/Debit Note?`;
                        this.confirmDeleteCdNotePopup.show();
                    } else {
                        this.canNotDeleteCdNotePopup.show();
                    }
                },
            );
    }

    onDeleteCdNote() {
        this._progressRef.start();
        this._documentationRepo.deleteCdNote(this.selectedCdNoteId)
            .pipe(
                catchError(this.catchError),
                finalize(() => {
                    this._progressRef.complete();
                    this.confirmDeleteCdNotePopup.hide();
                })
            ).subscribe(
                (respone: CommonInterface.IResult) => {
                    if (respone.status) {
                        this._toastService.success(respone.message, 'Delete Success !');
                        this.getListCdNote(this.idMasterBill);
                    }
                },
            );
    }

    sortCdNotes(property) {
        this.isDesc = !this.isDesc;
        this.sortKey = property;
        this.cdNoteGroups.forEach(element => {
            element.listCDNote = this._sortService.sort(element.listCDNote, property, this.isDesc);
        });
    }

    // sortCdNotes(sort: string): void {
    //     this.cdNoteGroups.forEach(element => {
    //         element.listCDNote = this._sortService.sort(element.listCDNote, sort, this.order);
    //     });
    // }

    onRequestCdNoteChange($event) {
        this.getListCdNote(this.idMasterBill);
        // Show detail popup
        this.openPopupDetail($event.jobId, $event.code);
    }

    onDeletedCdNote() {
        this.getListCdNote(this.idMasterBill);
    }

    // Charge keyword search
    onChangeKeyWord(keyword: string) {
        this.cdNoteGroups = this.initGroup;
        // TODO improve search.
        if (!!keyword) {
            if (keyword.indexOf('\\') !== -1) { return this.cdNoteGroups = []; }
            keyword = keyword.toLowerCase();
            // Search group
            let dataGrp = this.cdNoteGroups.filter((item: any) => item.partnerNameEn.toLowerCase().toString().search(keyword) !== -1);
            // Không tìm thấy group thì search tiếp list con của group
            if (dataGrp.length === 0) {
                const arrayCharge = [];
                for (const group of this.cdNoteGroups) {
                    const data = group.listCDNote.filter((item: any) => item.type.toLowerCase().toString().search(keyword) !== -1
                        || item.code.toLowerCase().toString().search(keyword) !== -1
                        || item.userCreated.toLowerCase().toString().search(keyword) !== -1
                        || item.soaNo.toLowerCase().toString().search(keyword) !== -1);
                    if (data.length > 0) {
                        arrayCharge.push({ id: group.id, partnerNameEn: group.partnerNameEn, partnerNameVn: group.partnerNameVn, listCDNote: data });
                    }
                }
                dataGrp = arrayCharge;
            }
            return this.cdNoteGroups = dataGrp;
        } else {
            this.cdNoteGroups = this.initGroup;
        }
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.reportContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.reportContainerRef.viewContainerRef.clear();
            });

        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.idMasterBill,
                            hblId: SystemConstants.EMPTY_GUID,
                            templateCode: this.getCodeFromCD(),
                            transactionType: TransactionTypeEnum[this.transactionType]
                        };
                        return this._fileMngtRepo.uploadPreviewTemplateEdoc([body]);
                    }
                    return of(false);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) return;
                    if (res.status) {
                        this._toastService.success(res.message);
                        //this.closeReport();
                    } else {
                        this._toastService.success(res.message || "Upload fail");
                    }
                },
                (errors) => {
                    console.log("error", errors);
                },
                () => {
                    sub.unsubscribe();
                }
            );
    }

    checkValidCDNote() {
        this.cdNotePrint = [];
        const listCheck = [];
        this.cdNoteGroups.forEach(element => {
            const item = element.listCDNote.filter(cdNote => cdNote.isSelected === true);
            if (item.length > 0) {
                listCheck.push(item);
                this.cdNotePrint = item;
            }
        }
        );

        if (listCheck.length === 0) {
            this._toastService.warning("Please select C/D Note to preview.");
            return false;
        }
        const type = [];
        listCheck.forEach(x => x.map(y => type.push(y.type)))
        if (listCheck.length > 1 || _uniq(type).length > 1) {
            this._toastService.warning("You can not print C/D Notes that are different type! Please check again");
            return false;
        }
        return true;
    }

    getCodeFromCD() {
        console.log('running');
        for (const element of this.cdNoteGroups) {
            const item = element.listCDNote.filter(cdNote => cdNote.isSelected === true);
            if (item.length > 0) {
                return item[0].type;
            }
        }
    }

    previewCdNote(data: string) {
        if (!this.checkValidCDNote()) {
            return;
        }

        let transType = '';
        if (this.transactionType === TransactionTypeEnum.AirExport || this.transactionType === TransactionTypeEnum.AirImport) {
            transType = ChargeConstants.AI_CODE;
        } else {
            transType = ChargeConstants.SFI_CODE;
        }

        this._documentationRepo.previewASCdNoteList(this.cdNotePrint, data, transType)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }
}
