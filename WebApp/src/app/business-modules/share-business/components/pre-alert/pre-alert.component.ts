import { Component, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { Router, Params, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { HttpErrorResponse } from '@angular/common/http';

import { AppForm } from '@app';
import { DocumentationRepo, ExportRepo } from '@repositories';
import { IAppState } from '@store';
import { ChargeConstants } from '@constants';
import { Crystal, EmailContent } from '@models';
import { ReportPreviewComponent } from '@common';
import { RoutingConstants, SystemConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
import { getTransactionLocked, TransactionGetDetailAction } from '@share-bussiness';


import { combineLatest, forkJoin, of, from, Observable, timer, throwError } from 'rxjs';
import { catchError, finalize, map, take, switchMap, mergeMap, delay, takeUntil, retryWhen, delayWhen, concatMap } from 'rxjs/operators';

import { ShareBusinessAddAttachmentPopupComponent } from '../add-attachment/add-attachment.popup';
import { environment } from 'src/environments/environment';
import { NgxSpinnerService } from 'ngx-spinner';
import { InjectViewContainerRefDirective } from '@directives';
import { lowerCase } from 'lodash';
@Component({
    selector: 'share-pre-alert',
    templateUrl: './pre-alert.component.html'
})
export class ShareBusinessReAlertComponent extends AppForm implements ICrystalReport {

    @ViewChild(ShareBusinessAddAttachmentPopupComponent) attachmentPopup: ShareBusinessAddAttachmentPopupComponent;
    @ViewChild(ReportPreviewComponent) reportPopup: ReportPreviewComponent;
    @ViewChild('formReportPDF', { static: true }) formRp: ElementRef;
    @ViewChild(InjectViewContainerRefDirective) public reportContainerRef: InjectViewContainerRefDirective;

    srcReportPDF: any = `${environment.HOST.EXPORT_CRYSTAL}`;
    valuePDF: any = null;
    numOfFileExp: number = 1;

    form: FormGroup;
    files: IShipmentAttachFile[] = [];
    jobId: string;
    hblId: string;
    hawbDetails: any[] = [];

    isSubmited: boolean = false;

    formMail: FormGroup;
    from: AbstractControl;
    to: AbstractControl;
    cc: AbstractControl;
    subject: AbstractControl;
    body: AbstractControl;

    attachedFile: string[] = [];

    sendMailButtonName: string = '';
    serviceId: string = '';
    name: string = '';
    isPreAlert: boolean = false;
    // Import
    isExitsArrivalNotice: boolean = false;
    isCheckedArrivalNotice: boolean = false;
    isExitsDebitNote: boolean = false;
    isCheckedDebitNote: boolean = false;
    isExitsDO: boolean = false;
    isCheckedDO: boolean = false;

    isExitsManifest: boolean = false;
    isCheckedManifest: boolean = false;
    isExitsHawb: boolean = false;
    isCheckedHawb: boolean = false;
    isExitsSI: boolean = false;
    isCheckedSI: boolean = false;
    isExitsSISummary: boolean = false;
    isCheckedSISummary: boolean = false;
    isExitsSIDetailCont: boolean = false;
    isCheckedSIDetailCont: boolean = false;

    pathGeneralArrivalNotice: string = '';
    pathGeneralDebitNote: string = '';
    pathGeneralDO: string = '';
    pathGeneralManifest: string = '';
    pathGeneralMawb: string = '';
    pathGeneralSI: string = '';
    pathGeneralSISummary: string = '';
    pathGeneralSIDetailCont: string = '';
    hblRptName: string = '';
    listSI: string[] = [];
    debitNos: any[] = [];

    headers: any[] = [
        { title: 'Attach File', field: 'name' }
    ];

    constructor(
        private _documentRepo: DocumentationRepo,
        private _export: ExportRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _store: Store<IAppState>,
        private _activedRouter: ActivatedRoute,
        private _fb: FormBuilder,
        private _spinner: NgxSpinnerService,
        private _router: Router,
        private _cd: ChangeDetectorRef) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit(): void {
        this.subscription = combineLatest([
            this._activedRouter.params,
            this._activedRouter.data,
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            take(1),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (params: Params) => {
                if (params.jobId) {
                    this.jobId = params.jobId;
                    this.hblId = !params.hblId || params.hblId === 'undefined' ? SystemConstants.EMPTY_GUID : params.hblId;
                    this.serviceId = params.serviceId;
                    this.name = params.name;
                    this.isPreAlert = this.name === "Pre Alert";
                    this.hblRptName = (this.serviceId === ChargeConstants.SFE_CODE || this.serviceId === ChargeConstants.SLE_CODE) ? "HBL" : "HAWB";

                    this.getDetailHAWB();
                    this.getContentMail(this.serviceId, this.hblId, this.jobId);

                    // Is Locked Shipment?
                    this._store.dispatch(new TransactionGetDetailAction(this.jobId));
                    this.isLocked = this._store.select(getTransactionLocked);
                }
            }
        );

        this.initForm();
    }

    initForm() {
        this.formMail = this._fb.group({
            from: ['',
                Validators.compose([
                    Validators.required,
                    Validators.pattern(SystemConstants.CPATTERN.EMAIL_SINGLE)
                ])
            ],
            to: ['',
                Validators.compose([
                    Validators.required,
                    Validators.pattern(SystemConstants.CPATTERN.EMAIL_MULTIPLE)
                ])
            ],
            cc: this.serviceId.indexOf('I') !== -1 ? null : ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            subject: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            body: []
        });

        this.from = this.formMail.controls['from'];
        this.to = this.formMail.controls['to'];
        this.cc = this.formMail.controls['cc'];
        this.subject = this.formMail.controls['subject'];
        this.body = this.formMail.controls['body'];
    }

    exportFileCrystalToPdf(serviceId: string) {
        // Export Report Arrival Notice to PDF
        switch (serviceId) {
            case ChargeConstants.AI_CODE: // Air Import
                this.isExitsArrivalNotice = true;
                this.isCheckedArrivalNotice = true;
                
                this.isExitsDO = true;
                this.checkExistDebitNote();
                break;
            case ChargeConstants.AE_CODE: // Air Export
                this.checkExistManifestExport();
                break;
            case ChargeConstants.SFI_CODE: // Sea FCL Import
            case ChargeConstants.SLI_CODE: // Sea LCL Import
                this.isExitsArrivalNotice = true;
                this.isCheckedArrivalNotice = true;
                this.isExitsDO = true;
                this.checkExistDebitNote();
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.isPreAlert) {
                    this.checkExistManifestExport();
                } else {
                    this.checkExistSIExport();
                }
                break;
            default:
                break;
        }
    }

    checkExistSIExport() {
        this._documentRepo.checkExistSIExport(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: string[]) => {
                    this.listSI = res;
                    if (res[0]) {
                        this.isExitsSISummary = true;
                        this.isCheckedSISummary = true;
                    } else {
                        this.isExitsSISummary = false;
                        this.isCheckedSISummary = false;
                    }
                    if (res[1]) {
                        this.isExitsSI = true;
                        this.isCheckedSI = true;
                    } else {
                        this.isExitsSI = false;
                        this.isCheckedSI = false;
                    }
                    if (res[2]) {
                        this.isExitsSIDetailCont = true;
                        this.isCheckedSIDetailCont = true;
                    } else {
                        this.isExitsSIDetailCont = false;
                        this.isCheckedSIDetailCont = false;
                    }
                });
    }

    checkExistManifestExport() {
        this._documentRepo.checkExistManifestExport(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: boolean) => {

                    if (res) {
                        this.isExitsManifest = true;
                        this.isCheckedManifest = true;
                    } else {
                        this.isExitsManifest = false;
                        this.isCheckedManifest = false;
                    }
                    this.isExitsHawb = true;
                    this.isCheckedHawb = true;
                });
    }

    checkExistDebitNote(){
        this._documentRepo.getListCDNoteWithHbl(this.hblId)
        .pipe(
            catchError(this.catchError),
            finalize(() => { })
        )
        .subscribe(
            (res: any) => {                
                if (res) {
                    this.debitNos = res.filter(x=> lowerCase(x.type) !== 'credit').map(v => ({ ...v, isCheckedDebitNote: false }));
                    this.isExitsDebitNote = true;
                } else {
                    this.isExitsDebitNote = false;
                }
            });
    }

    onAddFile(files: any) {
        if (this.files.length === 0) {
            this.files = files;
        } else {
            files.forEach(element => {
                for (const file of this.files) {
                    if (file.id !== element.id) {
                        this.files.push(element);
                        break;
                    }
                }
            });
        }

        const filesToUpdate = files.filter(f => f.isTemp === true);
        if (filesToUpdate.length > 0) {
            this._progressRef.start();
            this._documentRepo.updateFilesToShipment(filesToUpdate)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res) {
                            this._toastService.success("Upload file successfully!");

                        }
                    }
                );
        }
    }

    showPopup() {
        this.attachmentPopup.files.forEach(element => {
            element.isChecked = false;
        });
        this.attachmentPopup.checkAll = false;
        this.attachmentPopup.getFileShipment(this.jobId);
        this.attachmentPopup.show();
    }

    getStreamUploadFile(service: string) {
        const streamUploadReport = [];
        switch (service) {
            case ChargeConstants.AI_CODE:
                if (this.isExitsArrivalNotice && this.isCheckedArrivalNotice) {
                    streamUploadReport.push(this._documentRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: 'VND' }));
                }
                if (this.isExitsDO && this.isCheckedDO) {
                    streamUploadReport.push(this._documentRepo.previewAirProofofDelivery(this.hblId));
                }
                this.debitNos.forEach(element => {
                    if (element.isCheckedDebitNote) {
                        streamUploadReport.push(this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }))
                    }
                });
                break;
            case ChargeConstants.AE_CODE:
                this.hawbDetails.forEach(ele => {
                    if (ele.isCheckedHawb) {
                        streamUploadReport.push(this._documentRepo.previewHouseAirwayBillLastest(ele.id, 'LASTEST_ITL_FRAME'));
                    }
                });
                if (this.isCheckedManifest) {
                    streamUploadReport.push(this._documentRepo.previewAirExportManifestByJobId(this.jobId));
                }
                break;
            case ChargeConstants.SFI_CODE:
            case ChargeConstants.SLI_CODE:
                if (this.isExitsArrivalNotice && this.isCheckedArrivalNotice) {
                    streamUploadReport.push(this._documentRepo.previewArrivalNotice({ hblId: this.hblId, currency: 'VND' }))
                }
                if (this.isExitsDO && this.isCheckedDO) {
                    streamUploadReport.push(this._documentRepo.previewDeliveryOrder(this.hawbDetails[0].id));
                }
                this.debitNos.forEach(element => {
                    if (element.isCheckedDebitNote) {
                        streamUploadReport.push(this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }));
                    }
                });
                break;
            case ChargeConstants.SFE_CODE:
            case ChargeConstants.SLE_CODE:
                if (this.isPreAlert) {
                    if (this.isCheckedManifest) {
                        streamUploadReport.push(this._documentRepo.previewSeaExportManifestByJobId(this.jobId));
                    }
                    this.hawbDetails.forEach(ele => {
                        if (ele.isCheckedHawb) {
                            streamUploadReport.push(this._documentRepo.previewSeaHBLOfLanding(ele.id, 'ITL_FRAME'));
                        }
                    });
                } else {
                    if (this.isCheckedSISummary) {
                        streamUploadReport.push(this._documentRepo.previewSISummaryByJobId(this.jobId));
                    }
                    if (this.isCheckedSI) {
                        streamUploadReport.push(this._documentRepo.previewSIReportByJobId(this.jobId));
                    }
                    if (this.isCheckedSIDetailCont) {
                        streamUploadReport.push(this._documentRepo.previewSIContReport(this.jobId));
                    }
                }
                break;
            default:
                break;
        }

        return streamUploadReport;
    }

    sendMailSubmit() {
        this.isSubmited = true;
        if (!this.formMail.valid) {
            return;
        }
        const streamUploadFile = this.getStreamUploadFile(this.serviceId);

        this.attachedFile = [];
        if (!!streamUploadFile.length) {
            this.numOfFileExp = streamUploadFile.length;
            this.uploadFileStream(streamUploadFile);
        } else {
            this.sendMail();
        }
    }

    uploadFileStream(streamUploadFile: Observable<any>[]) {
        let dataStreamCount = 0;
        const sourcePromises = streamUploadFile.map(x => x.toPromise());
        Promise.all([...sourcePromises])
            .then((res) => {
                const pathReports = res.map(x => x.pathReportGenerate);
                this.attachedFile = [...pathReports];
                const sources = res.map(x => this._export.exportCrystalReportPDF(x).toPromise())
                Promise.all(sources)
                    .then(
                        (error) => {
                            console.log(error);
                        },
                    )
                    .catch((err) => {
                        console.error(err);
                    })
                    .finally(() => {
                        setTimeout(() => {
                            this.sendMail();
                        }, 3000);
                        
                        console.log('Experiment completed');
                    });
            })
            .catch((err) => {
                console.error(err);
            });
    }

    sendMail() {
        console.log("after 5s send");

        this.attachFileUpload();

        const emailContent: EmailContent = {
            from: this.from.value,
            to: this.to.value,
            cc: this.cc.value,
            subject: this.subject.value,
            body: this.body.value,
            attachFiles: this.attachedFile.filter(x => Boolean(x))
        };
        this._spinner.show();
        this._documentRepo.sendMailDocument(emailContent)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._spinner.hide()),
                mergeMap((err: CommonInterface.IResult) => {
                    if (!err.status) {
                        return throwError("Error when sendmail");
                    }
                    return of(err);
                }),
                retryWhen(errors => errors.pipe(
                    take(3),
                )),
                map(err => err),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.deleteFileTemp(this.jobId);
                    } else {
                        this._toastService.error(res.message);
                    }
                },
                (err) => {
                    this._toastService.error(err);
                }
            );
    }

    attachFileUpload() {
        // const _attachFileUpload = this.hashedUrlFileUpload();
        // _attachFileUpload.forEach(element => {
        //     const idxOf = this.attachedFile.indexOf(element);
        //     if (idxOf === -1) {
        //         this.attachedFile.push(element);
        //     }
        // });
        const shipmentFileUrls: string[] = (this.files || []).map(x => x.url);
        this.attachedFile = [...this.attachedFile, ...shipmentFileUrls];
    }

    hashedUrlFileUpload() {
        const attachFiles = [];
        const prefix = "/files/";
        // tslint:disable-next-line: prefer-for-of
        for (let i = 0; i < this.files.length; i++) {
            const urlFile = this.files[i].url;
            if (urlFile.indexOf(prefix) > -1) {
                const urlWeb = urlFile.substring(0, urlFile.indexOf(prefix));
                const url = urlFile.replace(urlWeb, './wwwroot').split('/').join('\\');
                attachFiles.push(url);
            }
        }
        return attachFiles;
    }

    deleteFileTemp(jobId: string) {
        this._documentRepo.deleteFileTempPreAlert(jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe();
    }

    //#region Content Mail
    getContentMail(serviceId: string, hblId: string, jobId: string) {
        switch (serviceId) {
            case ChargeConstants.AI_CODE: // Air Import
                this.sendMailButtonName = "Send Arrival Notice";
                this.getInfoMailHBLAirImport(hblId);
                break;
            case ChargeConstants.SFI_CODE: // Sea Import
            case ChargeConstants.SLI_CODE:
                this.sendMailButtonName = "Send Arrival Notice";
                this.getInfoMailHBLSeaImport(hblId, serviceId);
                break;
            case ChargeConstants.AE_CODE: // Air Export
                this.sendMailButtonName = "Send Pre Alert";
                this.getInfoMailHBLAirExport(hblId, jobId);
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
                if (this.isPreAlert) {
                    this.sendMailButtonName = "Send Pre Alert";
                    this.getInfoMailHBLPreAlertSeaExport(hblId, jobId, serviceId);
                } else {
                    this.sendMailButtonName = "Send S.I";
                    this.getInfoMailSISeaExport(jobId);
                }
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.isPreAlert) {
                    this.sendMailButtonName = "Send Pre Alert";
                    this.getInfoMailHBLPreAlertSeaExport(hblId, jobId, serviceId);
                } else {
                    this.sendMailButtonName = "Send S.I";
                    this.getInfoMailSISeaExport(jobId);
                }
                break;
            default:
                break;
        }
    }

    getInfoMailHBLAirImport(hblId: string) {
        this._documentRepo.getInfoMailHBLAirImport(hblId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailHBLSeaImport(hblId: string, serviceId: string) {
        this._documentRepo.getInfoMailHBLSeaImport(hblId, serviceId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailHBLAirExport(hblId: string, jobId: string) {
        this._documentRepo.getInfoMailHBLAirExport(hblId, jobId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailHBLPreAlertSeaExport(hblId: string, jobId: string, serviceId: string) {
        this._documentRepo.getInfoMailHBLPreAlertSeaExport(hblId, jobId, serviceId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailSISeaExport(jobId: string) {
        this._progressRef.start();
        this._documentRepo.getInfoMailSISeaExport(jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.setValue({
                        from: res.from,
                        to: res.to,
                        cc: res.cc,
                        subject: res.subject,
                        body: res.body
                    });
                },
            );
    }
    //#endregion Content Mail

    //#region Preview Report
    previewArrivalNotice() {
        this._progressRef.start();
        switch (this.serviceId) {
            case ChargeConstants.AI_CODE:
                this._documentRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: 'VND' })
                    .pipe(
                        catchError(this.catchError),
                        finalize(() => { this._progressRef.complete(); })
                    )
                    .subscribe(
                        (res: Crystal) => {
                            this.dataReport = res;
                            if (res?.dataSource?.length > 0) {
                                this.dataReport = res;
                                this.renderAndShowReport();
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        },
                    );
                break;
            case ChargeConstants.SFI_CODE:
            case ChargeConstants.SLI_CODE:
                this._documentRepo.previewArrivalNotice({ hblId: this.hblId, currency: 'VND' })
                    .pipe(
                        catchError(this.catchError),
                        finalize(() => { this._progressRef.complete(); })
                    )
                    .subscribe(
                        (res: Crystal) => {
                            if (res?.dataSource?.length > 0) {
                                this.dataReport = res;
                                this.renderAndShowReport();
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        },
                    );
                break;
        }
    }

    previewManifest() {
        if (this.serviceId === ChargeConstants.SFE_CODE || this.serviceId === ChargeConstants.SLE_CODE) {
            this._progressRef.start();
            this._documentRepo.previewSeaExportManifestByJobId(this.jobId)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { })
                )
                .subscribe(
                    (res: Crystal) => {
                        this.dataReport = res;
                        if (res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
        } else {
            this._progressRef.start();
            this._documentRepo.previewAirExportManifestByJobId(this.jobId)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); })
                )
                .subscribe(
                    (res: Crystal) => {
                        this.dataReport = res;
                        if (res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
        }
    }

    previewHawb(hblId: string) {
        if (this.serviceId === ChargeConstants.SFE_CODE || this.serviceId === ChargeConstants.SLE_CODE) {
            this.previewHBL(hblId);
        } else {
            this._progressRef.start();
            this._documentRepo.previewHouseAirwayBillLastest(hblId, 'LASTEST_ITL_FRAME')
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); })
                )
                .subscribe(
                    (res: Crystal) => {
                        this.dataReport = res;
                        if (res.dataSource.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
        }
    }

    previewHBL(hblId: string) {
        this._progressRef.start();
        this._documentRepo.previewSeaHBLOfLanding(hblId, 'ITL_FRAME')
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewSI() {
        this._progressRef.start();
        this._documentRepo.previewSIReportByJobId(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewSISummary() {
        this._progressRef.start();
        this._documentRepo.previewSISummaryByJobId(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewSIDetailContFCL() {
        this._progressRef.start();
        this._documentRepo.previewSIContReport(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewSIDetailContLCL() {
        this._progressRef.start();
        this._documentRepo.previewSIContLCLReport(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (res.dataSource.length > 0) {
                        this.renderAndShowReport();
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    previewDO() {
        if (this.serviceId === 'AI') {
            this._documentRepo.previewAirImportAuthorizeLetter1(this.hblId, false)
                .pipe(
                ).subscribe(
                    (res: any) => {
                        if (res !== false) {
                            if (res?.dataSource?.length > 0) {
                                this.dataReport = res;
                                this.renderAndShowReport();
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        }

                    },
                );
        } else{
            this._documentRepo.previewDeliveryOrder(this.hawbDetails[0].id)
            .pipe(
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        this.dataReport = res;
                        if (this.dataReport.dataSource?.length > 0) {
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no container data to display preview');
                        }
                    }
                },
            );
        }
    }

    previewDebitNote(code: string) {
        if (this.serviceId === 'AI') {
            this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: code, currency: 'VND' })
                .pipe(
                ).subscribe(
                    (res: any) => {
                        if (res !== false) {
                            if (res?.dataSource?.length > 0) {
                                this.dataReport = res;
                                this.renderAndShowReport();
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        }
                    },
                );
        }else{
            this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: code, currency: 'VND' })
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res != null) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport();
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
        }
    }

    previewSIDetailCont(serviceType) {
        switch (serviceType) {
            case ChargeConstants.SFE_CODE:
                this.previewSIDetailContFCL();
                break;
            case ChargeConstants.SLE_CODE:
                this.previewSIDetailContLCL();
                break;
        }
    }

    onChangeCheckBox() {
        switch (this.serviceId) {
            case ChargeConstants.AI_CODE: // Air Import               
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralArrivalNotice, this.isCheckedArrivalNotice);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralDO, this.isCheckedDO);
                break;
            case ChargeConstants.AE_CODE: // Air Export               
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralManifest, this.isCheckedManifest);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralMawb, this.isCheckedHawb);
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
                if (this.isPreAlert) {
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralManifest, this.isCheckedManifest);
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralMawb, this.isCheckedHawb);
                } else {
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSISummary, this.isCheckedSISummary);
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSI, this.isCheckedSI);
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSIDetailCont, this.isCheckedSIDetailCont);
                }
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.isPreAlert) {
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralManifest, this.isCheckedManifest);
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralMawb, this.isCheckedHawb);
                } else {
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSISummary, this.isCheckedSISummary);
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSI, this.isCheckedSI);
                    this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSIDetailCont, this.isCheckedSIDetailCont);
                }
                break;
            case ChargeConstants.SFI_CODE: // Air Import
            case ChargeConstants.SLI_CODE:
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralArrivalNotice, this.isCheckedArrivalNotice);
                break;
            default:
                break;
        }
    }

    UpdateAttachFileByPathGeneralReport(pathGeneral: string, isChecked: boolean) {
        const idxOf = (this.attachedFile || []).indexOf(pathGeneral);
        if (!isChecked) {
            if (idxOf > -1) {
                (this.attachedFile || []).splice(idxOf, 1);
            }
        } else {
            if (idxOf === -1) {
                (this.attachedFile || []).push(pathGeneral);
            }
        }
    }

    cancelPreAlert() {
        switch (this.serviceId) {
            case ChargeConstants.AI_CODE: // Air Import
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                break;
            case ChargeConstants.AE_CODE: // Air Export
                if (this.hblId === SystemConstants.EMPTY_GUID) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_EXPORT}/${this.jobId}/hbl/${this.hblId}`]);
                }
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
                if (this.hblId === SystemConstants.EMPTY_GUID) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_EXPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_EXPORT}/${this.jobId}/si`]);
                }
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.hblId === SystemConstants.EMPTY_GUID) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/si`]);
                }
                break;
            case ChargeConstants.SFI_CODE: // Sea FCL Import
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                break;
            case ChargeConstants.SLI_CODE: // Sea LCL Import
                this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                break;
            default:
                break;
        }
    }

    getDetailHAWB() {
        this._documentRepo.getHAWBListOfShipment(this.jobId)
            .subscribe(
                (res: any[]) => {
                    if (this.hblId === SystemConstants.EMPTY_GUID) {
                        this.hawbDetails = [...res.map(v => ({ ...v, isCheckedHawb: true }))];
                    } else {
                        this.hawbDetails = res.filter(x => x.id === this.hblId).map(v => ({ ...v, isCheckedHawb: true }));
                    }
                    this.exportFileCrystalToPdf(this.serviceId);
                },
            );
    }

    @delayTime(1000)
    showReport(): void {
        // this.reportPopup.frm.nativeElement.submit();
        // this.reportPopup.show();
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.show();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.reportContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.reportContainerRef.viewContainerRef.clear();
            });
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
}
