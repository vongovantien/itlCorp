import { PopupBase } from "src/app/popup.base";
import { Component, Output, EventEmitter, Input } from "@angular/core";
import { DocumentationRepo } from "src/app/shared/repositories";
import { catchError } from "rxjs/operators";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'input-shipment-popup',
    templateUrl: './input-shipment.popup.html'
})
export class ShareAccountingInputShipmentPopupComponent extends PopupBase {
    @Input() set type(t: string) {
        this._type = t;
        console.log(this._type)
        if (this._type !== 'soa') {
            this.shipmentTypes = [
                { text: 'JOB ID', id: 'JOBID' },
                { text: 'MBL/MAWB', id: 'MBL' },
                { text: 'HAWB/HBL', id: 'HBL' },
                { text: 'Custom No', id: 'CustomNo' }
            ];
        }
    }

    @Output() onInputShipment: EventEmitter<any> = new EventEmitter<any>();

    private _type: string = 'soa';

    shipmentTypes = [
        { text: 'JOB ID', id: 'JOBID' },
        { text: 'MBL/MAWB', id: 'MBL' },
        { text: 'HAWB/HBL', id: 'HBL' },
    ];
    selectedShipmentType: string = '';
    shipmentSearch: string = '';

    constructor(
        private _documentRepo: DocumentationRepo,
        private _toastService: ToastrService) {
        super();

        this.selectedShipmentType = "JOBID";
    }

    ngOnInit() { }

    onChangeShipmentType(shipmentType: any) {
        this.selectedShipmentType = shipmentType.id;
    }

    Ok() {
        let data: OperationInteface.IInputShipment = null;
        if (this.shipmentSearch.length > 0) {
            data = {
                type: this.selectedShipmentType,
                keyword: this.shipmentSearch,
            };
            const keywords: string[] = this.shipmentSearch.split(/\n/).filter(item => item.trim() !== '').map(item => item.trim());
            this._documentRepo.GetShipmentNotExist(this.selectedShipmentType, keywords)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (res: any) => {
                        if (res.status) {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
        this.onInputShipment.emit(data);
        this.hide();
    }

    closePopup() {
        this.hide();
        let data: OperationInteface.IInputShipment = null;
        if (this.shipmentSearch.length > 0) {
            data = {
                type: this.selectedShipmentType,
                keyword: this.shipmentSearch,
            };
        }
        this.onInputShipment.emit(data);
    }
}
