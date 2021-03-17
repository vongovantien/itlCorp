import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { SOA } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import { RoutingConstants } from '@constants';
import { ReportPreviewComponent, ConfirmPopupComponent, InfoPopupComponent } from '@common';
import { listAnimation } from '@animations';
import { AccountingConstants } from '@constants';
import { NgxSpinnerService } from 'ngx-spinner';
import { StatementOfAccountPaymentMethodComponent } from '../components/poup/payment-method/soa-payment-method.popup';
import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState, IAppState } from '@store';
import { ShareModulesReasonRejectPopupComponent } from 'src/app/business-modules/share-modules/components';
@Component({
    selector: 'app-statement-of-account-detail',
    templateUrl: './detail-soa.component.html',
    animations: [listAnimation]
})
export class StatementOfAccountDetailComponent extends AppList {
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild(ConfirmPopupComponent) confirmSoaPopup: ConfirmPopupComponent;
    @ViewChild(StatementOfAccountPaymentMethodComponent) paymentMethodPopupComponent: StatementOfAccountPaymentMethodComponent;
    @ViewChild(ShareModulesReasonRejectPopupComponent) reasonRejectPopupComponent: ShareModulesReasonRejectPopupComponent;
    @ViewChild('validateSyncedSOAPopup') validateSyncedPopup: InfoPopupComponent;
    soaNO: string = '';
    currencyLocal: string = 'VND';

    soa: SOA = new SOA();
    headers: CommonInterface.IHeaderTable[] = [];

    isClickSubMenu: boolean = false;

    dataExportSOA: ISOAExport;
    dataReport: any = null;
    initGroup: any[] = [];
    TYPE: string = 'LIST';
    confirmMessage: string = '';
    paymentMethodSelected: string = '';
    confirmType: string = 'SYNC';
    reasonReject: string = '';
    messageValidate: string = '';

    constructor(
        private _activedRoute: ActivatedRoute,
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _router: Router,
        private _progressService: NgProgress,
        private _exportRepo: ExportRepo,
        private _spinner: NgxSpinnerService,
        private _store: Store<IAppState>
    ) {
        super();
        this.requestSort = this.sortChargeList;
        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        this.headers = [
            { title: 'No.', field: 'i', sortable: false },
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'C/D Note', field: 'cdNote', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
            { title: 'Synced From', field: 'syncedFromBy', sortable: true },
            { title: 'ExcRate to Local', field: 'exchangeRate', sortable: true },
        ];
        this._activedRoute.queryParams.subscribe((params: any) => {
            if (!!params.no && params.currency) {
                this.soaNO = params.no;
                this.currencyLocal = params.currency;
                this.getDetailSOA(this.soaNO, this.currencyLocal)
            }
        });
    }

    getDetailSOA(soaNO: string, currency: string) {
        this._progressRef.start();
        this.isLoading = true;
        this._accoutingRepo.getDetaiLSOA(soaNO, currency)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); this.isLoading = false; })
            )
            .subscribe(
                (res: any) => {
                    this.soa = new SOA(res);
                    this.totalItems = this.soa.chargeShipments.length;
                    this.initGroup = this.soa.groupShipments;
                },
            );
    }

    sortChargeList(sortField?: string) {
        if (this.TYPE === 'GROUP') {
            this.soa.groupShipments.forEach(element => {
                element.chargeShipments = this._sortService.sort(element.chargeShipments, sortField, this.order);
            });
        } else {
            this.soa.chargeShipments = this._sortService.sort(this.soa.chargeShipments, sortField, this.order);
        }
    }

    exportExcelSOA() {
        this.isClickSubMenu = false;
        this._progressRef.start();
        this._exportRepo.exportDetailSOA(this.soaNO, 'VND')
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    const fileName = "Export SOA " + this.soaNO + ".xlsx";
                    this.downLoadFile(response, "application/ms-excel", fileName);
                },
            );

    }

    exportSOAAF() {
        const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
        this._progressRef.start();
        this._exportRepo.exportSOAAirFreight(this.soaNO, userLogged.officeId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'SOA AirFreight.xlsx');
                    } else {
                        this._toastService.warning('No data found');
                    }
                },
            );
    }

    exportSOASupplierAF() {
        const userLogged = JSON.parse(localStorage.getItem('id_token_claims_obj'));
        this._progressRef.start();
        this._exportRepo.exportSOASupplierAirFreight(this.soaNO, userLogged.officeId)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'AIR -SOA COST.xlsx');
                    } else {
                        this._toastService.warning('No data found');
                    }
                },
            );
    }

    async getDetailSOAExport(soaNO: string) {
        this._progressRef.start();
        try {
            const res: any = await (this._accoutingRepo.getDetailSOAToExport(soaNO, 'VND').toPromise());
            if (!!res) {
                return res;
            }
        } catch (errors) {
            this.handleError(errors, (data: any) => {
                this._toastService.error(data.message, data.title);
            });
        }
        finally {
            this._progressRef.complete();
        }
    }

    back() {
        this._router.navigate([`${RoutingConstants.ACCOUNTING.STATEMENT_OF_ACCOUNT}`]);
    }

    export() {
        this._progressRef.start();
        this._exportRepo.exportBravoSOA(this.soaNO)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    this.downLoadFile(response, "application/ms-excel", 'Bravo SOA.xlsx');
                },
            );
    }

    exportSOAOPS() {
        this._progressRef.start();
        this._exportRepo.exportSOAOPS(this.soaNO)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (response: ArrayBuffer) => {
                    if (response.byteLength > 0) {
                        this.downLoadFile(response, "application/ms-excel", 'SOA OPS.xlsx');
                    } else {
                        this._toastService.warning('No data found');
                    }
                },
            );
    }

    previewAccountStatementFull(soaNo: string) {
        this._progressRef.start();
        this._accoutingRepo.previewAccountStatementFull(soaNo)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
            )
            .subscribe(
                (res: any) => {
                    this.dataReport = res;
                    if (this.dataReport != null && res.dataSource.length > 0) {
                        setTimeout(() => {
                            this.previewPopup.frm.nativeElement.submit();
                            this.previewPopup.show();
                        }, 1000);
                    } else {
                        this._toastService.warning('There is no data to display preview');
                    }
                },
            );
    }

    // Charge keyword search
    onChangeKeyWord(keyword: string) {
        if (this.TYPE === 'GROUP') {
            this.soa.groupShipments = this.initGroup;
            // TODO improve search.
            if (!!keyword) {
                if (keyword.indexOf('\\') !== -1) { return this.soa.groupShipments = []; }
                keyword = keyword.toLowerCase();
                // Search group
                let dataGrp = this.soa.groupShipments.filter((item: any) => item.jobId.toLowerCase().toString().search(keyword) !== -1
                    || item.hbl.toLowerCase().toString().search(keyword) !== -1
                    || item.mbl.toLowerCase().toString().search(keyword) !== -1);
                // Không tìm thấy group thì search tiếp list con của group
                if (dataGrp.length === 0) {
                    const arrayCharge = [];
                    for (const group of this.soa.groupShipments) {
                        const data = group.chargeShipments.filter((item: any) => item.chargeCode.toLowerCase().toString().search(keyword) !== -1
                            || item.currency.toLowerCase().toString().search(keyword) !== -1
                            || item.jobId.toLowerCase().toString().search(keyword) !== -1
                            || item.hbl.toLowerCase().toString().search(keyword) !== -1
                            || item.mbl.toLowerCase().toString().search(keyword) !== -1
                            || item.chargeName.toLowerCase().toString().search(keyword) !== -1);
                        if (data.length > 0) {
                            arrayCharge.push({ jobId: group.jobId, hbl: group.hbl, mbl: group.mbl, totalDebit: group.totalDebit, totalCredit: group.totalCredit, chargeShipments: data });
                        }
                    }
                    dataGrp = arrayCharge;
                }
                return this.soa.groupShipments = dataGrp;
            } else {
                this.soa.groupShipments = this.initGroup;
            }
        }
    }

    switchToGroup() {
        if (this.TYPE === 'GROUP') {
            this.TYPE = 'LIST';
        } else {
            this.TYPE = 'GROUP';
        }
    }

    confirmSendToAcc() {
        this.confirmMessage = `Are you sure you want to send data to accountant system?`;
        this.confirmSoaPopup.show();
    }

    showConfirmed() {
        this._accoutingRepo.checkSoaSynced(this.soa.id)
            .pipe(
                catchError(this.catchError),
            ).subscribe(
                (res: any) => {
                    if (res) {
                        if (this.soa.type !== 'Credit') {
                            this.messageValidate = "Existing charge has been synchronized to the accounting system or the charge has issue VAT invoices on eFMS! Please you check again!";
                        } else {
                            this.messageValidate = "Existing charge has been synchronized to the accounting system! Please you check again!";
                        }
                        this.validateSyncedPopup.show();
                    } else {
                        this.confirmType = "SYNC";
                        if (this.soa.type === "All") {
                            this._toastService.warning("Not allow send soa with type All");
                            return;
                        }
                        if (this.soa.type === 'Credit' && this.soa.creditPayment === 'Direct') {
                            this.paymentMethodPopupComponent.show();
                        } else {
                            this.paymentMethodSelected = 'Other'; // CR 14979: 03-12-2020
                            this.confirmSendToAcc();
                        }
                    }
                },
            );
    }

    onApplyPaymentMethod($event) {
        this.paymentMethodSelected = $event;
        this.confirmSendToAcc();
    }

    sendSoaToAccountant() {
        const soaSyncIds: AccountingInterface.IRequestIntType[] = [];
        const soaId: AccountingInterface.IRequestIntType = {
            id: this.soa.id,
            type: this.soa.type,
            action: this.soa.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD',
            paymentMethod: this.paymentMethodSelected
        };
        soaSyncIds.push(soaId);
        this._spinner.show();
        this._accoutingRepo.syncSoaToAccountant(soaSyncIds)
            .pipe(
                finalize(() => this._spinner.hide()),
                catchError(this.catchError),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success("Send Data to Accountant System Successful");
                        this.getDetailSOA(this.soa.soano, this.soa.currency);
                    } else {
                        this._toastService.error("Send Data Fail");
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }

    onConfirmSoa() {
        this.confirmSoaPopup.hide();
        if (this.confirmType === "SYNC") {
            this.sendSoaToAccountant();
        }
        if (this.confirmType === "REJECT") {
            this.rejectSoa();
        }
    }

    showPopupReason() {
        this.confirmType = "REJECT";
        this.reasonRejectPopupComponent.show();
    }

    confirmReject() {
        this.confirmMessage = `Are you sure you want to reject soa?`;
        this.confirmSoaPopup.show();
    }

    onApplyReasonReject($event) {
        this.reasonReject = $event;
        this.confirmReject();
    }

    rejectSoa() {
        this._spinner.show();
        this._accoutingRepo.rejectSoaCredit({ id: this.soa.id, reason: this.reasonReject })
            .pipe(
                finalize(() => this._spinner.hide()),
                catchError(this.catchError),
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    console.log(res);
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success(res.message);
                        this.getDetailSOA(this.soa.soano, this.soa.currency);
                    } else {
                        this._toastService.error(res.message);
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }

}

interface ISOAExport {
    soaNo: string;
    taxCode: string;
    totalBalanceExchange: number;
    totalCreditExchange: number;
    totalDebitExchange: number;
    currencySOA: string;
    customerAddress: string;
    customerName: string;
    listCharges: any[];
}
