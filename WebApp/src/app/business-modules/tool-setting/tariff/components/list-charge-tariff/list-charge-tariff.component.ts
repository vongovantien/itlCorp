import { Component, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { TariffChargePopupComponent } from '../popup/tariff-charge/tariff-charge.popup';
import { TariffCharge } from 'src/app/shared/models';

@Component({
    selector: 'list-charge-tariff',
    templateUrl: './list-charge-tariff.component.html',
})
export class TariffListChargeComponent extends AppList {

    @Input() charges: any[] = [];
    @Output() chargesChange: EventEmitter<any[]> = new EventEmitter<any[]>();

    @ViewChild(TariffChargePopupComponent, { static: false }) tariffChargePopup: TariffChargePopupComponent;

    headers: CommonInterface.IHeaderTable[];

    selectedTariffCharge: TariffCharge = new TariffCharge();

    constructor() {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Route', field: 'route', sortable: true },
            { title: 'Comondities Group', field: 'commodityName', sortable: true },
            { title: 'Payer', field: 'payerName', sortable: true },
            { title: 'Range From', field: 'rangeFrom', sortable: true },
            { title: 'Range To', field: 'rangeTo', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Min', field: 'min', sortable: true },
            { title: 'Max', field: 'max', sortable: true },
            { title: 'Next Unit', field: 'nextUnit', sortable: true },
            { title: 'Next Unit Price', field: 'nextUnitPrice', sortable: true },
            { title: 'Unit', field: 'unitId', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: 'Location Clearance', field: 'portName', sortable: true },
            { title: 'WareHouse', field: 'warehouseName', sortable: true },
            { title: 'Curerncy', field: 'currencyId', sortable: true },
        ];

    }

    openAddChargePopupTariff() {
        this.selectedTariffCharge = new TariffCharge();

        this.tariffChargePopup.formChargeTariff.value.tariffChargeDetail.currencyId = this.tariffChargePopup.currencyList.filter(i => i.id === 'VND')[0];
        this.tariffChargePopup.show();
    }

    showListChargeTariff() {
        console.log(this.charges);
    }

    onChangeChargeTariff(tariffCharge?: TariffCharge) {
        this.charges.push(tariffCharge);
        console.log(this.charges);
    }
}
