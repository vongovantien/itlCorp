import { Directive, Renderer2, ElementRef } from '@angular/core';

@Directive({ selector: '[calendar]' })
export class IconCalendarDirective {

    constructor(
        private renderer: Renderer2,
        private el: ElementRef,
    ) { }

    ngOnInit(): void {
        const parent: HTMLElement = this.el.nativeElement.parentElement;

        const spanInputIconWrapper: HTMLElement = this.renderer.createElement('span');
        const spanInner: HTMLElement = this.renderer.createElement('span');
        const i: HTMLElement = this.renderer.createElement('i');

        this.renderer.addClass(spanInputIconWrapper, "m-input-icon__icon");
        this.renderer.addClass(spanInputIconWrapper, "m-input-icon__icon--right");
        this.renderer.addClass(i, "la");
        this.renderer.addClass(i, "la-calendar");

        this.renderer.appendChild(parent, spanInputIconWrapper);
        this.renderer.appendChild(spanInputIconWrapper, spanInner);
        this.renderer.appendChild(spanInner, i);

    }

}