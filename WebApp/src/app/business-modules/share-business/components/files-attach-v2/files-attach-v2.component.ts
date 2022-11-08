import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, EventEmitter, Input, OnInit, Output, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { ConfirmPopupComponent } from '@common';
import { ContextMenuDirective, InjectViewContainerRefDirective } from '@directives';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from '@repositories';
import { SortService } from '@services';
import { getCurrentUserState, IAppState } from '@store';
import _uniqBy from 'lodash/uniqBy';
import { ToastrService } from 'ngx-toastr';
import { catchError, skip, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { getAdvanceDetailRequestState, getAdvanceDetailState } from 'src/app/business-modules/accounting/advance-payment/store';
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
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    isDelete: boolean = false;
    get readonly(): boolean {
        return this._readonly;
    }
    headersGen: CommonInterface.IHeaderTable[];
    headersAcc: CommonInterface.IHeaderTable[];
    documentTypes: any[] = [];
    jobId: string = '';
    isOps: boolean = false;
    edocByJob: any[] = [];
    edocByAcc: any[] = [];
    selectedEdoc: IEDocItem;
    transactionType: string = '';
    housebills: any[] = [];
    jobs: any[] = [];

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
        //{ title: 'House Bill No', field: 'hbl' },
        { title: 'Note', field: 'note' },
    ]
    // accountantAttach: any[] = [{ title: 'No', field: 'no' },
    // { title: 'Alias Name', field: 'aliasName' },
    // { title: 'Real File Name', field: 'realFilename' },
    // { title: 'Document Type', field: 'docType', required: true },
    // { title: 'Note', field: 'note' },]
    jobNo: string = '';
    private _readonly: boolean = false;
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
                            this.getDocumentType(res.transactionType, null);
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
                            this.getDocumentType('CL', null);
                            this.getEDoc('CL');
                            this.jobNo = res.opstransaction.jobNo;
                            //this.documentAttach.fileNo = res.jobNo;
                            this.isLocked = res.opstransaction.isLocked;
                            console.log(this.jobNo);

                        }
                    );
            }
        }
        else if (this.typeFrom === 'Settlement') {

            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom, this.billingId);
            this.getEDoc(this.typeFrom);
            this.getJobList();
            this.headersAcc = [{ title: 'Alias Name', field: 'userFileName', sortable: true },
            { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
            //{ title: 'House Bill No', field: 'hblNo', sortable: true },
            { title: 'Job No', field: 'jobNo' },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
            ];
        } else {
            this.getJobList();
            this.transactionType = this.typeFrom;
            this.getDocumentType(this.typeFrom, this.billingId);
            this.getEDoc(this.typeFrom);
            this.headersAcc = [{ title: 'Alias Name', field: 'userFileName', sortable: true },
            { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
            //{ title: 'House Bill No', field: 'hblNo', sortable: true },
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
            { title: 'Billing Nos', field: 'billingNo', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
        ];
        this.headersGen = [
            { title: 'Alias Name', field: 'systemFileName', sortable: true },
            { title: 'Real File Name', field: 'userFileName', sortable: true },
            { title: 'House Bill No', field: 'hblNo', sortable: true },
            //{ title: 'Source', field: 'source', sortable: true },
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
        if (this.typeFrom === 'SOA') {
            this.headerAttach = [
                { title: 'Alias Name', field: 'aliasName', width: 300 },
                { title: 'Real File Name', field: 'realFilename', width: 300 },
                { title: 'Document Type', field: 'docType', required: true },
                { title: 'Note', field: 'note' }
            ]
            this.headersAcc = [{ title: 'Alias Name', field: 'userFileName', sortable: true },
            { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
            { title: 'Note', field: 'note' },
            { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
            { title: 'Attach Person', field: 'userCreated', sortable: true },
            ];
        }
        this.getHblList();
        if ((this.typeFrom === 'Settlement' || this.selectedEdoc?.source === 'Shipment' || this.selectedEdoc?.source === null)) {
            this.isDelete = true;
        } else {
            this.isDelete = false;
        }
        if ((this.typeFrom === 'Advance')) {
            this._store.select(getAdvanceDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        console.log(data);

                        if (data.statusApproval === 'Done') {
                            this.isDelete = false;
                        } else {
                            this.isDelete = true;
                        }
                    });
        }
    }
    getJobList() {
        if (this.typeFrom === 'Settlement') {
            this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
                takeUntil(this.ngUnsubscribe)
            )
                .subscribe(
                    (data) => {
                        if (!!data) {
                            console.log(_uniqBy(data, 'jobId'));
                            console.log(this.jobs);
                            this.jobs = [];
                            _uniqBy(data, 'hbl').forEach(element => {
                                let item = ({
                                    jobNo: element.jobId,
                                    id: element.shipmentId
                                    // hwbno: element.hbl,
                                    // jobNo: element.jobId,
                                    // id: element.hblid,
                                    // jobId: element.shipmentId,
                                })

                                this.jobs.push(item);
                                //console.log(this.housebills);
                                console.log(this.jobs);
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
                            console.log(_uniqBy(data, 'jobId'));
                            console.log(data);
                            this.jobs = [];
                            for (let element of data) {
                                this.jobs.push({ jobNo: element.jobId, })
                            }
                            console.log(this.jobs);
                            // data.forEach((element: any) => {
                            //     this.jobs.push(({ jobNo: element.jobId }))
                            //     console.log(element);
                            // });

                            // _uniqBy(data, 'jobId').forEach(element => {
                            //     let item = ({
                            //         jobNo: element.jobId,
                            //         //id: element.shipmentId
                            //         // hwbno: element.hbl,
                            //         // jobNo: element.jobId,
                            //         // id: element.hblid,
                            //         // jobId: element.shipmentId,
                            //     })

                            //     this.jobs.push(item);
                            //     //console.log(this.housebills);
                            //     console.log(this.jobs);
                            // }
                            // );
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
                        console.log(this.housebills);
                        this.housebills = res;
                    }
                },
            );
        // if (this.typeFrom === 'Shipment') {
        //     this._documentationRepo.getListHouseBillOfJob({ jobId: this.jobId })
        //         .pipe(
        //             catchError(this.catchError),
        //         ).subscribe(
        //             (res: any) => {
        //                 if (!!res) {
        //                     console.log(this.housebills);
        //                     this.housebills = res;
        //                 }
        //             },
        //         );
        // } else if (this.typeFrom === 'Settlement') {
        //     this._store.select(getGrpChargeSettlementPaymentDetailState).pipe(
        //         takeUntil(this.ngUnsubscribe)
        //     )
        //         .subscribe(
        //             (data) => {
        //                 if (!!data) {
        //                     console.log(_uniqBy(data, 'hbl'));
        //                     console.log(this.housebills);
        //                     this.housebills = [];
        //                     _uniqBy(data, 'hbl').forEach(element => {
        //                         let item = ({
        //                             hwbno: element.hbl,
        //                             jobNo: element.jobId,
        //                             id: element.hblid,
        //                             jobId: element.shipmentId,
        //                         })

        //                         this.housebills.push(item);
        //                         console.log(this.housebills);

        //                     }
        //                     );
        //                 }
        //             }
        //         );
        //this.chargeSM
        //     } else {

        // }
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
        if (this.typeFrom === 'Settlement' || this.typeFrom === 'Advance') {
            this.documentAttach.headers = this.headerSettleAttach;

        } else {
            this.documentAttach.headers = this.headerAttach;
        }
        this.documentAttach.isUpdate = true;
        this.documentAttach.resetForm();
        console.log(this.documentTypes);
        let docType = this.typeFrom === 'Shipment' ? this.documentTypes.find(x => x.id === this.selectedEdoc.documentTypeId) :
            this.documentTypes.find(x => x.nameEn === this.selectedEdoc.documentTypeName);
        console.log(docType);
        let hwbNo = this.housebills.find(x => x.id === this.selectedEdoc.hblid);
        if (this.selectedEdoc.userFileName.substring(0, 3) === 'OTH') {
            this.documentTypes.push(({ id: this.selectedEdoc.documentTypeId, code: "OTH", nameEn: 'Other' }));
            docType = this.documentTypes.find(x => x.id === this.selectedEdoc.documentTypeId);
        }
        else {
            this.documentTypes.splice(this.documentTypes.findIndex(x => x.code === 'OTH'), 1);
        }
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
        console.log(detailSeletedEdoc);
        this.documentAttach.detailDocId = this.selectedEdoc.departmentId;
        this.documentAttach.selectedtTrantype = this.selectedEdoc.transactionType;
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
    getDocumentType(transactionType: string, billingId: string) {
        this._systemFileRepo.getDocumentType(transactionType, billingId)
            .pipe(
                catchError(this.catchError),
            )
            .subscribe(
                (res: any[]) => {
                    console.log(res);

                    this.documentTypes = res;
                    this.documentAttach.documentTypes = res;
                    console.log(this.documentAttach.documentTypes);
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
                    (res: any) => {
                        this.edocByAcc = res;
                        this.onChange.emit(res);
                    },
                );
        }
    }
    showDocumentAttach() {
        if (this.typeFrom === 'Settlement' || this.typeFrom === 'Advacne') {
            this.documentAttach.headers = this.headerSettleAttach;
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
}

interface IEDoc {
    documentType: any;
    eDocs: IEDocItem[];
}