import { Component, EventEmitter, Input, OnInit, Output, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { ConfirmPopupComponent } from '@common';
import { ContextMenuDirective, InjectViewContainerRefDirective } from '@directives';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { getCurrentUserState, IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError, skip, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { getGrpChargeSettlementPaymentDetailState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { getTransactionDetailCsTransactionState } from '../../store';
import { ShareDocumentTypeAttachComponent } from '../document-type-attach/document-type-attach.component';
@Component({
    selector: 'files-attach-v2',
    templateUrl: './files-attach-v2.component.html'
})
export class ShareBussinessAttachFileV2Component extends AppList implements OnInit {
    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;
    @Input() typeFrom: string = 'Shipment';
    @Input() billingId: string = '';
    @Output() onChange: EventEmitter<any[]> = new EventEmitter<any[]>();
    headersGen: CommonInterface.IHeaderTable[];
    headersAcc: CommonInterface.IHeaderTable[];
    documentTypes: any[] = [];
    jobId: string = '';
    isOps: boolean = false;
    edocByJob: any[] = [];
    edocByAcc: any[] = [];
    selectedEdoc: IEDoc;
    transactionType: string = '';
    housebills: any[] = [];
    headerAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType', required: true },
    { title: 'Job Ref', field: 'jobRef' },
    { title: 'House Bill No', field: 'hbl' },
    { title: 'Note', field: 'note' },
    { title: 'Source', field: 'source' },]
    accountantAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType', required: true },
    { title: 'Note', field: 'note' },]
    jobNo: string = '';
    constructor(
        private readonly _systemFileRepo: SystemFileManageRepo,
        private readonly _activedRoute: ActivatedRoute,
        private readonly _store: Store<IAppState>,
        private readonly _toast: ToastrService,
        private readonly _exportRepo: ExportRepo,
        private readonly _sortService: SortService,
        private _documentationRepo: DocumentationRepo,
    ) {
        super();
        this.requestSort = this.sortEdoc;
    }
    ngOnInit() {
        console.log(this.typeFrom);

        if (this.typeFrom === 'Shipment') {
            this._activedRoute.params
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((params: Params) => {
                    if (params.jobId) {
                        this.jobId = params.jobId;
                        //this.documentAttach.jobId = params.jobId;
                    } else {
                        this.jobId = params.id;
                        //this.documentAttach.jobId = params.jobId;
                        this.isOps = true;
                    }
                });
            if (this.isOps == false) {
                this._store.select(getTransactionDetailCsTransactionState)
                    .pipe(skip(1), takeUntil(this.ngUnsubscribe))
                    .subscribe(
                        (res: CsTransaction) => {
                            this.transactionType = res.transactionType;
                            //  this.documentAttach.transactionType = res.transactionType;
                            this.getDocumentType(res.transactionType);
                            this.getEDoc(res.transactionType);
                            this.jobNo = res.jobNo;
                            //this.documentAttach.jobNo = res.jobNo;
                            this.isLocked = res.isLocked;
                        }
                    );
            } else {
                this._store.select(getOperationTransationState)
                    .pipe(takeUntil(this.ngUnsubscribe))
                    .subscribe(
                        (res: any) => {
                            this.transactionType = 'CL';
                            //this.documentAttach.transactionType = 'CL'
                            this.getDocumentType('CL');
                            this.getEDoc('CL');
                            this.jobNo = res.opstransaction.jobNo;
                            //this.documentAttach.fileNo = res.jobNo;
                            this.isLocked = res.opstransaction.isLocked;
                            console.log(this.jobNo);

                        }
                    );
            }
        } else {
            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom);
            this.getEDoc(this.typeFrom);
            this.headersAcc = [{ title: 'Alias Name', field: 'systemFileName', sortable: true },
            { title: 'Real File Name', field: 'userFileName', sortable: true },
            { title: 'House Bill No', field: 'hblNo', sortable: true },
            { title: 'Job No', field: 'jobNo' },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
            ];
        }
        this.headers = [
            { title: 'Alias Name', field: 'systemFileName', sortable: true },
            { title: 'Real File Name', field: 'userFileName', sortable: true },
            { title: 'House Bill No', field: 'hblNo', sortable: true },
            { title: 'Billing No', field: 'billingNo', sortable: true },
            { title: 'Source', field: 'source', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
        ];
        this.headersGen = [
            { title: 'Alias Name', field: 'systemFileName', sortable: true },
            { title: 'Real File Name', field: 'userFileName', sortable: true },
            { title: 'House Bill No', field: 'hblNo', sortable: true },
            { title: 'Source', field: 'source', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
        ]
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res) => {
                    this.currentUser = res;
                }
            )
        this.getHblList();
    }
    getHblList() {
        if (this.typeFrom === 'Shipment') {
            this._documentationRepo.getListHouseBillOfJob({ jobId: this.jobId })
                .pipe(
                    catchError(this.catchError),
                ).subscribe(
                    (res: any) => {
                        if (!!res) {
                            console.log(this.housebills);

                        }
                    },
                );
        } else {
            this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            data.forEach(element => {
                                let item = ({
                                    hwbno: element.hbl,
                                    jobNo: element.jobId,
                                    id: element.hblid,
                                    jobId: element.shipmentId,
                                })
                                this.housebills.push(item);
                            }
                            );
                        }
                    }
                );
            //this.chargeSM
        }
    }
    onSelectEDoc(edoc: any) {
        this.selectedEdoc = edoc;
        console.log(edoc);
        this.documentAttach.selectedtDocType = edoc.documentTypeId;
        const qContextMenuList = this.queryListMenuContext.toArray();
        if (!!qContextMenuList.length) {
            qContextMenuList.forEach((c: ContextMenuDirective) => c.close());
        }
    }
    downloadEdoc() {
        this._exportRepo.downloadExport(this.selectedEdoc.imageUrl);
    }
    editEdoc() {
        console.log(this.selectedEdoc);
        if (this.typeFrom === 'Shipment') {
            this.documentAttach.headers = this.headerAttach;
        } else {
            this.documentAttach.headers = this.accountantAttach;
        }
        this.documentAttach.isUpdate = true;
        this.documentAttach.resetForm();
        console.log(this.selectedEdoc);

        let docType = this.documentTypes.find(x => x.id === this.selectedEdoc.documentTypeId);
        let detailSeletedEdoc = ({
            aliasName: this.selectedEdoc.systemFileName,
            name: this.selectedEdoc.userFileName,
            id: this.selectedEdoc.id,
            docType: docType,
            note: this.selectedEdoc.note,
            hwbNo: this.selectedEdoc.hblid,//hblNo 
            hblid: this.selectedEdoc.hblid,//hblNo 
        })
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        this.documentAttach.listFile.push(detailSeletedEdoc);
        this.documentAttach.show();
        this.getEDoc(this.transactionType);
    }
    confirmDelete() {
        let messageDelete = `Do you want to delete this Attach File ? `;
        let itemDelete = this.selectedEdoc.id;
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainer.viewContainerRef, {
            title: 'Delete Attach File',
            body: messageDelete,
            labelConfirm: 'Yes',
            classConfirmButton: 'btn-danger',
            iconConfirm: 'la la-trash',
            center: true
        }, () => this.deleteEdoc(itemDelete))
    }
    deleteEdoc(id: string = '') {
        this._systemFileRepo.deleteEdoc(id)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toast.success("Delete Sucess")
                        this.getEDoc(this.transactionType);
                    }
                },
            );
    }
    getDocumentType(transactionType: string) {
        this._systemFileRepo.getDocumentType(transactionType, null)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    this.documentTypes = res;
                    this.documentAttach.documentTypes = res;
                },
            );
    }

    getEDoc(transactionType: string) {
        if (this.typeFrom === 'Shipment') {
            this._systemFileRepo.getEDocByJob(this.jobId, this.transactionType)
                .pipe(
                    catchError(this.catchError),
                )
                .subscribe(
                    (res: any[]) => {
                        console.log(res);
                        this.edocByJob = res.filter(x => x.documentType.type !== 'Accountant');
                        this.edocByAcc = res.filter(x => x.documentType.type === 'Accountant');
                        this.onChange.emit(res);
                    },
                );
        } else {
            this._systemFileRepo.getEDocByAccountant(this.billingId, transactionType)
                .pipe(
                    catchError(this.catchError),
                )
                .subscribe(
                    (res: any[]) => {
                        console.log(res);
                        this.edocByAcc = res;
                        this.onChange.emit(res);
                    },
                );
        }
    }
    showDocumentAttach() {
        if (this.typeFrom === 'Shipment') {
            this.documentAttach.headers = this.headerAttach;
        } else {
            this.documentAttach.headers = this.headerAttach;
        }
        this.documentAttach.isUpdate = false;
        this.documentAttach.show();
    }
    viewFileEdoc() {
        if (!this.selectedEdoc.imageUrl) {
            return;
        }
        const extension = this.selectedEdoc.imageUrl.split('.').pop();
        if (['xlsx'].includes(extension)) {
            this._exportRepo.previewExport(this.selectedEdoc.imageUrl);
        } else {
            this._exportRepo.downloadExport(this.selectedEdoc.imageUrl);
        }
    }

    sortEdoc(type: string, index: number, sort: string): void {
        if (!type) return;
        this.setSortBy(sort, this.sort !== sort ? true : !this.order);

        if (type === 'General') {
            this.edocByJob[index].eDocs = this._sortService.sort(this.edocByJob[index].eDocs, sort, this.order);
        } else {
            this.edocByAcc[index].eDocs = this._sortService.sort(this.edocByAcc[index].eDocs, sort, this.order);
        }
    }
}
interface IEDoc {
    billingNo: string;
    billingType: string;
    datetimeCreated: Date;
    datetimeModified: Date;
    departmentId: number;
    documentTypeId: number;
    expiredDate: number;
    groupId: number;
    hblNo: string;
    hblid: string;
    id: string;
    imageUrl: string;
    jobId: string;
    jobNo: string;
    note: string;
    officeId: string;
    source: string;
    sysImageId: string;
    systemFileName: string;
    userCreated: string;
    userFileName: string;
    userModified: string;
}
