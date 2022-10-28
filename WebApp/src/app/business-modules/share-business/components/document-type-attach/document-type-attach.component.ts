import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
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
    @Input() jobNo: string = '';
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();
    headers: CommonInterface.IHeaderTable[] = [];
    @Input() jobId: string = '';
    isOps: boolean = false;
    @Input() transactionType: string = '';
    EdocUploadFile: IEDocUploadFile;
    listFile: any[] = [];
    isUpdate: boolean = false;
    selectedtDocType: any = null;
    edocSelected: any;
    detailDocId: number;
    formData: IEDocUploadFile;
    @Input() typeFrom: string = 'Shipment';
    documentTypes: any[] = [];
    source: string = 'Shipment';
    accepctFilesUpload = 'image/*,.txt,.pdf,.doc,.xlsx,.xls';
    @Input() housebills: any[] = [];
    billingNo: string = '';
    @Input() billingId: string = '';
    chargeSM: any;
    isSubmitted: boolean = false;
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
        if (this.typeFrom !== 'Shipment') {
            this.transactionType = this.typeFrom;
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
                console.log(event);
                this.listFile[index].Code = event.code;
                this.listFile[index].DocumentId = event.id;
                this.listFile[index].aliasName = this.isUpdate ? event.code + this.listFile[index].name : event.code + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                this.selectedtDocType = event.id;
                break;
            case 'aliasName':
                this.listFile[index].aliasName = event;
                break;
            case 'houseBill':
                if (this.typeFrom === 'Shipment') {
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
        this.isSubmitted = true;
        this.listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                JobId: this.jobId !== undefined ? this.jobId : SystemConstants.EMPTY_GUID,
                Code: x.Code,
                TransactionType: this.transactionType,
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: '',
                HBL: x.hblid !== undefined ? x.hblid : SystemConstants.EMPTY_GUID,
                FileName: x.name,
                Note: x.note !== undefined ? x.note : '',
                BillingId: this.billingId !== '' ? this.billingId : SystemConstants.EMPTY_GUID,
                Id: x.id !== undefined ? x.id : SystemConstants.EMPTY_GUID,
                DocumentId: x.DocumentId
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
            if (edocUploadModel.DocumentTypeId === undefined) {
                console.log(edocUploadModel.DocumentTypeId);

                this._toastService.error("Please fill all field!");
                return;
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
            console.log(edocFileList.find(x => x.DocumentId));

            if (edocFileList.find(x => x.DocumentId === undefined)) {

                this._toastService.error("Please fill all field!");
                return;
            }
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
    Id: string,
    DocumentId: string
}
