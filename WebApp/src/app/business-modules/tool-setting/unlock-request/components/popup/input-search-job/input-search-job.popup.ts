import { PopupBase } from "src/app/popup.base";
import { Component, Output, EventEmitter } from "@angular/core";

@Component({
    selector: 'input-search-job-popup',
    templateUrl: './input-search-job.popup.html'
})
export class UnlockRequestInputSearchJobPopupComponent extends PopupBase {
    @Output() onInputJob: EventEmitter<any> = new EventEmitter<any>();
    shipmentTypes = [
        { text: 'JOB ID', id: 'JOBID' },
        { text: 'MBL', id: 'MBL' },
        { text: 'Custom No', id: 'CUSTOMNO' }
    ];
    selectedShipmentType: string = '';
    shipmentSearch: string = '';
    constructor(
    ) {
        super();
        this.selectedShipmentType = "JOBID";
    }

    ngOnInit() { }

    onChangeShipmentType(shipmentType: any) {
        this.selectedShipmentType = shipmentType.id;
    }

    add() {

    }

    closePopup() {
        this.hide();
    }
}