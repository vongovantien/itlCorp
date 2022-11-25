import { HttpResponse } from "@angular/common/http";
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ViewChild } from "@angular/core";
import { AbstractControl, FormBuilder, FormGroup } from "@angular/forms";
import { ReportPreviewComponent } from "@common";
import { SystemConstants } from "@constants";
import { delayTime } from "@decorators";
import { InjectViewContainerRefDirective } from "@directives";
import { ICrystalReport } from "@interfaces";
import { Crystal, CsTransactionDetail } from "@models";
import { DocumentationRepo, ExportRepo, SystemFileManageRepo } from "@repositories";
import { ToastrService } from "ngx-toastr";
import { of } from "rxjs";
import { catchError, concatMap, finalize, mergeMap, switchMap, takeUntil } from "rxjs/operators";
import { PopupBase } from "src/app/popup.base";

@Component({
    selector: 'input-booking-note-popup',
    templateUrl: './input-booking-note.popup.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class InputBookingNotePopupComponent extends PopupBase implements ICrystalReport {
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    formInputBN: FormGroup;

    flexId: AbstractControl;
    flightNo2: AbstractControl;
    contactPerson: AbstractControl;
    closingTime: AbstractControl;

    reportType: string = '';
    hblId: string = '';
    hblDetail: CsTransactionDetail;

    constructor(
        private _documentationRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _fb: FormBuilder,
        private _cd: ChangeDetectorRef,
        private _exportRepo: ExportRepo,
        private _fileMngtRepo: SystemFileManageRepo
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
    }

    initForm() {
        this.formInputBN = this._fb.group({
            flexId: [],
            flightNo2: [],
            contactPerson: [],
            closingTime: []
        });

        this.flexId = this.formInputBN.controls['flexId'];
        this.flightNo2 = this.formInputBN.controls['flightNo2'];
        this.contactPerson = this.formInputBN.controls['contactPerson'];
        this.closingTime = this.formInputBN.controls['closingTime'];
    }

    previewBookingNote() {
        const body: IBookingNoteCriteria = {
            reportType: this.reportType,
            hblId: this.hblId,
            flexId: this.flexId.value,
            flightNo2: this.flightNo2.value,
            contactPerson: this.contactPerson.value,
            closingTime: this.closingTime.value
        };
        this._documentationRepo.validateCheckPointContractPartner({
            partnerId: this.hblDetail.customerId,
            hblId: this.hblId,
            transactionType: 'DOC',
            type: 7
        }).pipe(
            switchMap((res: CommonInterface.IResult) => {
                if (res.status) {
                    return this._documentationRepo.previewBookingNote(body);
                } else {
                    this._toastService.warning(res.message);
                    return of(false);
                }
            }),
            concatMap((data: any) => {
                if (!!data) {
                    if (data !== null && data.dataSource.length > 0) {
                        this.dataReport = data;
                        this.renderAndShowReport();
                        return this._documentationRepo.updateInputBookingNoteAirExport(body);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                }
            })
        )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.hblDetail.flexId = body.flexId;
                        this.hblDetail.flightNoRowTwo = body.flightNo2;
                        this.hblDetail.contactPerson = body.contactPerson;
                        this.hblDetail.closingTime = body.closingTime;
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    updateInputBookingNote(body: IBookingNoteCriteria) {
        this._documentationRepo.updateInputBookingNoteAirExport(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.hblDetail.flexId = body.flexId;
                        this.hblDetail.flightNoRowTwo = body.flightNo2;
                        this.hblDetail.contactPerson = body.contactPerson;
                        this.hblDetail.closingTime = body.closingTime;
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    bindingFormBN(housebill: CsTransactionDetail) {
        this.hblDetail = housebill;
        this.formInputBN.setValue({
            flexId: this.hblDetail.flexId,
            flightNo2: this.hblDetail.flightNoRowTwo,
            contactPerson: this.hblDetail.contactPerson,
            closingTime: this.hblDetail.closingTime
        });
    }

    closePopup() {
        // this.formInputBN.reset();
        this.hide();
    }

    @delayTime(1000)
    showReport(): void {
        this.componentRef.instance.frm.nativeElement.submit();
        this.componentRef.instance.ShowWithDelay();
        this._cd.detectChanges();
    }

    renderAndShowReport() {
        // * Render dynamic
        this.componentRef = this.renderDynamicComponent(ReportPreviewComponent, this.viewContainerRef.viewContainerRef);
        (this.componentRef.instance as ReportPreviewComponent).data = this.dataReport;

        this.showReport();

        this.subscription = ((this.componentRef.instance) as ReportPreviewComponent).$invisible.subscribe(
            (v: any) => {
                this.subscription.unsubscribe();
                this.viewContainerRef.viewContainerRef.clear();
            });

        let sub = ((this.componentRef.instance) as ReportPreviewComponent).onConfirmEdoc
            .pipe(
                concatMap(() => this._exportRepo.exportCrystalReportPDF(this.dataReport, 'response', 'text')),
                mergeMap((res: any) => {
                    if ((res as HttpResponse<any>).status == SystemConstants.HTTP_CODE.OK) {
                        const body = {
                            url: (this.dataReport as Crystal).pathReportGenerate || null,
                            module: 'Document',
                            folder: 'Shipment',
                            objectId: this.hblDetail.jobId,
                            hblId: this.hblId,
                            templateCode: 'OTH',
                            transactionType: 'AE'
                        };
                        return this._fileMngtRepo.uploadPreviewTemplateEdoc([body]);
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
}

interface IBookingNoteCriteria {
    reportType: string;
    hblId: string;
    flexId: string;
    flightNo2: string;
    contactPerson: string;
    closingTime: string;
}
