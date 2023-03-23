import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: 'quantityRange'
})

export class QuantityRangePipe implements PipeTransform {
    transform(range: [number | null, number | null]): string {
        if (range[0] === null && range[1] === null) {
            return '';
        }
        if (range[0] === null) {
            return `< ${range[1]}`;
        }
        if (range[1] === null) {
            return `> ${range[0]}`;
        }
        return `${range[0]} - ${range[1]}`;
    }
}