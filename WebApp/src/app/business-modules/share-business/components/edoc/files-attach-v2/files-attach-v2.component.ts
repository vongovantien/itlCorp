import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { SystemConstants } from '@constants';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { IAppState, getCurrentUserState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError, skip, takeUntil } from 'rxjs/operators';
import { getGrpChargeSettlementPaymentDetailState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { getTransactionDetailCsTransactionState } from '../../../store';
import { IEDocFile, IEDocUploadFile, ShareDocumentTypeAttachComponent } from '../document-type-attach/document-type-attach.component';
import { AppShareEDocBase } from '../edoc.base';
import { ShareListFilesAttachComponent } from '../list-file-attach/list-file-attach.component';
import { log } from 'console';
@Component({
    selector: 'files-attach-v2',
    templateUrl: './files-attach-v2.component.html',
    styleUrls: ['./files-attach-v2.component.scss']
})

export class ShareBussinessAttachFileV2Component extends AppShareEDocBase implements OnInit {

    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;
    @ViewChild(ShareListFilesAttachComponent) listFileAttach: ShareListFilesAttachComponent;
    @Output() onChange: EventEmitter<any[]> = new EventEmitter<any[]>();
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }

    get readonly(): boolean {
        return this._readonly;
    }
    private _readonly: boolean = false;

    documentTypes: any[] = [];
    isOps: boolean = false;
    housebills: any[] = [];
    modifiedDocTypes: any;
    isView: boolean = true;
    elementInput: HTMLElement = null;
    isEdocByJob: boolean = false;
    docTypeId: number = 0;
    haveAdv: boolean = false;

    headersGen: CommonInterface.IHeaderTable[] = [
        { title: 'Alias Name', field: 'systemFileName', sortable: true },
        { title: 'Real File Name', field: 'userFileName', sortable: true },
        { title: 'House Bill No', field: 'hblNo', sortable: true },
        { title: 'Note', field: 'note' },
        { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
        { title: 'Attach Person', field: 'userCreated', sortable: true },
    ];

    headersAcc: CommonInterface.IHeaderTable[] = [
        { title: 'Alias Name', field: 'systemFileName', sortable: true },
        { title: 'Job No', field: 'jobNo' },
        { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
        { title: 'Note', field: 'note' },
        { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
        { title: 'Attach Person', field: 'userCreated', sortable: true },
    ];

    headerAttach: any[] = [
        { title: 'Alias Name', field: 'aliasName', width: 300 },
        { title: 'Real File Name', field: 'realFilename', width: 300 },
        { title: 'Document Type', field: 'docType', required: true },
        { title: 'Job Ref', field: 'jobRef' },
        { title: 'House Bill No', field: 'hbl' },
        { title: 'Note', field: 'note' },
    ]

    headerSettleAttach: any[] = [
        { title: 'Alias Name', field: 'aliasName', width: 200 },
        { title: 'Real File Name', field: 'realFilename' },
        { title: 'Document Type', field: 'docType', required: true },
        { title: 'Payee', field: 'payee' },
        { title: 'Invoice No', field: 'invoiceNo' },
        { title: 'Series No', field: 'seriesNo' },
        { title: 'Job Ref', field: 'jobRef' },
        { title: 'Note', field: 'note' },
    ]

    headerAccAttach: any[] = [
        { title: 'Alias Name', field: 'aliasName', width: 200 },
        { title: 'Real File Name', field: 'realFilename' },
        { title: 'Document Type', field: 'docType', required: true },
        { title: 'Job Ref', field: 'jobRef' },
        { title: 'Note', field: 'note' },
    ]


    constructor(
        protected readonly _systemFileRepo: SystemFileManageRepo,
        protected readonly _activedRoute: ActivatedRoute,
        protected readonly _store: Store<IAppState>,
        protected readonly _toast: ToastrService,
        protected readonly _exportRepo: ExportRepo,
        protected readonly _sortService: SortService,
        protected _documentationRepo: DocumentationRepo,
    ) {
        super(_toast, _systemFileRepo, _exportRepo, _store);
        this.requestSort = this.sortEdoc;
    }

    ngOnInit() {
        if (this.typeFrom === 'Shipment') {
            this._activedRoute.params
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((params: Params) => {
                    if (params.jobId) {
                        this.jobId = params.jobId;
                    } else {
                        this.jobId = params.id;
                        this.isOps = true;
                    }
                });
            if (this.isOps == false) {
                this._store.select(getTransactionDetailCsTransactionState)
                    .pipe(skip(1), takeUntil(this.ngUnsubscribe))
                    .subscribe(
                        (res: CsTransaction) => {
                            this.transactionType = res.transactionType;
                            this.getDocumentType(res.transactionType);
                            this.getEDoc(res.transactionType);
                            this.jobNo = res.jobNo;
                            this.isLocked = res.isLocked;
                        }
                    );
            } else {
                this._store.select(getOperationTransationState)
                    .pipe(takeUntil(this.ngUnsubscribe))
                    .subscribe(
                        (res: any) => {
                            console.log(res);
                            console.log(this.transactionType);
                            //his.transactionType = res.transactionType;
                            this.transactionType = res.opstransaction.jobNo.includes('LOG') ? 'CL' : 'TK';
                            this.getDocumentType('CL');
                            this.getEDoc('CL');
                            this.jobNo = res.opstransaction?.jobNo;
                            this.isLocked = res.opstransaction?.isLocked;
                        }
                    );
            }
        } else {
            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom);
            if (this.typeFrom === 'Settlement') {
                this.getEDoc(this.typeFrom);
            }
        }
        this.headers = [
            { title: 'Alias Name', field: 'systemFileName', sortable: true },
            { title: 'Real File Name', field: 'userFileName', sortable: true },
            { title: 'House Bill No', field: 'hblNo', sortable: true },
            { title: 'Billing Nos', field: 'billingNo', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
        ];
        this._store.select(getCurrentUserState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res) => {
                    this.currentUser = res;
                }
            )
        if (this.typeFrom === 'Settlement') {
            this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            this.haveAdv = data.some(x => x.advanceNo !== null);
                        }
                    }
                );
        }

        this.getHblList();
    }

    getHblList() {
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
    }

    onSelectEDoc(edoc: any) {
        this.selectedEdoc = edoc;
        this.selectedEdoc1 = edoc;
        this.documentAttach.selectedDocType = edoc.documentTypeId;
        this.isView = true;
        const extension = this.selectedEdoc.imageUrl.split('.').pop();
        if (!['xlsx', 'docx', 'doc', 'xls', 'html', 'htm', 'pdf', 'txt', 'png', 'jpeg', 'jpg'].includes(extension)) {
            this.isView = false;
        }
        this.clearMenuContext(this.queryListMenuContext);
    }



    editEdoc() {
        if (this.typeFrom === 'Settlement') {
            this.documentAttach.headers = this.headerSettleAttach;
        }
        else if (this.typeFrom === 'Advance' || this.typeFrom === 'SOA') {
            this.documentAttach.headers = this.headerAccAttach;
        }
        else {
            this.documentAttach.headers = this.headerAttach;
        }
        this.documentAttach.isUpdate = true;
        this.documentAttach.resetForm();
        let docType = this.documentTypes.find(x => x.id === this.selectedEdoc.documentTypeId);
        let hwbNo = this.housebills.find(x => x.id === this.selectedEdoc.hblid);

        let detailSeletedEdoc = ({
            aliasName: this.selectedEdoc.systemFileName,
            name: this.selectedEdoc.userFileName,
            nameALS: this.selectedEdoc.userFileName,
            id: this.selectedEdoc.id,
            docType: docType,
            note: this.selectedEdoc.note,
            hwbNo: hwbNo,
            hblid: this.selectedEdoc.hblid,
            jobNo: this.selectedEdoc.jobNo,
            jobId: this.selectedEdoc.jobId,
            Code: docType?.code,
            tranType: this.selectedEdoc.transactionType,
            AccountingType: null
        })
        this.docTypeId = docType.id;
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        this.documentAttach.selectedTrantype = this.selectedEdoc.transactionType;
        this.documentAttach.listFile.push(detailSeletedEdoc);
        this.documentAttach.show();
    }

    getDocumentType(transactionType: string) {
        this._systemFileRepo.getDocumentType(transactionType)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    this.documentTypes = res;
                    this.documentAttach.configDocType.dataSource = res;
                },
            );
    }

    showDocumentAttach() {
        if (this.typeFrom === 'Settlement') {
            this.documentAttach.headers = this.headerSettleAttach;
        }
        else if (this.typeFrom === 'Advance' || this.typeFrom === 'SOA') {
            this.documentAttach.headers = this.headerAccAttach;
        }
        else {

            this.documentAttach.headers = this.headerAttach;
        }
        this.documentAttach.isUpdate = false;
        this.documentAttach.show();
    }

    setDocTypeSelected(docType: any) {
        this.selectedEdoc1 = Object.assign({});
        if (docType !== null) {
            this.selectedEdoc1.documentTypeId = docType?.documentType.id;
            this.selectedEdoc1.documentCode = docType?.documentType.code;
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

    chooseFile(event: any) {
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        let docType = this.selectedEdoc1?.documentTypeId;
        let listFile: any[] = [];

        for (let i = 0; i < files?.length; i++) {
            if (!!docType) {
                files[i].DocumentId = docType;
                files[i].DocumentCode = this.selectedEdoc1?.documentCode;
            }
            listFile.push(files[i]);
            listFile[i].aliasName = this.selectedEdoc1?.documentCode + '_' + files[i].name.substring(0, files[i].name.lastIndexOf('.'));
        }
        if (fileList?.length > 0) {
            let validSize: boolean = true;
            for (let i = 0; i <= fileList?.length - 1; i++) {
                const fileSize: number = fileList[i].size / Math.pow(1024, 2); //TODO Verify BE
                if (fileSize >= 100) {
                    validSize = false;
                    break;
                }
            }
            if (!validSize) {
                this._toast.warning("maximum file size < 100Mb");
                return;
            }
        }
        event.target.value = ''
        this.uploadEDoc(listFile);
    }

    // ** UPLOAD PER FILE ON CONTEXT MENU LIST EDOC ON JOB
    uploadEDoc(listFile: any[]) {
        let edocFileList: IEDocFile[] = [];
        let files: any[] = [];
        listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                JobId: this.jobId,
                Code: x.DocumentCode,
                TransactionType: this.transactionType,
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: this.typeFrom,
                HBL: SystemConstants.EMPTY_GUID,
                FileName: x.name,
                Note: '',
                BillingId: SystemConstants.EMPTY_GUID,
                Id: SystemConstants.EMPTY_GUID,
                DocumentId: x.DocumentId,
                AccountingType: null
            }));
        });
        let EdocUploadFile: IEDocUploadFile;
        EdocUploadFile = ({
            ModuleName: 'Document',
            FolderName: this.typeFrom,
            Id: this.jobId,
            EDocFiles: edocFileList,
        })
        this._systemFileRepo.uploadEDoc(EdocUploadFile, files, this.typeFrom)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toast.success("Upload file successfully!");
                        this.getEDoc(this.transactionType);
                    }
                }
            );
    }

    genFileToSM(billingType: string) {
        this._systemFileRepo.genEdocFromBilling(this.billingNo, billingType)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        //this.getEDoc(this.transactionType);
                        this.listFileAttach.requestListEDocSettle();
                        this._toast.success(res.message);
                    }
                    else {
                        this._toast.warning(res.message)
                    }
                },
            );
    }

    getListEdoc(event: any) {
        if (this.typeFrom === 'Settlement') {
            this.listFileAttach.requestListEDocSettle();
        }
        this.listFileAttach.getEDoc(event);
        //this.getEDoc(this.transactionType);
    }

    emitAttach(event: any) {
        this.onChange.emit(event);
    }
}



