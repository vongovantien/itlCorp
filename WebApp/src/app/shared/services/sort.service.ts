// import * as lodash from 'lodash';
import orderBy from 'lodash/orderBy'
import deburr from 'lodash/deburr'
export class SortService {
    sort(items: any[], property: string, isDesc: boolean){       
        if(isDesc){
           return orderBy(items,[item => (!item[property]?null:this.prepareStringToSort(item[property]))],['asc']);
        }else{
            return orderBy(items,[item => (!item[property]?null:this.prepareStringToSort(item[property]))],['desc']);
        }        
      }

    // sort1(items: any[], property: string, isDesc: boolean){
    //     var listItems = items.map(a=>a[property]);
    //     console.log(listItems);
    //     var finalResult:any=[];
    //     var returnList:any[]=[];

    //     if(isDesc){
    //         var results:any[] = this.letterSort("en",listItems);

    //         for(var i=0;i<=results.length;i++){
    //             items.forEach(element => {
    //                 if(element[property]==results[i]){
    //                     returnList.push(element);
    //                 }
    //             });
    //         }
    //         console.log(returnList); 
    //      }else{
    //         var results:any[] = this.letterSort("en",listItems).reverse();
  
    //         for(var i=0;i<=results.length;i++){
    //             items.forEach(element => {
    //                 if(element[property]==results[i]){
    //                     returnList.push(element);
    //                 }
    //             });
    //         }
    //         console.log(returnList);           
    //      }
    //      return returnList;
    // }

    private prepareStringToSort(str:any){
        if(typeof str === "string"){
            str = str.toString().toLowerCase();
            return  deburr(str);
        }
        if(typeof str === "number"){
            return str
        }
       
    }

    private letterSort(lang:any, letters:any) {
        letters.sort(new Intl.Collator(lang).compare);
        return letters;
      }

}


