import { Directive, ElementRef, HostListener, Renderer2 } from '@angular/core';
import { CurrencyPipe } from '@angular/common';

@Directive({
    selector: '[autoFormatCurrency]',
})
export class AutoFormatCurrencyDirective {
    currentValue: any = null;

    currencyCode: string = '';
    digitNumber = '.0-3';
    private el: HTMLInputElement;

    // private ngControl: NgControl;
    timeOut: any;
    constructor(
        private currencyPipe: CurrencyPipe,
        private _elementRef: ElementRef,
        // private _ngControl: NgControl,
        private renderer: Renderer2

    ) {
        this.el = this._elementRef.nativeElement;
        // this.ngControl = this._ngControl;
    }

    ngOnInit(): void {
        if (!this._elementRef.nativeElement.getAttribute('type') || this._elementRef.nativeElement.getAttribute('type') === 'number') {
            this.renderer.setAttribute(this._elementRef.nativeElement, 'type', 'text');
        }

        this.timeOut = setTimeout(() => {
            this.currentValue = this.el.value;
            this.el.value = this.currencyPipe.transform(this.el.value, this.currencyCode, '', this.digitNumber);
        }, 1000);
    }

    @HostListener("focus", ["$event.target.value"])
    onFocus(value) {
        this.el.value = this.currentValue + ''.toString(); // opossite of transform
    }

    @HostListener("blur", ["$event.target.value"])
    onBlur(value) {
        this.currentValue = value;
        this.el.value = this.currencyPipe.transform(value, this.currencyCode, '', this.digitNumber);
    }

    ngOnDestroy(): void {
        clearTimeout(this.timeOut);
    }
}
