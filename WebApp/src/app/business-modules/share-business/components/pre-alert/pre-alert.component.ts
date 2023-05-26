import { Component, ViewChild, ChangeDetectorRef } from '@angular/core';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { NgProgress } from '@ngx-progressbar/core';
import { Router, Params, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Store } from '@ngrx/store';
import { HttpResponse } from '@angular/common/http';

import { AppForm } from '@app';
import { ExportRepo, SystemFileManageRepo } from '@repositories';
import { DocumentationRepo }from "src/app/shared/repositories";
import { IAppState } from '@store';
import { ChargeConstants } from '@constants';
import { Crystal, EmailContent } from '@models';
import { ReportPreviewComponent, SubHeaderComponent } from '@common';
import { RoutingConstants, SystemConstants } from '@constants';
import { ICrystalReport } from '@interfaces';
import { delayTime } from '@decorators';
import { getTransactionLocked, TransactionGetDetailAction } from '@share-bussiness';


import { combineLatest, of, Observable, throwError } from 'rxjs';
import { catchError, finalize, map, take, mergeMap, takeUntil, retryWhen, concatMap, skip, switchMap } from 'rxjs/operators';

import { ShareBusinessAddAttachmentPopupComponent } from '../add-attachment/add-attachment.popup';
import { environment } from 'src/environments/environment';
import { NgxSpinnerService } from 'ngx-spinner';
import { InjectViewContainerRefDirective } from '@directives';
import lowerCase from 'lodash-es/lowerCase';
import { OPEXConstants } from 'src/constants/OPEX-Constants.const';
@Component({
    selector: 'share-pre-alert',
    templateUrl: './pre-alert.component.html'
})
export class ShareBusinessReAlertComponent extends AppForm implements ICrystalReport {

    @ViewChild(ShareBusinessAddAttachmentPopupComponent) attachmentPopup: ShareBusinessAddAttachmentPopupComponent;
    @ViewChild(ReportPreviewComponent) reportPopup: ReportPreviewComponent;
    @ViewChild(SubHeaderComponent) headerComponent: SubHeaderComponent;
    @ViewChild(InjectViewContainerRefDirective) public reportContainerRef: InjectViewContainerRefDirective;

    srcReportPDF: any = `${environment.HOST.EXPORT_CRYSTAL}`;
    valuePDF: any = null;
    numOfFileExp: number = 1;

    form: FormGroup;
    files: IShipmentAttachFile[] = [];
    jobId: string;
    hblId: string;
    lstHblId: any[] = [];
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
    isSI: boolean = false; // Shipping instruction
    isArrivalNotice: boolean = false; // Arrival Notice
    isDO: boolean = false; // Delivery order
    isAL: boolean = false; // Authorize letter
    isPOD: boolean = false; // Proof of delivery
    isSendHbl: boolean = false; // Send HBL (Sea Services)
    isSendHawb: boolean = false; // Send HAWB (Air Services)
    isDbtInv: boolean = false; // Send Debit Note/Invoice (All Service)
    // Import
    isExitsArrivalNotice: boolean = false;
    isCheckedArrivalNotice: boolean = false;

    isExitsDO: boolean = false;
    isCheckedDO: boolean = false;

    isExitsDebitNote: boolean = false;
    isCheckedDebitNote: boolean = false;
    isDefaultDebitNote: boolean = false;

    isExitsPOD: boolean = false;
    isCheckedPOD: boolean = false;

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
    stageType: string = '';
    cdNoteNo: string = '';
    CdNoteDetail: any = null;
    partnerId: any = null;
    lstStage: any[]= [];
    signImgUrl: string = '';

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
        private _cd: ChangeDetectorRef,
        private _systemfileManageRepo: SystemFileManageRepo,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }

    ngOnInit(): void {
        this.subscription = combineLatest([
            this._activedRouter.params,
            this._activedRouter.data,
            this._activedRouter.queryParams
        ]).pipe(
            map(([params, qParams, namingParams]) => ({ ...params, ...qParams, ...namingParams })),
            take(1),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (params: Params) => {
                if (params.jobId) {
                    this.jobId = params.jobId;
                    this.hblId = !params.hblId || params.hblId === 'undefined' ? SystemConstants.EMPTY_GUID : params.hblId;
                    this.serviceId = params.serviceId;

                    if(params.name === "Send_Debit_Invoice"){
                        this.cdNoteNo = params.cdNoteNo;
                        this.name = "Send Debit Invoice";
                        this.partnerId = params.partnerId;
                        this.getDetailCdNote(this.jobId, this.cdNoteNo);
                    }
                    else{
                        this.name = params.name;
                    }
                    this.checkReportType();

                    this.hblRptName = (this.serviceId === ChargeConstants.SFE_CODE || this.serviceId === ChargeConstants.SLE_CODE || this.serviceId === ChargeConstants.SCE_CODE) ? "HBL" : "HAWB";

                    this.getDetailHAWB();
                    // this.getContentMail(this.serviceId, this.hblId, this.jobId);

                    // Is Locked Shipment?
                    this._store.dispatch(new TransactionGetDetailAction(this.jobId));
                    this.isLocked = this._store.select(getTransactionLocked);
                }
            }
        );

        this.initForm();
    }

    checkReportType() {
        switch (this.name) {
            case "Pre Alert":
                this.isPreAlert = true;
                break;
            case "Arrival Notice":
                this.isArrivalNotice = true;
                break;
            case "Delivery Order":
                this.isDO = true;
                break;
            case "Authorize Letter":
                this.isAL = true;
                break;
            case "Proof Of Delivery":
                this.isPOD = true;
                break;
            case "Send HBL":
                this.isSendHbl = true;
                break;
            case "Send HAWB":
                this.isSendHbl = true;
                this.isSendHawb = true;
                break;
            case "Send S.I":
                this.isSI = true;
                break;
            case "Send Debit Invoice":
                this.isDbtInv = true;
                break;
        }
    }

    initForm() {
        this.formMail = this._fb.group({
            // from: ['',
            //     Validators.compose([
            //         Validators.required,
            //         Validators.pattern(SystemConstants.CPATTERN.EMAIL_SINGLE)
            //     ])
            // ],
            from: [],
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
                if (this.isArrivalNotice) {
                    this.isExitsArrivalNotice = true;
                    this.isCheckedArrivalNotice = true;

                    if (this.hawbDetails[0].deliveryOrderNo) {
                        this.isCheckedDO = true;
                        this.isExitsDO = true;
                    }
                }
                if (this.isAL) {
                    this.isCheckedDO = true;
                    this.isExitsDO = true;
                }
                if (this.isPOD) {
                    this.isCheckedDO = true;
                    this.isExitsDO = true;
                }
                else if (this.isDbtInv) {
                    this.checkExistDebitNoteSendInv();
                }
                break;
            case ChargeConstants.AE_CODE: // Air Export
                if (this.isPreAlert) {
                    this.checkExistManifestExport();
                }
                else if(this.isDbtInv){
                    this.checkExistDebitNoteSendInv();
                }
                break;
            case ChargeConstants.SFI_CODE: // Sea FCL Import
            case ChargeConstants.SLI_CODE: // Sea LCL Import
            case ChargeConstants.SCI_CODE: // Sea Consol Import
                if (this.isArrivalNotice) {
                    this.isExitsArrivalNotice = true;
                    this.isCheckedArrivalNotice = true;
                    if (this.hawbDetails[0].deliveryOrderNo) {
                        this.isCheckedDO = true;
                        this.isExitsDO = true;
                    }
                }
                if (this.isDO) {
                    if (this.hawbDetails[0].deliveryOrderNo) {
                        this.isCheckedDO = true;
                        this.isExitsDO = true;
                    }
                }
                if (this.isPOD) {
                    this.isCheckedDO = true;
                    this.isExitsDO = true;
                }
                if (this.isDbtInv) {
                    this.checkExistDebitNoteSendInv();
                }
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
            case ChargeConstants.SCE_CODE: // Sea Consol Export
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.isPreAlert) {
                    this.checkExistManifestExport();
                } else if (this.isSI) {
                    this.checkExistSIExport();
                } else if (this.isDbtInv) {
                    this.checkExistDebitNoteSendInv();
                }
                break;
            default:
                break;
        }
        if (!this.isDbtInv) {
            this.checkExistDebitNote();
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

    checkExistDebitNote() {
        this._documentRepo.getListCDNoteWithHbl(this.hblId, this.jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res) {
                        if (this.serviceId === 'AE' || this.serviceId === 'SFE' || this.serviceId === 'SLE' || this.serviceId === 'SCE') {
                            this.debitNos = res.map(v => ({ ...v, isCheckedDebitNote: false }));
                        } else {
                            this.debitNos = res.filter(x => lowerCase(x.type) !== 'credit').map(v => ({ ...v, isCheckedDebitNote: false }));
                        }
                    }
                    if (this.debitNos.length > 0) {
                        this.isExitsDebitNote = true;
                        this.isCheckedDebitNote = true;
                    } else {
                        this.isExitsDebitNote = false;
                    }
                });
    }

    checkExistDebitNoteSendInv() {
        this._documentRepo.getListCDNoteWithPartnerId({ jobId: this.jobId, partnerId: this.partnerId })
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    if (res) {
                        // this.debitNos = res.map(v => ({ ...v, isCheckedDebitNote: true }));
                        this.debitNos = res.filter(x => lowerCase(x.type) !== 'credit').map(v => (v.code === this.cdNoteNo) ? { ...v, isCheckedDebitNote: true, isDefaultDebitNote: true } : { ...v, isCheckedDebitNote: false });
                    }
                    if (this.debitNos.length > 0) {
                        this.isExitsDebitNote = true;
                        this.isCheckedDebitNote = true;
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
            this._systemfileManageRepo.uploadFile('Document', 'Shipment', this.jobId, filesToUpdate)
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
                if (this.isArrivalNotice) {
                    if (this.isExitsArrivalNotice && this.isCheckedArrivalNotice) {
                        streamUploadReport.push(this._documentRepo.previewArrivalNoticeAir({ hblId: this.hblId, currency: 'VND' }));
                    }
                    if (this.isExitsDO && this.isCheckedDO) {
                        streamUploadReport.push(this._documentRepo.previewAirImportAuthorizeLetter1(this.hblId, false));
                    }
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }))
                        }
                    });
                }
                if (this.isAL) {
                    if (this.isExitsDO && this.isCheckedDO)
                        streamUploadReport.push(this._documentRepo.previewAirImportAuthorizeLetter1(this.hblId, false));
                }
                if (this.isPOD) {
                    if (this.isExitsDO && this.isCheckedDO) {
                        streamUploadReport.push(this._documentRepo.previewProofofDelivery(this.hawbDetails[0].id));
                    }
                }
                if (this.isDbtInv) {
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }));
                        }
                    });
                }
                break;
            case ChargeConstants.AE_CODE:
                if (this.isPreAlert) {
                    this.hawbDetails.forEach(ele => {
                        if (ele.isCheckedHawb) {
                            streamUploadReport.push(this._documentRepo.previewHouseAirwayBillLastest(ele.id, 'LASTEST_ITL_FRAME'));
                        }
                    });
                    if (this.isCheckedManifest) {
                        streamUploadReport.push(this._documentRepo.previewAirExportManifestByJobId(this.jobId));
                    }
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'ORIGIN' }))
                        }
                    });
                }
                if (this.isSendHbl) {
                    this.hawbDetails.forEach(ele => {
                        if (ele.isCheckedHawb) {
                            streamUploadReport.push(this._documentRepo.previewHouseAirwayBillLastest(ele.id, 'LASTEST_ITL_FRAME'));
                        }
                    });
                }
                if (this.isDbtInv) {
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }));
                        }
                    });
                }
                break;
            case ChargeConstants.SFI_CODE:
            case ChargeConstants.SLI_CODE:
            case ChargeConstants.SCI_CODE:
                if (this.isArrivalNotice) {
                    if (this.isExitsArrivalNotice && this.isCheckedArrivalNotice) {
                        streamUploadReport.push(this._documentRepo.previewArrivalNotice({ hblId: this.hblId, currency: 'VND' }))
                    }
                    if (this.isExitsDO && this.isCheckedDO) {
                        streamUploadReport.push(this._documentRepo.previewDeliveryOrder(this.hawbDetails[0].id));
                    }
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }));
                        }
                    });
                }
                if (this.isDO) {
                    if (this.isCheckedDO && this.isExitsDO) {
                        streamUploadReport.push(this._documentRepo.previewDeliveryOrder(this.hawbDetails[0].id));
                    }
                }
                if (this.isPOD) {
                    if (this.isExitsDO && this.isCheckedDO) {
                        streamUploadReport.push(this._documentRepo.previewProofofDelivery(this.hawbDetails[0].id));
                    }
                }
                if (this.isDbtInv) {
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }));
                        }
                    });
                }
                break;
            case ChargeConstants.SFE_CODE:
            case ChargeConstants.SCE_CODE:
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
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'ORIGIN' }))
                        }
                    });
                } else if (this.isSI) {
                    if (this.isCheckedSISummary) {
                        streamUploadReport.push(this._documentRepo.previewSISummaryByJobId(this.jobId));
                    }
                    if (this.isCheckedSI) {
                        streamUploadReport.push(this._documentRepo.previewSIReportByJobId(this.jobId));
                    }
                    if (this.isCheckedSIDetailCont) {
                        streamUploadReport.push(this._documentRepo.previewSIContReport(this.jobId));
                    }
                } else if (this.isSendHbl) {
                    this.hawbDetails.forEach(ele => {
                        if (ele.isCheckedHawb) {
                            streamUploadReport.push(this._documentRepo.previewSeaHBLOfLanding(ele.id, 'ITL_FRAME'));
                        }
                    });
                } else if (this.isDbtInv) {
                    this.debitNos.forEach(element => {
                        if (element.isCheckedDebitNote) {
                            streamUploadReport.push(this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: element.code, currency: 'VND' }));
                        }
                    });
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

    addSignImgToBody() {
        if (!!this.body.value) {
            return "<span><img src='" + this.signImgUrl + "' width='459'/></span>";
        }
    }

    sendMail() {
        this.lstHblId = [];
        if (this.isArrivalNotice || this.isDbtInv) {
            this.debitNos.forEach(el => {
                if (!!el.isCheckedDebitNote) {
                    this.getDetailCdNote(this.jobId, el.code)
                }
            })
        }
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

        if (this.isArrivalNotice) {
            emailContent.body += this.addSignImgToBody();
        }

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
                        if (!!this.isDbtInv) {
                            this.assignStageByEventType(this.jobId, this.hblId);
                        } else {
                            this.assignStageByEventType(this.jobId, this.hblId);
                        }
                        this._toastService.success(res.message);
                        //this.deleteFileTemp(this.jobId);
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
        var shipmentFileUrls: string[] = (this.files || []).map(x => x.url);
        shipmentFileUrls = shipmentFileUrls.filter(element => this.attachmentPopup.getFileShipmentUrls().map(element => element.url).includes(element));
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

    // deleteFileTemp(jobId: string) {
    //     this._systemfileManageRepo.deleteFolder('Document', 'Shipment', jobId)
    //         .pipe(
    //             catchError(this.catchError),
    //             finalize(() => { }),
    //         ).subscribe();
    // }

    //#region Content Mail

    getContentMail(serviceId: string, hblId: string, jobId: string) {
        switch (serviceId) {
            case ChargeConstants.AI_CODE: // Air Import
                if (this.isAL) {
                    this.sendMailButtonName = "Send Authorize Letter";
                    this.getInfoMailHBLAirImport(hblId);
                } else if (this.isPOD) {
                    this.sendMailButtonName = "Send Proof Of Delivery";
                    this.getInfoMailPODHBLAirService(hblId);
                } else if (this.isSendHawb) {
                    this.sendMailButtonName = "Send HAWB";
                    this.getInfoMailHawbHBLAirService(hblId);
                } else if (this.isDbtInv) {
                    this.sendMailButtonName = "Send";
                    this.getInfoMailDebitInv(this.cdNoteNo);;
                }
                else {
                    this.sendMailButtonName = "Send Arrival Notice";
                    this.getInfoMailHBLAirImport(hblId);
                }
                break;
            case ChargeConstants.SFI_CODE: // Sea Import
            case ChargeConstants.SLI_CODE:
            case ChargeConstants.SCI_CODE:
                if (this.isDO) {
                    this.sendMailButtonName = "Send Delivery Order";
                    this.getInfoMailHBLSeaImport(hblId, serviceId);
                } else if (this.isPOD) {
                    this.sendMailButtonName = "Send Proof Of Delivery";
                    this.getMailProofOfDeliveryHBLSea(hblId, serviceId);
                } else if (this.isSendHbl) {
                    this.sendMailButtonName = "Send HBL";
                    this.getMailSendHBLSeaServices(hblId, serviceId);
                } else if (this.isArrivalNotice) {
                    this.sendMailButtonName = "Send Arrival Notice";
                    this.getInfoMailHBLSeaImport(hblId, serviceId);
                } else if (this.isDbtInv) {
                    this.sendMailButtonName = "Send";
                    this.getInfoMailDebitInv(this.cdNoteNo);;
                }
                break;
            case ChargeConstants.AE_CODE: // Air Export
                if (this.isPOD) {
                    this.sendMailButtonName = "Send Proof Of Delivery";
                    this.getInfoMailPODHBLAirService(hblId);
                } else if (this.isSendHawb) {
                    this.sendMailButtonName = "Send HAWB";
                    this.getInfoMailHawbHBLAirService(hblId);
                } else if (this.isDbtInv) {
                    this.sendMailButtonName = "Send";
                    this.getInfoMailDebitInv(this.cdNoteNo);;
                }
                else {
                    this.sendMailButtonName = "Send Pre Alert";
                    this.getInfoMailHBLAirExport(this.hawbDetails.map(x => x.id), jobId);
                }
                break;
            case ChargeConstants.SFE_CODE: // Sea FCL Export
            case ChargeConstants.SCE_CODE:
                if (this.isPreAlert) {
                    this.sendMailButtonName = "Send Pre Alert";
                    this.getInfoMailHBLPreAlertSeaExport(this.hawbDetails.map(x => x.id), jobId, serviceId);
                } else if (this.isPOD) {
                    this.sendMailButtonName = "Send Proof Of Delivery";
                    this.getMailProofOfDeliveryHBLSea(hblId, serviceId);
                } else if (this.isSendHbl) {
                    this.sendMailButtonName = "Send HBL";
                    this.getMailSendHBLSeaServices(hblId, serviceId);
                } else if (this.isSI) {
                    this.sendMailButtonName = "Send S.I";
                    this.getInfoMailSISeaExport(jobId);
                } else if (this.isDbtInv) {
                    this.sendMailButtonName = "Send";
                    this.getInfoMailDebitInv(this.cdNoteNo);;
                }
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.isPreAlert) {
                    this.sendMailButtonName = "Send Pre Alert";
                    this.getInfoMailHBLPreAlertSeaExport(this.hawbDetails.map(x => x.id), jobId, serviceId);
                } else if (this.isPOD) {
                    this.sendMailButtonName = "Send Proof Of Delivery";
                    this.getMailProofOfDeliveryHBLSea(hblId, serviceId);
                } else if (this.isSendHbl) {
                    this.sendMailButtonName = "Send HBL";
                    this.getMailSendHBLSeaServices(hblId, serviceId);
                } else if (this.isSI) {
                    this.sendMailButtonName = "Send S.I";
                    this.getInfoMailSISeaExport(jobId);
                } else if (this.isDbtInv) {
                    this.sendMailButtonName = "Send";
                    this.getInfoMailDebitInv(this.cdNoteNo);;
                }
                break;
            default:
                break;
        }
        if (this.isDbtInv) {
            this.headerComponent.resetBreadcrumb("Send Debit Invoice");
        }
    }

    getInfoMailHBLAirImport(hblId: string) {
        if (this.isAL) {
            this._documentRepo.getMailAuthorizeLetterHBLAirImport(hblId)
                .subscribe(
                    (res: EmailContent) => {
                        this.formMail.patchValue(res);
                    },
                );
        } else {
            this._documentRepo.getInfoMailHBLAirImport(hblId)
                .subscribe(
                    (res: EmailContent) => {
                        this.formMail.patchValue(res);
                        this.signImgUrl = res.attachFiles[0];
                        res.attachFiles = [];
                    },
                );
        }
    }

    getInfoMailHBLSeaImport(hblId: string, serviceId: string) {
        if (this.isDO) {
            this._documentRepo.getMailDOHBLSeaImport(hblId, serviceId)
                .subscribe(
                    (res: EmailContent) => {
                        this.formMail.patchValue(res);
                    },
                );
        } else {
            this._documentRepo.getInfoMailHBLSeaImport(hblId, serviceId)
                .subscribe(
                    (res: EmailContent) => {
                        this.formMail.patchValue(res);
                        this.signImgUrl = res.attachFiles[0];
                        res.attachFiles = [];
                    },
                );
        }
    }

    getMailProofOfDeliveryHBLSea(hblId: string, serviceId: string) {
        this._documentRepo.getMailProofOfDeliveryHBLSea(hblId, serviceId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getMailSendHBLSeaServices(hblId: string, serviceId: string) {
        this._documentRepo.getMailSendHBLSeaServices(hblId, serviceId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailHBLAirExport(hblIds: any[], jobId: string) {
        this._documentRepo.getInfoMailHBLAirExport(hblIds, jobId, this.hblId === SystemConstants.EMPTY_GUID)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailPODHBLAirService(hblId: string) {
        this._documentRepo.getMailProofOfDeliveryHBLAir(hblId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailHawbHBLAirService(hblId: string) {
        this._documentRepo.getMailSendHawbHBLAir(hblId)
            .subscribe(
                (res: EmailContent) => {
                    this.formMail.patchValue(res);
                },
            );
    }

    getInfoMailHBLPreAlertSeaExport(hblIds: any, jobId: string, serviceId: string) {
        this._documentRepo.getInfoMailHBLPreAlertSeaExport(hblIds, jobId, serviceId, this.hblId === SystemConstants.EMPTY_GUID)
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

    getInfoMailDebitInv(cdNoteNo:string) {
        this._progressRef.start();
        this._documentRepo.getInfoMailDebitInv(cdNoteNo)
            .pipe(
                catchError(this.catchError),
            finalize(() => {this._progressRef.complete(); })
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
            )
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
                                this.renderAndShowReport('AN');
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        },
                    );
                break;
            case ChargeConstants.SFI_CODE:
            case ChargeConstants.SLI_CODE:
            case ChargeConstants.SCI_CODE:
                this._documentRepo.previewArrivalNotice({ hblId: this.hblId, currency: 'VND' })
                    .pipe(
                        catchError(this.catchError),
                        finalize(() => { this._progressRef.complete(); })
                    )
                    .subscribe(
                        (res: Crystal) => {
                            if (res?.dataSource?.length > 0) {
                                this.dataReport = res;
                                this.renderAndShowReport('AN');
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        },
                    );
                break;
        }
    }

    previewManifest() {
        if (this.serviceId === ChargeConstants.SFE_CODE || this.serviceId === ChargeConstants.SLE_CODE || this.serviceId === ChargeConstants.SCE_CODE) {
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
                            this.renderAndShowReport('MNF');
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
                            this.renderAndShowReport('MNF');
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    },
                );
        }
    }

    previewHawb(hblId: string) {
        if (this.serviceId === ChargeConstants.SFE_CODE || this.serviceId === ChargeConstants.SLE_CODE || this.serviceId === ChargeConstants.SCE_CODE) {
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
                            this.renderAndShowReport('HBL');
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
                        this.renderAndShowReport('HBL');
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
                        this.renderAndShowReport('SI');
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
                        this.renderAndShowReport('SI');
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
                        this.renderAndShowReport('SI');
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
                        this.renderAndShowReport('SI');
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
                            this.renderAndShowReport('DO');
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }

                },
            );
        } else {
            this._documentRepo.previewDeliveryOrder(this.hawbDetails[0].id)
                .pipe(
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        this.dataReport = res;
                        if (this.dataReport.dataSource?.length > 0) {
                            this.renderAndShowReport('DO');
                        } else {
                            this._toastService.warning('There is no container data to display preview');
                        }
                    }
                },
            );
        }
    }

    previewDebitNote(debit: any) {
        if (this.serviceId === 'AI' || this.serviceId === 'AE') {
            this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: debit?.code, currency: 'VND' })
                .pipe(
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport(debit?.type);
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
        } else {
            this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: debit?.code, currency: 'VND' })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res != null) {
                            if (res?.dataSource?.length > 0) {
                                this.dataReport = res;
                                this.renderAndShowReport('INV');
                            } else {
                                this._toastService.warning('There is no data to display preview');
                            }
                        }
                    },
                );
        }
    }

    previewCDNoteExport(debit: any) {
        if (this.serviceId === 'AE') {
            this._documentRepo.previewAirCdNote({ jobId: this.jobId, creditDebitNo: debit?.code, currency: 'ORIGIN' })
                .pipe(
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport(debit?.type);
                        } else {
                            this._toastService.warning('There is no data to display preview');
                        }
                    }
                },
            );
        } else {
            this._documentRepo.previewSIFCdNote({ jobId: this.jobId, creditDebitNo: debit?.code, currency: 'ORIGIN' })
                .pipe(
            ).subscribe(
                (res: any) => {
                    if (res !== false) {
                        if (res?.dataSource?.length > 0) {
                            this.dataReport = res;
                            this.renderAndShowReport(debit?.type);
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
            case ChargeConstants.SCE_CODE:
                this.previewSIDetailContFCL();
                break;
            case ChargeConstants.SLE_CODE:
                this.previewSIDetailContLCL();
                break;
        }
    }

    previewProofofDelivery() {
        this._documentRepo.previewProofofDelivery(this.hblId)
            .pipe(
        ).subscribe(
            (res: any) => {
                if (res !== false) {
                    if (res?.dataSource?.length > 0) {
                        this.dataReport = res;
                        this.renderAndShowReport('POD');
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                }
            },
        );
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
            case ChargeConstants.SCE_CODE: // Sea Consol Export
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
            case ChargeConstants.SCI_CODE:
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
                if (this.hblId === SystemConstants.EMPTY_GUID && this.isDbtInv) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.AIR_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                }
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
                    if (this.isSI) {
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_EXPORT}/${this.jobId}/si`]);
                    } else {
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_EXPORT}/${this.jobId}/hbl/${this.hblId}`]);
                    }
                }
                break;
            case ChargeConstants.SCE_CODE: // Sea FCL Export
                if (this.hblId === SystemConstants.EMPTY_GUID) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_EXPORT}/${this.jobId}`]);
                } else {
                    if (this.isSI) {
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_EXPORT}/${this.jobId}/si`]);
                    } else {
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_EXPORT}/${this.jobId}/hbl/${this.hblId}`]);
                    }
                }
                break;
            case ChargeConstants.SLE_CODE: // Sea LCL Export
                if (this.hblId === SystemConstants.EMPTY_GUID) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}`]);
                } else {
                    if (this.isSI) {
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/si`]);
                    } else {
                        this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_EXPORT}/${this.jobId}/hbl/${this.hblId}`]);
                    }
                }
                break;
            case ChargeConstants.SFI_CODE: // Sea FCL Import
                if (this.hblId === SystemConstants.EMPTY_GUID && this.isDbtInv) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_FCL_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                }
                break;
            case ChargeConstants.SLI_CODE: // Sea LCL Import
                if (this.hblId === SystemConstants.EMPTY_GUID && this.isDbtInv) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_LCL_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                }
                break;
            case ChargeConstants.SCI_CODE:
                if (this.hblId === SystemConstants.EMPTY_GUID && this.isDbtInv) {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_IMPORT}/${this.jobId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.DOCUMENTATION.SEA_CONSOL_IMPORT}/${this.jobId}/hbl/${this.hblId}`]);
                }
                break;
            default:
                break;
        }
    }

    getDetailHAWB() {
        this._documentRepo.getHAWBListOfShipment(this.jobId, this.hblId)
            .subscribe(
                (res: any[]) => {
                    // if (this.hblId === SystemConstants.EMPTY_GUID) {
                    //     this.hawbDetails = [...res.map(v => ({ ...v, isCheckedHawb: true }))];
                    // } else {
                    //     this.hawbDetails = res.filter(x => x.id === this.hblId).map(v => ({ ...v, isCheckedHawb: true }));
                    // }      
                    if (res.length > 0) {
                        var invalidHawb = res.filter(x => !!x.errorMessage && !!x.errorMessage.length);
                        if (this.hblId === SystemConstants.EMPTY_GUID && res.length === invalidHawb.length) {
                            this._toastService.warning(invalidHawb[0].errorMessage);
                            this.cancelPreAlert();
                        } else {
                            this.hawbDetails = res.filter(x => !x.errorMessage || !x.errorMessage.length).map(v => ({ ...v.hbl, isCheckedHawb: true }));
                        }
                    }
                    this.exportFileCrystalToPdf(this.serviceId);
                    this.getContentMail(this.serviceId, this.hblId, this.jobId);
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

    renderAndShowReport(templateCode?: string) {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.reportContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.reportContainerRef.viewContainerRef.clear();
            });

        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._export.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.jobId,
                            hblId: SystemConstants.EMPTY_GUID,
                            templateCode: templateCode || 'OTH',
                            transactionType: this.serviceId
                        };
                        return this._systemfileManageRepo.uploadPreviewTemplateEdoc([body]);
                    }
                    return of(false);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (!res) return;
                    if (res.status) {
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.success(res.message || "Upload fail");
                    }
                },
                (errors) => {
                    console.log("error", errors);
                },
                () => {
                    sub.unsubscribe();
                }
            );
    }

    assignStageByEventType(jobId: string, hblId: string) {
        if (this.isPOD) {
            this.stageType = "SEND_POD"
        }
        if (this.isAL) {
            this.stageType = "SEND_AL"
        }
        if (this.isArrivalNotice) {
            this.stageType = "SEND_AN"
        }
        if (this.isDO) {
            this.stageType = "SEND_DO"
        }
        if (this.isPreAlert) {
            this.stageType = "SEND_PA"
        }
        if (this.isSendHbl || this.isSendHawb) {
            this.stageType = "SEND_HB"
        }
        if (this.isDbtInv) {
            this.stageType = "SEND_INV";
            const body = this.assignMultipleStageSendInv(this.lstHblId);
            this._documentRepo.addMultipleStageToJob(this.jobId, body)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe();
        }
        if (this.stageType.length !== 0 && !this.isDbtInv) {
            if (this.attachedFile.length > 0 && this.stageType == "SEND_AN") {
                let stream: Observable<any> = null;
                if (this.lstHblId.length > 0) {
                    stream = this._documentRepo.assignStageByEventType({ stageType: this.stageType, jobId, hblId: this.hblId })
                        .pipe(
                            switchMap(() => {
                                const body = this.assignMultipleStageSendInv(this.lstHblId);
                                return this._documentRepo.addMultipleStageToJob(this.jobId, body);
                            }))
                } else {
                    stream = this._documentRepo.assignStageByEventType({ stageType: this.stageType, jobId, hblId: this.hblId })
                }
                if (!stream) return;
                stream.subscribe();
            }
            else {
                if (this.stageType.length !== 0 && !this.isDbtInv) {
                    this._documentRepo.assignStageByEventType({ stageType: this.stageType, jobId, hblId })
                        .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                        .subscribe();
                }
            }
        }
    }

    getDetailCdNote(jobId: string, cdNote: string) {
        this._documentRepo.getDetailsCDNote(jobId, cdNote)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (dataCdNote: any) => {
                    this.CdNoteDetail = dataCdNote;
                    this.lstHblId = [...new Set(this.lstHblId.concat(this.CdNoteDetail.listSurcharges.map(x => x.hblid)))];
                    // this.lstHblId = this.lstHblId.concat(this.CdNoteDetail.listSurcharges.map(x => x.hblid));
                },
            );

    }

    assignMultipleStageSendInv(lstHblId) {
        this.isSubmitted = true;
        const stageCode = OPEXConstants.S_INV_CODE;
        let Obj_body = lstHblId.map((value) => ({ hblId: value, code: stageCode, jobId: this.jobId, type: "System" }));
        return Obj_body
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
