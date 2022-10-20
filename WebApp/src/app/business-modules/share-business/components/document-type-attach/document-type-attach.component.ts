import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { ConfirmPopupComponent } from '@common';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { SystemFileManageRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError, skip, takeUntil } from 'rxjs/operators';
import { getOperationTransationState } from 'src/app/business-modules/operation/store';
import { PopupBase } from 'src/app/popup.base';
import { getTransactionDetailCsTransactionState, getTransactionLocked, getTransactionPermission } from '../../store';


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

    isOps: boolean = false;

    fileNo: string;
    transactionType: string = '';
    EdocUploadFile: IEDocUploadFile;
    listFile: any[] = [];

    isUpdate: boolean = false;

    selectedtDocType: any = null;

    formData: IEDocUploadFile;

    documentTypes: any[] = [];
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
                    console.log(params);
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

        if (this.isUpdate) {

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

    onSelectDataFormInfo(event: any, index: number, type: string) {
        console.log(event);
        console.log(this.listFile[index]);
        switch (type) {
            case 'docType':

                this.listFile[index].docType = event;
                this.listFile[index].aliasName = this.isUpdate ? event + this.listFile[index].name : event + this.listFile[index].name.substring(0, this.listFile[index].name.lastIndexOf('.'))
                console.log(this.listFile);
                break;
            case 'aliasName':
                console.log(event);
                this.listFile[index].aliasName = event;
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
        console.log(this.listFile);
        let edocFileList: IEDocFile[] = [];
        let files: any[] = [];
        console.log(this.listFile);

        this.listFile.forEach(x => {
            files.push(x);
            edocFileList.push(({
                JobId: this.jobId,
                Code: x.docType,
                TransactionType: this.transactionType,
                AliasName: x.aliasName,
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
    HBL: string,
    FileName: string
}
