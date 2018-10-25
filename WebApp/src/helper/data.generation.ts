import * as SearchHelper from 'src/helper/SearchHelper';
import { BaseService } from 'src/services-base/base.service';
import { ToastrService } from 'ngx-toastr';
import { Ng4LoadingSpinnerService } from 'ng4-loading-spinner';
import { API_MENU } from 'src/constants/api-menu.const';
import { StageModel } from 'src/app/shared/models/catalogue/stage.model';

export function generateData(quantity:Number,seed:any,api_url:string,ignore:[string]){
   
   
    let array = Object.keys(seed);
    array.forEach(element => {
      
        var type = eval("seed."+element);        
        console.log(type + " | " + typeof(type));
    });
    
}
