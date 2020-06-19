import { Component } from "@angular/core";
import { PopupBase } from "src/app/popup.base";

@Component({
    selector: 'input-denied-comment-popup',
    templateUrl: './input-denied-comment.popup.html'
})
export class UnlockRequestInputDeniedCommentPopupComponent extends PopupBase {
    comment: string = '';
    constructor(
    ) {
        super();
    }

    ngOnInit() { }

    ok() {

    }

    closePopup() {
        this.hide();
    }
}