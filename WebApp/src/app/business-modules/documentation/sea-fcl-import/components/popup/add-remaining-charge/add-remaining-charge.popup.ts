import { Component } from "@angular/core";
import { PopupBase } from "src/app/popup.base";
import { DocumentationRepo } from "src/app/shared/repositories";

@Component({
    selector: 'add-remaining-charge-popup',
    templateUrl: './add-remaining-charge.popup.html'
})
export class CdNoteAddRemainingChargePopupComponent extends PopupBase {
    constructor(
        private _documentationRepo: DocumentationRepo,
    ) {
        super();
        
    }

    ngOnInit() {
        
    }

    addCharge(){

    }

    deleteCdNote(){

    }
    
    closePopup(){
        this.hide();
    }
}