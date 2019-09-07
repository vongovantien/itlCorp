import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'searchStage' })
export class SearchStage implements PipeTransform {
    transform(sources: any[], args: RegExp): any {
        const searchText = new RegExp(args, 'ig');
        if (!!sources.length) {
            return sources.filter((item) => {
                if (!!item) {
                    return item.stageCode.search(searchText) !== -1
                        || item.stageNameEN.search(searchText) !== -1;
                } return item;
            });
        } return sources;
    }
}
