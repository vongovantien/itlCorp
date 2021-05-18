import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'sortTableClass' })
export class SortTableClassPipe implements PipeTransform {
    transform(sortable: boolean, field: string, key: string, order: boolean = false): any {
        if (!!sortable) {
            let classes = 'sortable ';
            if (field === key) {
                classes += ('sort-' + (order ? 'asc' : 'desc') + ' ');
            }

            return classes;
        }
        return '';
    }
}