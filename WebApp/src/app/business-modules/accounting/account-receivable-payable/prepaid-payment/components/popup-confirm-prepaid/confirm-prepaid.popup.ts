import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { InjectViewContainerRefDirective } from '@directives';
import { PopupBase } from 'src/app/popup.base';

@Component({
    selector: 'confirm-prepaid-popup',
    templateUrl: './confirm-prepaid.popup.html',
})
export class ARPrePaidPaymentConfirmPopupComponent extends PopupBase implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    @Output() onConfirm: EventEmitter<any> = new EventEmitter<any>();

    groupData: any[] = [];

    sumAmountUSD: number = 0;
    sumAmountVND: number = 0;

    constructor() {
        super();
    }

    ngOnInit(): void { }

    submitConfirm() {
        if (!this.groupData.length) {
            return;
        }
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: `Are you sure you want to confirm paid ?`,
            iconConfirm: 'la la-cloud-upload',
            labelConfirm: 'Yes',
            center: true
        }, () => {
            const flattenArr = this.utility.deepFlatten(this.groupData.map(x => x.details.map(x => x)));
            this.onConfirm.emit(flattenArr);
            this.hide();
        });
    }

    deletegroupItem(groupIndex: number) {
        this.groupData.splice(groupIndex, 1);
        this.calculateTotalAmountAfterSelectItems();

        return;
    }

    deleteDebitnoteItem(groupIndex: number, itemIndex: number) {
        if (this.groupData[groupIndex].details.length === 1) {
            this.groupData.splice(groupIndex, 1);
            this.calculateTotalAmountAfterSelectItems();

            return;
        }
        this.groupData[groupIndex].details.splice(itemIndex, 1);

        this.calculateTotalAmountAfterSelectItems();

    }

    calculateTotalAmountAfterSelectItems() {
        const flattenArr = this.utility.deepFlatten(this.groupData.map(x => x.details.map(x => x)));

        this.sumAmountVND = 0;
        this.sumAmountUSD = 0;
        flattenArr.reduce((acc, curr) => this.sumAmountVND += curr.totalAmountVND, 0);
        flattenArr.reduce((acc, curr) => this.sumAmountUSD += curr.totalAmountUSD, 0);
    }
}
