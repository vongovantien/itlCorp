import { formatDate } from '@angular/common';
import { ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { AppForm } from '@app';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { InjectViewContainerRefDirective } from '@directives';
import { CsTransaction } from '@models';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { catchError, concatMap, finalize, map, switchMap, takeUntil, tap } from 'rxjs/operators';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { SystemFileManageRepo } from 'src/app/shared/repositories/system-file-manage.repo';
import { getTransactionDetailCsTransactionState } from '../../../store';
import { IEDocFile } from '../../edoc/document-type-attach/document-type-attach.component';
import { Observable, of } from 'rxjs';
@Component({
    selector: 'hbl-proof-of-delivery',
    templateUrl: './proof-of-delivery.component.html'
})

export class ShareBusinessProofOfDelieveyComponent extends AppForm {
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;
    constructor(
        protected _activedRoute: ActivatedRoute,
        private _documentRepo: DocumentationRepo,
        private _systemFileManageRepo: SystemFileManageRepo,
        private _toastService: ToastrService,
        private _store: Store<IAppState>,
        private _ngProgress: NgProgress,
    ) {
        super();
        this._progressRef = this._ngProgress.ref();
    }

    hblid: string = '';
    jobId: string = '';
    proofOfDelievey: ProofOfDelivery = new ProofOfDelivery();
    fileList: any = null;
    files: any = {};
    listFileUpload: any[] = [];
    transactionType: string = '';

    ngOnInit() {
        this._activedRoute.params.pipe(
            takeUntil(this.ngUnsubscribe),
            switchMap((p: Params) => {
                this.hblid = p.hblId;
                this.jobId = p.jobId;
                return this._store.select(getTransactionDetailCsTransactionState).pipe(
                    takeUntil(this.ngUnsubscribe),
                    tap((res: CsTransaction) => {
                        this.transactionType = res.transactionType;
                        this.getProofOfDeliveryAttachedFiles();
                    }),
                    switchMap(() => this.getProofOfDelivery$())
                );
            })
        ).subscribe();
    }


    getProofOfDelivery$(): Observable<ProofOfDelivery> {
        if (!!this.hblid) {
            return this._documentRepo.getProofOfDelivery(this.hblid).pipe(
                tap((data: ProofOfDelivery) => {
                    this.proofOfDelievey.deliveryDate = (!!data.deliveryDate)
                        ? { startDate: new Date(data.deliveryDate), endDate: new Date(data.deliveryDate) }
                        : null;
                    this.proofOfDelievey.referenceNo = data.referenceNo;
                    this.proofOfDelievey.deliveryPerson = data.deliveryPerson;
                    this.proofOfDelievey.note = data.note;
                }),
                map(() => this.proofOfDelievey)
            );
        }

        return of(this.proofOfDelievey);
    }

    saveProofOfDelivery() {
        this._progressRef.start();
        const deliveryDate = {
            deliveryDate: !!this.proofOfDelievey.deliveryDate && !!this.proofOfDelievey.deliveryDate.startDate ? formatDate(this.proofOfDelievey.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : this.proofOfDelievey.deliveryDate.startDate == null ? null : this.proofOfDelievey.deliveryDate,
        };
        this.proofOfDelievey.hblid = this.hblid;
        this._documentRepo.updateProofOfDelivery(Object.assign({}, this.proofOfDelievey, deliveryDate))
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    handleFileInput(event: any) {
        this.fileList = event.target['files'];
        const listFileUpload = event.target['files']
        for (let i = 0; i < listFileUpload.length; i++) {
            listFileUpload[i].DocumentId = 0;
            listFileUpload[i].docType = 'POD';
            listFileUpload[i].Code = 'POD';
            listFileUpload[i].aliasName = "POD" + '_' + listFileUpload[i].name.substring(0, listFileUpload[i].name.lastIndexOf('.'));
            listFileUpload[i].transactionType = this.transactionType;
            this.listFileUpload.push(listFileUpload[i]);
        }
        if (listFileUpload?.length > 0) {
            let validSize: boolean = true;
            for (let i = 0; i <= listFileUpload?.length - 1; i++) {
                const fileSize: number = listFileUpload[i].size / Math.pow(1024, 2);
                if (fileSize >= 100) {
                    validSize = false;
                    break;
                }
            }
            if (!validSize) {
                this._toastService.warning("maximum file size < 100Mb");
                return;
            }
            this.uploadFilePOD();
        }
    }

    uploadFilePOD() {
        if (!!this.hblid) {
            let eDocFileList: IEDocFile[] = [];
            let files: any[] = [];
            this.listFileUpload.forEach(x => {
                files.push(x);
                eDocFileList.push(({
                    JobId: this.jobId,
                    Code: x.Code,
                    TransactionType: this.transactionType,
                    AliasName: x.aliasName,
                    BillingNo: '',
                    BillingType: SystemConstants.EMPTY_GUID,
                    HBL: this.hblid,
                    FileName: x.name,
                    Note: x.note !== undefined ? x.note : '',
                    BillingId: SystemConstants.EMPTY_GUID,
                    Id: x.id !== undefined ? x.id : SystemConstants.EMPTY_GUID,
                    DocumentId: x.DocumentId,
                    AccountingType: x.AccountingType,
                }));
            });
            const eDocUploadFile = ({
                ModuleName: "Document",
                FolderName: "Shipment",
                Id: this.hblid,
                EDocFiles: eDocFileList,
            })

            this._systemFileManageRepo.uploadEDoc(eDocUploadFile, files, "Shipment")
                .pipe(catchError(this.catchError), finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                }))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this.fileList = null;
                            this.listFileUpload = [];
                            this._toastService.success("Upload file successfully!");
                            this.getProofOfDeliveryAttachedFiles();
                        }
                    }
                );
        }
    }

    onConfirmDelete(file: any) {
        let messageDelete = `Do you want to delete this Attach File ? `;
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainer.viewContainerRef, {
            title: 'Delete Attach File',
            body: messageDelete,
            labelConfirm: 'Yes',
            classConfirmButton: 'btn-danger',
            iconConfirm: 'la la-trash',
            center: true
        }, () => this.deleteFilePOD(file))
    }

    deleteFilePOD(file: any) {
        this._progressRef.start();
        this._systemFileManageRepo.deleteEdoc(file.id, file.jobId)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    if (res.status === true) {
                        this._toastService.success("Delete Success");
                        this.getProofOfDeliveryAttachedFiles();
                    } else {
                        this._toastService.error("Some Thing Wrong");
                    }
                }
            );
    }

    getProofOfDeliveryAttachedFiles() {
        this.isLoading = true;
        if (!!this.transactionType && !!this.hblid) {
            this._systemFileManageRepo.GetProofOfDeliveryAttachedFiles(this.transactionType, this.jobId, this.hblid).
                pipe(catchError(this.catchError), finalize(() => {
                    this._progressRef.complete();
                    this.isLoading = false;
                }))
                .subscribe(
                    (res: any = []) => {
                        this.files = res;
                    }
                );
        }
    }
}



