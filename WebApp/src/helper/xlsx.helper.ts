

/**
 * Export excel file with single sheet from JSON data 
 * https://www.npmjs.com/package/xlsx
 * @param data 
 * @param fileName 
 * @param sheetName 
 */

import * as XLSX from 'xlsx';
export function exportExcelFileWithSingleSheet(data: any, fileName: string, sheetName: string) {
    try {
        /* generate worksheet */
        const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(data);
        /* generate workbook and add the worksheet */
        const wb: XLSX.WorkBook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, sheetName);
        /* save to file */
        XLSX.writeFile(wb, fileName+".xlsx");
    } catch (error) {
        throw error;
    }
}