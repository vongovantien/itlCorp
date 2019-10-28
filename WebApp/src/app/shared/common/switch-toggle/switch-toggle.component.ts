import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-switch',
    templateUrl: './switch-toggle.component.html',
})
export class SwitchToggleComponent {

    @Input() toggle: boolean = false;
    @Input() class: string = 'success';
    @Output() toggleChange: EventEmitter<boolean> = new EventEmitter<boolean>();
    constructor() { }

    ngOnInit(): void { }
}
