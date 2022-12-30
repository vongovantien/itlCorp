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
import { getCurrentUserState, IAppState } from '@store';
import _uniqBy from 'lodash/uniqBy';
import { ToastrService } from 'ngx-toastr';
import { catchError, skip, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { getAdvanceDetailRequestState } from 'src/app/business-modules/accounting/advance-payment/store';
import { getGrpChargeSettlementPaymentDetailState } from 'src/app/business-modules/accounting/settlement-payment/components/store';
import { getSOADetailState } from 'src/app/business-modules/accounting/statement-of-account/store/reducers';
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
    edocByAcc: any[] = [];
    selectedEdoc: IEDocItem;
    selectedEdoc1: IEDocItem;
    transactionType: string = '';
    housebills: any[] = [];
    jobs: any[] = [];
    modifiedDocTypes: any;
    jobNo: string = '';
    private _readonly: boolean = false;
    isView: boolean = true;
    elementInput: HTMLElement = null;

    headersGen: CommonInterface.IHeaderTable[] = [
        { title: 'Alias Name', field: 'systemFileName', sortable: true },
        { title: 'Real File Name', field: 'userFileName', sortable: true },
        { title: 'House Bill No', field: 'hblNo', sortable: true },
        { title: 'Note', field: 'note' },
        { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
        { title: 'Attach Person', field: 'userCreated', sortable: true },
    ];

    headersAcc: CommonInterface.IHeaderTable[] = [{ title: 'Alias Name', field: 'userFileName', sortable: true },
    { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
    { title: 'Job No', field: 'jobNo' },
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
        { title: 'Alias Name', field: 'aliasName', width: 300 },
        { title: 'Real File Name', field: 'realFilename', width: 300 },
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
                            this.getDocumentType(res.transactionType, null);
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
                            this.getDocumentType('CL', null);
                            this.getEDoc('CL');
                            this.jobNo = res.opstransaction.jobNo;
                            this.isLocked = res.opstransaction.isLocked;
                        }
                    );
            }
        } else {
            this.transactionType = this.typeFrom;
            this.getJobList();
            this.getDocumentType(this.typeFrom, this.billingId);
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
        this.getHblList();
    }

    getJobList() {
        if (this.typeFrom === 'Settlement') {
            this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            _uniqBy(data, 'hbl').forEach(element => {
                                let item = ({
                                    jobNo: element.jobId,
                                    id: element.shipmentId
                                })
                                this.jobs.push(item);
                            }
                            );
                        }
                    }
                );
        } else if (this.typeFrom === 'Advance') {
            this._store.select(getAdvanceDetailRequestState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            for (let element of data) {
                                console.log(element);

                                this.jobs.push({ jobNo: element.jobId, id: element.shipmentId })
                            }
                        }
                    }
                );
        } else if (this.typeFrom === 'SOA') {
            this._store.select(getSOADetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            for (let element of data.groupShipments) {
                                console.log(element);
                                this.jobs.push({ jobNo: element.jobId, id: element.shipmentId })
                            }
                        }
                    }
                );
        }

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
        this.documentAttach.selectedtDocType = edoc.documentTypeId;
        this.isView = true;
        const extension = this.selectedEdoc.imageUrl.split('.').pop();
        if (extension === 'zip') {
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
        if (this.typeFrom === 'Settlement' || this.typeFrom === 'Advance' || this.typeFrom === 'SOA') {
            this.documentAttach.headers = this.headerSettleAttach;

        } else {
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
            AccountingType: this.typeFrom
        })
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        this.documentAttach.selectedtTrantype = this.selectedEdoc.transactionType;
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

    getDocumentType(transactionType: string, billingId: string) {
        this._systemFileRepo.getDocumentType(transactionType, billingId)
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
                        this.onChange.emit(res);
                    },
                );
        }
    }

    showDocumentAttach() {
        if (this.typeFrom === 'Settlement' || this.typeFrom === 'Advance' || this.typeFrom === 'SOA') {
            this.documentAttach.headers = this.headerSettleAttach;
        } else {
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

    viewEdocFromName(imageUrl: string) {
        this.selectedEdoc = Object.assign({}, this.selectedEdoc);
        this.selectedEdoc.imageUrl = imageUrl;
        this.viewFileEdoc();
    }

    viewFileEdoc() {
        if (!this.selectedEdoc.imageUrl) {
            return;
        }
        const extension = this.selectedEdoc.imageUrl.split('.').pop();
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
        else {
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

    downloadAllEdoc() {
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

        for (let i = 0; i < files.length; i++) {
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
                DocumentId: x.DocumentId
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

    getFilesAdvance() {
        this._systemFileRepo.genEdocFromBilling(this.billingNo, this.typeFrom)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this.getEDoc(this.transactionType);
                    }
                },
            );
    }
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
