
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import * as XLSX from 'xlsx';

/**
 * Return list provinces that belong to country has countryId
 * @param countryId 
 * @param pager 
 */
export async function getProvinces(countryId: any, baseService: BaseService, api_menu: API_MENU) {

    // var baseService: BaseService;
    // var api_menu: API_MENU;
    var searchObj = {
        countryId: countryId,
        placeType: PlaceTypeEnum.Province
    }
    var provinces = await baseService.postAsync(api_menu.Catalogue.CatPlace.query, searchObj, false, false);
    return provinces;
}

/**
 *  Return list districts that belong to province-city / country 
 * @param countryId 
 * @param provinceId 
 * @param pager 
 */
export async function getDistricts(countryId: any, provinceId: any, baseService: BaseService, api_menu: API_MENU) {
    var baseService: BaseService;
    var api_menu: API_MENU;
    var searchObj = {
        countryId: countryId,
        provinceId: provinceId,
        placeType: 4
    }
    var districts = await baseService.postAsync(api_menu.Catalogue.CatPlace.query, searchObj, false, false);
    return districts;
}

/**
 * Return list town-ward that belong to district/province/country 
 * @param countryId 
 * @param provinceId 
 * @param districtId 
 * @param pager 
 */
export async function getTownWards(countryId: any, provinceId: any, districtId, pager: PagerSetting, baseService: BaseService, api_menu: API_MENU) {
    var baseService: BaseService;
    var api_menu: API_MENU;
    var searchObj = {
        countryId: countryId,
        provinceId: provinceId,
        districtId: districtId,
        placeType: 11
    }
    var townWards = await baseService.postAsync(api_menu.Catalogue.CatPlace.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, searchObj, false, false);
    return townWards;
}

/**
 * Export excel file with single sheet from JSON data 
 * https://www.npmjs.com/package/xlsx
 * @param data 
 * @param fileName 
 * @param sheetName 
 */
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

