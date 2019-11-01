import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
    selector: 'app-switch',
    templateUrl: './switch-toggle.component.html',
})
export class SwitchToggleComponent {

    @Input() toggle: boolean = false;
    @Input() class: string = 'success';
    @Input() size: string = "md";
    @Output() toggleChange: EventEmitter<boolean> = new EventEmitter<boolean>();

    constructor() { }

    ngOnInit(): void { }

    onChangeToggle() {
        this.toggleChange.emit(this.toggle);
    }
}
