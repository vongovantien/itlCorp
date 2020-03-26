import { HostListener, Directive, Input } from "@angular/core";

@Directive({
    selector: '[NoDbClick]'
})
export class NoDblClickDirective {
    @Input() timer: number = 1000; // * delay 1s.
    constructor(
    ) { }

    @HostListener('click', ['$event'])
    clickEvent(event: any) {
        event.srcElement.setAttribute('disabled', true);
        setTimeout(function () {
            event.srcElement.removeAttribute('disabled');
        }, this.timer);
    }
}
