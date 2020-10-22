import { Directive, HostListener } from '@angular/core';
import { SelectComponent } from 'ng2-select';

@Directive({
    selector: '[ng2ControlTrigger]',
})
export class Ng2ControlTriggerDirective {

    @HostListener('keydown', ['$event'])
    onKeydownEnterHandler(e: KeyboardEvent) {
        if (e.ctrlKey) {
            this.openNg2Select(this._ng2);
        }
    }

    @HostListener('focus', ['$event'])
    onFocusHandle(e: any) {
        console.log(123);
    }

    constructor(private _ng2: SelectComponent) {
    }

    openNg2Select(ng2Select: SelectComponent) {
        setTimeout(() => {
            const ng2 = ng2Select.element.nativeElement.querySelector('.ui-select-toggle');
            if (!!ng2) {
                ng2.dispatchEvent(new Event('click'));
            }
        });
    }
}
