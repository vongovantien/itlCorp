import { Component, OnInit, Output, EventEmitter, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { ToastrService } from 'ngx-toastr';
import { CommonEnum } from '@enums';
import { formatDate } from '@angular/common';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'unlock-shipment',
    templateUrl: './unlock-shipment.component.html',
    styleUrls: ['./unlock-shipment.component.scss']
})

export class UnlockShipmentComponent extends AppForm implements OnInit {

    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;

    options: CommonInterface.INg2Select[] = [
        { id: 1, text: 'Job ID' },
        { id: 2, text: 'MBL' },
        { id: 3, text: 'HBL' },
    ];
    selectedOption: CommonInterface.INg2Select[] = [this.options[0]];

    services: CommonInterface.INg2Select[] = [
        { id: 'All', text: 'All' },
        { id: CommonEnum.TransactionTypeEnum.AirExport, text: 'Air Export' },
        { id: CommonEnum.TransactionTypeEnum.AirImport, text: 'Air Import' },
        { id: CommonEnum.TransactionTypeEnum.CustomLogistic, text: 'Custom logistic' },
        { id: CommonEnum.TransactionTypeEnum.SeaFCLExport, text: 'Sea FCL Export' },
        { id: CommonEnum.TransactionTypeEnum.SeaFCLImport, text: 'Sea FCL Import' },
        { id: CommonEnum.TransactionTypeEnum.SeaLCLExport, text: 'Sea LCL Export' },
        { id: CommonEnum.TransactionTypeEnum.SeaLCLImport, text: 'Sea LCL Import' },
    ];
    selectedService: CommonInterface.INg2Select[] = [this.services[0]];

    isSelectOption: boolean = true;
    isSelectServiceDate: boolean = false;

    lockHistory: string;

    constructor(
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() {
        // * default disabled serviceDate.
        this.isDisabled = true;
    }

    onSelectOptionChange(e: any) {
        if (e.target.checked) {
            this.isSelectOption = true;
            this.isSelectServiceDate = false;

            this.selectedOption = [this.options[0]];
            this.isDisabled = true;
            this.selectedRange = null;

        } else {
            this.isSelectOption = false;
        }
    }

    onSelectServiceDate(e: any) {
        if (e.target.checked) {
            this.isSelectServiceDate = true;
            this.isSelectOption = false;

            this.isDisabled = null;
            this.keyword = '';
            this.selectedService = [this.services[0]];


        } else {
            this.isSelectServiceDate = false;
        }
    }

    unlock() {
        if (this.isSelectOption) {
            if (!this.keyword.trim()) {
                this._toastService.warning("Please input keyword");
                return;
            }
        }
        const body: IShipmentLock = {
            shipmentPropertySearch: this.selectedOption[0].id,
            keywords: !!this.keyword ? this.keyword.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            fromDate: !!this.selectedRange && !!this.selectedRange.startDate ? formatDate(this.selectedRange.startDate, 'yyyy-MM-dd', 'en') : null,
            toDate: !!this.selectedRange && !!this.selectedRange.endDate ? formatDate(this.selectedRange.endDate, 'yyyy-MM-dd', 'en') : null,
            transactionType: this.selectedService[0].id !== 'All' ? this.selectedService[0].id : null
        };

        if (!!body) {
            this.confirmPopup.show();
        }

        console.log(body);
    }
}

interface IShipmentLock {
    shipmentPropertySearch: number;
    keywords: string[];
    transactionType: number;
    fromDate: string;
    toDate: string;
}

