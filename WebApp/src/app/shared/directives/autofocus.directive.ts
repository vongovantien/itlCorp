import { Directive, Input, ElementRef } from '@angular/core';

@Directive({
	selector : '[focus]'
})
export class FocusDirective {
	@Input()
	focus : boolean;

	constructor(private element : ElementRef) {
	}

	public ngAfterContentInit() {

        setTimeout(() => {

            this.element.nativeElement.focus();

        }, 500);

    }
}