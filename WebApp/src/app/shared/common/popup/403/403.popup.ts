import { Component, ChangeDetectionStrategy } from '@angular/core';
import { eFMSPopup } from '../popup';

@Component({
    selector: 'permission-403-popup',
    templateUrl: './403.popup.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class Permission403PopupComponent extends eFMSPopup {
    constructor() {
        super();
    }

    ngOnInit(): void { }

}