import { Directive, ElementRef, HostListener } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { NgControl } from '@angular/forms';

@Directive({
    selector: '[autoFormatCurrency]',
})
export class AutoFormatCurrencyDirective {
    currentValue: any = null;

    currencyCode: string = '';
    digitNumber = '.0-3';
    private el: HTMLInputElement;

    private ngControl: NgControl;

    constructor(
        private currencyPipe: CurrencyPipe,
        private _elementRef: ElementRef,
        private _ngControl: NgControl
    ) {
        this.el = this._elementRef.nativeElement;
        this.ngControl = this._ngControl;
    }

    ngOnInit(): void {
        setTimeout(() => {
            this.currentValue = this.el.value;
            this.el.value = this.currencyPipe.transform(this.el.value, this.currencyCode, '', this.digitNumber);
        }, 100);
    }

    @HostListener("focus", ["$event.target.value"])
    onFocus(value) {
        this.el.value = this.currentValue.toString(); // opossite of transform
    }

    @HostListener("blur", ["$event.target.value"])
    onBlur(value) {
        this.currentValue = value;
        this.el.value = this.currencyPipe.transform(value, this.currencyCode, '', this.digitNumber);
    }
}
