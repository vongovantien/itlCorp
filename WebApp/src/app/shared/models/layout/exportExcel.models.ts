export class ExportExcel  {

    

    title:string;
    titleStyle:CellStyle = new CellStyle();
    type?: 'xlsx' | 'csv';
    header:string[];
    sheetName:string;
    author?:string;
    fileName?:string;
    logoPath?:string;
    company?:string;
    data:any[];
    cellStyle?:CellStyle= new CellStyle();

}

class CellStyle {    
    backgroundColor?:string;
    fontColor?:string;
    fontSize?:number;
    fontFamily?: 'Century Gothic' | 'Calibri' | 'Times New Roman' | 'Courier New' | 'Arial Black' | 'Kodchasan SemiBold';
    isBold?:boolean;
}