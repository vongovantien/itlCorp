import { Component, Input, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'confirm-delete-popup',
    templateUrl: './confirm-delete.popup.html',
    styleUrls: ['./confirm-delete.popup.scss']
})
export class ConfirmDeletePopupComponent extends PopupBase {

    @Input() title: string = 'Notification';
    @Input() body: string = 'You are not allow to delete';
    @Input() labelConfirm: string = 'Yes';
    @Input() labelCancel: string = 'Cancel';

    @Output() onSubmit: EventEmitter<any> = new EventEmitter<any>();
    @Output() onCancel: EventEmitter<any> = new EventEmitter<any>();

    constructor() {
        super();
    }

    ngOnInit(): void { }

    onConfirm() {
        this.onSubmit.emit(true);
    }
}
