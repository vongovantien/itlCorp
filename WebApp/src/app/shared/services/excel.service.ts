import { Injectable, } from '@angular/core';
import { Workbook } from 'exceljs';
import * as fs from 'file-saver';

// import { WorkBook } from 'xlsx/types';
import {ExportExcel} from 'src/app/shared/models/layout/exportExcel.models';
@Injectable({
  providedIn: 'root'
})
export class ExcelService {
  constructor() {
  }
  generateExcel(exportModel:ExportExcel) {
    
    //Excel Title, Header, Data
    const title =  exportModel.title;  
    const header = exportModel.header;  
    const data = exportModel.data;
    
    //Create workbook and worksheet
    let workbook = new Workbook();
    workbook.views
    let worksheet = workbook.addWorksheet(exportModel.sheetName);
    //Add Row and formatting
    let titleRow = worksheet.addRow([title]);
    titleRow.font = { name: exportModel.titleStyle.fontFamily, family: 4, size: exportModel.titleStyle.fontSize,bold: exportModel.titleStyle.isBold }
    worksheet.addRow([]);
    let exportedBy = worksheet.addRow(['Export By : ' + exportModel.author]);
    exportedBy.font = {name:exportModel.titleStyle.fontFamily,size:14,bold:true}
    //Add Image
    // let logo = workbook.addImage({
    //   base64: logoFile.logoBase64,
    //   extension: 'png',
    // });
    // worksheet.addImage(logo, 'E1:F3');
    worksheet.mergeCells('A1:D2');
    //Blank Row 
    worksheet.addRow([]);
    //Add Header Row
    let headerRow = worksheet.addRow(header);
    
    // Cell Style : Fill and Border
    headerRow.eachCell((cell, number) => {
      // cell.fill = {
      //   type: 'pattern',
      //   pattern: 'solid',
      //   fgColor: { argb: 'FFFFFF00' },
      //   bgColor: { argb: 'FF0000FF' }
      // }
      cell.fill;
      cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } }
      cell.font = {name: exportModel.cellStyle.fontFamily, size: exportModel.cellStyle.fontSize, bold: exportModel.cellStyle.isBold }
    })

    
    // worksheet.addRows(data);
    // Add Data and Conditional Formatting
    data.forEach(d => {
      let row = worksheet.addRow(d);
      let qty = row.getCell(5);
      qty.fill;
     // let color = 'FF99FF99';      
      // qty.fill = {
      //   type: 'pattern',
      //   pattern: 'solid',
      //   fgColor: { argb: color }
      // }
    }
    );
    for(var i = 1; i<=header.length;i++){
      worksheet.getColumn(i).width = 25;
    }
    // worksheet.getColumn(3).width = 30;
    // worksheet.getColumn(4).width = 30;

    worksheet.addRow([]);
    // //Footer Row
    // let footerRow = worksheet.addRow(['This is system generated excel sheet.']);
    // footerRow.getCell(1).fill = {
    //   type: 'pattern',
    //   pattern: 'solid',
    //   fgColor: { argb: 'FFCCFFE5' }
    // };
    // footerRow.getCell(1).border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } }
    // //Merge Cells
    // worksheet.mergeCells(`A${footerRow.number}:F${footerRow.number}`);
    //Generate Excel File with given name
    workbook.xlsx.writeBuffer().then((data:any) => {
      let blob = new Blob([data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
      fs.saveAs(blob, exportModel.fileName +'.xlsx');
    })
  }
}