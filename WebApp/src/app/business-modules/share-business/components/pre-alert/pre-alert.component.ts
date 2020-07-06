import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ShareBusinessAddAttachmentPopupComponent } from '../add-attachment/add-attachment.popup';
import { DocumentationRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize, map, take } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { IAppState, getParamsRouterState, getDataRouterState } from '@store';
import { combineLatest } from 'rxjs';
import { ChargeConstants } from 'src/constants/charge.const';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { EmailContent } from 'src/app/shared/models/document/emailContent';
import { Crystal } from '@models';
import { ReportPreviewComponent, ExportCrystalComponent } from '@common';
@Component({
    selector: 'share-pre-alert',
    templateUrl: './pre-alert.component.html'
})
export class ShareBusinessReAlertComponent extends AppList {
    @ViewChild(ShareBusinessAddAttachmentPopupComponent, { static: false }) attachmentPopup: ShareBusinessAddAttachmentPopupComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) reportPopup: ReportPreviewComponent;
    @ViewChild(ExportCrystalComponent, { static: false }) exportReportPopup: ExportCrystalComponent;
    @ViewChild(ExportCrystalComponent, { static: false }) exportReportPopup2: ExportCrystalComponent;
    @ViewChild(ExportCrystalComponent, { static: false }) exportReportPopup3: ExportCrystalComponent;
    files: IShipmentAttachFile[] = [];
    jobId: string;
    hblId: string;

    isSubmited: boolean = false;

    formMail: FormGroup;
    from: AbstractControl;
    to: AbstractControl;
    cc: AbstractControl;
    subject: AbstractControl;
    body: AbstractControl;

    dataReport: Crystal = null;
    dataExportReport: Crystal = null;
    dataExportReport2: Crystal = null;
    dataExportReport3: Crystal = null;
    attachedFile: string[] = [];

    sendMailButtonName: string = '';
    serviceId: string = '';
    isExitsArrivalNotice: boolean = true;
    isCheckedArrivalNotice: boolean = false;
    isExitsManifest: boolean = true;
    isCheckedManifest: boolean = false;
    isExitsMawb: boolean = true;
    isCheckedMawb: boolean = false;
    isExitsSI: boolean = true;
    isCheckedSI: boolean = false;
    isExitsSISummary: boolean = true;
    isCheckedSISummary: boolean = false;
    isExitsSIDetailCont: boolean = true;
    isCheckedSIDetailCont: boolean = false;

    pathGeneralArrivalNotice: string = '';
    pathGeneralManifest: string = '';
    pathGeneralMawb: string = '';
    pathGeneralSI: string = '';
    pathGeneralSISummary: string = '';
    pathGeneralSIDetailCont: string = '';

    constructor(
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _ngProgressService: NgProgress,
        private _store: Store<IAppState>,
        private _fb: FormBuilder) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit(): void {
        combineLatest([
            this._store.select(getParamsRouterState),
            this._store.select(getDataRouterState),
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
            take(1)
        ).subscribe(
            (params: any) => {
                if (params.jobId) {
                    this.jobId = params.jobId;
                    this.hblId = params.hblId;
                    this.serviceId = params.serviceId;
                    this.exportFileCrystalToPdf(params.serviceId);
                    this.getContentMail(params.serviceId, params.hblId, params.jobId);
                }
            }
        );
        this.headers = [
            { title: 'Attach File', field: 'name' }
        ];

        this.formMail = this._fb.group({
            from: [],
            to: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            cc: ['',
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
                this.exportCrystalArrivalNoticeToPdf();
                break;
            case ChargeConstants.AE_CODE: // Air Export
                this.exportCrystalManifestToPdf();
                this.exportCrystalMawbFrameToPdf();
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
                this.exportCrystalSISummaryToPdf();
                this.exportCrystalSIToPdf();
                this.exportCrystalSIDetailContFCL();
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                this.exportCrystalSISummaryToPdf();
                this.exportCrystalSIToPdf();
                this.exportCrystalSIDetailContLCL();
                break;
            default:
                break;
        }
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

    sendMail() {
        this.isSubmited = true;
        if (this.formMail.valid) {
            this.attachFileUpload();
            const emailContent: EmailContent = {
                from: this.from.value,
                to: this.to.value,
                cc: this.cc.value,
                subject: this.subject.value,
                body: this.body.value,
                attachFiles: this.attachedFile
            };

            this._progressRef.start();
            this._documentRepo.sendMailDocument(emailContent)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._progressRef.complete(); })
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
                );
        }
    }

    attachFileUpload() {
        const _attachFileUpload = this.hashedUrlFileUpload();
        _attachFileUpload.forEach(element => {
            const idxOf = this.attachedFile.indexOf(element);
            if (element !== this.pathGeneralArrivalNotice
                && element !== this.pathGeneralManifest
                && element !== this.pathGeneralMawb
                && element !== this.pathGeneralSI
                && idxOf === -1) {
                this.attachedFile.push(element);
            }
        });
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
            ).subscribe(
                (res: any) => {

                },
            );
    }

    //#region Content Mail
    getContentMail(serviceId: string, hblId: string, jobId: string) {
        switch (serviceId) {
            case ChargeConstants.AI_CODE: // Air Import
                this.sendMailButtonName = "Send Arrival Notice";
                this.getInfoMailHBLAirImport(hblId);
                break;
            case ChargeConstants.AE_CODE: // Air Export
                this.sendMailButtonName = "Send Pre Alert";
                this.getInfoMailHBLAirExport(hblId);
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
                this.sendMailButtonName = "Send S.I";
                this.getInfoMailSISeaExport(jobId);
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                this.sendMailButtonName = "Send S.I";
                this.getInfoMailSISeaExport(jobId);
                break;
            default:
                break;
        }
    }

    getInfoMailHBLAirImport(hblId: string) {
        this._progressRef.start();
        this._documentRepo.getInfoMailHBLAirImport(hblId)
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

    getInfoMailHBLAirExport(hblId: string) {
        this._progressRef.start();
        this._documentRepo.getInfoMailHBLAirExport(hblId)
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
        this._documentRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: 'VND' })
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }

    previewManifest() {
        this._progressRef.start();
        this._documentRepo.previewAirExportManifestByJobId(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }

    previewMawb() {
        this._progressRef.start();
        this._documentRepo.previewHouseAirwayBillLastest(this.hblId, 'LASTEST_ITL_FRAME')
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataReport = res;
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }

    // SI Detail HBL
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
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }

    // SI Summary
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
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }

    // SI Detail (Cont) FCL
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
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
    }

    // SI Detail (Cont) LCL
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
                    if (this.dataReport !== null && this.dataReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.reportPopup.frm.nativeElement.submit();
                            this.reportPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data charge to display preview');
                    }
                },
            );
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
    //#endregion Preview Report

    //#region  Export Report
    exportCrystalArrivalNoticeToPdf() {
        this._progressRef.start();
        this._documentRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: 'VND' })
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); }),
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport = res;
                    if (this.dataExportReport !== null && this.dataExportReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.exportReportPopup.frm.nativeElement.submit();
                        }, 1000);

                        this.pathGeneralArrivalNotice = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsArrivalNotice = true;
                        this.isCheckedArrivalNotice = true;
                    } else {
                        this.isExitsArrivalNotice = false;
                        this.isCheckedArrivalNotice = false;
                    }
                },
            );
    }

    exportCrystalManifestToPdf() {
        this._progressRef.start();
        this._documentRepo.previewAirExportManifestByJobId(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport = res;
                    if (this.dataExportReport !== null && this.dataExportReport.dataSource.length > 0) {
                        setTimeout(() => {
                            this.exportReportPopup.frm.nativeElement.submit();
                        }, 1000);

                        this.pathGeneralManifest = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsManifest = true;
                        this.isCheckedManifest = true;
                    } else {
                        this.isExitsManifest = false;
                        this.isCheckedManifest = false;
                    }
                },
            );
    }

    exportCrystalMawbFrameToPdf() {
        this._progressRef.start();
        this._documentRepo.previewHouseAirwayBillLastest(this.hblId, 'LASTEST_ITL_FRAME')
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport2 = res;
                    if (this.dataExportReport2 !== null && this.dataExportReport2.dataSource.length > 0) {
                        setTimeout(() => {
                            this.exportReportPopup2.frm.nativeElement.submit();
                        }, 1000);

                        this.pathGeneralMawb = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsMawb = true;
                        this.isCheckedMawb = true;
                    } else {
                        this.isExitsMawb = false;
                        this.isCheckedMawb = false;
                    }
                },
            );
    }

    exportCrystalSISummaryToPdf() {
        this._progressRef.start();
        this._documentRepo.previewSISummaryByJobId(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport = res;
                    if (this.dataExportReport !== null && this.dataExportReport.dataSource.length > 0) {
                        this.pathGeneralSISummary = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsSISummary = true;
                        this.isCheckedSISummary = true;

                        if (this.exportReportPopup.frm) {
                            this.exportReportPopup.frm.nativeElement.submit();
                        }
                    } else {
                        this.isExitsSISummary = false;
                        this.isCheckedSISummary = false;
                    }
                },
            );
    }

    exportCrystalSIToPdf() {
        this._progressRef.start();
        this._documentRepo.previewSIReportByJobId(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport2 = res;
                    if (this.dataExportReport2 !== null && this.dataExportReport2.dataSource.length > 0) {
                        this.pathGeneralSI = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsSI = true;
                        this.isCheckedSI = true;

                        if (this.exportReportPopup2.frm) {
                            this.exportReportPopup2.frm.nativeElement.submit();
                        }
                    } else {
                        this.isExitsSI = false;
                        this.isCheckedSI = false;
                    }
                },
            );
    }

    exportCrystalSIDetailContFCL() {
        this._progressRef.start();
        this._documentRepo.previewSIContReport(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport3 = res;
                    if (this.dataExportReport3 !== null && this.dataExportReport3.dataSource.length > 0) {
                        this.pathGeneralSIDetailCont = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsSIDetailCont = true;
                        this.isCheckedSIDetailCont = true;

                        if (this.exportReportPopup3.frm) {
                            this.exportReportPopup3.frm.nativeElement.submit();
                        }
                    } else {
                        this.isExitsSIDetailCont = false;
                        this.isCheckedSIDetailCont = false;
                    }
                },
            );
    }

    exportCrystalSIDetailContLCL() {
        this._progressRef.start();
        this._documentRepo.previewSIContLCLReport(this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
            )
            .subscribe(
                (res: Crystal) => {
                    this.dataExportReport3 = res;
                    if (this.dataExportReport3 !== null && this.dataExportReport3.dataSource.length > 0) {
                        this.pathGeneralSIDetailCont = res.pathReportGenerate;
                        this.attachedFile.push(res.pathReportGenerate);
                        this.isExitsSIDetailCont = true;
                        this.isCheckedSIDetailCont = true;

                        if (this.exportReportPopup3.frm) {
                            this.exportReportPopup3.frm.nativeElement.submit();
                        }
                    } else {
                        this.isExitsSIDetailCont = false;
                        this.isCheckedSIDetailCont = false;
                    }
                },
            );
    }
    //#endregion Export Report

    onChangeCheckBox() {
        switch (this.serviceId) {
            case ChargeConstants.AI_CODE: // Air Import               
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralArrivalNotice, this.isCheckedArrivalNotice);
                break;
            case ChargeConstants.AE_CODE: // Air Export               
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralManifest, this.isCheckedManifest);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralMawb, this.isCheckedMawb);
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSISummary, this.isCheckedSISummary);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSI, this.isCheckedSI);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSIDetailCont, this.isCheckedSIDetailCont);
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSISummary, this.isCheckedSISummary);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSI, this.isCheckedSI);
                this.UpdateAttachFileByPathGeneralReport(this.pathGeneralSIDetailCont, this.isCheckedSIDetailCont);
                break;
            default:
                break;
        }
    }

    UpdateAttachFileByPathGeneralReport(pathGeneral: string, isChecked: boolean) {
        const idxOf = this.attachedFile.indexOf(pathGeneral);
        if (!isChecked) {
            if (idxOf > -1) {
                this.attachedFile.splice(idxOf, 1);
            }
        } else {
            if (idxOf === -1) {
                this.attachedFile.push(pathGeneral);
            }
        }
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
