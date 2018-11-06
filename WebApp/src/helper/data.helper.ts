import * as SearchHelper from 'src/helper/SearchHelper';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { StageModel } from 'src/app/shared/models/catalogue/stage.model';
import { CatPlaceModel } from 'src/app/shared/models/catalogue/catPlace.model';
import { async } from 'rxjs/internal/scheduler/async';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';

// export class DataHelper {
//     constructor(private baseService : BaseService , private api_menu: API_MENU) {
//     }
//     // baseService: BaseService;
//     // api_menu: API_MENU;
//     searchObj = {
//         countryId: null,
//         provinceId: null,
//         districtId: null,
//         placeType: null,
//     }

//     public async getProvinces(countryId: any, pager: PagerSetting) {
//         this.searchObj.countryId = countryId;
//         var provinces = await this.baseService.postAsync(this.api_menu.Catalogue.CatPlace.paging + "?page=" + pager.currentPage + "&size=" + pager.pageSize, this.searchObj, false, false);
//         return provinces;
//     }
// }


/**
 * Return list provinces that belong to country has countryId
 * @param countryId 
 * @param pager 
 */
export async function getProvinces(countryId: any,baseService:BaseService,api_menu:API_MENU) {
   
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
export async function getDistricts(countryId: any, provinceId: any,baseService:BaseService,api_menu:API_MENU) {
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
export async function getTownWards(countryId: any, provinceId: any, districtId, pager: PagerSetting,baseService:BaseService,api_menu:API_MENU) {
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

