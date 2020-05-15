import { Component, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { catchError, finalize, takeUntil, take } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import * as fromShareBussiness from '../../../../share-business/store';
import { DocumentationRepo } from 'src/app/shared/repositories';
import { CsShippingInstruction } from 'src/app/shared/models/document/shippingInstruction.model';
import { ToastrService } from 'ngx-toastr';
import * as fromShare from '../../../../share-business/store';
import { ActivatedRoute, Router } from '@angular/router';
import { DataService } from 'src/app/shared/services';
import { SystemConstants } from 'src/constants/system.const';
import { CsTransaction } from 'src/app/shared/models';
import { ReportPreviewComponent } from 'src/app/shared/common';
import { getTransactionPermission, getTransactionLocked } from '../../../../share-business/store';
import { ShareBussinessBillInstructionSeaExportComponent, ShareBussinessBillInstructionHousebillsSeaExportComponent } from '@share-bussiness';

@Component({
    selector: 'app-sea-lcl-export-shipping-instruction',
    templateUrl: './sea-lcl-export-shipping-instruction.component.html'
})
export class SeaLclExportShippingInstructionComponent extends AppList {
    @ViewChild(ShareBussinessBillInstructionSeaExportComponent, { static: false }) billSIComppnent: ShareBussinessBillInstructionSeaExportComponent;
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
        private _dataService: DataService,
        private _router: Router) {
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
        this._store.select(fromShare.getTransactionDetailCsTransactionState)
            .pipe(takeUntil(this.ngUnsubscribe), take(1))
            .subscribe(
                (res) => {
                    if (!!res) {
                        console.log(res);
                        if (data != null) {
                            this.billSIComppnent.shippingInstruction = data;
                            this.billSIComppnent.shippingInstruction.refNo = res.jobNo;
                        } else {
                            this.initNewShippingInstruction(res);
                            this.getGoods();
                        }

                        this.billSIComppnent.shippingInstruction.csTransactionDetails = this.houseBills;
                        this.billSIComppnent.termTypes = this.termTypes;
                        this.billSIComppnent.setformValue(this.billSIComppnent.shippingInstruction);
                        console.log(this.billSIComppnent.shippingInstruction);
                    }
                }
            );
    }
    getGoods() {
        if (this.houseBills != null) {
            let desOfGoods = '';
            let packages = '';
            let containerNotes = '';
            let contSealNos = '';
            let gw = 0;
            let volumn = 0;
            this.houseBills.forEach(x => {
                gw += x.gw;
                volumn += x.cbm;
                desOfGoods += !!x.desOfGoods ? (x.desOfGoods + '\n') : '';
                containerNotes += !!x.packageContainer ? (x.packageContainer + '\n') : '';
                packages += !!x.packages ? (x.packages + '\n') : '';
                contSealNos += !!x.contSealNo ? (x.contSealNo) : '';
            });
            this.billSIComppnent.shippingInstruction.grossWeight = gw;
            this.billSIComppnent.shippingInstruction.volume = volumn;
            this.billSIComppnent.shippingInstruction.goodsDescription = desOfGoods;
            this.billSIComppnent.shippingInstruction.packagesNote = packages;
            this.billSIComppnent.shippingInstruction.containerNote = "A PART Of CONTAINER"; // containerNotes;
            this.billSIComppnent.shippingInstruction.containerSealNo = ""; //contSealNos;
        }
    }
    initNewShippingInstruction(res: CsTransaction) {
        const user: SystemInterface.IClaimUser = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));

        this.billSIComppnent.shippingInstruction = new CsShippingInstruction();
        this.billSIComppnent.shippingInstruction.refNo = res.jobNo;
        this.billSIComppnent.shippingInstruction.bookingNo = res.bookingNo;
        this.billSIComppnent.shippingInstruction.paymenType = "Prepaid";
        this.billSIComppnent.shippingInstruction.invoiceDate = new Date();
        this.billSIComppnent.shippingInstruction.supplier = res.coloaderId;
        this.billSIComppnent.shippingInstruction.issuedUser = user.id;
        this.billSIComppnent.shippingInstruction.consigneeId = res.agentId;
        this.billSIComppnent.shippingInstruction.pol = res.pol;
        this.billSIComppnent.shippingInstruction.pod = res.pod;
        this.billSIComppnent.shippingInstruction.loadingDate = res.etd;
        this.billSIComppnent.shippingInstruction.voyNo = res.flightVesselName + " - " + res.voyNo;
        this.getExportDefault(res);
    }
    getExportDefault(res: CsTransaction) {
        this.billSIComppnent.shippingInstruction.cargoNoticeRecevier = "SAME AS CONSIGNEE";
        this.billSIComppnent.shippingInstruction.containerNote = "A PART Of CONTAINER";
        this.billSIComppnent.shippingInstruction.containerSealNo = '';
        if (res.creatorOffice) {
            if (!!res.creatorOffice.nameEn) {
                this.billSIComppnent.shippingInstruction.shipper = !!res.creatorOffice.nameEn ? res.creatorOffice.nameEn : '';
                if (!!res.creatorOffice.addressEn) {
                    this.billSIComppnent.shippingInstruction.shipper = this.billSIComppnent.shippingInstruction.shipper + '\nAddress: ' + res.creatorOffice.addressEn;
                }
                if (!!res.creatorOffice.tel) {
                    this.billSIComppnent.shippingInstruction.shipper = this.billSIComppnent.shippingInstruction.shipper + '\nTel: ' + res.creatorOffice.tel;
                }
                this.billSIComppnent.shippingInstruction.shipper = this.billSIComppnent.shippingInstruction.shipper + (!!res.groupEmail ? 'Email: ' + res.groupEmail : '');
            } else {
                this.billSIComppnent.shippingInstruction.shipper = !!res.groupEmail ? 'Email: ' + res.groupEmail : '';
            }
        }
    }
    save() {
        this.billSIComppnent.isSubmitted = true;
        if (!this.checkValidateForm()) {
            return;
        }

        const data = this.billSIComppnent.onSubmitForm();
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
                        this._router.navigate([`home/documentation/sea-lcl-export/${this.jobId}`]);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }
    checkValidateForm() {
        let valid: boolean = true;
        if (!this.billSIComppnent.formSI.valid
            || (!!this.billSIComppnent.loadingDate.value && !this.billSIComppnent.loadingDate.value.startDate)
            || (!!this.billSIComppnent.issueDate.value && !this.billSIComppnent.issueDate.value.startDate)
        ) {
            valid = false;
        }
        return valid;
    }
    refresh() {
        this.getHouseBills();
    }
    previewSIReport() {
        if (this.billSIComppnent.shippingInstruction.jobId === '00000000-0000-0000-0000-000000000000') {
            this._toastService.warning('This shipment have not saved. please save.');
            return;
        }
        this._documentRepo.previewSIReport(this.billSIComppnent.shippingInstruction)
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
    previewOCL() {
        if (this.billSIComppnent.shippingInstruction.jobId === '00000000-0000-0000-0000-000000000000') {
            this._toastService.warning('This shipment have not saved. please save.');
            return;
        }
        this._documentRepo.previewOCLReport(this.billSIComppnent.shippingInstruction)
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

