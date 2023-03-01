import { Directive, EventEmitter, Input, Output, QueryList, ViewChild, ViewChildren } from "@angular/core";
import { ToastrService } from "ngx-toastr";
import { AppList } from "src/app/app.list";

import { ConfirmPopupComponent } from "@common";
import { SystemConstants } from '@constants';
import { ContextMenuDirective, InjectViewContainerRefDirective } from "@directives";
import { ExportRepo, SystemFileManageRepo } from "@repositories";
import { catchError } from "rxjs/operators";

@Directive()
export abstract class AppShareEDocBase extends AppList {
    @Output() onChange: EventEmitter<any[]> = new EventEmitter<any[]>();
    @ViewChildren(ContextMenuDirective) queryListMenuContext: QueryList<ContextMenuDirective>;
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;

    @Input() typeFrom: string = 'Shipment';
    @Input() isEdocByAcc: boolean = false;
    @Input() jobNo: string = '';
    @Input() billingId: string = '';
    @Input() billingNo: string = '';
    @Input() jobOnSettle: boolean = false;

    transactionType: string = '';
    edocByJob: any[] = [];
    jobId: string = '';
    selectedEdoc1: IEDocItem;
    selectedEdoc: IEDocItem;
    lstEdocExist: any[] = [];
    isView: boolean = true;
    edocByAcc: IEdocAcc[] = [({
        documentType: null,
        eDocs: [],
    })];

    headersAcc: CommonInterface.IHeaderTable[] = [
        { title: 'Alias Name', field: 'systemFileName', sortable: true },
        { title: 'Job No', field: 'jobNo' },
        { title: 'Document Type Name', field: 'documentTypeName', sortable: true },
        { title: 'Note', field: 'note' },
        { title: 'Attach Time', field: 'datetimeCreated', sortable: true },
        { title: 'Attach Person', field: 'userCreated', sortable: true },
    ];

    constructor(
        protected readonly _toast: ToastrService,
        protected readonly _systemFileRepo: SystemFileManageRepo,
        protected readonly _exportRepo: ExportRepo,
    ) {
        super();
    }

    ngOnInit() {
    }

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

    viewEdocFromName(edoc: any) {
        this.selectedEdoc = edoc;
        this.viewFileEdoc();
    }

    downloadAllEdoc() {
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

    viewFileEdoc() {
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
                        if (this.jobOnSettle) {
                            this.lstEdocExist = res.eDocs.filter(x => x.jobNo === this.jobNo || x.jobNo === null);
                        } else {
                            this.lstEdocExist = res.eDocs;
                        }
                        console.log(res);
                        if (res.eDocs.length > 0) {
                            this.isEdocByAcc = true
                        }
                        this.onChange.emit(res);
                    },
                );
        }
    }

}

export interface IEdocAcc {
    documentType: any;
    eDocs: any[];
}

export interface IEDocItem {
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
