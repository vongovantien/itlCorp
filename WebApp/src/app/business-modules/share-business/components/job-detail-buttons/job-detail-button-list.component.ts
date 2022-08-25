import { Store } from '@ngrx/store';
import { getMenuUserSpecialPermissionState } from '@store';
import { AppPage } from 'src/app/app.base';
import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectionStrategy, ViewEncapsulation } from '@angular/core';
import * as fromShareBussiness from '../../store/index';
@Component({
    selector: 'job-detail-button-list',
    templateUrl: './job-detail-button-list.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})

export class ShareBussinessJobDetailButtonListComponent extends AppPage implements OnInit {

    @Input() shipmentDetail: any;
    @Input() isDuplicate: boolean = false;
    @Input() selectedTab: string = 'SHIPMENT';

    @Output() onLock: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPreviewPL: EventEmitter<any> = new EventEmitter<any>();
    @Output() onDuplicate: EventEmitter<any> = new EventEmitter<any>();
    @Output() onDelete: EventEmitter<any> = new EventEmitter<any>();
    @Output() onSave: EventEmitter<any> = new EventEmitter<any>();
    @Output() onCancel: EventEmitter<any> = new EventEmitter<any>();
    @Output() onFinish: EventEmitter<any> = new EventEmitter<any>();
    @Output() onReopen: EventEmitter<any> = new EventEmitter<any>();

    constructor(private _store: Store<fromShareBussiness.IShareBussinessState>,) { super(); }

    ngOnInit() {
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
    }

    lockShipment() {
        this.onLock.emit();
    }

    previewPLsheet(currency: string) {
        this.onPreviewPL.emit(currency);
    }

    showDuplicateConfirm() {
        this.onDuplicate.emit()
    }

    prepareDeleteJob() {
        this.onDelete.emit();
    }

    onSaveJob() {
        this.onSave.emit();
    }

    handleCancelForm() {
        this.onCancel.emit();
    }

    onFinishJob() {
        this.onFinish.emit()
    }

    onReopenJob() {
        this.onReopen.emit()
    }
}
