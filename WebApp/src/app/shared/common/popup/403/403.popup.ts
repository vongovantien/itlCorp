import { Component, ChangeDetectionStrategy, Input, EventEmitter, Output } from '@angular/core';
import { eFMSPopup } from '../popup';

@Component({
    selector: 'permission-403-popup',
    templateUrl: './403.popup.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class Permission403PopupComponent extends eFMSPopup {
    @Input() center: boolean = false;
    @Output() onSubmit: EventEmitter<boolean> = new EventEmitter<boolean>();

    constructor() {
        super();
    }

    ngOnInit(): void { }

    close() {
        this.hide();
        this.onSubmit.emit(true);
    }

}