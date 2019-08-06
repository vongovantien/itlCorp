import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { NgxSpinnerService } from 'ngx-spinner';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { SOA } from 'src/app/shared/models';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';

import { Workbook } from "exceljs/dist/exceljs.min.js";
import fs from 'file-saver';
import { Worksheet, Cell, Row } from 'exceljs';
import { formatDate } from '@angular/common';
@Component({
    selector: 'app-statement-of-account-detail',
    templateUrl: './detail-soa.component.html',
})
export class StatementOfAccountDetailComponent extends AppList {

    soaNO: string = '';
    currencyLocal: string = 'VND';

    soa: SOA = new SOA();
    headers: CommonInterface.IHeaderTable[] = [];

    dataExportSOA: ISOAExport;
    constructor(
        private _activedRoute: ActivatedRoute,
        private _accoutingRepo: AccoutingRepo,
        private _spinner: NgxSpinnerService,
        private _toastService: ToastrService,
        private _sortService: SortService,
        private _router: Router
    ) {
        super();
        this.requestList = this.sortChargeList;
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
        this._spinner.show();
        this._accoutingRepo.getDetaiLSOA(soaNO, currency)
            .pipe(
                catchError(this.catchError),
                finalize(() => { this._spinner.hide(); })
            )
            .subscribe(
                (res: any) => {
                    this.soa = new SOA(res);
                    this.totalItems = this.soa.chargeShipments.length;
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                () => { },
            );
    }

    sortChargeList(sortField?: string, order?: boolean) {
        this.soa.chargeShipments = this._sortService.sort(this.soa.chargeShipments, sortField, order);
    }

    async exportExcelSOA() {
        this.dataExportSOA = await this.getDetailSOAExport(this.soa.soano);
        if (!!this.dataExportSOA) {
            this.exportExcel(this.dataExportSOA);
        }
    }

    async getDetailSOAExport(soaNO: string) {
        this._spinner.show();
        try {
            const res: any = await (this._accoutingRepo.getDetailSOAToExport(soaNO).toPromise());
            if (!!res) {
                return res;
            }
        } catch (error) {
            this.handleError(error);
        }
        finally {
            this._spinner.hide();
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
                { name: "INVOCE NO", width: 10 },
                { name: "Revenue", width: 10 },
                { name: "Cost", width: 10 },
                { name: "Currency", width: 10 },
                { name: "Revenue", width: 10 },
                { name: "Cost", width: 10 },
            ],
            sheetName: `SOA ${soaData.soaNo}`,
            data: soaData.listCharges.map((item: any) => {
                return [
                    formatDate(item['serviceDate'], 'dd/MM/yyyy', 'vi'),
                    item['jobId'],
                    item['mbl'],
                    item['hbl'],
                    item['customNo'],
                    item['chargeCode'],
                    item['chargeName'],
                    item['creditDebitNo'],
                    item['debit'],
                    item['credit'],
                    item['currencySOA'],
                    item['creditExchange'],
                    item['debitExchange'],
                ];
            })
        };

        const headersName: string[] = [];
        const headersWidth: number[] = [];

        exportModel.headers.forEach((header: any) => {
            headersName.push(header.name);
            headersWidth.push(header.width);
        });
        const numRowData = exportModel.data.length || 0;

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
            worksheet.getCell(`G${index}`).alignment = { horizontal: 'center', vertical: 'middle' };
            worksheet.getCell(`G${index}`).font = {
                bold: true,
                size: 12
            };
        }
        worksheet.getCell("G3").value = "SOA No";
        worksheet.getCell("G4").value = "Customer";
        worksheet.getCell("G5").value = "Taxcode";
        worksheet.getCell("G6").value = "Address";
        worksheet.getCell("G7").value = "Currency";

        for (let index = 3; index <= 7; index++) {
            worksheet.mergeCells(`H${index}`, `M${index}`);
            worksheet.getCell(`H${index}`).font = {
                bold: false,
                size: 12
            };
        }

        worksheet.getCell("H3").value = '' + soaData.soaNo + '';  // ? soaNo
        worksheet.getCell("H4)").value = soaData.customerName;  // ? Customer
        worksheet.getCell("H5").value = '' + soaData.taxCode + '';  // ? TaxCode
        worksheet.getCell("H6").value = soaData.customerAddress; // ? Address
        worksheet.getCell("H7").value = soaData.currencySOA;   // ? Currency

        // * Handle header.
        const headerRow: Row = worksheet.addRow(headersName);
        worksheet.addRow([]);
        headerRow.eachCell((cell: Cell, number: any) => {
            cell.alignment = { horizontal: 'center', vertical: 'middle' };
            cell.font = {
                bold: true,
                size: 10
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
                size: 10
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
            row.eachCell((cell: any, number: any) => {
                cell.alignment = { horizontal: 'center', vertical: 'middle' };
                cell.font = {
                    bold: false,
                    size: 10
                };
            });
        });

        worksheet.addRows([]);  // * total Cell.
        worksheet.mergeCells(`A${indexPositionData + numRowData}`, `G${indexPositionData + numRowData}`);

        const totalCell: Cell = worksheet.getCell(`A${indexPositionData + numRowData}`);
        totalCell.alignment = { horizontal: 'center', vertical: 'middle' };
        totalCell.value = "Total";

        const creditTotalExchange: Cell = worksheet.getCell(`L${indexPositionData + numRowData}`);
        const debitTotalExchange: Cell = worksheet.getCell(`M${indexPositionData + numRowData}`);
        creditTotalExchange.value = '' + soaData.totalCreditExchange;
        debitTotalExchange.value = '' + soaData.totalDebitExchange;

        const balanceTotalCell: Cell = worksheet.getCell(`J${indexPositionData + numRowData + 1}`);
        worksheet.mergeCells(balanceTotalCell.address, `K${indexPositionData + numRowData + 1}`);
        balanceTotalCell.value = "Balance";

        const balanceTotalExchange: Cell = worksheet.getCell(`L${indexPositionData + numRowData + 1}`);
        worksheet.mergeCells(balanceTotalExchange.address, `M${indexPositionData + numRowData + 1}`);
        balanceTotalExchange.value = soaData.totalBalanceExchange;

        [totalCell.address, creditTotalExchange.address, debitTotalExchange.address, balanceTotalCell.address, balanceTotalExchange.address].map((key: string) => {
            worksheet.getCell(key).alignment = { horizontal: 'center', vertical: 'middle' };
            worksheet.getCell(key).font = {
                bold: true,
                size: 10
            };

            // worksheet.getCell(key).border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
        });

        // * save file
        workbook.xlsx.writeBuffer().then((data: any) => {
            const blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
            fs.saveAs(blob, exportModel.fileName + '.xlsx');
        });

    }

    handleError(errors?: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }

    back() {
        this._router.navigate(['home/accounting/statement-of-account']);
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
