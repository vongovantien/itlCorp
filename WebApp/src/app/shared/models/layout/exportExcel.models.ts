export class ExportExcel  {

    title:string;
    type?: 'xlsx' | 'csv';
    header:string[];
    author?:string;
    fileName?:string;
    logoPath?:string;
    company?:string;
    data:any[];
    cellStyle?:CellStyle

}

class CellStyle {
    
    backgroundColor?:string;
    fontColor?:string;
    fontSize?:number;
    fontFamily?:string;
    isBold?:boolean;
}