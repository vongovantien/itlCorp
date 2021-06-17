import { Directive, ElementRef, Input } from '@angular/core';
import { NgControl } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { takeUntil } from 'rxjs/operators';
import { DestroyService } from '@services';

@Directive({
    selector: '([formControl])[formatDecimalFormControl],([formControlName])[formatDecimalFormControl]',
    providers: [DecimalPipe, DestroyService]
})
export class FormatDecimalFormControlDirective {

    @Input() set(format: string) {
        this._format = format;
    }

    private _format = '.0-3';

    get format() {
        return this._format;
    }
    constructor(
        private ngControl: NgControl,
        private decimalPipe: DecimalPipe,
        private _el: ElementRef<any>,
        private _destroyService: DestroyService
    ) {
    }

    ngOnInit() {
        this.ngControl.valueChanges
            .pipe(takeUntil(this._destroyService))
            .subscribe(
                (value: string) => {
                    this._el.nativeElement.value = this.decimalPipe.transform(value, this.format);
                }
            );
    }
}