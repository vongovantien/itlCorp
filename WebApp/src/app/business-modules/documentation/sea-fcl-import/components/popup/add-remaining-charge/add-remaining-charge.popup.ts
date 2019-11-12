import { Component, Output, EventEmitter } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { ChargeCdNote } from "src/app/shared/models/document/chargeCdNote.model";

@Component({
    selector: 'add-remaining-charge-popup',
    templateUrl: './add-remaining-charge.popup.html'
})
export class CdNoteAddRemainingChargePopupComponent extends PopupBase {
    @Output() onAddCharge: EventEmitter<any> = new EventEmitter<any>();

    headers: CommonInterface.IHeaderTable[];
    isCheckAllCharge: boolean = false;
    listChargePartnerAddMore: ChargeCdNote[] = [];
    listChargePartner: ChargeCdNote[] = [];
    partner: string = "";
    constructor() {
        super();
    }

    ngOnInit() {
        this.headers = [
            { title: 'HBL No', field: 'hwbno', sortable: true },
            { title: 'Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'nameEn', sortable: true },
            { title: 'Quantity', field: 'quantity', sortable: true },
            { title: 'Unit', field: 'unit', sortable: true },
            { title: 'Unit Price', field: 'unitPrice', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'VAT', field: 'vatrate', sortable: true },
            { title: "Credit Value", field: 'total', sortable: true },
            { title: "Debit Value", field: 'total', sortable: true },
            { title: 'Note', field: 'notes', sortable: true }
        ];
    }

    addCharge() {
        console.log('add charge')
        console.log(this.listChargePartnerAddMore);
        let chargesSelected = [];
        let grpChargeSelected = [];
        if (this.listChargePartnerAddMore.length > 0) {
            for (const charges of this.listChargePartnerAddMore) {
                console.log(charges.listCharges.filter(group => group.isSelected))
                chargesSelected = charges.listCharges.filter(group => group.isSelected);
                if (chargesSelected.length > 0) {
                    grpChargeSelected.push({ id: charges.id, hwbno: charges.hwbno, listCharges: chargesSelected });
                }
            }
        }
        let result = [];
        console.log('grpChargeSelected')
        console.log(grpChargeSelected);
        for (const group of grpChargeSelected) {
            if (this.listChargePartner.length > 0) {
                for (const item of this.listChargePartner) {
                    if (item.hwbno == group.hwbno) {
                        for (const charge of group.listCharges) {
                            item.listCharges.push(charge)
                        }
                    }
                }
            } else {
                result.push({ id: group.id, hwbno: group.hwbno, listCharges: group.listCharges });
            }
        }
        console.log('result');
        console.log(result);
        this.listChargePartner = this.listChargePartner.length > 0 ? this.listChargePartner : result;
        console.log(this.listChargePartner);
        this.onAddCharge.emit(this.listChargePartner);
        
        this.isCheckAllCharge = false;
        this.closePopup();
    }

    closePopup() {
        this.isCheckAllCharge = false;
        this.hide();       
    }

    checkUncheckAllCharge() {
        for (const group of this.listChargePartnerAddMore) {
            group.isSelected = this.isCheckAllCharge;

            for (const item of group.listCharges) {
                item.isSelected = this.isCheckAllCharge;
            }
        }
    }

    onChangeCheckBoxItemCharge(chargeGroup: any) {
        chargeGroup.isSelected = chargeGroup.listCharges.every((item: any) => item.isSelected);
        this.isCheckAllCharge = this.listChargePartnerAddMore.every((item: any) => item.isSelected);
    }

}