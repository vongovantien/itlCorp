import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { ReportPreviewComponent } from '@common';
import { SysImage } from '@models';
import { Store } from '@ngrx/store';
import { takeUntil } from 'rxjs/operators';
import { AppPage } from 'src/app/app.base';
import { ShareDocumentTypeAttachComponent } from 'src/app/business-modules/share-business/components/edoc/document-type-attach/document-type-attach.component';
import { ISettlementPaymentState, getSettlementPaymentDetailState } from '../store';
import { SettlementShipmentAttachFilePopupComponent } from './../popup/shipment-attach-files/shipment-attach-file-settlement.popup';

@Component({
    selector: 'shipment-item',
    templateUrl: './shipment-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class SettlementShipmentItemComponent extends AppPage {
    @ViewChild(ReportPreviewComponent) previewPopup: ReportPreviewComponent;
    @ViewChild(SettlementShipmentAttachFilePopupComponent) shipmentAttachFilePopup: SettlementShipmentAttachFilePopupComponent;
    @ViewChild(ShareDocumentTypeAttachComponent) documentAttach: ShareDocumentTypeAttachComponent;

    @Output() onCheck: EventEmitter<any> = new EventEmitter<any>();
    @Output() onClick: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPrintPlUSD: EventEmitter<any> = new EventEmitter<any>();
    @Output() onPrintPlVND: EventEmitter<any> = new EventEmitter<any>();
    @Output() onViewFiles: EventEmitter<any> = new EventEmitter<any>();

    @Input() data: ISettlementShipmentGroup = null;
    lstEdocExist: any[] = [];

    initCheckbox: boolean = false;
    isCheckAll: boolean = false;

    constructor(
        private _store: Store<ISettlementPaymentState>,
    ) {
        super();
    }

    ngOnInit() {
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
        this.documentAttach.headers = [
            { title: 'Alias Name', field: 'aliasName', width: 200 },
            { title: 'Real File Name', field: 'realFilename' },
            { title: 'Document Type', field: 'docType', required: true },
            { title: 'Payee', field: 'payee' },
            { title: 'Invoice No', field: 'invoiceNo' },
            { title: 'Series No', field: 'seriesNo' },
            { title: 'Job Ref', field: 'jobRef' },
            { title: 'Note', field: 'note' },
        ]

        this.documentAttach.isUpdate = false;
        this.documentAttach.jobOnSettle = true;
        this.documentAttach.jobNo = this.data.jobId;
        this.documentAttach.jobId = this.data.shipmentId;
        this._store.select(getSettlementPaymentDetailState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res) => {
                if (res) {
                    this.documentAttach.billingId = res.settlement.id;
                    this.documentAttach.billingNo = res.settlement.settlementNo
                    //this.documentAttach.getListEdocExist();
                }
            })
        this.documentAttach.show();

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
