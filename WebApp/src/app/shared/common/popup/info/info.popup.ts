import { Component, Input, EventEmitter, Output } from '@angular/core';
import { eFMSPopup } from '../popup';

@Component({
    selector: 'info-popup',
    templateUrl: 'info.popup.html',
    styleUrls: ['./info.popup.scss'],
})

export class InfoPopupComponent extends eFMSPopup {

    @Input() title: string = 'Notification';
    @Input() body: string = 'Default Info Popup';
    @Input() isShowButton: boolean = true;
    @Input() label: string = 'Ok';
    @Input() align: CommonType.DIRECTION = 'center';

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