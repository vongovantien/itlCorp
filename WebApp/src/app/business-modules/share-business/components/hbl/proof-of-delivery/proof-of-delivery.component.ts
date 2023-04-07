import { formatDate } from '@angular/common';
import { Component } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { AppForm } from '@app';
import { SystemConstants } from '@constants';
import { Store } from '@ngrx/store';
import { NgProgress } from '@ngx-progressbar/core';
import { DocumentationRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { of } from 'rxjs';
import { catchError, concatMap, finalize, map, switchMap, takeUntil } from 'rxjs/operators';
import { ProofOfDelivery } from 'src/app/shared/models/document/proof-of-delivery';
import { SystemFileManageRepo } from 'src/app/shared/repositories/system-file-manage.repo';
import { IEDocFile } from '../../edoc/document-type-attach/document-type-attach.component';
@Component({
    selector: 'hbl-proof-of-delivery',
    templateUrl: './proof-of-delivery.component.html'
})

export class ShareBusinessProofOfDelieveyComponent extends AppForm {
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

    ngOnInit() {
        this._activedRoute.params
            .pipe(
                takeUntil(this.ngUnsubscribe),
                map((p: Params) => {
                    console.log(p)
                    if (p.hblId) {
                        this.hblid = p.hblId;
                        this.jobId = p.jobId;
                        this.getFilePOD();
                    } else {
                        this.hblid = SystemConstants.EMPTY_GUID;
                    }
                    return of(this.hblid);
                }),
                // * Get data delivery order
                switchMap((p) => {
                    return this._documentRepo.getProofOfDelivery(this.hblid);
                }),
                concatMap((data: ProofOfDelivery) => {
                    // * Update deliveryOrder model from dataDefault.
                    this.proofOfDelievey.deliveryDate = (!!data.deliveryDate) ? { startDate: new Date(data.deliveryDate), endDate: new Date(data.deliveryDate) } : null;
                    this.proofOfDelievey.referenceNo = data.referenceNo;
                    this.proofOfDelievey.deliveryPerson = data.deliveryPerson;
                    this.proofOfDelievey.note = data.note;
                    return of(this.proofOfDelievey);

                })
            )
            .subscribe((res) => { console.log("subscribe", res); });

    }

    saveProofOfDelivery() {
        this._progressRef.start();
        const deliveryDate = {
            deliveryDate: !!this.proofOfDelievey.deliveryDate && !!this.proofOfDelievey.deliveryDate.startDate ? formatDate(this.proofOfDelievey.deliveryDate.startDate, 'yyyy-MM-dd', 'en') : this.proofOfDelievey.deliveryDate.startDate == null ? null : this.proofOfDelievey.deliveryDate,
        };
        this.proofOfDelievey.hblid = this.hblid !== SystemConstants.EMPTY_GUID ? this.hblid : this.proofOfDelievey.hblid;
        this._documentRepo.updateProofOfDelivery(Object.assign({}, this.proofOfDelievey, deliveryDate))
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        if (this.fileList !== null && this.fileList.length !== 0 && Object.keys(this.files).length === 0) {
                            this.uploadFilePOD();
                        }
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
            listFileUpload[i].aliasName = "POD" + '_'+ listFileUpload[i].name.substring(0, listFileUpload[i].name.lastIndexOf('.'));
            this.listFileUpload.push(listFileUpload[i]);
        }
        this.uploadFilePOD();
    }

    uploadFilePOD() {
        const hblId = this.hblid !== SystemConstants.EMPTY_GUID ? this.hblid : this.proofOfDelievey.hblid;
        let eDocFileList: IEDocFile[] = [];
        let files: any[] = [];
        this.listFileUpload.forEach(x => {
            files.push(x);
            eDocFileList.push(({
                JobId: x.jobId !== undefined ? x.jobId : SystemConstants.EMPTY_GUID,
                Code: x.Code,
                TransactionType: "Shipment",
                AliasName: x.aliasName,
                BillingNo: '',
                BillingType: SystemConstants.EMPTY_GUID,
                HBL: hblId,
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
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.fileList = null;
                        this._toastService.success("Upload file successfully!");
                        if (!!hblId) {
                            this.getFilePOD();
                        }
                    }
                }
            );
    }

    deleteFilePOD(fileName: string) {
        this._progressRef.start();
        const hblId = this.hblid !== SystemConstants.EMPTY_GUID ? this.hblid : this.proofOfDelievey.hblid;
        // this._documentRepo.deletePODFilesAttach(this.files.id)
        //     .pipe(catchError(this.catchError), finalize(() => {
        //         this._progressRef.complete();
        //         this.isLoading = false;
        //     }))
        //     .subscribe(
        //         (res: any) => {
        //             if (res.result.success) {
        //                 this.uploadFilePOD();
        //             } else {
        //                 this._toastService.error("some thing wrong");
        //             }
        //         }
        //     );
        this._systemFileManageRepo.deleteFile('Document', 'Shipment', hblId, fileName)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any) => {
                    console.log(res);

                    if (res.status === true) {
                        //this.uploadFilePOD();
                        this._toastService.success("Delete Success");
                        this.getFilePOD();
                    } else {
                        this._toastService.error("Some Thing Wrong");
                    }
                }
            );
    }

    getFilePOD() {
        this.isLoading = true;
        // this._documentRepo.getPODFilesAttach(this.hblid !== SystemConstants.EMPTY_GUID ? this.hblid : this.proofOfDelievey.hblid).
        //     pipe(catchError(this.catchError), finalize(() => {
        //         this._progressRef.complete();
        //         this.isLoading = false;
        //     }))
        //     .subscribe(
        //         (res: any = []) => {
        //             this.files = res;
        //         }
        //     );
        this._systemFileManageRepo.getFile('Document', 'Shipment', this.hblid !== SystemConstants.EMPTY_GUID ? this.hblid : this.proofOfDelievey.hblid).
            pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            }))
            .subscribe(
                (res: any = []) => {
                    this.files = res;
                    console.log(this.files);
                }
            );
    }


}



