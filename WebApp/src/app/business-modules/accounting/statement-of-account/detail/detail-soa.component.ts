import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountingRepo, ExportRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { SOA } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';

import { Workbook } from "exceljs/dist/exceljs.min.js";
import fs from 'file-saver';
import { Worksheet, Cell, Row } from 'exceljs';
import { formatDate } from '@angular/common';
import { NgProgress } from '@ngx-progressbar/core';
@Component({
    selector: 'app-statement-of-account-detail',
    templateUrl: './detail-soa.component.html',
})
export class StatementOfAccountDetailComponent extends AppList {

    soaNO: string = '';
    currencyLocal: string = 'VND';

    soa: SOA = new SOA();
    headers: CommonInterface.IHeaderTable[] = [];

    isClickSubMenu: boolean = false;

    dataExportSOA: ISOAExport;
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
                },
            );
    }

    sortChargeList(sortField?: string, order?: boolean) {
        this.soa.chargeShipments = this._sortService.sort(this.soa.chargeShipments, sortField, order);
    }

    exportExcelSOA() {
        // this.dataExportSOA = await this.getDetailSOAExport(this.soa.soano);
        // if (!!this.dataExportSOA) {
        //     this.exportExcel(this.dataExportSOA);
        // }
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

    exportExcel(soaData: ISOAExport) {
        const indexPositionData: number = 10;

        const exportModel: IExcel = {
            fileName: `Export SOA ${soaData.soaNo}`,
            title: 'Report SOA',
            author: localStorage.getItem('currently_userName') || 'Admin',
            headers: [
                { name: "Service Date", width: 10 },
                { name: "Job No.", width: 20 },
                { name: "M-B/L", width: 10 },
                { name: "H-B/L", width: 10 },
                { name: "Customs No.", width: 15 },
                { name: "Code Fee", width: 10 },
                { name: "Description", width: 20 },
                { name: "Invoice No", width: 10 },
                { name: "Revenue", width: 10 },
                { name: "Cost", width: 10 },
                { name: "Currency", width: 10 },
                { name: "Revenue", width: 10 },
                { name: "Cost", width: 10 },
            ],
            sheetName: `SOA ${soaData.soaNo}`,
            data: soaData.listCharges.map((item: any) => {
                return [
                    !!item['serviceDate'] ? formatDate(item['serviceDate'], 'dd/MM/yyyy', 'en') : '-',
                    !!item['jobId'] ? item['jobId'] : '-',
                    !!item['mbl'] ? item['mbl'] : '-',
                    !!item['hbl'] ? item['hbl'] : '-',
                    !!item['customNo'] ? item['customNo'] : '-',
                    !!item['chargeCode'] ? item['chargeCode'] : '-',
                    !!item['chargeName'] ? item['chargeName'] : '-',
                    !!item['creditDebitNo'] ? item['creditDebitNo'] : '-',
                    !!item['debit'] ? item['debit'] : 0,
                    !!item['credit'] ? item['credit'] : 0,
                    !!item['currencyCharge'] ? item['currencyCharge'] : '-',
                    !!item['debitExchange'] ? item['debitExchange'] : 0,
                    !!item['creditExchange'] ? item['creditExchange'] : 0
                ];
            })
        };

        const headersName: string[] = [];
        const headersWidth: number[] = [];

        exportModel.headers.forEach((header: any) => {
            headersName.push(header.name);
            headersWidth.push(header.width);
        });
        const numRowData = exportModel.data.length || 0;  // * total num row of table

        // * Init WorkSheet
        const workbook: Workbook = new Workbook();
        const worksheet: Worksheet = workbook.addWorksheet(exportModel.sheetName);

        worksheet.addRow([]);
        worksheet.mergeCells('A1', "M1");
        const headingCell: Cell = worksheet.getCell('A1');
        headingCell.value = "Statement Of Account";
        headingCell.alignment = { horizontal: 'center', vertical: 'middle' };
        headingCell.font = {
            bold: true,
            size: 16
        };

        for (let index = 3; index <= 7; index++) {
            worksheet.getCell(`F${index}`).alignment = { horizontal: 'center', vertical: 'middle' };
            worksheet.getCell(`F${index}`).font = {
                bold: true,
                size: 11
            };
        }
        worksheet.getCell("F3").value = "SOA No";
        worksheet.getCell("F4").value = "Customer";
        worksheet.getCell("F5").value = "Taxcode";
        worksheet.getCell("F6").value = "Address";
        worksheet.getCell("F7").value = "Currency";

        for (let index = 3; index <= 7; index++) {
            worksheet.mergeCells(`G${index}`, `M${index}`);
            worksheet.getCell(`G${index}`).font = {
                bold: false,
                size: 11
            };
        }
        worksheet.getCell("G3").value = soaData.soaNo;  // ? soaNo
        worksheet.getCell("G4)").value = soaData.customerName;  // ? Customer
        worksheet.getCell("G5").value = '' + soaData.taxCode + '';  // ? TaxCode
        worksheet.getCell("G6").value = soaData.customerAddress; // ? Address
        worksheet.getCell("G7").value = soaData.currencySOA;   // ? Currency

        // * Handle header.
        const headerRow: Row = worksheet.addRow(headersName);
        headerRow.eachCell((cell: Cell, number: any) => {
            cell.alignment = { horizontal: 'center', vertical: 'middle' };
            cell.font = {
                bold: true,
                size: 11
            };

            if (cell.address !== "I8" && cell.address !== "J8" && cell.address !== "L8" && cell.address !== "M8") {
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                const nextCellRow: any = cell.address.slice(0, 1) + (+(cell.address.slice(1)) + 1);
                worksheet.mergeCells(`${cell.address}`, nextCellRow);
            }
        });

        //  * Merge Header
        worksheet.mergeCells(`I8`, "J8");
        worksheet.getCell("I8").value = "Total Amount";
        worksheet.getCell("I9").value = "Revenue";
        worksheet.getCell("J9").value = "Cost";

        worksheet.mergeCells(`L8`, "M8");
        worksheet.getCell("L8").value = "Exchange Total Amount";
        worksheet.getCell("L9").value = "Revenue";
        worksheet.getCell("M9").value = "Cost";

        ["I8", "I9", "J9", "L8", "L9", "M9"].map((key: string) => {
            worksheet.getCell(key).alignment = { horizontal: 'center', vertical: 'middle' };
            worksheet.getCell(key).font = {
                bold: true,
                size: 11
            };
            worksheet.getCell(key).border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        for (let i = 0; i < headersName.length; i++) {
            for (let j = 0; j < headersWidth.length; j++) {
                if (i === j) {
                    worksheet.getColumn(i + 1).width = headersWidth[j];
                }
            }
        }

        // * Handle Data (A9)
        exportModel.data.forEach((d: any, index: number) => {
            const row = worksheet.addRow(d);
            row.eachCell((cell: Cell, number: any) => {
                cell.alignment = { horizontal: 'center', vertical: 'middle' };
                cell.font = {
                    bold: false,
                    size: 11
                };
                cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
                cell.numFmt = '#,##0.00';
            });
        });

        worksheet.addRows([]);  // * total Cell.
        worksheet.mergeCells(`J${indexPositionData + numRowData}`, `K${indexPositionData + numRowData}`);
        const totalCell: Cell = worksheet.getCell(`J${indexPositionData + numRowData}`);
        totalCell.alignment = { horizontal: 'center', vertical: 'middle' };
        totalCell.value = "Total";
        totalCell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };

        const creditTotalExchange: Cell = worksheet.getCell(`L${indexPositionData + numRowData}`);
        const debitTotalExchange: Cell = worksheet.getCell(`M${indexPositionData + numRowData}`);
        creditTotalExchange.value = soaData.totalCreditExchange;
        debitTotalExchange.value = soaData.totalDebitExchange;
        creditTotalExchange.numFmt = '#,##0.00'; // * format with negative number
        debitTotalExchange.numFmt = '#,##0.00'; // * format with negative number
        // * balance Total cell
        const balanceTotalCell: Cell = worksheet.getCell(`J${indexPositionData + numRowData + 1}`);
        worksheet.mergeCells(balanceTotalCell.address, `K${indexPositionData + numRowData + 1}`);
        balanceTotalCell.value = "Balance";

        const balanceTotalExchange: Cell = worksheet.getCell(`L${indexPositionData + numRowData + 1}`);
        worksheet.mergeCells(balanceTotalExchange.address, `M${indexPositionData + numRowData + 1}`);
        balanceTotalExchange.value = soaData.totalBalanceExchange;
        balanceTotalExchange.numFmt = '#,##0.00; (#,##0.00)'; // * format with negative number

        [totalCell.address, creditTotalExchange.address, debitTotalExchange.address, balanceTotalCell.address, balanceTotalExchange.address].map((key: string) => {
            worksheet.getCell(key).alignment = { horizontal: 'center', vertical: 'middle' };
            worksheet.getCell(key).font = {
                bold: true,
                size: 11
            };

            worksheet.getCell(key).border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        // * save file
        workbook.xlsx.writeBuffer().then((data: any) => {
            const blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            fs.saveAs(blob, exportModel.fileName + '.xlsx');
        });

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
