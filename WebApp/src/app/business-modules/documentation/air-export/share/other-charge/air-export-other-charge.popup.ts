import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'air-export-other-charge-popup',
    templateUrl: './air-export-other-charge.popup.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ShareAirExportOtherChargePopupComponent extends PopupBase implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }

    closePopup() {
        this.hide();
    }

    onSave() {

    }
}
