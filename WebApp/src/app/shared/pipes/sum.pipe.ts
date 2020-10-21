import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'sum' })
export class SumPipe implements PipeTransform {
    transform(items: any[], attr?: string): any {
        if (attr) {
            return items.reduce((a, b) => a + b[attr], 0);
        }
        return !Array.isArray(items) ? items : items.reduce((previous: number, current: number) => previous + current, 0);
    }
}