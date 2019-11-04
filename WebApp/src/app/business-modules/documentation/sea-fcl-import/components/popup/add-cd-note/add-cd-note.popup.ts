import { PopupBase } from "src/app/popup.base";
import { Component } from "@angular/core";
@Component({
    selector: 'cd-note-add-popup',
    templateUrl: './add-cd-note.popup.html'
})
export class CdNoteAddPopupComponent extends PopupBase {
    //@ViewChild('exitPopup', { static: false }) exitPopup: ConfirmPopupComponent;

    constructor(
        
    ) {
        super();
    }

    ngOnInit() {
        
    }

    closePopup() {
        this.hide();
        //this.resetForm();
    }
}