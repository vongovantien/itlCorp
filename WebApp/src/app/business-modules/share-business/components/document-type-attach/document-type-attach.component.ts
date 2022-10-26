import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { Store } from '@ngrx/store';
import { SystemFileManageRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { PopupBase } from 'src/app/popup.base';
import { getTransactionLocked, getTransactionPermission } from '../../store';
@Component({
    selector: 'document-type-attach',
    templateUrl: './document-type-attach.component.html',
    styleUrls: ['./document-type-attach.component.scss']
})
export class ShareDocumentTypeAttachComponent extends PopupBase implements OnInit {
    @ViewChild('confirmDelete') confirmDeletePopup: ConfirmPopupComponent;
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();
    headers: CommonInterface.IHeaderTable[];
    @Input() jobId: string = '';
    isOps: boolean = false;
    fileNo: string;
    transactionType: string = '';
    EdocUploadFile: IEDocUploadFile;
    listFile: any[] = [];
    isUpdate: boolean = false;
    selectedtDocType: any = null;
    edocSelected: any;
    detailDocId: number;
    formData: IEDocUploadFile;
    @Input() typeFrom: string = 'Job';
    documentTypes: any[] = [];
    source: string = 'Job';
    accepctFilesUpload = 'image/*,.txt,.pdf,.doc,.xlsx,.xls';
    @Input() housebills: any[] = [];
    billingNo: string = '';
    billingId: string = '';
    chargeSM: any;
    constructor(
        private _toastService: ToastrService,
        private _store: Store<IAppState>,
        private _systemFileManagerRepo: SystemFileManageRepo,
    ) {
        super();
        this.isLocked = this._store.select(getTransactionLocked);
        this.permissionShipments = this._store.select(getTransactionPermission);
    }
    ngOnInit(): void {
        if (this.typeFrom !== 'Job') {
            this.transactionType = 'Accountant';
        }
        // this.getHblList();
    }
    chooseFile(event: any) {
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        //this.listFile = files;
        for (let i = 0; i < files.length; i++) {
            this.listFile.push(files[i]);
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
                this._toastService.warning("maximum file size < 100Mb");
                return;
            }
        }
    }
    onSelectDataFormInfo(event: any, index: number, type: string) {
        switch (type) {
            case 'docType':
                this.listFile[index].docType = event;
                this.listFile[index].aliasName = this.isUpdate ? event + this.listFile[index].name : event + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                this.selectedtDocType = event;
                break;
            case 'aliasName':
                this.listFile[index].aliasName = event;
                break;
            case 'houseBill':
                if (this.typeFrom === 'Job') {
                    this.listFile[index].hblid = event;
                } else {
                    this.listFile[index].jobNo = this.housebills.find(x => x.id === event).jobNo;
                    this.listFile[index].hblid = event;
                }
                break;
            case 'note':
                this.listFile[index].note = event;
                break;
        }
    }
    resetForm() {
        this.listFile?.splice(0, this.listFile.length);
    }
    removeFile(index: number) {
        this.listFile?.splice(index, 1);
    }
    uploadEDoc() {
        let edocFileList: IEDocFile[] = [];
        let files: any[] = [];
        this.listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                JobId: this.jobId !== undefined ? this.jobId : SystemConstants.EMPTY_GUID,
                Code: x.docType,
                TransactionType: this.transactionType,
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: '',
                HBL: x.hblid !== undefined ? x.hblid : SystemConstants.EMPTY_GUID,
                FileName: x.name,
                Note: x.note !== undefined ? x.note : '',
                BillingId: this.billingId !== '' ? this.billingId : SystemConstants.EMPTY_GUID,
                Id: x.id !== undefined ? x.id : SystemConstants.EMPTY_GUID,
            }));
        });
        this.EdocUploadFile = ({
            ModuleName: 'Document',
            FolderName: 'Shipment',
            Id: this.jobId !== undefined ? this.jobId : SystemConstants.EMPTY_GUID,
            EDocFiles: edocFileList,
        })
        if (this.isUpdate) {
            let edocUploadModel: any = {
                Hblid: edocFileList[0].HBL,
                SystemFileName: edocFileList[0].AliasName,
                Note: edocFileList[0].Note,
                Id: edocFileList[0].Id,
                DocumentTypeId: this.selectedtDocType,
            }
            this._systemFileManagerRepo.updateEdoc(edocUploadModel)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.resetForm();
                            this.hide();
                            this.onSearch.emit(this.transactionType);
                        }
                    }
                );
        } else {
            this._systemFileManagerRepo.uploadEDoc(this.EdocUploadFile, files, this.typeFrom)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.resetForm();
                            this.hide();
                            this.onSearch.emit(this.transactionType);
                        }
                    }
                );
        }
    }
}
export interface IEDocUploadFile {
    FolderName: string,
    ModuleName: string,
    EDocFiles: IEDocFile[],
    Id: string,
}
export interface IEDocFile {
    JobId: string,
    Code: string,
    TransactionType: string,
    AliasName: string,
    BillingNo: string,
    BillingType: string,
    HBL: string
    FileName: string,
    Note: string,
    BillingId: string,
    Id: string
}
