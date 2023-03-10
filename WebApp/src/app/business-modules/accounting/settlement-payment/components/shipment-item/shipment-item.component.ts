import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { ReportPreviewComponent } from '@common';
import { SysImage } from '@models';
import { Store } from '@ngrx/store';
import { takeUntil } from 'rxjs/operators';
import { AppPage } from 'src/app/app.base';
import { ISettlementPaymentState, getListEdocState } from '../store';
import { SettlementShipmentAttachFilePopupComponent } from './../popup/shipment-attach-files/shipment-attach-file-settlement.popup';

@Component({
    selector: 'shipment-item',
    templateUrl: './shipment-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class SettlementShipmentItemComponent extends AppPage {
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild(SettlementShipmentAttachFilePopupComponent) shipmentAttachFilePopup: SettlementShipmentAttachFilePopupComponent;

    @Output() onCheck: EventEmitter<any> = new EventEmitter<any>();
    @Output() onClick: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPrintPlUSD: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPrintPlVND: EventEmitter<any> = new EventEmitter<any>();
    @Output() onViewFiles: EventEmitter<any> = new EventEmitter<any>();

    @Input() data: ISettlementShipmentGroup = null;

    initCheckbox: boolean = false;
    isCheckAll: boolean = false;
    countFile: number = 0;

    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }

    private _readonly: boolean = false;

    get readonly(): boolean {
        return this._readonly;
    }

    constructor(
        private _store: Store<ISettlementPaymentState>,
    ) {
        super();
    }

    ngOnInit() {
        console.log(this.data);
        this._store.select(getListEdocState).pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any) => {
                    console.log(res.filter(x => x.jobNo === null || x.jobNo === this.data.jobId));

                    this.countFile = res.filter(x => x.jobNo === null || x.jobNo === this.data.jobId).length;
                }
            );
    }

    showPaymentManagement($event: Event): any {
        this.onClick.emit();
        return false;
    }

    checkUncheckAllRequest($event: Event) {
        this.initCheckbox = !this.initCheckbox;
        this.isCheckAll = this.initCheckbox;
        this.onCheck.emit(this.isCheckAll);

        return false;
    }

    previewPLsheet($event: Event, currency: string) {
        if (currency === 'VND') {
            this.onPrintPlVND.emit();
            return false;
        }
        this.onPrintPlUSD.emit();
        return false;
    }

    showShipmentAttachFile($event: Event) {
        $event.stopPropagation();
        $event.preventDefault();

        this.onViewFiles.emit();

        return false;
    }
}

export interface ISettlementShipmentGroup {
    advanceAmount: number;
    advanceNo: string;
    balance: number;
    chargeSettlements: any[]
    currencyShipment: string;
    customNo: string;
    hbl: string;
    hblId: string;
    jobId: string;
    mbl: string;
    settlementNo: string;
    shipmentId: string;
    totalAmount: number;
    type: string;
    isSelected?: boolean;
    files: SysImage[];
    isLocked: boolean;
}
