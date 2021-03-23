import { Component, OnInit, Input } from '@angular/core';
import { PopupBase } from '@app';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { SysImage } from '@models';

@Component({
    selector: 'shipment-attach-file',
    templateUrl: './shipment-attach-file-settlement.popup.html',
    styleUrls: ['./../../attach-file/attach-file-list-settlement.component.scss']
})

export class SettlementShipmentAttachFilePopupComponent extends PopupBase implements OnInit {
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }

    get readonly(): boolean {
        return this._readonly;
    }

    private _readonly: boolean = false;

    files: SysImage[] = [];
    selectedFile: SysImage;
    accepctFilesUpload: string = ''
    constructor() {
        super();
    }

    ngOnInit() { }

    chooseFile() {

    }

    deleteFile() {

    }
}