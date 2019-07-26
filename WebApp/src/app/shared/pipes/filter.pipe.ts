import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'filter', pure: false })
export class FilterPipe implements PipeTransform {
    transform(sources: any[], args: RegExp, keys: string[]): any {
        const searchText = new RegExp(args, 'ig');
        if (!!sources.length) {
            return sources.filter((item: any) => {
                for (const key of keys) {
                    if (item.hasOwnProperty(key) && item[key].search(searchText) === -1) {
                        continue;
                    }
                    return item[key].search(searchText) !== -1;
                }
            });
        } return sources;
    }
}
