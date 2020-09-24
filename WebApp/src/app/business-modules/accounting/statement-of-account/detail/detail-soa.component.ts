import { Component, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { SOA } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';
import { NgProgress } from '@ngx-progressbar/core';
import { ReportPreviewComponent } from '@common';
@Component({
    selector: 'app-statement-of-account-detail',
    templateUrl: './detail-soa.component.html',
})
export class StatementOfAccountDetailComponent extends AppList {
    @ViewChild(ReportPreviewComponent, { static: false }) previewPopup: ReportPreviewComponent;
    soaNO: string = '';
    currencyLocal: string = 'VND';

    soa: SOA = new SOA();
    headers: CommonInterface.IHeaderTable[] = [];

    isClickSubMenu: boolean = false;

    dataExportSOA: ISOAExport;
    dataReport: any = null;
    initGroup: any[] = [];
    TYPE: string = 'LIST';
    constructor(
        private _activedRoute: ActivatedRoute,
        private _accoutingRepo: AccountingRepo,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _router: Router,
        private _progressService: NgProgress,
        private _exportRepo: ExportRepo
    ) {
        super();
        this.requestSort = this.sortChargeList;
        this._progressRef = this._progressService.ref();

    }

    ngOnInit() {
        this.headers = [
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
        this._accoutingRepo.getDetaiLSOA(soaNO, currency)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._progressRef.complete(); })
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
        this._router.navigate(['home/accounting/statement-of-account']);
    }

    export() {
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
}

interface IExcel {
    title: string;
    author: string;
    headers: any[];
    data: any;
    fileName: string;
    sheetName: string;
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
