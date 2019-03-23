import { Injectable, } from '@angular/core';
// import { Workbook } from 'exceljs'
import * as  fs from 'file-saver';
import {Workbook} from "exceljs/dist/exceljs.min.js";
import {ExportExcel} from 'src/app/shared/models/layout/exportExcel.models';
@Injectable({
  providedIn: 'root'
})


/**
 * author: Thor-The
 * More informations about this services, please reference to the link https://www.ngdevelop.tech/export-to-excel-in-angular-6/
 */
export class ExcelService {
  constructor() {
  }
  generateExcel(exportModel:ExportExcel) {
    
    
    //Excel Title, Header, Data
    const title =  exportModel.title;  
    const header = exportModel.header;  
    const author = exportModel.author;    
    const data = exportModel.data;

    var HeaderName:string[]=[];
    var HeaderWidth:number[]=[];
     header.forEach(hed => {
      HeaderName.push(hed.name);
      HeaderWidth.push(hed.width);
    });

    var titleFontStyle:any;
    var cellStyle:any;
    var authorFontStyle:any


    if(Object.keys(exportModel.titleFontStyle).length!==0){
      titleFontStyle = {
        name:exportModel.titleFontStyle.fontFamily,
        size:exportModel.titleFontStyle.fontSize,
        family:4,
        bold:exportModel.titleFontStyle.isBold,
        italic:exportModel.titleFontStyle.isItalic
      };
    }else{
      titleFontStyle = {name:'Century Gothic',family:4,size:20,bold:true,italic:false};
    }


    if(Object.keys(exportModel.cellStyle).length!==0){
      cellStyle = {
        name:exportModel.cellStyle.fontFamily,
        size:exportModel.cellStyle.fontSize,
        family:4,
        bold:exportModel.cellStyle.isBold,
        italic:exportModel.cellStyle.isItalic
      };
    }else{
      cellStyle = {name:'Century Gothic',family:4,size:11,bold:true,italic:false};
    }

    if(Object.keys(exportModel.authorFontSyle).length!==0){
      authorFontStyle = {
        name:exportModel.authorFontSyle.fontFamily,
        size:exportModel.authorFontSyle.fontSize,
        family:4,
        bold:exportModel.authorFontSyle.isBold,
        italic:exportModel.authorFontSyle.isItalic
      };
    }else{
      authorFontStyle = {name:'Century Gothic',family:4,size:13,bold:true,italic:false};
    }

    
    //Create workbook and worksheet
    let workbook = new Workbook();
    workbook.views
    let worksheet = workbook.addWorksheet(exportModel.sheetName);
    //Add Row and formatting
    let titleRow = worksheet.addRow([title]);
    titleRow.font = titleFontStyle 
    worksheet.addRow([]);
    let exportedBy = worksheet.addRow(['Exported By : ' + author.toUpperCase()]);
    exportedBy.font = authorFontStyle
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
    let headerRow = worksheet.addRow(HeaderName);
    
    // Cell Style : Fill and Border
    headerRow.eachCell((cell, number) => {
      // cell.fill = {
      //   type: 'pattern',
      //   pattern: 'solid',
      //   fgColor: { argb: 'FFFFFF00' },
      //   bgColor: { argb: 'FF0000FF' }
      // }
      cell.fill;
      cell.border = { top: { style: 'thin' }, left: { style: 'thin' }, bottom: { style: 'thin' }, right: { style: 'thin' } };
      cell.alignment = {horizontal:'center',vertical:'middle'}
      cell.font =  cellStyle;  
    })

    // Add Data and Conditional Formatting
    data.forEach(d => {
      let row = worksheet.addRow(d);
      row.eachCell((cell,number)=>{
        cell.alignment = {horizontal:'center',vertical:'middle'} ;
      });
    }
    );

    for(var i = 0; i<HeaderName.length;i++){      
      for(var j=0;j<HeaderWidth.length;j++){
        if(i===j){
          worksheet.getColumn(i+1).width = HeaderWidth[j];
        }
      }
     
    }

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