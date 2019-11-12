import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'filter', pure: true })
export class FilterPipe implements PipeTransform {
    transform(sources: any[], args: RegExp, keys: string[]): any {
        const searchText = new RegExp(args, 'ig');
        if (!!sources.length && !!keys.length) {
            const data = sources.filter((item: any) => {
                for (const key of keys) {
                    if (item.hasOwnProperty(key) && !!item[key]) {
                        if (item[key].toString().search(searchText) === -1) {
                            continue;
                        }
                        return item[key].toString().search(searchText) !== -1;
                    }
                }
            });
            return data;
        } 
        return sources;
    }
}
