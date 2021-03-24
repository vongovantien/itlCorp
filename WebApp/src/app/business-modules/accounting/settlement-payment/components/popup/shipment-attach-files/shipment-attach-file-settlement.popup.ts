import { AccountingRepo } from '@repositories';
import { Component, OnInit, Input, ViewChild, Output, EventEmitter } from '@angular/core';
import { PopupBase } from '@app';
import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { SysImage } from '@models';
import { ToastrService } from 'ngx-toastr';
import { InjectViewContainerRefDirective } from '@directives';
import { ISettlementShipmentGroup } from '../../shipment-item/shipment-item.component';

@Component({
    selector: 'shipment-attach-file',
    templateUrl: './shipment-attach-file-settlement.popup.html',
    styleUrls: ['./../../attach-file/attach-file-list-settlement.component.scss']
})

export class SettlementShipmentAttachFilePopupComponent extends PopupBase implements OnInit {

    @ViewChild(InjectViewContainerRefDirective) confirmPopupContainerRef: InjectViewContainerRefDirective;
    @Output() onChange: EventEmitter<SysImage[]> = new EventEmitter<SysImage[]>();

    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }

    get readonly(): boolean {
        return this._readonly;
    }

    private _readonly: boolean = false;

    private files: SysImage[] = [];
    private selectedFile: SysImage;

    settlementId: string;
    hblId: string;

    shipmentGroups: ISettlementShipmentGroup = null;

    constructor(
        private _accountingRepo: AccountingRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() { }

    chooseFile() {
        if (this._readonly) {
            return;
        }
        const fileList: FileList[] = event.target['files'];
        console.log(this.shipmentGroups);
        if (fileList.length > 0 && !!this.settlementId && !!this.shipmentGroups) {
            const folderChild = this.generateChild(this.shipmentGroups);
            this._accountingRepo.uploadAttachedFiles("Settlement", this.settlementId, fileList, folderChild)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.getFiles(this.settlementId, this.hblId);
                        }
                    }
                );
        }
    }

    getFiles(settlementId: string, hblId: string) {
        this._accountingRepo.getAttachedFiles('Settlement', hblId)
            .subscribe(
                (data: any) => {
                    this.files = data || [];
                    this.onChange.emit(this.files);
                }
            )
    }

    deleteFile() {

    }

    generateChild(shipmentGroups: ISettlementShipmentGroup): string {
        let child: string = '';

        const grp = {
            folder1: shipmentGroups.hblId,
            folder2: shipmentGroups.advanceNo,
            folder3: shipmentGroups.customNo
        };
        const c = Object.entries(grp);
        console.log(c);

        c.forEach((item: string[]) => {
            if (!!item[1]) {
                child += "/" + item[1];
            }
        })
        return child;
    }
}
