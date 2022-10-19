import { Currency } from './../../../../shared/models/catalogue/catCurrency.model';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { catchError, finalize, skip, takeUntil } from 'rxjs/operators';
import { DocumentationRepo, SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { IAppState } from '@store';
import { Params, ActivatedRoute } from '@angular/router';
import { ConfirmPopupComponent } from '@common';
import { getTransactionDetailCsTransactionState, getTransactionLocked, getTransactionPermission } from '../../store';
import { CsTransaction } from '@models';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { PopupBase } from 'src/app/popup.base';
import { FILE } from 'dns';


@Component({
    selector: 'document-type-attach',
    templateUrl: './document-type-attach.component.html',
    styleUrls: ['./document-type-attach.component.scss']
})
export class ShareDocumentTypeAttachComponent extends PopupBase implements OnInit {

    @ViewChild('confirmDelete') confirmDeletePopup: ConfirmPopupComponent;
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();
    headers: CommonInterface.IHeaderTable[];


    jobId: string;

    files: IShipmentAttachFile[] = [];
    selectedFile: IShipmentAttachFile;

    isOps: boolean = false;

    fileNo: string;
    transactionType: string = '';
    EdocUploadFile: IEDocUploadFile;
    listFile: any[] = [];

    isUpdate: boolean = false;

    selectedtDocType: any = null;

    formData: IEDocUploadFile;

    documentTypes: any[] = [];
    //['BOOKING', 'CD', 'MV', 'HAWB', 'CI', 'PL', 'CO', 'PHY', 'FUMI', 'QC', 'TXTN', 'INV ITL', 'ROD', 'IRR', 'INV OBH', 'OTHERS', 'AN', 'CONTRACT, PO', 'TNTX', 'POD', 'HBL', 'BL', 'TNTX/TXTK', 'Shipper booking', 'Carrier booking', 'FMC rate', 'MBL', 'INV', 'CN', 'AGENT AN', 'IRR', 'Prealert', 'WH AN', 'Rate', 'Airline booking', 'Airline Rate', 'SID', 'MAWB', 'MNF'];
    source: string = 'Job';
    accepctFilesUpload = 'image/*,.txt,.pdf,.doc,.xlsx,.xls';

    constructor(
        private _toastService: ToastrService,
        private _store: Store<IAppState>,
        private _activedRoute: ActivatedRoute,
        private _systemFileManagerRepo: SystemFileManageRepo,
    ) {
        super();

        this.isLocked = this._store.select(getTransactionLocked);

        this.permissionShipments = this._store.select(getTransactionPermission);

    }

    ngOnInit(): void {
        this._activedRoute.params
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((params: Params) => {
                console.log(params);
                if (params.jobId) {
                    this.jobId = params.jobId;
                    this.getFileShipment(this.jobId);
                    console.log(params);
                } else {
                    this.jobId = params.id;
                    this.getFileShipment(this.jobId);
                    this.isOps = true;
                }
            });

        if (this.isOps == false) {
            this._store.select(getTransactionDetailCsTransactionState)
                .pipe(skip(1), takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: CsTransaction) => {
                        this.fileNo = res.jobNo;
                        this.transactionType = res.transactionType;
                    }
                );
        } else {
            this._store.select(getOperationTransationState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (res: any) => {
                        this.fileNo = res.opstransaction.jobNo;
                    }
                );
        }
    }

    chooseFile(event: any) {
        const fileList = event.target['files'];
        const files: any[] = event.target['files'];
        //this.listFile = files;
        console.log(this.listFile);
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

    onSelectDataFormInfo(event: any, index: number) {
        console.log(event);
        this.listFile[index].docType = event;
        this.listFile[index].aliasName = event + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
        console.log(this.listFile);
    }

    getFileShipment(jobId: string) {
        this.isLoading = true;
        this._systemFileManagerRepo.getFile('Document', 'Shipment', jobId).
            pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            }))
            .subscribe(
                (res: IShipmentAttachFile[] = []) => {
                    this.files = res;
                    this.filterViewFile();
                    this.files.forEach(f => f.extension = f.name.split("/").pop().split('.').pop());
                }
            );
    }

    deleteFile(file: IShipmentAttachFile) {
        if (!!file) {
            this.selectedFile = file;
            console.log(this.selectedFile);
            this.confirmDeletePopup.show();
        }
    }

    onDeleteFile() {
        this.confirmDeletePopup.hide();
        this._systemFileManagerRepo.deleteFile('Document', 'Shipment', this.jobId, this.selectedFile.name)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.status) {
                        this._toastService.success("File deleted successfully!");
                        this.getFileShipment(this.jobId);
                    } else {
                        this._toastService.error("some thing wrong");
                    }
                }
            );
    }

    dowloadAllAttach() {
        if (this.fileNo) {
            let arr = this.fileNo.split("/");
            let model = {
                folderName: "Shipment",
                objectId: this.jobId,
                fileName: arr[0] + "_" + arr[1] + ".zip"
            }
            this._systemFileManagerRepo.dowloadallAttach(model)
                .subscribe(
                    (res: any) => {
                        this.downLoadFile(res, "application/zip", model.fileName);
                    }
                )
        }
    }

    filterViewFile() {
        if (this.files) {
            let type = ["xlsx", "xls", "doc", "docx"];
            for (let i = 0; i < this.files.length; i++) {
                let f = this.files[i];
                if (type.includes(f.name.split('.').pop())) {
                    f.dowFile = true
                    f.viewFileUrl = `https://gbc-excel.officeapps.live.com/op/view.aspx?src=${f.url}`;
                }
                else {
                    f.dowFile = false;
                    f.viewFileUrl = f.url;
                }
            }
        }
    }

    resetForm() {
        this.listFile.splice(0, this.listFile.length);
    }
    removeFile(index: number) {

        this.listFile.splice(index, 1);


    }

    uploadEDoc() {
        console.log(this.listFile);
        let edocFileList: IEDocFile[] = [];
        let files: any[] = [];
        this.listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                //FileInput: x,
                JobId: this.jobId,
                Code: x.docType,
                TransactionType: this.transactionType,
                AliasName: x.docType + x.name,
                BillingNo: null,
                BillingType: null,
                HBL: null,
                FileName: x.name
            }));
        });
        console.log(edocFileList);

        this.EdocUploadFile = ({
            ModuleName: 'Document',
            FolderName: 'Shipment',
            Id: this.jobId,
            EDocFiles: edocFileList,
        })
        console.log(this.EdocUploadFile);

        this._systemFileManagerRepo.uploadEDoc(this.EdocUploadFile, files)
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


interface IShipmentAttachFile {
    id: string;
    name: string;
    thumb: string;
    url: string;
    folder: string;
    objectId: string;
    extension: string;
    userCreated: string;
    dateTimeCreated: string;
    fileName: string;
    dowFile: boolean;
    viewFileUrl: string;
}

export interface IEDocUploadFile {
    FolderName: string,
    ModuleName: string,
    EDocFiles: IEDocFile[],
    Id: string,
}
export interface IEDocFile {
    //FileInput: any,
    JobId: string,
    Code: string,
    TransactionType: string,
    AliasName: string,
    BillingNo: string,
    BillingType: string,
    HBL: string,
    FileName: string
}
