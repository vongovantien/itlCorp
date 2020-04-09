import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';

export async function getShipmentCommonData(baseService: BaseService,api_menu:API_MENU) {
    const data = await baseService.getAsync(api_menu.Documentation.Terminology.getShipmentCommonData);
    return data;
}

export async function getOPSShipmentCommonData(baseService: BaseService,api_menu:API_MENU) {
    const data = await baseService.getAsync(api_menu.Documentation.Terminology.getOPSShipmentCommonData);
    return data;
}
