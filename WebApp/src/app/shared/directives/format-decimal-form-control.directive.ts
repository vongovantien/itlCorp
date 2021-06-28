import { Directive, ElementRef, HostListener, Input, Renderer2 } from '@angular/core';
import { NgControl } from '@angular/forms';
import { DecimalPipe } from '@angular/common';
import { takeUntil } from 'rxjs/operators';
import { DestroyService } from '@services';

@Directive({
    selector: '([formControl])[formatDecimalFormControl],([formControlName])[formatDecimalFormControl]',
    providers: [DecimalPipe, DestroyService]
})
export class FormatDecimalFormControlDirective {
    @Input() set minimum(c: string) { this.minValue = c; }
    
    minValue: string = null;
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
                    if (!!value) {
                        value = this.formatNumber(value);
                        if (value.indexOf('.', value.length - 1) === -1) {
                            this._el.nativeElement.value = this.decimalPipe.transform(value, '1.0-3');
                        }
                    }
                }
            );
    }

    private formatNumber(value: any) {
        if(!!value){
            value = value.toString().replace(/[^-0-9.]+/g, '');
        }
        return this.check(value);
    }

    private check(input) {
        if (!!this.minValue && !isNaN(input)) {
            input = input < this.minValue ? null : input;
        }
        if(!input){
            return input;
        }
        
        var index = input.indexOf('.');      
        if ( index > -1 ) {
            input = input.substr( 0, index + 1 ) + input.slice( index ).replace( /\./g, '' );
        }
    
        return input;
    }
}