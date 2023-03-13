import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { PopupBase } from '@app';
import { ConfirmPopupComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { SysImage } from '@models';
import { Store } from '@ngrx/store';
import { SystemFileManageRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { finalize, takeUntil } from 'rxjs/operators';
import { ISettlementShipmentGroup } from '../../shipment-item/shipment-item.component';
import { ISettlementPaymentState } from '../../store';
import { getSettlementPaymentDetailLoadingState, getSettlementPaymentDetailState } from '../../store/reducers/index';

@Component({
    selector: 'shipment-attach-file',
    templateUrl: './shipment-attach-file-settlement.popup.html',
    styleUrls: ['./../../../../components/attach-file/attach-file-list.component.scss']
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

    files: SysImage[] = [];
    settlementId: string;
    settlementNo: string;

    shipmentGroups: ISettlementShipmentGroup = null;

    constructor(
        private _fileRepo: SystemFileManageRepo,
        private _toastService: ToastrService,
        private _store: Store<ISettlementPaymentState>,
    ) {
        super();
    }

    ngOnInit() {
        this.isLoading = this._store.select(getSettlementPaymentDetailLoadingState).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res) => {
                if (!res) {
                    console.log(res);
                    this._store.select(getSettlementPaymentDetailState)
                        .pipe(takeUntil(this.ngUnsubscribe))
                        .subscribe((res) => {
                            if (res) {
                                this.settlementId = res?.settlement.id;
                                this.settlementNo = res?.settlement.settlementNo;
                            }
                        })
                }
            })
    }

    chooseFile() {
        if (this._readonly) {
            return;
        }
        const fileList: FileList[] = event.target['files'];
        if (fileList.length > 0 && !!this.settlementId && !!this.shipmentGroups) {
            const folderChild = this.generateChild(this.shipmentGroups);
            this._fileRepo.uploadAttachedFiles("Settlement", this.settlementId, fileList, folderChild)
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success("Upload file successfully!");
                            this.getFiles(this.settlementId, folderChild);
                        }
                    }
                );
        }
    }

    getFiles(settlementId: string, folderChild: string) {
        this.isLoading = true;
        this._fileRepo.getAttachedFiles('Settlement', settlementId, folderChild)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe(
                (data: any) => {
                    this.files = data || [];
                    this.onChange.emit(this.files);

                }
            )
    }

    onDeleteFile(id: string) {
        this._fileRepo.deleteAttachedFile('Settlement', id)
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this.getFiles(this.settlementId, this.generateChild(this.shipmentGroups));
                        this._toastService.success(res.message);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            )
    }

    deleteFile(file: SysImage) {
        if (this._readonly) {
            return;
        }
        this.showPopupDynamicRender<ConfirmPopupComponent>(
            ConfirmPopupComponent,
            this.confirmPopupContainerRef.viewContainerRef, {
            body: 'Do you want to delete this file ?',
            labelConfirm: 'Yes',
            labelCancel: 'No',
            iconConfirm: 'la la-trash'

        }, () => {
            this.onDeleteFile(file.id)
        })
    }

    generateChild(shipmentGroups: ISettlementShipmentGroup): string {
        let child: string = '';

        const grp = {
            folder1: shipmentGroups.hblId,
            folder2: shipmentGroups?.advanceNo,
            folder3: shipmentGroups?.customNo
        };

        Object.entries(grp).forEach((item: string[]) => {
            if (!!item[1]) {
                child += item[1] + '/';
            }
        })
        return child;
    }
    dowloadAllAttach() {
        if (this.settlementNo) {
            let arr = this.settlementNo.split("/");
            let model = {
                folderName: 'Settlement',
                objectId: this.settlementId,
                chillId: this.generateChild(this.shipmentGroups),
                fileName: arr[0] + "_" + arr[1] + ".zip"
            }
            this._fileRepo.dowloadallAttach(model)
                .subscribe(
                    (res: any) => {
                        this.downLoadFile(res, "application/zip", model.fileName);
                    }
                )
        }
    }
}
