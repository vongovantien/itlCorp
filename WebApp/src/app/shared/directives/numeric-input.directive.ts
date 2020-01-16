import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
    selector: '[numeric]'
})

export class NumericDirective {
    @Input() decimals: number = 0;

    private specialKeys = [
        'Backspace', 'Tab', 'End', 'Home', 'ArrowLeft', 'ArrowRight', 'Delete'
    ];

    constructor(private el: ElementRef) {
    }

    private check(value: string, decimals: number) {
        if (decimals <= 0) {
            return String(value).match(new RegExp(/^\d+$/));
        } else {
            const regExpString = "^\\s*((\\d+(\\.\\d{0," + decimals + "})?)|((\\d*(\\.\\d{1," + decimals + "}))))\\s*$";
            return String(value).match(new RegExp(regExpString));
        }
    }
    // tslint:disable: deprecation
    @HostListener('keydown', ['$event'])
    onKeyDown(e: KeyboardEvent) {
        // if (this.specialKeys.indexOf(event.key) !== -1) {
        //     return;
        // }
        if (
            // Allow: Delete, Backspace, Tab, Escape, Enter
            [46, 8, 9, 27, 13].indexOf(e.keyCode) !== -1 ||
            (e.keyCode === 65 && e.ctrlKey === true) || // Allow: Ctrl+A
            (e.keyCode === 67 && e.ctrlKey === true) || // Allow: Ctrl+C
            (e.keyCode === 86 && e.ctrlKey === true) || // Allow: Ctrl+V
            (e.keyCode === 88 && e.ctrlKey === true) || // Allow: Ctrl+X
            (e.keyCode === 65 && e.metaKey === true) || // Cmd+A (Mac)
            (e.keyCode === 67 && e.metaKey === true) || // Cmd+C (Mac)
            (e.keyCode === 86 && e.metaKey === true) || // Cmd+V (Mac)
            (e.keyCode === 88 && e.metaKey === true) || // Cmd+X (Mac)
            (e.keyCode >= 35 && e.keyCode <= 39) // Home, End, Left, Right
        ) {
            return;
        }

        // Do not use event.keycode this is deprecated.
        // See: https://developer.mozilla.org/en-US/docs/Web/API/KeyboardEvent/keyCode
        const value: string = this.el.nativeElement.value;
        const next: string = value.concat(e.key);
        if (next && !this.check(next, this.decimals)) {
            e.preventDefault();
        }
    }
}
