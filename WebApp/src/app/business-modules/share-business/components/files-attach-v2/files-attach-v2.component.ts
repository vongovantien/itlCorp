import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, EventEmitter, Input, OnInit, Output, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { ContextMenuDirective, InjectViewContainerRefDirective } from '@directives';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { AccountingRepo, DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { IAppState, getCurrentUserState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError, skip, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { getGrpChargeSettlementPaymentDetailState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { getTransactionDetailCsTransactionState } from '../../store';
import { IEDocFile, IEDocUploadFile, ShareDocumentTypeAttachComponent } from '../document-type-attach/document-type-attach.component';
@Component({
    selector: 'files-attach-v2',
    templateUrl: './files-attach-v2.component.html',
    styleUrls: ['./files-attach-v2.component.scss']
})

export class ShareBussinessAttachFileV2Component extends AppList implements OnInit {
    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;

    @Input() typeFrom: string = 'Shipment';
    @Input() billingId: string = '';
    @Input() billingNo: string = '';
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    get readonly(): boolean {
        return this._readonly;
    }

    @Output() onChange: EventEmitter<any[]> = new EventEmitter<any[]>();

    documentTypes: any[] = [];
    jobId: string = '';
    isOps: boolean = false;
    edocByJob: any[] = [];
    //edocByAcc: any[] = [];
    selectedEdoc: IEDocItem;
    selectedEdoc1: IEDocItem;
    transactionType: string = '';
    housebills: any[] = [];
    //jobs: any[] = [];
    modifiedDocTypes: any;
    jobNo: string = '';
    private _readonly: boolean = false;
    isView: boolean = true;
    elementInput: HTMLElement = null;
    isEdocByJob: boolean = false;
    isEdocByAcc: boolean = false;
    edocByAcc: IEdocAcc[] = [({
        documentType: null,
        eDocs: [],
    })];
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

    // headersAcc: CommonInterface.IHeaderTable[] = [{ title: 'Alias Name', field: 'userFileName', sortable: true },
    // { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
    // { title: 'Job No', field: 'jobNo' },
    // { title: 'Note', field: 'note' },
    // { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
    // { title: 'Attach Person', field: 'userCreated', sortable: true },
    // ];
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
        private readonly _systemFileRepo: SystemFileManageRepo,
        private readonly _activedRoute: ActivatedRoute,
        private readonly _store: Store<IAppState>,
        private readonly _toast: ToastrService,
        private readonly _exportRepo: ExportRepo,
        private readonly _sortService: SortService,
        private readonly _accoutingRepo: AccountingRepo,
        private _documentationRepo: DocumentationRepo,
    ) {
        super();
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
                            this.transactionType = 'CL';
                            this.getDocumentType('CL');
                            this.getEDoc('CL');
                            this.jobNo = res.opstransaction.jobNo;
                            this.isLocked = res.opstransaction.isLocked;
                        }
                    );
            }
        } else {
            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom);
            this.getEDoc(this.typeFrom);
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
        console.log(this.selectedEdoc);
    }

    // downloadEdocFromName(doc: any) {
    //     this.selectedEdoc = Object.assign({}, this.selectedEdoc);
    //     this.selectedEdoc.sysImageId = doc.sysImageId;
    //     this.selectedEdoc.imageUrl = doc.imageUrl;
    //     this.selectedEdoc.systemFileName = doc.ssystemFileName;
    //     console.log(doc);
    //     this.downloadEdoc();
    // }

    downloadEdoc() {
        const selectedEdoc = Object.assign({}, this.selectedEdoc);
        if (selectedEdoc.id === selectedEdoc.sysImageId) {
            this._systemFileRepo.getFileEdoc(selectedEdoc.sysImageId).subscribe(
                (data) => {
                    const extention = selectedEdoc.imageUrl.split('.').pop();
                    this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedEdoc.systemFileName + '.' + extention);
                }
            )
        }
        this._systemFileRepo.getFileEdoc(selectedEdoc.sysImageId).subscribe(
            (data) => {
                const extention = selectedEdoc.imageUrl.split('.').pop();
                this.downLoadFile(data, SystemConstants.FILE_EXCEL, selectedEdoc.systemFileName + '.' + extention);
            }
        )
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
            hwbNo: hwbNo,//hblNo
            hblid: this.selectedEdoc.hblid,//hblNo
            jobNo: this.selectedEdoc.jobNo,
            jobId: this.selectedEdoc.jobId,
            Code: docType?.code,
            tranType: this.selectedEdoc.transactionType,
            AccountingType: null
        })
        console.log(docType);
        this.docTypeId = docType.id;
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        this.documentAttach.selectedTrantype = this.selectedEdoc.transactionType;
        this.documentAttach.listFile.push(detailSeletedEdoc);
        this.documentAttach.show();
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
        this._systemFileRepo.getDocumentType(transactionType)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    console.log(res);
                    this.documentTypes = res;
                    this.documentAttach.configDocType.dataSource = res;
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
                    (res: any) => {
                        this.edocByAcc = res;
                        console.log(res);
                        if (res.eDocs.length > 0) {
                            this.isEdocByAcc = true
                        }
                        this.onChange.emit(res);
                    },
                );
        }
        console.log(this.edocByAcc);

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
        console.log(docType);

        if (docType !== null) {
            this.selectedEdoc1.documentTypeId = docType?.documentType.id;
            this.selectedEdoc1.documentCode = docType?.documentType.code;
        }
    }

    viewEdocFromName(edoc: any) {
        // this.selectedEdoc = Object.assign({}, this.selectedEdoc);
        // this.selectedEdoc.imageUrl = edoc.imageUrl;
        console.log(edoc);

        this.selectedEdoc = edoc;
        this.viewFileEdoc();
    }

    viewFileEdoc() {
        console.log(this.selectedEdoc);

        if (!this.selectedEdoc.imageUrl) {
            return;
        }
        const extension = this.selectedEdoc.imageUrl.split('.').pop();
        console.log(extension);

        if (['xlsx', 'docx', 'doc', 'xls'].includes(extension)) {
            this._exportRepo.previewExport(this.selectedEdoc.imageUrl);
        }
        else if (['html', 'htm'].includes(extension)) {
            console.log();
            this._systemFileRepo.getFileEdocHtml(this.selectedEdoc.imageUrl).subscribe(
                (res: any) => {
                    window.open('', '_blank').document.write(res.body);
                }
            )
        }
        else if (['pdf', 'txt', 'png', 'jpeg', 'jpg'].includes(extension.toLowerCase())) {
            this._exportRepo.downloadExport(this.selectedEdoc.imageUrl);
        } else {
            this.downloadEdoc();
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

    downloadAllEdoc() {
        console.log(this.isEdocByAcc);

        // let countEdocJob = _some(this.edocByJob, x => (x.eDocs !== null && x.eDocs?.length > 0));
        // let countEdocAcc = _some(this.edocByAcc, x => (x.eDocs !== null && x.eDocs?.length > 0));
        //console.log(this.edocByJob);
        console.log(this.edocByAcc);
        if (this.typeFrom === 'Shipment') {
            if (!this.edocByJob?.some(x => x.eDocs?.length > 0)) {
                return this._toast.warning("No data to Export");
            }
        }
        else {
            if (!this.isEdocByAcc) {
                return this._toast.warning("No data to Export");
            }
        }
        let model = {
            folderName: this.typeFrom,
            objectId: this.typeFrom === 'Shipment' ? this.jobId : this.billingId,
            chillId: null,
            fileName: this.typeFrom === 'Shipment' ? this.jobNo : this.billingNo
        }
        this._systemFileRepo.dowloadallEDoc(model)
            .subscribe(
                (res: any) => {
                    this.downLoadFile(res, "application/zip", model.fileName);
                }
            )
    }

    chooseFile(event: any) {
        console.log(this.selectedEdoc1);

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
                        this.getEDoc(this.transactionType);
                        this._toast.success(res.message);
                    }
                    else {
                        this._toast.warning(res.message)
                    }
                },
            );
    }
}

interface IEdocAcc {
    documentType: any;
    eDocs: any[];
}

interface IEDocItem {
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
    documentTypeName: string;
    transactionType: string;
    documentCode: string;
}
