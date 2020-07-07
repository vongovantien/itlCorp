import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { catchError, finalize, takeUntil, take } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import * as fromShareBussiness from '../../../../share-business/store';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { CsShippingInstruction } from 'src/app/shared/models/document/shippingInstruction.model';
import { ToastrService } from 'ngx-toastr';
import * as fromShare from '../../../../share-business/store';
import { ActivatedRoute } from '@angular/router';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CsTransaction } from 'src/app/shared/models';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { getTransactionPermission, getTransactionLocked } from '../../../../share-business/store';
import { ShareBussinessBillInstructionSeaExportComponent, ShareBussinessBillInstructionHousebillsSeaExportComponent } from '@share-bussiness';
import _groupBy from 'lodash/groupBy';
@Component({
    selector: 'app-sea-lcl-export-shipping-instruction',
    templateUrl: './sea-lcl-export-shipping-instruction.component.html'
})
export class SeaLclExportShippingInstructionComponent extends AppList {
    @ViewChild(ShareBussinessBillInstructionSeaExportComponent, { static: false }) billSIComponent: ShareBussinessBillInstructionSeaExportComponent;
    @ViewChild(ShareBussinessBillInstructionHousebillsSeaExportComponent, { static: false }) billDetail: ShareBussinessBillInstructionHousebillsSeaExportComponent;
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    jobId: string;
    termTypes: CommonInterface.INg2Select[];
    houseBills: any[] = [];
    dataReport: any = null;

    constructor(private _store: Store<fromShareBussiness.TransactionActions>,
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService,
        private _activedRouter: ActivatedRoute,
        private _dataService: DataService) {
        super();
    }

    ngOnInit() {
        this.getTerms();
        this._activedRouter.params.subscribe((param: any) => {
            if (!!param && param.jobId) {
                this.jobId = param.jobId;
                this._store.dispatch(new fromShareBussiness.TransactionGetDetailAction(this.jobId));
                this.getHouseBills();
            }
        });

        this.permissionShipments = this._store.select(getTransactionPermission);
        this.isLocked = this._store.select(getTransactionLocked);
    }
    getHouseBills() {
        this.isLoading = true;
        this._documentRepo.getListHouseBillOfJob({ jobId: this.jobId }).pipe(
            catchError(this.catchError),
            finalize(() => { this.isLoading = false; }),
        ).subscribe(
            (res: any) => {
                this.houseBills = res;
                this.billDetail.housebills = res;

                this.getBillingInstruction(this.jobId);
            },
        );
    }
    async getTerms() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA)) {
            const commonData = this._dataService.getDataByKey(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA);
            this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');

        } else {
            const commonData: { [key: string]: CommonInterface.IValueDisplay[] } = await this._documentRepo.getShipmentDataCommon().toPromise();
            this._dataService.setDataService(SystemConstants.CSTORAGE.SHIPMENT_COMMON_DATA, commonData);
            this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');
        }
    }
    getBillingInstruction(jobId: string) {
        this._documentRepo.getShippingInstruction(jobId)
            .pipe(
                catchError(this.catchError),
                finalize(() => { })
            )
            .subscribe(
                (res: any) => {
                    this.setDataBillInstructionComponent(res);
                },
            );
    }
    setDataBillInstructionComponent(data: any) {
        console.log("data set: ", data);
        this._store.select(fromShare.getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), take(1))
            .subscribe(
                (res) => {
                    if (!!res) {
                        if (data != null) {
                            this.billSIComponent.shippingInstruction = data;
                            this.billSIComponent.shippingInstruction.refNo = res.jobNo;
                        } else {
                            this.initNewShippingInstruction(res);
                            this.getGoods();
                        }

                        this.billSIComponent.shippingInstruction.csTransactionDetails = this.houseBills;
                        this.billSIComponent.termTypes = this.termTypes;
                        this.billSIComponent.setformValue(this.billSIComponent.shippingInstruction);
                    }
                }
            );
    }
    getGoods() {
        if (this.houseBills != null) {
            const lstPackages = [];
            let containerNotes = '';
            let gw = 0;
            let volumn = 0;
            this.houseBills.forEach(x => {
                gw += x.gw;
                volumn += x.cbm;
                containerNotes += !!x.packageContainer ? (x.packageContainer + '\n') : '';
                if (!!x.packages) {
                    const a = x.packages.split(", ");
                    if (a.length > 0) {
                        a.forEach(element => {
                            const b = element.split(" ");
                            if (b.length > 1) {
                                lstPackages.push({ quantity: b[0], package: b[1] });
                            }
                        });
                    }
                }
            });
            const packages = this.getPackages(lstPackages);
            this.billSIComponent.shippingInstruction.grossWeight = gw;
            this.billSIComponent.shippingInstruction.volume = volumn;
            this.billSIComponent.shippingInstruction.packagesNote = packages;
            this.billSIComponent.shippingInstruction.containerNote = "A PART Of CONTAINER";
            this.billSIComponent.shippingInstruction.containerSealNo = "";
        }
    }
    getPackages(lstPackages: any[]): string {
        const t = _groupBy(lstPackages, "package");
        let packages = '';
        for (const key in t) {
            // check if the property/key is defined in the object itself, not in parent
            if (t.hasOwnProperty(key)) {
                let sum = 0;
                t[key].forEach(x => {
                    sum = sum + Number(x.quantity);
                });
                packages += sum + " " + key + "\n";
            }
        }
        return packages;
    }
    initNewShippingInstruction(res: CsTransaction) {
        const user: SystemInterface.IClaimUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

        this.billSIComponent.shippingInstruction = new CsShippingInstruction();
        this.billSIComponent.shippingInstruction.refNo = res.jobNo;
        this.billSIComponent.shippingInstruction.bookingNo = res.bookingNo;
        this.billSIComponent.shippingInstruction.paymenType = res.paymentTerm;
        this.billSIComponent.shippingInstruction.invoiceDate = new Date();
        this.billSIComponent.shippingInstruction.supplier = res.coloaderId;
        this.billSIComponent.shippingInstruction.issuedUser = user.id;
        this.billSIComponent.shippingInstruction.consigneeId = res.agentId;
        this.billSIComponent.shippingInstruction.pol = res.pol;
        this.billSIComponent.shippingInstruction.pod = res.pod;
        this.billSIComponent.shippingInstruction.loadingDate = res.etd;
        this.billSIComponent.shippingInstruction.voyNo = (!!res.flightVesselName ? res.flightVesselName : '') + " - " + (!!res.voyNo ? res.voyNo : '');
        this.billSIComponent.shippingInstruction.remark = res.mbltype;
        this.getExportDefault(res);
    }
    getExportDefault(res: CsTransaction) {
        this.billSIComponent.shippingInstruction.cargoNoticeRecevier = "SAME AS CONSIGNEE";
        this.billSIComponent.shippingInstruction.containerNote = "A PART Of CONTAINER";
        this.billSIComponent.shippingInstruction.containerSealNo = '';
        if (res.creatorOffice) {
            if (!!res.creatorOffice.nameEn) {
                this.billSIComponent.shippingInstruction.shipper = !!res.creatorOffice.nameEn ? res.creatorOffice.nameEn : '';
                if (!!res.creatorOffice.addressEn) {
                    this.billSIComponent.shippingInstruction.shipper = this.billSIComponent.shippingInstruction.shipper + '\nAddress: ' + res.creatorOffice.addressEn;
                }
                if (!!res.creatorOffice.tel) {
                    this.billSIComponent.shippingInstruction.shipper = this.billSIComponent.shippingInstruction.shipper + '\nTel: ' + res.creatorOffice.tel;
                }
                if (!!res.groupEmail) {
                    this.billSIComponent.shippingInstruction.shipper = this.billSIComponent.shippingInstruction.shipper + (!!res.groupEmail ? '\nEmail: ' + res.groupEmail : '');
                }
            } else {
                if (!!res.groupEmail) {
                    this.billSIComponent.shippingInstruction.shipper = this.billSIComponent.shippingInstruction.shipper + (!!res.groupEmail ? '\nEmail: ' + res.groupEmail : '');
                }
                this.billSIComponent.shippingInstruction.shipper = !!res.groupEmail ? 'Email: ' + res.groupEmail : '';
            }
        }
    }
    save() {
        this.billSIComponent.isSubmitted = true;
        if (!this.checkValidateForm() || this.billSIComponent.voyNo.value.trim().length === 0 || this.billSIComponent.shipper.value.trim().length === 0) {
            return;
        }

        const data = this.billSIComponent.onSubmitForm();
        data.jobId = this.jobId;
        this.saveData(data);
    }
    saveData(data: CsShippingInstruction) {
        this._documentRepo.updateShippingInstruction(data).pipe(
            catchError(this.catchError)
        )
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getBillingInstruction(this.jobId);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }
    checkValidateForm() {
        let valid: boolean = true;
        if (!this.billSIComponent.formSI.valid
            || (!!this.billSIComponent.loadingDate.value && !this.billSIComponent.loadingDate.value.startDate)
            || (!!this.billSIComponent.issueDate.value && !this.billSIComponent.issueDate.value.startDate)
            || (this.billSIComponent.pod.value === this.billSIComponent.pol.value)
        ) {
            valid = false;
        }
        return valid;
    }
    refresh() {
        //this.getHouseBills();
        this.setDataBillInstructionComponent(null);
    }
    previewSIReport() {
        if (this.billSIComponent.shippingInstruction.jobId === '00000000-0000-0000-0000-000000000000') {
            this._toastService.warning('This shipment have not saved. please save.');
            return;
        }
        this._documentRepo.previewSIReport(this.billSIComponent.shippingInstruction)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('This shipment does not have any house bill ');
                    }
                },
            );
    }

    previewSIContReport() {
        if (this.billSIComponent.shippingInstruction.jobId === '00000000-0000-0000-0000-000000000000') {
            this._toastService.warning('This shipment have not saved. please save.');
            return;
        }
        this._documentRepo.previewSIContLCLReport(this.billSIComponent.shippingInstruction.jobId)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res != null) {

                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.previewPopup.frm.nativeElement.submit();
                                this.previewPopup.show();
                            }, 1000);
                        } else {
                            this._toastService.warning('This shipment does not have any house bill ');
                        }
                    }
                },
            );
    }

    previewSummaryReport() {
        if (this.billSIComponent.shippingInstruction.jobId === '00000000-0000-0000-0000-000000000000') {
            this._toastService.warning('This shipment have not saved. please save.');
            return;
        }
        this._documentRepo.previewSummaryReport(this.billSIComponent.shippingInstruction)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (res != null) {

                        this.dataReport = res;
                        if (this.dataReport != null && res.dataSource.length > 0) {
                            setTimeout(() => {
                                this.previewPopup.frm.nativeElement.submit();
                                this.previewPopup.show();
                            }, 1000);
                        } else {
                            this._toastService.warning('This shipment does not have any house bill ');
                        }
                    }
                },
            );
    }
    previewOCL() {
        if (this.billSIComponent.shippingInstruction.jobId === '00000000-0000-0000-0000-000000000000') {
            this._toastService.warning('This shipment have not saved. please save.');
            return;
        }
        this._documentRepo.previewOCLReport(this.billSIComponent.shippingInstruction)
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('This shipment does not have any house bill ');
                    }
                },
            );
    }
}

