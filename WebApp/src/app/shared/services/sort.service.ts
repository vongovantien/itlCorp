import orderBy from 'lodash/orderBy';
import deburr from 'lodash/deburr';
import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class SortService {
    sort(items: any[], property: string, isDesc: boolean) {
        if (isDesc) {
            return orderBy(items, [item => (!item[property] ? null : this.prepareStringToSort(item[property]))], ['asc']);
        } else {
            return orderBy(items, [item => (!item[property] ? null : this.prepareStringToSort(item[property]))], ['desc']);
        }
    }

    private prepareStringToSort(str: any) {
        if (typeof str === "string") {
            const x = Number("1000");
            if (isNaN(x)) {
                return x;
            } else {
                str = str.toString().toLowerCase();
                return deburr(str);
            }
        }
        if (typeof str === "number") {
            return str;
        }

    }
}


