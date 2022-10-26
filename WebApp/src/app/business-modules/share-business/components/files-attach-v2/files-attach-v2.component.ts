import { Component, Input, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { ContextMenuDirective } from '@directives';
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
    @Input() typeFrom: string = 'Job';
    @Input() billingId: string = '';
    headersGen: CommonInterface.IHeaderTable[];
    documentTypes: any[] = [];
    jobId: string = '';
    isOps: boolean = false;
    edocByJob: any[] = [];
    edocByAcc: any[] = [];
    selectedEdoc: IEDoc;
    transactionType: string = '';
    housebills: any[];
    headerAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType' },
    { title: 'Job Ref', field: 'jobRef' },
    { title: 'House Bill No', field: 'hbl' },
    { title: 'Note', field: 'note' },
    { title: 'Source', field: 'source' },]
    accountantAttach: any[] = [{ title: 'No', field: 'no' },
    { title: 'Alias Name', field: 'aliasName' },
    { title: 'Real File Name', field: 'realFilename' },
    { title: 'Document Type', field: 'docType' },
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
        if (this.typeFrom === 'Job') {
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
                            this.documentAttach.transactionType = res.transactionType;
                            this.getDocumentType(res.transactionType);
                            this.getEDocByJobID(res.transactionType);
                            this.jobNo = res.jobNo;
                            this.documentAttach.fileNo = res.jobNo;
                        }
                    );
            } else {
                this._store.select(getOperationTransationState)
                    .pipe(takeUntil(this.ngUnsubscribe))
                    .subscribe(
                        (res: any) => {
                            this.transactionType = 'CL';
                            this.documentAttach.transactionType = 'CL'
                            this.getDocumentType('CL');
                            this.getEDocByJobID('CL');
                            this.jobNo = res.jobNo;
                            this.documentAttach.fileNo = res.jobNo;
                        }
                    );
            }
        } else {
            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom);
        }
        this.headers = [
            { title: 'Alias Name', field: 'aliasName', sortable: true },
            { title: 'Real File Name', field: 'realFilename', sortable: true },
            { title: 'House Bill No', field: 'houseBillNo', sortable: true },
            { title: 'Billing No', field: 'billingNo', sortable: true },
            { title: 'Source', field: 'source', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'attachTime', sortable: true },
            { title: 'Attach Person', field: 'attachPerson', sortable: true },
        ];
        this.headersGen = [
            { title: 'Alias Name', field: 'aliasName', sortable: true },
            { title: 'Real File Name', field: 'realFilename', sortable: true },
            { title: 'House Bill No', field: 'houseBillNo', sortable: true },
            { title: 'Source', field: 'source', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'attachTime', sortable: true },
            { title: 'Attach Person', field: 'attachPerson', sortable: true },
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
        if (this.typeFrom === 'Job') {
            this._documentationRepo.getListHouseBillOfJob({ jobId: this.jobId })
                .pipe(
                    catchError(this.catchError),
                ).subscribe(
                    (res: any) => {
                        if (!!res) {
                            this.housebills = res;
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
                                    id: element.hblid
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
        const qContextMenuList = this.queryListMenuContext.toArray();
        if (!!qContextMenuList.length) {
            qContextMenuList.forEach((c: ContextMenuDirective) => c.close());
        }
    }
    downloadEdoc() {
        this._exportRepo.downloadExport(this.selectedEdoc.imageUrl);
    }
    editEdoc() {
        if (this.typeFrom === 'Job') {
            this.documentAttach.headers = this.headerAttach;
        } else {
            this.documentAttach.headers = this.accountantAttach;
        }
        this.documentAttach.isUpdate = true;
        this.documentAttach.resetForm();
        let detailSeletedEdoc = ({
            aliasName: this.selectedEdoc.systemFileName,
            name: this.selectedEdoc.userFileName,
            id: this.selectedEdoc.id,
            docType: this.selectedEdoc.documentTypeId,
            note: this.selectedEdoc.note,
            hwbNo: this.selectedEdoc.hblid,//hblNo 
            hblid: this.selectedEdoc.hblid//hblNo 
        })
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        this.documentAttach.listFile.push(detailSeletedEdoc);
        this.documentAttach.show();
        this.getEDocByJobID(this.transactionType);
    }
    deleteEdoc() {
        this._systemFileRepo.deleteEdoc(this.selectedEdoc.id)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toast.success("Delete Sucess")
                        this.getEDocByJobID(this.transactionType);
                    }
                },
            );
    }
    getDocumentType(transactionType: string) {
        this._systemFileRepo.getDocumentType(transactionType)
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
    getEDocByJobID(transactionType: string) {
        this._systemFileRepo.getEDocByJob(this.jobId, transactionType)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    this.edocByJob = res.filter(x => x.documentType.type !== 'Accountant');
                    this.edocByAcc = res.filter(x => x.documentType.type === 'Accountant');
                },
            );
    }
    showDocumentAttach() {
        if (this.typeFrom === 'Job') {
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
    sortEdoc(edos: IEDoc[], sort: string): void {
        edos = this._sortService.sort(edos, sort, this.order);
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
