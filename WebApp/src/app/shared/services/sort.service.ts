import * as lodash from 'lodash';
// export class SortService {
//     sort(items: any[], property: string, isDesc: boolean){
//         let direction = isDesc?'asc':'desc';
//         return lodash.orderBy(items,[property],[direction]);
//       }
// }


export class SortService {
    sort(items: any[], property: string, isDesc: boolean){       
        if(isDesc){
            return lodash.orderBy(items,[item => (!item[property]?null:item[property].toString().toLowerCase())],['asc']);
        }else{
            return lodash.orderBy(items,[item => (!item[property]?null:item[property].toString().toLowerCase())],['desc']);
        }        
      }
}